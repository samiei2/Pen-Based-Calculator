using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace HW1Armin
{
    internal class PolyLineFit2
    {
        private List<Point> corners;
        private bool m_allLinesPassed;
        private double m_err;
        private int m_fail;
        private double m_lsqe;
        private int m_numPassed;
        private bool m_passed = true;
        private List<StylusPointCollection> m_subStrokes;
        private List<double> m_subStrokesLength;
        private Recognizer recognizer;
        public static double M_DCR_TO_BE_POLYLINE = 6.0; // J
        public static double M_POLYLINE_LS_ERROR = 1.3;// 1.0;//0.0036; // I
        private double m_avgAngularDirection;

        /**
	     * Post-processing flag that occurs after corner finding. This will combine
	     * any segments that are considered too small to be relevant. This is
	     * helpful if the recognizer uses a corner finder that produces many
	     * false-positives. Default = true
	     */
        public bool SMALL_POLYLINE_COMBINE = true;
        /**
	     * Post-processing flag that occurs after corner finding. This will combine
	     * any consecutive segments have similar slopes. This is helpful if the
	     * recognizer uses a corner finder that produces many false-positives.
	     * Default = true
	     */
        public bool SIM_SLOPE_POLYLINE_COMBINE = true;
        /**
	     * Post-processing flag that occurs after corner finding. This will combine
	     * any consecutive segments that (when combined) pass a Paleo line test.
	     * This is helpful if the recognizer uses a corner finder that produces many
	     * false-positives. Default = true
	     */
        public bool LINE_TEST_COMBINE = true;

        public PolyLineFit2(Recognizer recognizer)
        {
            this.recognizer = recognizer;
        }

        public PolyLineFit2(Recognizer recognizer, List<Point> list) : this(recognizer)
        {
            this.corners = list;
        }

        internal void Test()
        {
            m_allLinesPassed = true;
            m_err = 0;
            m_lsqe = 0;
            m_subStrokes = GetSegments();
            m_subStrokesLength = new List<double>(m_subStrokes.Count);

            // check for small, extraneous lines in interpretation
            if (SMALL_POLYLINE_COMBINE
                    && m_subStrokes.Count < 10)
            {
                m_subStrokes = combineSmallLines(m_subStrokes);
            }

            //// System.out.println("num substrokes = " + m_subStrokes.size());

            //// check for consecutive lines with similar slopes, combine if similar
            if (SIM_SLOPE_POLYLINE_COMBINE)
            {
                m_subStrokes = combineSimilarSlopes(m_subStrokes);
            }

            //// System.out.println("num substrokes = " + m_subStrokes.size());

            //// check for consecutive lines that, when combined, pass a line test
            if (LINE_TEST_COMBINE && m_subStrokes.Count > 2)
            {
                m_subStrokes = combineLineTest(m_subStrokes);
            }

            // test 1: we need at least 2 substrokes
            if (m_subStrokes.Count < 2)
            {
                m_passed = false;
                m_fail = 0;
            }
            else
            {
                // test 2: run line test on all sub strokes and get a cumulative
                // error
                for (int i = 0; i < m_subStrokes.Count; i++)
                {

                    // if we have 2 or fewer points we automatically have a line
                    if (m_subStrokes[i].Count > 1)
                    {
                        LineFit lf = new LineFit();
                        lf.getFit(m_subStrokes[i]);
                        m_subStrokesLength.Add(lf.lineLength);
                        m_err += lf.getError();
                        m_lsqe += lf.getLSQE();
                        //m_subshapes.add(lf.getShape());
                        if (!lf.passed())
                        {
                            m_allLinesPassed = false;
                            m_passed = false;
                            m_fail = 1;
                        }
                        else
                            m_numPassed++;
                    }
                    else
                        m_numPassed++;
                }
                m_err /= recognizer.getStrokeLength();
                m_lsqe /= recognizer.getStrokeLength();

                // dcr should be high
                if (recognizer.getDCR() < M_DCR_TO_BE_POLYLINE)
                {
                    m_passed = false;
                    m_fail = 2;
                }

                // test 3.2: if total lsqe is low then accept even if not all
                // substrokes passed polyline test
                if (m_lsqe < M_POLYLINE_LS_ERROR)
                {// || m_allLinesPassed)
                    m_passed = true;
                    m_fail = 3;
                }
            }

            // calcular avg angular direction
            double sum = 0.0;
            foreach (var s in m_subStrokes)
            {
                sum += Math.Abs(angularDirection(s));
            }
            m_avgAngularDirection = (sum * 180.0) / (m_subStrokes.Count * Math.PI);

        }

        internal List<double> GetSubStrokesLength()
        {
            return m_subStrokesLength;
        }

        internal List<StylusPointCollection> GetSubStrokes()
        {
            return m_subStrokes;
        }

        internal bool passed()
        {
            return m_passed;
        }

        /**
	     * Get the angular direction of a stroke
	     * 
	     * @param str
	     *            stroke
	     * @return angular direction
	     */
        protected double angularDirection(StylusPointCollection str)
        {
            double angle;
            angle = Math.Atan2(str[str.Count - 1].Y
                    - str[0].Y, str[str.Count - 1].X
                    - str[0].X);
            return angle;
        }

        private List<StylusPointCollection> GetSegments()
        {
            var strokePoints = recognizer.getPoints();
            List <StylusPoint> exactCornerPoints = getExactPoints();
            var cornersCopy = new List<Point>(corners);
            List<StylusPointCollection> sub_Strokes = new List<StylusPointCollection>();
            StylusPointCollection sub_collection = new StylusPointCollection();
            int index = 1;
            foreach (var point in strokePoints)
            {
                sub_collection.Add(point);
                if (point.X == exactCornerPoints[index].X && point.Y == exactCornerPoints[index].Y)
                {
                    sub_Strokes.Add(sub_collection);
                    sub_collection = new StylusPointCollection();
                    index++;
                    if (index == cornersCopy.Count)
                        break;
                }
            }
            return sub_Strokes;
        }

        private List<StylusPoint> getExactPoints()
        {
            List<StylusPoint> points = new List<StylusPoint>();
            int index = 0;
            foreach (var corner in corners)
            {
                StylusPoint foundPoint = new StylusPoint();
                double pointDist = Double.MaxValue;
                foreach (var item in recognizer.getPoints())
                {
                    var calcDist = distance(corner,item);
                    if (calcDist < pointDist)
                    {
                        pointDist = calcDist;
                        foundPoint = item;
                    }
                }
                points.Add(foundPoint);
            }
            return points;
        }

        /**
	 * Check list of strokes to see if any can be removed as insignificant
	 * substrokes (small percentage of total stroke)
	 * 
	 * @param strokes
	 *            strokes to check
	 * @return merged/combined strokes
	 */
        private List<StylusPointCollection> combineSmallLines(List<StylusPointCollection> strokes)
        {
            if (strokes.Count <= 1)
                return strokes;
            double numTotal = 0.0;
            foreach (StylusPointCollection s in strokes)
                numTotal += s.Count;
            // System.out.println("numPoints = " + numTotal);
            for (int i = 0; i < strokes.Count; i++)
            {
                // System.out.println("ratio = " + strokes.get(i).getNumPoints()
                // / numTotal);
                if (strokes[i].Count / numTotal < 0.06)
                {
                    // last stroke
                    if (i == strokes.Count - 1)
                    {
                        if (i != 0)
                        {
                            StylusPointCollection newStroke = combine(strokes[i - 1],
                                    strokes[i]);
                            strokes.RemoveAt(i - 1);
                            strokes.RemoveAt(i - 1);
                            strokes.Insert(i - 1, newStroke);
                        }
                    }
                    // middle stroke
                    else
                    {
                        StylusPointCollection newStroke = combine(strokes[i],
                                strokes[i + 1]);
                        strokes.RemoveAt(i);
                        strokes.RemoveAt(i);
                        strokes.Insert(i, newStroke);
                    }
                    strokes = combineSmallLines(strokes);
                }
            }
            return strokes;
        }

        /**
	     * Combine two strokes into one
	     * 
	     * @param s1
	     *            stroke one
	     * @param s2
	     *            stroke two
	     * @return new stroke containing stroke one followed by stroke two
	     */
        protected StylusPointCollection combine(StylusPointCollection s1, StylusPointCollection s2)
        {
            StylusPointCollection newStroke = new StylusPointCollection();
            for (int i = 0; i < s1.Count; i++)
                newStroke.Add(s1[i]);
            for (int i = 1; i < s2.Count; i++)
                newStroke.Add(s2[i]);

            //newStroke.setParent(s1.getParent());
            return newStroke;
        }

        /**
	 * Check list of strokes to see if any can be combined by checking to see if
	 * there is a small change in slope between consecutive lines
	 * 
	 * @param strokes
	 *            strokes to check
	 * @return merged/combined strokes
	 */
        private List<StylusPointCollection> combineSimilarSlopes(List<StylusPointCollection> strokes)
        {
            if (strokes.Count <= 1)
                return strokes;
            for (int i = 1; i < strokes.Count; i++)
            {
                double s1 = getSlope(strokes[i]);
                double s2 = getSlope(strokes[i - 1]);
                // double slopeDiff = Math.abs(s1 - s2);
                double value = Math.Atan((s1 - s2) / (1 + s1 * s2))
                        * (180 / Math.PI);
                if (Double.IsNaN(value))
                    value = 90.0;
                // System.out.println("angle diff = " + value);
                value = Math.Abs(value);
                double ySignS1 = Math.Sign(strokes[i][strokes[i].Count - 1].Y
                        - strokes[i][0].Y);
                double xSignS1 = Math.Sign(strokes[i][strokes[i].Count - 1].X
                        - strokes[i][0].X);
                double ySignS2 = Math.Sign(strokes[i - 1][strokes[i - 1].Count - 1]
                        .Y
                        - strokes[i - 1][0].Y);
                double xSignS2 = Math.Sign(strokes[i - 1][strokes[i - 1].Count - 1]
                        .X
                        - strokes[i - 1][0].X);
                /*
                 * double length = strokes.get(i).getFirstPoint().distance(
                 * strokes.get(i).getLastPoint()) + strokes.get(i -
                 * 1).getFirstPoint().distance( strokes.get(i - 1).getLastPoint());
                 */

                /*
                 * System.out.println("xSignS1 = " + xSignS1 + " ySignS1 = " +
                 * ySignS1 + " xSignS2 = " + xSignS2 + " ySignS2 = " + ySignS2 +
                 * " value = " + value + " length = " + length + " value*length = "
                 * + value length);
                 */

                if (value < 25.0 && value > 0 && ySignS1 == ySignS2
                        && xSignS1 == xSignS2 /* slopeDiff < (2.0 / 3.0) */)
                {
                    StylusPointCollection newStroke = combine(strokes[i - 1], strokes[i]);
                    strokes.RemoveAt(i - 1);
                    strokes.RemoveAt(i - 1);
                    strokes.Insert(i - 1, newStroke);
                    strokes = combineSimilarSlopes(strokes);
                    // System.out.println("fail = " + value);
                }
            }
            return strokes;
        }

        /**
	     * Get the slope of a given Stroke
	     * 
	     * @param s
	     *            stroke to find slope of
	     * @return slope of stroke (assumed to be a line)
	     */
        private double getSlope(StylusPointCollection s)
        {
            StylusPoint p1 = s[0];
            StylusPoint p2 = s[s.Count - 1];
            return (p2.Y - p1.Y) / (p2.X - p1.X);
        }

        /**
	     * Check list of strokes to see if any can be combined by checking to see if
	     * together they pass a line test
	     * 
	     * @param strokes
	     *            strokes to check
	     * @return merged/combined strokes
	     */
        private List<StylusPointCollection> combineLineTest(List<StylusPointCollection> strokes)
        {
            if (strokes.Count <= 1)
                return strokes;
            for (int i = 1; i < strokes.Count; i++)
            {
                StylusPointCollection newStroke = combine(strokes[i - 1], strokes[i]);
                Recognizer newFeatures;
                newFeatures = new Recognizer(newStroke);
                LineFit lineFit = new LineFit();
                lineFit.FitTest(newFeatures, true);
                if (lineFit.passed())
                {
                    // System.out.println("line fit: " + lineFit.m_err + " lsqe = "
                    // + lineFit.m_lsqe/lineFit.m_features.getStrokeLength() +
                    // " ratio = " + lineFit.m_ratio);
                    strokes.RemoveAt(i - 1);
                    strokes.RemoveAt(i - 1);
                    strokes.Insert(i - 1, newStroke);
                    strokes = combineLineTest(strokes);
                }
            }
            return strokes;
        }

        private double distance(Point point, StylusPoint item)
        {
            return Math.Sqrt(Math.Pow((point.X - item.X),2)+Math.Pow((point.Y - item.Y),2));
        }

        private int GetNumberOfLines()
        {
            return m_subStrokes.Count;
        }
    }
}