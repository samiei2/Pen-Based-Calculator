using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace HW1Armin
{
    internal class ArrowFit
    {
        private int m_fail;
        private bool m_passed = true;
        private List<StylusPointCollection> m_subStrokes;
        private Recognizer recognizer;
        private double m_lastTwoDiff;
        private double m_headDistance;
        private double m_standardSum;
        private int m_numIntersect;
        private bool m_standardPassed;
        private bool m_trianglePassed;
        private bool m_diamondPassed;

        public ArrowFit(Recognizer recognizer, List<StylusPointCollection> list)
        {
            this.recognizer = recognizer;
            this.m_subStrokes = list;
        }

        internal void Test()
        {
            // test 1: must have enough sub-strokes to be an arrow (min 4)
            if (m_subStrokes.Count < 4)
            {
                m_passed = false;
                m_fail = 0;
            }

            // test 2: dcr must be high
            // if (m_features.getDCR() < M_DCR_TO_BE_POLYLINE)
            // m_passed = false;

            // only continue if we have enough strokes
            if (m_passed)
            {
                // test 2: see if stroke passes standard arrow test
                m_standardPassed = checkStandardArrow();

                // test 3: see if stroke passes standard arrow test
                // m_trianglePassed = checkTriangleArrow();

                // test 4: see if stroke passes diamond arrow test
                // m_diamondPassed = checkDiamondArrow();

                // if no tests passed then we don't have an arrow
                if (!m_standardPassed && !m_trianglePassed && !m_diamondPassed)
                {
                    m_passed = false;
                    m_fail = 1;
                }
            }
        }

        /**
	 * Check to see if stroke is a standard arrow
	 * 
	 * @return true if test passes; else false
	 */
        protected bool checkStandardArrow()
        {
            bool passed = true;
            StylusPointCollection last = m_subStrokes[m_subStrokes.Count - 1];
            StylusPointCollection secondLast = m_subStrokes[m_subStrokes.Count - 2];
            StylusPointCollection thirdLast = m_subStrokes[m_subStrokes.Count - 3];
            StylusPointCollection fourthLast = m_subStrokes[m_subStrokes.Count - 4];
            double lastLength = Recognizer.getStrokeLength(last);
            double secondLastLength = Recognizer.getStrokeLength(secondLast);
            double thirdLastLength = Recognizer.getStrokeLength(thirdLast);

            // test 1: last two sub-strokes must be close in size
            m_lastTwoDiff = Math.Abs(lastLength - secondLastLength)
                    / (lastLength + secondLastLength);
            if (m_lastTwoDiff > 0.5)
                passed = false;

            // test 2: two points at the "head" of the arrow should be close
            m_headDistance = distance(last[0],
                    thirdLast[0])
                    / recognizer.getStrokeLength();
            if (m_headDistance > 0.11)
                passed = false;
            m_standardSum = m_headDistance;

            // test 3: line connecting tips of arrow head should intersect shaft of
            // arrow
            Line2D.Double line1 = new Line2D.Double(
                    thirdLast[thirdLast.Count - 1].X, thirdLast[thirdLast.Count - 1].Y, 
                            last[last.Count - 1].X, last[last.Count - 1].Y);
            List<Point2D> intersect = Recognizer.getIntersection(
                    fourthLast, line1);
            m_numIntersect = intersect.Count;
            if (m_numIntersect <= 0)
                passed = false;
            // Line2D.Double line2 = new Line2D.Double(fourthLast.getPoints().get(
            // fourthLast.getNumPoints() / 2).getX(), fourthLast.getPoints()
            // .get(fourthLast.getNumPoints() / 2).getY(), fourthLast
            // .getLastPoint().getX(), fourthLast.getLastPoint().getY());
            // double perpDiff = Math.abs(getSlope(line1) - (1.0 /
            // getSlope(line2)));
            // if (perpDiff > 5)
            // passed = false;
            
            return passed;
        }

        internal bool passed()
        {
            return m_passed;
        }

        public double distance(StylusPoint p1, StylusPoint p2)
        {
            double Xdist = p1.X - p2.X;
            double Ydist = p1.Y - p2.Y;

            return Math.Sqrt(Xdist * Xdist + Ydist * Ydist);
        }
    }
}