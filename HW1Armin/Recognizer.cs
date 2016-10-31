using BitsOfStuff;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace HW1Armin
{
    public class Recognizer
    {
        private float errorTolerance = 0.1f;
        private StylusPointCollection stylusOriginalPoints;
        private StylusPointCollection m_points;
        private List<StylusPointCollection> strokes_toPaint;

        public Recognizer(StylusPointCollection stylusPoints)
        {
            strokes_toPaint = new List<StylusPointCollection>();
            stylusOriginalPoints = new StylusPointCollection(stylusPoints);
            m_points = stylusPoints;
        }

        internal ResultData Recognize()
        {
            try
            {
                if (m_points.Count <= 1)
                    return new ResultData(new List<StylusPointCollection>() { m_points }, false, "Dot");
                PreProcess();
                return Process();
            }
            catch (Exception)
            {
                return new ResultData(new List<StylusPointCollection>() { m_points }, false, "Unknown");
            }
        }

        public void PreProcess()
        {
            RemoveDuplicates();
            computeValues();
            removeHooks();
            computeValues();
            computePaleoFeatures();
        }

        public ResultData Process()
        {
            ShortStraw cornerFinder = new ShortStraw();
            var guid = Guid.NewGuid();
            var stroke = new Stroke(m_points);
            stroke.AddPropertyData(guid, new int[m_points.Count]);
            cornerFinder.FindCorner(stroke,guid.ToString());
            var corners = cornerFinder.m_corners;
            var nums = cornerFinder.m_numC;

            var scratchFit = new ScratchFit();
            scratchFit.FitTest(this, corners.ToList());


            var lineFit = new LineFit();
            lineFit.FitTest(this);
            Console.WriteLine(lineFit.passed());

            var polyFit = new PolyLineFit2(this, corners.ToList());
            polyFit.Test();
            
            var ellipseFit = new EllipseFit(this);
            ellipseFit.Test();

            var circleFit = new CircleFit(this, ellipseFit);
            circleFit.Test();

            var arrowFit = new ArrowFit(this,polyFit.GetSubStrokes());
            arrowFit.Test();

            strokes_toPaint.Add(m_points);
            if (scratchFit.passed())
            {
                return new ResultData(
                    strokes_toPaint,
                    scratchFit.passed(), "Scratch");
            }
            if (lineFit.passed())
            {
                return new ResultData(
                    strokes_toPaint,
                    lineFit.passed(),"Line");
            }
            else
            {
                if(polyFit.passed() && corners.Count > 3 && !arrowFit.passed()) // Poly
                {
                    if(corners.Count == 4 && polyFit.GetSubStrokes().Count == 3) // Triangle
                    {
                        return new ResultData(
                            strokes_toPaint,
                            lineFit.passed(), "Triangle");
                    }
                    if (corners.Count == 5 && polyFit.GetSubStrokes().Count == 4) // Square or Rectangle
                    {
                        var substrokes = polyFit.GetSubStrokes();
                        var subStrokesLength = polyFit.GetSubStrokesLength();
                        var ratio = Math.Max(subStrokesLength[0] , subStrokesLength[1]) /
                            Math.Min(subStrokesLength[0], subStrokesLength[1]);
                        if(ratio < 1.2) //  Square
                        {
                            return new ResultData(
                            strokes_toPaint,
                            lineFit.passed(), "Square");
                        }
                        else // Rectangle
                        {
                            return new ResultData(
                            strokes_toPaint,
                            lineFit.passed(), "Rectangle");
                        }
                    }
                }

                if (ellipseFit.passed() && !circleFit.passed())  // Ellipse
                {
                    return new ResultData(
                            strokes_toPaint,
                            lineFit.passed(), "Ellipse");
                }

                if(ellipseFit.passed() && circleFit.passed()) // Circle
                {
                    return new ResultData(
                            strokes_toPaint,
                            lineFit.passed(), "Circle");
                }

                if (arrowFit.passed())
                {
                    return new ResultData(
                            strokes_toPaint,
                            lineFit.passed(), "Arrow");
                }
            }

            return new ResultData(
                            strokes_toPaint,
                            lineFit.passed(), "Unknown");

            //strokes_toPaint.Add(m_points);
            //return new ResultData( 
            //    strokes_toPaint,
            //    lineFit.passed(),
            //    ""
            //    //+ "Num.Corners: " + corners 
            //    + "\nPolyLine?: " + polyFit.passed()
            //    + "\nArrow?: " + arrowFit.passed()
            //    + "\nEllipse?: " + ellipseFit.passed()
            //    + "\nCircle?: " + circleFit.passed()
            //    + "\nLine?: " + lineFit.passed());
        }
        
        private StylusPointCollection RemoveDuplicatePoints(StylusPointCollection stylusPoints)
        {
            StylusPointCollection newCollection = new StylusPointCollection(stylusPoints.ToArray());
            foreach (var item in stylusPoints)
            {
                foreach (var item2 in stylusPoints)
                {
                    if (item.X == item2.X && item.Y == item2.Y)
                        newCollection.Remove(item);
                }
            }
            return newCollection;
        }
        
        /// <summary>
        /// Douglas Peucker implementation.
        /// This is for smoothing the stroke
        /// </summary>
        /// <param name="stylusPoints"></param>
        /// <param name="errorTolerance"></param>
        /// <returns></returns>
        private StylusPointCollection DouglasPeucker(StylusPointCollection stylusPoints, float errorTolerance)
        {
            int n = stylusPoints.Count;
            if (stylusPoints.Count < 3)
                return stylusPoints;
            return stylusPoints;

            //line between p1,pn with form ax+by+c=0
            var p1 = stylusPoints[0];
            var pn = stylusPoints[stylusPoints.Count - 1];

            var a = (p1.Y - pn.Y);
            var b = (pn.X - p1.X);
            var c = ((p1.X - pn.X) * p1.Y + (pn.Y - p1.Y) * p1.X);
            /////////////////////////////////

            double maxDistance = -1;
            StylusPoint pz;
            int zindex;
            for (int i = 2; i < n - 2; i++)
            {
                
                var distance = PointLineDistance(a, b, c, stylusPoints[i]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    pz = stylusPoints[i];
                    zindex = i;
                }
            }
            if (maxDistance < errorTolerance)
                return new StylusPointCollection() { p1,pn};
            else
            {
                var left = DouglasPeucker(GetRange(stylusPoints,0,zindex),errorTolerance);
                var right = DouglasPeucker(GetRange(stylusPoints, zindex, stylusPoints.Count - 1), errorTolerance);
                var mergedCollection = new StylusPointCollection();
                mergedCollection.Add(left);
                mergedCollection.Add(right);
                return mergedCollection;
            }
        }
        
        private StylusPointCollection GetRange(StylusPointCollection stylusPoints, int v, int zindex)
        {
            StylusPointCollection newCollection = new StylusPointCollection();
            for (int i = v; i < zindex; i++)
            {
                newCollection.Add(stylusPoints[i]);
            }
            return newCollection;
        }

        private double PointLineDistance(double a, double b, double c, StylusPoint stylusPoint)
        {
            return Math.Abs(a * stylusPoint.X + b * stylusPoint.Y + c) / Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stylusPoints"></param>
        /// <returns></returns>
        private StylusPointCollection InterpolatePoints(StylusPointCollection stylusPoints)
        {
            float sumOfDistances = 0;
            for (int i = 1; i < stylusPoints.Count-1; i++)
            {
                float distance = GetEuclidianDistance(stylusPoints[i - 1], stylusPoints[i]);
                sumOfDistances += distance;
            }
            var le = sumOfDistances / stylusPoints.Count;
            double xPrime;
            double yPrime;
            int count = stylusPoints.Count;
            for (int i = 0; i < count - 2; i++)
            {
                var point1 = stylusPoints[i];
                var point2 = stylusPoints[i];
                var slope = point2.Y - point1.Y / point2.X - point1.X;
                if (point1.X < point2.X)
                {
                    xPrime = point1.X + Math.Sqrt(Math.Pow(le, 2) / (Math.Pow(slope, 2) + 1));
                }
                else if (point1.X > point2.X)
                {
                    xPrime = point1.X - Math.Sqrt(Math.Pow(le, 2) / (Math.Pow(slope, 2) + 1));
                }
                else
                {
                    xPrime = point1.X;
                }

                if (point1.X != point2.X)
                {
                    yPrime = slope * xPrime + point1.Y - slope * point1.X;
                }
                else if (point1.X != point2.X && point1.Y < point2.Y)
                {
                    yPrime = point1.Y + le;
                }
                else if (point1.X != point2.X && point1.Y > point2.Y)
                {
                    yPrime = point1.Y - le;
                }
            }
            return null;
        }

        private float GetEuclidianDistance(StylusPoint stylusPoint1, StylusPoint stylusPoint2)
        {
            throw new NotImplementedException();
        }

        #region PaleoSketch

        #region Variables
        /**
	     * Direction values over time (first derivative)
	     */
        protected List<double> m_dir;

        /**
	     * Direction values over time w/o shifting
	     */
        protected List<double> m_dirNoShift;

        /**
	     * Running total of stroke length over time
	     */
        protected List<double> m_lengthSoFar;

        /**
	     * List of consecutive lengths of individual line segments
	     */
        protected List<double> m_segLength;

        /**
	     * Running total of stroke length over time but from the second derivatives
	     * viewpoint
	     */
        protected List<double> m_2lengthSoFar;

        /**
	     * Absolute curvature values over time (second derivative)
	     */
        protected List<double> m_curvature;

        /**
         * Curvature values (no absolute value) over time
         */
        protected List<double> m_curvNoAbs;

        /**
         * Running total of the sum of the curvature values over time
         */
        protected List<double> m_totalCurvature;

        /**
	     * Maximum curvature value for the stroke
	     */
        protected double m_max_curv;

        /**
         * Index of the point with the maximum curvature value
         */
        protected int m_max_curv_index;

        /**
	     * Perform smoothing of the direction graph?
	     */
        protected bool m_smoothing;

        /**
         * Ratio of maximum curvature value to average curvature value
         */
        protected double m_maxCurvToAvgCurvRatio;

        /**
         * Average curvature of entire stroke
         */
        protected double m_avgCurvature;

        /**
	     * Total length of the stroke (Euclidean distance)
	     */

        /**
         * Normalized distance between direction extremes
         */
        protected double m_NDDE;

        /**
         * Direction change ratio
         */
        protected double m_DCR;

        /**
         * Total rotation of the stroke
         */
        protected double m_totalRotation;
        
        /**
         * Number of 2PI revolutions that the stroke makes
         */

        /**
         * Flag denoting whether or not a stroke is overtraced (makes multiple
         * revolutions)
        */
        protected bool m_overtraced;

        /**
         * Flag denoting whether or not a stroke is close to being complete
         * (closed); i.e. end points must be near
         */
        protected bool m_complete;

        /**
	     * Ratio of the distance (Euclidean) between the end points and the total
	     * stroke length
	     */
        protected double m_endPtStrokeLengthRatio;

        /**
	     * Left most point of stroke
	     */
        protected StylusPoint m_leftMostPoint;

        /**
         * Right most point of stroke
         */
        protected StylusPoint m_rightMostPoint;

        /**
         * Bottom most point of stroke
         */
        protected StylusPoint m_bottomMostPoint;

        /**
         * Top most point of stroke
         */
        protected StylusPoint m_topMostPoint;

        /**
	     * Maximum direction value
	     */
        protected double m_maxDir;

        /**
         * Minimum direction value
         */
        protected double m_minDir;

        /**
	     * Bounding box of the stroke
	     */
        protected BoundingBox m_bounds;

        /**
	     * Ratio between perimeter and stroke length
	     */
        protected double m_perimStrokeLengthRatio;

        /**
	     * Angle of the major axis (relative to [0,0])
	     */
        protected double m_majorAxisAngle;

        /**
	     * Major axis of the ellipse fit
	     */
        protected Line2D m_majorAxis;

        /**
	     * Best fit line of the stroke itself
	     */
        protected Line2D m_bestFitLine;

        /**
         * Flag specifying whether the direction window test passed (see
         * calcDirWindowPassed() for information about this test)
         */
        protected bool m_dirWindowPassed;

        /**
         * Percentage of the direction window test passed
         */
        protected double m_pctDirWindowPassed;

        /**
	     * Length of the major axis
	     */
        protected double m_majorAxisLength;

        /**
	     * Distance between furthest corner and the actual stroke
	     */
        protected double m_cornerStrokeDistance;

        /**
	     * Distance between closest corner and the actual stroke
	     */
        protected double m_minCornerStrokeDistance;

        /**
         * Std dev between corners and closest points
         */
        protected double m_stdDevCornerStrokeDistance;

        /**
	     * Avg distance between closest point to each corner of the bounding box
	     */
        protected double m_avgCornerStrokeDistance;

        protected double m_numRevolutions;

        protected double m_strokelength;

        public static int M_HOOK_MINPOINTS = 5; // B

        public static int M_HOOK_MINSTROKELENGTH = 70; // C

        public static int M_HOOK_MAXHOOKLENGTH = 10;

        public static double M_HOOK_MAXHOOKPERCENT = .2;

        public static double M_HOOK_MINHOOKCURVATURE = .5; // A

        public static double M_REVS_TO_BE_OVERTRACED = 1.31; // D

        public static double M_PERCENT_DISTANCE_TO_BE_COMPLETE = 0.17; // E

        public static double M_NUM_REVS_TO_BE_COMPLETE = 0.75;// 0.76;//0.75

        public static double M_DIR_WINDOW_SIZE = 5.0;

        #endregion

        private void RemoveDuplicates()
        {
            var stylusPoints = m_points;
            StylusPointCollection newPoints = new StylusPointCollection();
            newPoints.Add(stylusPoints[0]);
            for (int i = 1; i < stylusPoints.Count; i++)
            {
                if (stylusPoints[i - 1].X == stylusPoints[i].X
                    && stylusPoints[i - 1].Y == stylusPoints[i].Y)
                {
                    // do nothing
                }
                else
                {
                    // add point to new list
                    newPoints.Add(stylusPoints[i]);

                    // check for same time value
                    //if (newPoints.Count > 1
                    //        && newPoints[newPoints.Count - 1]..getTime() == newPoints
                    //                .get(newPoints.size() - 2).getTime())
                    //{
                    //    if (newPoints.size() == 2)
                    //    {
                    //        newPoints.set(newPoints.size() - 1, new Point(newPoints
                    //                .get(newPoints.size() - 1).getX(), newPoints
                    //                .get(newPoints.size() - 1).getY(), newPoints
                    //                .get(newPoints.size() - 1).getTime() + 1));
                    //    }
                    //    else
                    //    {
                    //        newPoints.set(newPoints.size() - 2, new Point(newPoints
                    //                .get(newPoints.size() - 2).getX(), newPoints
                    //                .get(newPoints.size() - 2).getY(), newPoints
                    //                .get(newPoints.size() - 3).getTime()
                    //                + newPoints.get(newPoints.size() - 1).getTime()
                    //                / 2));
                    //    }
                    //}
                }
            }
            m_points = newPoints;
        }

        protected void computeValues()
        {
            m_dir = new List<double>(new double[getNumPoints() - 1]);
            m_dirNoShift = new List<double>(new double[getNumPoints() - 1]);
            m_segLength = new List<double>(new double[getNumPoints() - 1]);
            m_lengthSoFar = new List<double>(new double[getNumPoints() - 1]);
            if (getNumPoints() > 1)
            {
                m_2lengthSoFar = new List<double>(new double[getNumPoints() - 2]);
                m_curvature = new List<double>(new double[getNumPoints() - 2]);
                m_curvNoAbs = new List<double>(new double[getNumPoints() - 2]);
                m_totalCurvature = new List<double>(new double[getNumPoints() - 2]);
            }
            else
            {
                m_2lengthSoFar = new List<double>(new double[getNumPoints() - 1]);
                m_curvature = new List<double>(new double[getNumPoints() - 1]);
                m_curvNoAbs = new List<double>(new double[getNumPoints() - 1]);
                m_totalCurvature = new List<double>(new double[getNumPoints() - 1]);
            }
            m_max_curv = 0.0;
            m_max_curv_index = 0;

            // compute direction graph
            for (int i = 0; i < getNumPoints() - 1; i++)
            {
                m_dir[i] = Math.Atan2(m_points[i + 1].Y
                        - m_points[i].Y, m_points[i + 1].X
                        - m_points[i].X);
                m_dirNoShift[i] = m_dir[i];
                while ((i > 0) && (m_dir[i] - m_dir[i - 1] > Math.PI))
                    m_dir[i] = m_dir[i] - 2 * Math.PI;
                while ((i > 0) && (m_dir[i - 1] - m_dir[i] > Math.PI))
                    m_dir[i] = m_dir[i] + 2 * Math.PI;
                m_segLength[i] = Math.Sqrt((m_points[i + 1].Y - m_points[i].Y)
                        * (m_points[i + 1].Y - m_points[i].Y)
                        + (m_points[i + 1].X - m_points[i].X)
                        * (m_points[i + 1].X - m_points[i].X));
                if (i == 0)
                    m_lengthSoFar[i] = m_segLength[i];
                else
                    m_lengthSoFar[i] = m_lengthSoFar[i - 1] + m_segLength[i];
            }
            if (m_lengthSoFar.Count > 0)
                m_strokelength = m_lengthSoFar[m_lengthSoFar.Count - 1];
            else
                m_strokelength = 1.0;

            // perform smoothing if desired
            if (m_smoothing)
            {
                m_dir = Filtering.medianFilter(m_dir);
                m_dirNoShift = Filtering.medianFilter(m_dirNoShift);
            }

            // compute curvature graph
            for (int i = 0; i < getNumPoints() - 2; i++)
            {
                m_2lengthSoFar[i] = m_lengthSoFar[i + 1];
                m_curvature[i] = Math.Abs(m_dir[i + 1] - m_dir[i])
                        / (m_segLength[i] + m_segLength[i + 1]);
                m_curvNoAbs[i] = (m_dir[i + 1] - m_dir[i])
                        / (m_segLength[i] + m_segLength[i + 1]);
                if (Math.Abs(m_curvature[i]) > m_max_curv && i != 0)
                {
                    m_max_curv = Math.Abs(m_curvature[i]);
                    m_max_curv_index = i;
                }
                if (i == 0)
                    m_totalCurvature[i] = m_curvature[i];
                else
                    m_totalCurvature[i] = m_totalCurvature[i - 1] + m_curvature[i];
            }
            if (m_totalCurvature.Count > 0)
            {
                m_avgCurvature = m_totalCurvature[m_totalCurvature.Count - 1]
                        / m_totalCurvature.Count;
                m_maxCurvToAvgCurvRatio = m_max_curv / m_avgCurvature;
            }
        }

        protected void removeHooks()
        {
            // conditions for not removing tails (basically if stroke is too small)
            if (getNumPoints() < M_HOOK_MINPOINTS || m_totalCurvature.Count == 0
                    || m_lengthSoFar[getNumPoints() - 2] < M_HOOK_MINSTROKELENGTH
                    || m_strokelength < M_HOOK_MINSTROKELENGTH)
                return;

            double hookcurvature = 0;
            int startindex = 0;

            // check for hooks at the beginning of the stroke
            for (int i = 1; i < getNumPoints() - 1; i++)
            {

                // only check for tails near endpoints; if we have gone too far into
                // the stroke then we are no longer checking for hooks
                if (m_lengthSoFar[i] > M_HOOK_MAXHOOKLENGTH
                        || m_lengthSoFar[i] / m_strokelength > M_HOOK_MAXHOOKPERCENT)
                    break;

                // finding the maximum curvature value at the beginning of the
                // stroke
                if (Math.Abs(m_totalCurvature[i]) > hookcurvature)
                {
                    hookcurvature = Math.Abs(m_totalCurvature[i]);
                    startindex = i + 1;
                }
            }

            // max curvature near start point is too small to denote a tail
            if (hookcurvature < M_HOOK_MINHOOKCURVATURE)
                startindex = 0;

            // check for hooks at the end of the stroke
            hookcurvature = 0;
            int endindex = getNumPoints();
            for (int i = 1; i < getNumPoints() - 1; i++)
            {

                int startIndex = m_totalCurvature.Count - 1 - i;
                if (startIndex < 0)
                    startIndex = 0;

                double c = m_totalCurvature[m_totalCurvature.Count - 1]
                        - m_totalCurvature[startIndex];
                double l = m_lengthSoFar[m_lengthSoFar.Count - 1]
                        - m_lengthSoFar[m_lengthSoFar.Count - 1 - i];

                // we have gone too far into the stroke so we stop
                if (l > M_HOOK_MAXHOOKLENGTH
                        || l / m_strokelength > M_HOOK_MAXHOOKPERCENT)
                    break;

                // finding max curvature value near end of the stroke
                if (Math.Abs(c) > hookcurvature)
                {
                    hookcurvature = Math.Abs(c);
                    endindex = getNumPoints() - i;
                }
            }

            // max curvature near end point is too small to denote a tail
            if (Math.Abs(hookcurvature) < M_HOOK_MINHOOKCURVATURE)
                endindex = getNumPoints();

            // update size, x, y, and time values with tails removed
            StylusPointCollection newPoints = new StylusPointCollection();
            for (int i = startindex; i < endindex; i++)
                newPoints.Add(m_points[i]);
            m_points = newPoints;
        }

        protected void computePaleoFeatures()
        {
            calcBounds();
            calcTotalRotation();
            calcNDDE();
            calcDCR();
            calcBestFitLineForDirGraph();
            calcBestFitLine();
            calcDirWindowPassed();
            calcMajorAxis();
            calcDistanceBetweenFarthestCornerAndStroke();
        }

        /**
	 * Calculates the bounding box of the stroke
	 */
        protected void calcBounds()
        {
            if (m_points.Count == 0)
                return;
            double maxX = m_points[0].X;
            double minX = m_points[0].X;
            double maxY = m_points[0].Y;
            double minY = m_points[0].Y;
            m_leftMostPoint = m_points[0];
            m_rightMostPoint = m_points[0];
            m_bottomMostPoint = m_points[0];
            m_topMostPoint = m_points[0];
            for (int i = 1; i < m_points.Count; i++)
            {
                if (m_points[i].X > maxX)
                {
                    maxX = m_points[i].X;
                    m_rightMostPoint = m_points[i];
                }
                if (m_points[i].X < minX)
                {
                    minX = m_points[i].X;
                    m_leftMostPoint = m_points[i];
                }
                if (m_points[i].Y > maxY)
                {
                    maxY = m_points[i].Y;
                    m_topMostPoint = m_points[i];
                }
                if (m_points[i].Y < minY)
                {
                    minY = m_points[i].Y;
                    m_bottomMostPoint = m_points[i];
                }
            }
            m_bounds = new BoundingBox(minX, minY, maxX, maxY);
            //strokes_toPaint.Add(new StylusPointCollection() {
            //    new StylusPoint(m_bounds.getBottomLeftPoint().X, m_bounds.getBottomLeftPoint().Y),
            //    new StylusPoint(m_bounds.getBottomRightPoint().X, m_bounds.getBottomRightPoint().Y),
            //    new StylusPoint(m_bounds.getTopRightPoint().X, m_bounds.getTopRightPoint().Y),
            //    new StylusPoint(m_bounds.getTopLeftPoint().X, m_bounds.getTopLeftPoint().Y),
            //});
            m_perimStrokeLengthRatio = m_strokelength / m_bounds.getPerimeter();
        }

        /**
         * Calculate total rotation and other rotation-related features (
         */
        protected void calcTotalRotation()
        {
            double sum = 0;
            double deltaX, deltaY, deltaX1, deltaY1;
            for (int i = 1; i < m_points.Count - 1; i++)
            {
                deltaX = m_points[i + 1].X - m_points[i].X;
                deltaY = m_points[i + 1].Y - m_points[i].Y;
                deltaX1 = m_points[i].X - m_points[i - 1].X;
                deltaY1 = m_points[i].Y - m_points[i - 1].Y;

                // check for divide by zero; add or subtract PI/2 accordingly (this
                // is the limit of atan as it approaches infinity)
                if (deltaX * deltaX1 + deltaY * deltaY1 == 0)
                {
                    if (deltaX * deltaY1 - deltaX1 * deltaY < 0)
                    {
                        sum += Math.PI / -2.0;
                    }
                    else if (deltaX * deltaY1 - deltaX1 * deltaY > 0)
                    {
                        sum += Math.PI / 2.0;
                    }
                }

                // otherwise sum the rotation
                else
                    sum += Math.Atan2((deltaX * deltaY1 - deltaX1 * deltaY),
                            (deltaX * deltaX1 + deltaY * deltaY1));
            }
            m_totalRotation = sum;

            // num revolutions = total rotation divided by 2PI
            m_numRevolutions = Math.Abs(m_totalRotation) / (Math.PI * 2.0);

            // this just accounts for numerical instabilities
            if (m_numRevolutions < .0000001)
                m_numRevolutions = 0.0;

            // overtraced check
            if (m_numRevolutions >= M_REVS_TO_BE_OVERTRACED)
                m_overtraced = true;
            else
                m_overtraced = false;

            // compute distance between end points divided by total stroke length
            m_endPtStrokeLengthRatio = distance(getFirstOrigPoint(),
                    getLastOrigPoint())
                    / m_strokelength;

            // closed shape test
            if (m_endPtStrokeLengthRatio <= M_PERCENT_DISTANCE_TO_BE_COMPLETE
                    && m_numRevolutions >= M_NUM_REVS_TO_BE_COMPLETE)
                m_complete = true;
            else
                m_complete = false;
        }

        public static double distance(StylusPoint p1, StylusPoint p2)
        {
            double Xdist = p1.X - p2.X;
            double Ydist = p1.Y - p2.Y;

            return Math.Sqrt(Xdist * Xdist + Ydist * Ydist);
        }

        public StylusPoint getLastOrigPoint()
        {
            return stylusOriginalPoints[stylusOriginalPoints.Count - 1];
        }

        public StylusPoint getFirstOrigPoint()
        {
            return stylusOriginalPoints[0];
        }

        /**
	     * Flag stating whether or not the stroke is overtraced (makes multiple
	     * revolutions)
	     * 
	     * @return true if overtraced; else false
	     */
        public bool isOvertraced()
        {
            return m_overtraced;
        }

        /**
	     * Get the number of 2PI (360 degree) revolutions that the stroke makes
	     * 
	     * @return number of revolutions that the stroke makes
	     */
        public double numRevolutions()
        {
            return m_numRevolutions;
        }

        /**
	     * Get the endpoint to stroke length ratio of the stroke
	     * 
	     * @return endpoint to stroke length ratio
	     */
        public double getEndptStrokeLengthRatio()
        {
            return m_endPtStrokeLengthRatio;
        }

        /**
	     * Get the angle of the major axis relative to (0,0)
	     * 
	     * @return angle of the major axis
	     */
        public double getMajorAxisAngle()
        {
            return m_majorAxisAngle;
        }

        /**
	     * Get the length of the major axis for this ellipse estimation
	     * 
	     * @return length of the major axis
	     */
        public double getMajorAxisLength()
        {
            return m_majorAxisLength;
        }

        /**
	     * Get the major axis of the stroke
	     * 
	     * @return major axis of the stroke
	     */
        public Line2D getMajorAxis()
        {
            return m_majorAxis;
        }

        /**
	     * Get the normalized distance between direction extremes
	     * 
	     * @return NDDE value
	     */
        public double getNDDE()
        {
            return m_NDDE;
        }

        /**
	     * Get the points of the stroke
	     * 
	     * @return points of the stroke
	     */
        public StylusPointCollection getPoints()
        {
            return m_points;
        }

        /**
	     * Get the total length of the stroke
	     * 
	     * @return total length of the stroke
	     */
        public double getStrokeLength()
        {
            return m_strokelength;
        }

        /**
	     * Get the number of revolutions (based on direction graph) that the stroke
	     * makes
	     * 
	     * @return number of revolutions
	     */
        public double getNumRevolutions()
        {
            return m_numRevolutions;
        }

        /**
	     * Get the direction change ratio
	     * 
	     * @return DCR value
	     */
        public double getDCR()
        {
            return m_DCR;
        }

        /**
         * Calculate the normalized distance between direction extremes (NDDE)
         */
        protected void calcNDDE()
        {
            int maxIndex = 0, minIndex = 0;
            if (m_dir.Count == 0)
                return;
            m_maxDir = m_dir[0];
            m_minDir = m_dir[0];

            // find minimum and maximum direction values
            for (int i = 1; i < m_dir.Count; i++)
            {
                if (m_dir[i] > m_maxDir)
                {
                    m_maxDir = m_dir[i];
                    maxIndex = i;
                }
                if (m_dir[i] < m_minDir)
                {
                    m_minDir = m_dir[i];
                    minIndex = i;
                }
            }

            // NDDE = difference in stroke lengths at the indeces of max and min
            // direction value divided by total stroke length
            if (m_lengthSoFar[maxIndex] > m_lengthSoFar[minIndex])
                m_NDDE = (m_lengthSoFar[maxIndex] - m_lengthSoFar[minIndex])
                        / m_strokelength;
            else
                m_NDDE = (m_lengthSoFar[minIndex] - m_lengthSoFar[maxIndex])
                        / m_strokelength;
        }

        /**
         * Compute the direction change ratio (max direction change divided by
         * average direction change)
         */
        protected void calcDCR()
        {
            double maxDC = Double.MinValue;
            double sum = 0.0;

            // ignore 5% at ends (to avoid tails)
            int start = (int)(m_dir.Count * .05);
            int end = m_dir.Count - start;
            int i;

            // compute average change in direction and find max change in direction
            for (i = start; i < end - 1; i++)
            {
                double dc = Math.Abs(m_dir[i] - m_dir[i + 1]);
                if (dc >= maxDC)
                    maxDC = dc;
                sum += dc;
            }
            double avgDC = sum / i;

            // DCR = max change in direction / avg change in direction
            if (avgDC == 0.0 || Double.IsNaN(avgDC))
                m_DCR = 0.0;
            else
                m_DCR = maxDC / avgDC;
        }

        /**
	     * Get the bounding box of the stroke
	     * 
	     * @return bounding box of the stroke
	     */
        public BoundingBox getBounds()
        {
            return m_bounds;
        }

        /**
	     * Compute the best fit line of the stroke itself
	     */
        protected void calcBestFitLine()
        {
            double sx = 0, sx2 = 0, sy = 0, sy2 = 0, sxy = 0;

            // calculate sum of the x values, y values, x^2 values, y^2 values and
            // x*y values (those needed to compute least squares line)
            for (int i = 0; i < m_points.Count; i++)
            {
                sx += m_points[i].X;
                sx2 += Math.Pow(m_points[i].X, 2);
                sy += m_points[i].Y;
                sy2 += Math.Pow(m_points[i].Y, 2);
                sxy += m_points[i].X * m_points[i].Y;
            }
            Line2D l1 = new Line2D.Double();
            Line2D l2 = new Line2D.Double();
            double err1 = Double.MaxValue;
            double err2 = Double.MaxValue;
            try
            {
                // compute least squares line and error in the x direction
                l1 = LeastSquares.bestFitLine(sx, sx2, sy, sxy, m_points.Count,
                        m_bounds);
                err1 = LeastSquares.error(m_points, l1);
            }
            catch (Exception e)
            {
            }
            try
            {
                // compute least squares line and error in the y direction
                l2 = LeastSquares.bestFitLine(sy, sy2, sx, sxy, m_points.Count,
                        m_bounds);
                err2 = LeastSquares.error(m_points, l2);
            }
            catch (Exception e)
            {
            }

            // choose the line (either in x or y direction) that had the least error
            if (err1 < err2)
                m_bestFitLine = l1;
            else
                m_bestFitLine = l2;
        }

        /**
         * Compute the line that best fits the direction graph
         */
        protected void calcBestFitLineForDirGraph()
        {
            double sx = 0, sx2 = 0, sy = 0, sxy = 0;
            double[] x = new double[m_dir.Count];

            // calculate sum of the x values (in this case the indexes), y values
            // (in this case the direction values), x^2 values, and x*y values
            // (those needed to compute least squares line)
            for (int i = 0; i < m_dir.Count; i++)
            {
                sx += i;
                sx2 += (i * i);
                sy += m_dir[i];
                sxy += (i * m_dir[i]);
                x[i] = i;
            }

            // calculate bounds of the direction graph
            //Rectangle2D bounds = new Rectangle2D.Double(0, m_minDir, m_dir.length,
            //        m_maxDir);
            //try
            //{
            //    // calculate best fit line of the direction graph and the slope of
            //    // that line
            //    m_bestFitDirGraph = LeastSquares.bestFitLine(sx, sx2, sy, sxy,
            //            m_dir.length, bounds);
            //    m_slopeDirGraph = (m_bestFitDirGraph.getY2() - m_bestFitDirGraph
            //            .getY1())
            //            / (m_bestFitDirGraph.getX2() - m_bestFitDirGraph.getX1());
            //    m_bestFitDirGraphError = LeastSquares.error(x, getDir(),
            //            m_bestFitDirGraph) / m_dir.length;
            //}
            //catch (Exception e)
            //{
            //}
        }

        /**
	     * Test used to determine if the direction graph is continuously increasing
	     * or decreasing. The idea is to split the graph into windows and see if
	     * each window get consecutively smaller or larger over time.
	     */
        protected void calcDirWindowPassed()
        {

            // each window contains 5 direction values
            double windowSize = M_DIR_WINDOW_SIZE;
            bool result = true;
            m_pctDirWindowPassed = 1.0;
            List<Double> dirWindows = new List<Double>();

            // ignore 6% at ends (to avoid tails)
            int start = (int)(m_dir.Count * .06);
            int end = m_dir.Count - start;
            int j = 0;
            double mean = 0;

            // cluster direction graph into windows
            for (int i = start; i < end; i++)
            {
                if (j >= windowSize)
                {
                    dirWindows.Add(mean / windowSize);
                    j = 0;
                    mean = 0;
                }
                else
                {
                    mean += m_dir[i];
                    j++;
                }
            }

            // see if average values in windows are consecutively increasing or
            // decreasing
            if (dirWindows.Count > 3)
            {
                bool increasing = true;
                if (dirWindows[1] < dirWindows[0])
                    increasing = false;
                double numPassed = 0.0;
                double numChecked = 0.0;
                for (int i = 0; i < dirWindows.Count - 1; i++)
                {
                    if (increasing && dirWindows[i + 1] < dirWindows[i]
                            || !increasing
                            && dirWindows[i + 1] > dirWindows[i])
                    {
                        result = false;
                    }
                    else
                    {
                        numPassed++;
                    }
                    numChecked++;
                }
                m_pctDirWindowPassed = numPassed / numChecked;
            }
            m_dirWindowPassed = result;
        }


        /**
	     * Estimate the major axis of the stroke(this is done by finding the two
	     * points that are farthest apart and joining them with a line)
	     */
        protected void calcMajorAxis()
        {
            double maxDistance = Double.MinValue;
            int max1 = 0, max2 = 0;
            for (int i = 0; i < getNumPoints(); i++)
            {
                for (int j = 0; j < getNumPoints(); j++)
                {
                    if (i != j)
                    {
                        double d = distance(getPoints()[i],getPoints()[j]);
                        if (d > maxDistance)
                        {
                            maxDistance = d;
                            max1 = i;
                            max2 = j;
                        }
                    }
                }
            }
            m_majorAxis = new Line2D.Double(getPoints()[max1].X,
                    getPoints()[max1].Y, getPoints()[max2].X,
                    getPoints()[max2].Y);
            m_majorAxisLength = m_majorAxis.GetP1().Distance(m_majorAxis.GetP2());
            m_majorAxisAngle = Math.Atan2(
                    m_majorAxis.GetY2() - m_majorAxis.GetY1(), m_majorAxis.GetX2()
                            - m_majorAxis.GetX1());
        }

        /**
	 * Compute the distance between the farthest corner of the bounding box and
	 * the stroke
	 * 
	 * @return distance between farthest corner and the stroke
	 */
        protected void calcDistanceBetweenFarthestCornerAndStroke()
        {
            m_cornerStrokeDistance = 0.0;
            m_minCornerStrokeDistance = Double.MaxValue;
            m_stdDevCornerStrokeDistance = 0.0;
            double dis = 0.0;
            double[] dist = new double[4];
            Point2D[] corners = new Point2D[4];
            // TODO: Marty: fix
            corners[0] = new Point2D(GetBounds().getBottomLeftPoint());
            corners[1] = new Point2D(GetBounds().getBottomRightPoint());
            corners[2] = new Point2D(GetBounds().getTopLeftPoint());
            corners[3] = new Point2D(GetBounds().getTopRightPoint());
            for (int i = 0; i < 4; i++)
                dist[i] = Double.MaxValue;
            foreach (var p in getOrigPoints())
            {
                for (int i = 0; i < 4; i++)
                {
                    dis = corners[i].Distance(ToPoint2D(p));
                    if (dis > m_cornerStrokeDistance)
                        m_cornerStrokeDistance = dis;
                    if (dis < m_minCornerStrokeDistance)
                        m_minCornerStrokeDistance = dis;
                    if (dis < dist[i])
                        dist[i] = dis;
                }
            }
            m_cornerStrokeDistance /= ((GetBounds().height + GetBounds().width) / 2.0);
            m_minCornerStrokeDistance /= ((GetBounds().height + GetBounds().width) / 2.0);
            double sum = 0.0;
            for (int i = 0; i < 4; i++)
                sum += dist[i];
            sum /= 4.0;
            m_avgCornerStrokeDistance = sum
                    / ((GetBounds().height + GetBounds().width) / 2.0);
            for (int i = 0; i < 4; i++)
            {
                m_stdDevCornerStrokeDistance += (m_avgCornerStrokeDistance - dist[i])
                        * (m_avgCornerStrokeDistance - dist[i]);
            }
            m_stdDevCornerStrokeDistance = Math
                    .Sqrt(m_stdDevCornerStrokeDistance / 4.0);
        }

        internal StylusPointCollection getOrigStroke()
        {
            return stylusOriginalPoints;
        }

        private Point2D ToPoint2D(StylusPoint p)
        {
            return new Point2D(p.X, p.Y);
        }


        /**
	     * Get the bounding box of the stroke
	     * 
	     * @return bounding box of the stroke
	    */
        public BoundingBox GetBounds()
        {
            return m_bounds;
        }

        /**
	     * Method used to find the point (or points) where the given line intersects
	     * the stroke
	     * 
	     * @param line
	     *            line to find the intersection with
	     * @return intersection points
	     */
        public List<Point2D> getIntersection(Line2D.Double line)
        {
            List<Point2D> intersectionPts = new List<Point2D>();
            Point2D intersect = null;
            for (int i = 0; i < getNumPoints() - 1; i++)
            {
                Point2D p1 = ToPoint2D(m_points[i]);
                Point2D p2 = ToPoint2D(m_points[i + 1]);
                if (line.intersectsLine(p1.X, p1.Y, p2.X, p2.Y))
                {
                    intersect = getIntersectionPt(line, new Line2D.Double(
                            p1.X, p1.Y, p2.X, p2.Y));
                    intersectionPts.Add(intersect);
                }
            }
            if (intersectionPts.Count < 2)
            {
                Point2D p1 = ToPoint2D(m_points[0]);
                Point2D p2 = ToPoint2D(m_points[getNumPoints() - 1]);
                if (line.intersectsLine(p1.X, p1.Y, p2.X, p2.Y))
                {
                    intersect = getIntersectionPt(line, new Line2D.Double(
                            p1.X, p1.Y, p2.X, p2.Y));
                    intersectionPts.Add(intersect);
                }
            }
            return intersectionPts;
        }

        /**
	 * Method used to find the intersection point between two lines
	 * 
	 * @param l1
	 *            line 1
	 * @param l2
	 *            line 2
	 * @return intersection point between line1 and line2
	 */
        public static Point2D getIntersectionPt(Line2D.Double l1, Line2D.Double l2)
        {
            Point2D intersect = null;
            double l1slope = (l1.GetY2() - l1.GetY1()) / (l1.GetX2() - l1.GetX1());
            double l2slope = (l2.GetY2() - l2.GetY1()) / (l2.GetX2() - l2.GetX1());
            if (l1slope == l2slope)
                return null;
            double l1intercept = (-l1.GetX1() * l1slope) + l1.GetY1();
            double l2intercept = (-l2.GetX1() * l2slope) + l2.GetY1();
            if ((l1.GetX2() - l1.GetX1()) == 0)
            {
                double x = l1.GetX2();
                double y = x * l2slope + l2intercept;
                return new Point2D(x, y);
            }
            if ((l2.GetX2() - l2.GetX1()) == 0)
            {
                double x = l2.GetX2();
                double y = x * l1slope + l1intercept;
                return new Point2D(x, y);
            }
            Matrix<double> a = Matrix<double>.Build.Random(2, 2);
            Matrix<double> b = Matrix<double>.Build.Random(2, 1);
            a[0, 0] = -l1slope;
            a[0, 1] =  1;
            a[1, 0] = -l2slope;
            a[1, 1] = 1;
            b[0, 0] = l1intercept;
            b[1, 0] = l2intercept;
            Matrix<double> result = a.Solve(b);
            intersect = new Point2D(result[0, 0], result[1, 0]);
            return intersect;
        }

        /**
	     * Flag denoting whether or not moving direction window test passed
	     * 
	     * @return true if test passed; else false
	     */
        public bool dirWindowPassed()
        {
            return m_dirWindowPassed;
        }

        /**
         * Get the percentage of the direction window passing
         * 
         * @return direction window test pass percent
         */
        public double getPctDirWindowPassed()
        {
            return m_pctDirWindowPassed;
        }

        /**
	     * Avg distance between closest point to each corner of the bounding box
	     * 
	     * @return average distance
	    */
        public double getAvgCornerStrokeDistance()
        {
            return m_avgCornerStrokeDistance;
        }

        /**
	     * Stroke length to perimeter (of bounding box) ratio
	     * 
	     * @return ratio
	     */
        public double getStrokeLengthPerimRatio()
        {
            return m_perimStrokeLengthRatio;
        }

        /**
	 * Method used to find the point (or points) where the given line intersects
	 * the stroke
	 * 
	 * @param stroke
	 *            stroke to find intersection for
	 * @param line
	 *            line to find the intersection with
	 * @return intersection points
	 */
        public static List<Point2D> getIntersection(StylusPointCollection stroke,
                Line2D.Double line)
        {
            List<Point2D> intersectionPts = new List<Point2D>();
            Point2D intersect = null;
            for (int i = 0; i < stroke.Count - 1; i++)
            {
                StylusPoint p1 = stroke[i];
                StylusPoint p2 = stroke[i + 1];
                if (line.intersectsLine(p1.X, p1.Y, p2.X, p2.Y))
                {
                    intersect = getIntersectionPt(line, new Line2D.Double(
                            p1.X, p1.Y, p2.X, p2.Y));
                    intersectionPts.Add(intersect);
                }
            }
            if (intersectionPts.Count < 2)
            {
                StylusPoint p1 = stroke[0];
                StylusPoint p2 = stroke[stroke.Count - 1];
                if (line.intersectsLine(p1.X, p1.Y, p2.X, p2.Y))
                {
                    intersect = getIntersectionPt(line, new Line2D.Double(
                            p1.X, p1.Y, p2.X, p2.Y));
                    intersectionPts.Add(intersect);
                }
            }
            return intersectionPts;
        }

        /**
         * Static method used to compute the stroke length of a given stroke
         * 
         * @param stroke
         *            stroke to compute length for
         * @return length of stroke
         */
        public static double getStrokeLength(StylusPointCollection stroke)
        {
            double strokelength = 0;
            for (int i = 1; i < stroke.Count; i++)
                strokelength += distance(stroke[i - 1], stroke[i]);
            return strokelength;
        }

        /**
	     * Get a list of the original stroke points (before stroke was cleaned)
	     * 
	     * @return list of original stroke points
	     */
        public StylusPointCollection getOrigPoints()
        {
            return stylusOriginalPoints;
        }

        /**
	     * Flag stating whether or not a stroke is closed/complete
	     * 
	     * @return true if stroke is near being closed; else false
	     */
        public bool isClosed()
        {
            return m_complete;
        }

        public int getNumPoints()
        {
            return m_points.Count;
        }
        #endregion
    }

    public struct ResultData
    {
        public List<StylusPointCollection> strokes;
        public bool recognitionResult;
        public string info;

        public ResultData(List<StylusPointCollection> strokes_toPaint, bool v,string info) : this()
        {
            this.strokes = strokes_toPaint;
            this.recognitionResult = v;
            this.info = info;
        }
    }
}
