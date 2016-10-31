using System;
using System.IO;
using System.Net;
using System.Collections.ObjectModel;
using System.Windows;                       //STANDARD for WPF App
using System.Windows.Controls;              //STANDARD for WPF App
using System.Windows.Data;                  //STANDARD for WPF App
using System.Windows.Media.Animation;       //STANDARD for WPF App
using System.Windows.Navigation;            //STANDARD for WPF App
using System.Windows.Controls.Primitives;   //STANDARD for WPF App
using System.Windows.Media;                 //For : DrawingGroup
using System.Windows.Shapes;                //For : Geometric shapes like Line
using System.Windows.Input;                 //For : ExecutedRoutedEventArgs
using Microsoft.Win32;                      //For : OpenFileDialog / SaveFileDialog
using System.Windows.Ink;                   //For : InkCanvas
using System.Windows.Markup;                //For : XamlWriter
using System.Windows.Media.Imaging;         //For : BitmapImage etc etc
using System.Windows.Input.StylusPlugIns;   //For : DrawingAttributes


namespace BitsOfStuff
{
    public class ShortStraw
    {
        // public variable
        public int m_numC;
        public PointCollection m_corners;

        // private variable
        int m_numP, m_numS;
        double m_cellDist;
        Stroke m_stroke;
        int[] m_time;
        PointCollection m_resamplePoints;
        double[] m_resampleTime;
        int[] m_cornerIndex;
        double m_meanTime;

        // find all the corners of a stroke
        public void FindCorner(Stroke in_stroke, String in_ID)
        {
            // initialization
            m_stroke = in_stroke;
            m_numP = m_stroke.StylusPoints.Count;
            Guid tGuid = new Guid(in_ID);
            m_time = (int[])m_stroke.GetPropertyData(tGuid);
            m_resamplePoints = new PointCollection();
            m_corners = new PointCollection();
            m_cornerIndex = new int[500];
            int[] old_Index = new int[500];

            // pass 0: find all the corners of first resampled point set
            ResampleStrokes(0);
            GetCorners();
            for (int i = 0; i < m_numC; i++)
            {
                old_Index[i] = m_cornerIndex[i];
            }
            int old_numC = m_numC;
            m_resamplePoints.Clear();

            // pass 1: find all the corners of the shifted point set
            ResampleStrokes(1);
            GetCorners();

            // combine the corners together
            int cIdx = 1;
            m_corners.Add(m_resamplePoints[0]);
            for (int i = 1; i < m_numC; i++)
            {
                int idx0 = old_Index[cIdx];
                int idx1 = m_cornerIndex[i];
                // make sure the two corners are not too close
                while (idx0 < idx1 - 1)
                {
                    m_corners.Add(m_resamplePoints[idx0]);
                    cIdx++;
                    idx0 = old_Index[cIdx];
                }
                if (idx0 <= idx1 + 1)
                {
                    cIdx++;
                }
                m_corners.Add(m_resamplePoints[idx1]);
            }

            // set number of corners
            m_numC = m_corners.Count;
        }

        // resample the stroke into point set with constant inter-distance
        private void ResampleStrokes(int pass)
        {
            // initial the variable
            m_resampleTime = new double[500];
            m_numS = 0;
            m_meanTime = 0;

            // init number of raw points between two resampled points
            int numBetween = 0; 

            // compute the diagonal of the stroke's bounding box
            Rect bound = m_stroke.GetBounds();
            double diag = GetDistance(bound.TopLeft, bound.BottomRight);
            // set the distance between resampled poits to be 1/40 of the diagonal
            m_cellDist = diag / 40;

            // resample the points
            double m_length = 0;
            int preIndex = 0;

            // pass 0: the first resampled points is the start point of the stroke
            if (pass == 0)
            {
                m_resamplePoints.Add(m_stroke.StylusPoints[0].ToPoint());
                m_resampleTime[m_numS] = 0;
                m_numS++;
            }

            // compute other resampled points
            for (int i = 1; i < m_numP; i++)
            {
                numBetween++;
                // compute the distance between ajacent two raw points in the stroke
                double dist = GetDistance( m_stroke.StylusPoints[i-1].ToPoint(), m_stroke.StylusPoints[i].ToPoint());
                // check the distance between the last resampled point and the visiting raw points
                if (m_length + dist >= m_cellDist
                    || (pass == 1 && m_numS == 0 && m_length + dist >= m_cellDist/2.0))
                {
                    // add a new point to the resampled points
                    Point newPoint = new Point();
                    StylusPoint preP = m_stroke.StylusPoints[i - 1];
                    StylusPoint curP = m_stroke.StylusPoints[i];
                    // compute the position of the new resampled point
                    if (pass == 1 && m_numS == 0)
                    {
                        // first shifted resampled point
                        newPoint.X = preP.X + ((m_cellDist/2.0 - m_length) / dist) * (curP.X - preP.X);
                        newPoint.Y = preP.Y + ((m_cellDist/2.0 - m_length) / dist) * (curP.Y - preP.Y);
                    }
                    else
                    {
                        // other resampled points
                        newPoint.X = preP.X + ((m_cellDist - m_length) / dist) * (curP.X - preP.X);
                        newPoint.Y = preP.Y + ((m_cellDist - m_length) / dist) * (curP.Y - preP.Y);
                    }
                    m_resamplePoints.Add( newPoint);
                    // compute the distance from the new resampled point to the raw point i
                    m_length = GetDistance(newPoint, curP.ToPoint());

                    // compute time for each resampled point
                    m_resampleTime[m_numS] = m_time[i] - m_time[preIndex];
                    if (m_resampleTime[m_numS] < 0)
                        m_resampleTime[m_numS] = 0;
                    // compute the mean time of the resampled points
                    m_meanTime += m_resampleTime[m_numS];

                    // add extra resampled points between newPoint and raw point i
                    preIndex = i;
                    numBetween = 0;
                    m_numS++;
                    // make sure m_length is less than m_cellDist
                    while (m_length > m_cellDist)
                    {
                        // find all possible resampled point befor i
                        newPoint.X += (m_cellDist / dist) * (curP.X - preP.X);
                        newPoint.Y += (m_cellDist / dist) * (curP.Y - preP.Y);
                        m_resamplePoints.Add(newPoint);
                        m_length -= m_cellDist;
                        // compute time for each resampled point
                        m_resampleTime[m_numS] = m_time[i] - m_time[i - 1];
                        if (m_resampleTime[m_numS] < 0)
                            m_resampleTime[m_numS] = 0;
                        // compute the mean time of the resampled points
                        m_meanTime += m_resampleTime[m_numS];
                        m_numS++;
                    }
                }
                else
                {
                    // get the last resampled point
                    m_length += dist;
                    if ((m_length > m_cellDist) && (i == m_numP - 1))
                    {
                        m_resamplePoints.Add(m_stroke.StylusPoints[i].ToPoint());
                        m_resampleTime[m_numS] = m_time[i] - m_time[preIndex];
                        m_numS++;
                    }
                }
            }

            // compute mean time of resampled point
            m_meanTime /= m_numS;
        }

        // get all the corners of a stroke
        private void GetCorners()
        {
            int m_w = 3;                                // set the window size to 3
            double totalStraw = 0;                      // the sum of all straws
            double[] straws = new double[m_numS];       // array to store all straws

            // add the first raw point index as the first corner index
            m_cornerIndex[0] = 0;
            m_numC = 1;

            // compute straws
            for (int i = m_w; i < m_numS - m_w; i++)
            {
                // compute the distance of the window
                straws[i] = GetDistance(m_resamplePoints[i - m_w], m_resamplePoints[i + m_w]);
                totalStraw += straws[i];
            }

            // compute the threshold for straws
            double threshold = totalStraw / (m_numS - 2 * m_w) * 0.95;

            // compute the straw value for the first 3 and last 3 points
            straws[1] = GetDistance(m_resamplePoints[0], m_resamplePoints[1 + m_w]) * 2 * m_w / (m_w + 1) ;
            straws[2] = GetDistance(m_resamplePoints[0], m_resamplePoints[2 + m_w]) * 2 * m_w / (m_w + 2);
            straws[m_numS - 2] = GetDistance(m_resamplePoints[m_numS - 1], m_resamplePoints[m_numS - 2 - m_w]) * 2 * m_w / (m_w + 1);
            straws[m_numS - 3] = GetDistance(m_resamplePoints[m_numS - 1], m_resamplePoints[m_numS - 3 - m_w]) * 2 * m_w / (m_w + 2);

            // compute the initial corner set
            for (int i = m_w; i < m_numS - m_w; i++)
            {
                // compare each straw with the threshold
                if (straws[i] < threshold)
                {
                    // get the point with local minimum straw
                    double localMin = 10000;
                    int localMinIndex = i;
                    while ((i < m_numS - m_w) && (straws[i] < threshold))
                    {
                        if (straws[i] < localMin)
                        {
                            localMin = straws[i];
                            localMinIndex = i;
                        }
                        i++;
                    }
                    // add the new corner index
                    m_cornerIndex[m_numC] = localMinIndex;
                    m_numC++;
                }
            }
            // add the last raw point index as the last corner index
            m_cornerIndex[m_numC] = m_numS-1;
            m_numC++;

            // stroke polyline part post process
            PostProcess(straws);

            // stroke curv part process
            CurvProcessPass0();
            CurvProcessPass1();
        }

        // polyline post process the inital corner set
        private void PostProcess(double[] in_straws)
        {
            // add missing corners to the list
            AddCorners_Collinear(in_straws);

            // add the point mean time is small as the possible corner.
            AddCorners_Time();

            // adjast the corner to the resampled point closer to the real corner
            AdjustCorners();

            // triplet collinear test pass 0: with a high threshold
            TriCollinearPass0();

            // triplet collinear test pass 1: with a lower threshold
            TriCollinearPass1();

            // Sharp Noise Avoidance
            SharpNoiseAvoid(in_straws);
        }

        // add missing corners by two adjacent collinear test
        private void AddCorners_Collinear(double[] in_straws)
        {
            // set a tag to check whether any two adjacent corners are in a line
            bool tag = false;
            while (!tag)
            {
                tag = true;
                for (int i = 1; i < m_numC; i++)
                {
                    // get the corner index
                    int c1 = m_cornerIndex[i - 1];
                    int c2 = m_cornerIndex[i];
                    // add extra corner if the two adjacent corners are not on a line
                    if (!IsLine(c1, c2, 0.975) && (c2 > c1 + 1))
                    {
                        // get the index of the new corner
                        int newCorner = HalfwayCorner(in_straws, c1, c2);
                        // insert the new corner index to the corner index list
                        for (int j = m_numC; j > i; j--)
                        {
                            // shift the corner index after cell i backward
                            m_cornerIndex[j] = m_cornerIndex[j - 1];
                        }
                        // insert new corner index
                        m_cornerIndex[i] = newCorner;
                        m_numC++;
                        // change tag to false
                        tag = false;
                    }
                }
            }
        }

        // add missing corners by checking drawing time
        private void AddCorners_Time()
        {
            // check each pair of adjacent corners
            for (int i = 1; i < m_numC; i++)
            {
                // get the corner index
                int c1 = m_cornerIndex[i - 1];
                int c2 = m_cornerIndex[i];
                if (c2 - c1 < 6) continue;
                int localMaxIdx = c1 + 3;

                // find local maximum time
                double localMax = m_resampleTime[localMaxIdx];
                for (int j = c1 + 3; j <= c2 - 3; j++)
                {
                    if (localMax < m_resampleTime[j])
                    {
                        localMax = m_resampleTime[j];
                        localMaxIdx = j;
                    }
                }

                // check whether the drawing speed is slow enough
                if (localMax > 2 * m_meanTime)
                {
                    // add this point to the corner list
                    for (int j = m_numC + 1; j > i; j--)
                    {
                        m_cornerIndex[j] = m_cornerIndex[j - 1];
                    }
                    m_cornerIndex[i] = localMaxIdx;
                    m_numC++;
                }
            }
        }

        // adjust corners to precise resampled point
        private void AdjustCorners()
        {
            // check each corner
            for (int i = 1; i < m_numC - 1; i++)
            {
                int index = m_cornerIndex[i];
                if (index < 3 || index > m_numS - 4)
                    continue;
                // compute adjacent resampled points
                Point point0 = m_resamplePoints[index - 3];
                Point point1 = m_resamplePoints[index - 2];
                Point point2 = m_resamplePoints[index - 1];
                Point point3 = m_resamplePoints[index];
                Point point4 = m_resamplePoints[index + 1];
                Point point5 = m_resamplePoints[index + 2];
                Point point6 = m_resamplePoints[index + 3];
                // compute angles of these adjacent points
                double angle0 = GetAngle(point1, point0, point2);
                double angle1 = GetAngle(point2, point1, point3);
                double angle2 = GetAngle(point3, point2, point4);
                double angle3 = GetAngle(point4, point3, point5);
                double angle4 = GetAngle(point5, point4, point6);
                // adjust the corner point based upon the angles
                if (angle1 < angle3)
                {
                    if (angle0 < angle1 && angle0 < angle2)
                        index -= 2;
                    else if (angle1 < angle2)
                        index--;
                }
                else
                {
                    if (angle4 < angle3 && angle4 < angle2)
                        index += 2;
                    else if (angle3 < angle2)
                        index++;
                }
                m_cornerIndex[i] = index;
            }
        }

        // triplet collinear test pass 0
        private void TriCollinearPass0()
        {
            // check each corner
            for (int i = 1; i < m_numC - 1; i++)
            {
                // get the corner index
                int c1 = m_cornerIndex[i - 1];
                int c2 = m_cornerIndex[i + 1];
                // check whether the three points is inline
                if (c1 == m_cornerIndex[i] || IsLine(c1, c2, 0.988))
                //if (IsLine(c1, c2, 0.95))
                {
                    // remove this corner index
                    for (int j = i; j < m_numC - 1; j++)
                    {
                        // shift the corner index after cell i forward
                        m_cornerIndex[j] = m_cornerIndex[j + 1];
                    }
                    // change corner number
                    m_numC--;
                    i--;
                }
            }
        }

        // triplet collinear test pass 1
        private void TriCollinearPass1()
        {
            // check each corner
            for (int i = 1; i < m_numC - 1; i++)
            {
                // get the corner index
                int c1 = m_cornerIndex[i - 1];
                int c2 = m_cornerIndex[i + 1];
                int cIdx = m_cornerIndex[i];
                // set a dynamic threshold
                double threshold = 0.9747;
                if (c2 - c1 > 10)
                    threshold += 0.0053;    // increase threshold with long the stroke segment
                if (m_resampleTime[cIdx] > 2 * m_meanTime || m_resampleTime[cIdx - 1] > 2 * m_meanTime
                    || m_resampleTime[cIdx + 1] > 2 * m_meanTime)
                    threshold += 0.0066;    // increase threshold if the adjacent points drawn slowly
                // check whether the three points is inline
                if (IsLine(c1, c2, threshold))
                {
                    // remove this corner index
                    for (int j = i; j < m_numC - 1; j++)
                    {
                        // shift the corner index after cell i forward
                        m_cornerIndex[j] = m_cornerIndex[j + 1];
                    }
                    // change corner number
                    m_numC--;
                    i--;
                }
            }
        }

        // sharp noice avoidance
        private void SharpNoiseAvoid(double[] in_straws)
        {
            // check each corenr
            for (int i = 0; i < m_numC - 1; i++)
            {
                // get the corner index
                int c1 = m_cornerIndex[i];
                int c2 = m_cornerIndex[i + 1];
                // check whether this two points are adjacent
                if (c2 - c1 <= 1 || (c2 - c1 <= 2 && (i == 0 || i == m_numC - 2)))
                {
                    // delete the point with larger straw value
                    int beginIndex = i;
                    // compare the straw value
                    if (in_straws[c1] < in_straws[c2])
                    {
                        beginIndex = i + 1;
                    }
                    // delete the point
                    for (int j = beginIndex; j < m_numC - 1; j++)
                    {
                        m_cornerIndex[j] = m_cornerIndex[j + 1];
                    }
                    m_numC--;
                    i--;
                }
            }
        }

        // stroke curve part process pass 0
        private void CurvProcessPass0()
        {
            int preCorner = m_cornerIndex[0];   // set the first corner as the previous corner 

            // check each corner candidate starting from the second corner
            for (int i = 1; i < m_numC - 1; i++)
            {
                // init variable for each iteration
                bool notCorner = false;
                int theCorner = m_cornerIndex[i];
                int nxtCorner = m_cornerIndex[i + 1];
                int preDiff = theCorner - preCorner;
                int nxtDiff = nxtCorner - theCorner;
                int startIndex = theCorner - 12;

                // set previous corner as the start if it is too close to current checking corner
                if (preDiff < 12) startIndex = preCorner;
                int endIndex = theCorner + 12;
                // set next corner as the end if it is too close to current checking corner
                if (nxtDiff < 12) endIndex = nxtCorner;
                // compute angle1 (alpha)
                double angle1 = GetAngle(m_resamplePoints[theCorner], m_resamplePoints[startIndex], m_resamplePoints[endIndex]);

                // compute angle2 (beta)
                startIndex = theCorner - (int)Math.Ceiling((decimal)(theCorner - startIndex) / 3);
                endIndex = theCorner + (int)Math.Ceiling((decimal)(endIndex - theCorner) / 3);
                double angle2 = GetAngle(m_resamplePoints[theCorner], m_resamplePoints[startIndex], m_resamplePoints[endIndex]);

                // compute angle3 
                if (preDiff < 6) startIndex = theCorner - 1;
                else startIndex = theCorner - 2;
                if (nxtDiff < 6) endIndex = theCorner + 1;
                else endIndex = theCorner + 2;
                double angle3 = GetAngle(m_resamplePoints[theCorner], m_resamplePoints[startIndex], m_resamplePoints[endIndex]);
                
                // check all the angles to decide whether the current corner is real
                if ((angle2 > 36 + 0.85 * angle1 && angle1 > 20 && angle3 > 80 + 0.55 * angle1)
                    || angle3 > 161
                    || ((preDiff < 3 || nxtDiff < 3) && angle2 > 130))
                {
                    notCorner = true;
                } 

                // update previous corner
                preCorner = m_cornerIndex[i];

                // delete the incorrect corner
                if (notCorner)
                {
                    for (int j = i; j < m_numC - 1; j++)
                    {
                        m_cornerIndex[j] = m_cornerIndex[j + 1];
                    }
                    m_numC--;
                    i--;
                }
            }
        }

        // stroke curve part process pass 1
        private void CurvProcessPass1()
        {
            // check each corner candidate
            for (int i = 1; i < m_numC - 1; i++)
            {
                // init variable for each iteration
                bool notCorner = false;
                bool hasCross = false;
                int theCorner = m_cornerIndex[i];
                int start0 = theCorner - 12;
                int end0 = theCorner + 12;
                if (start0 < m_cornerIndex[i - 1]) start0 = m_cornerIndex[i - 1];
                if (end0 > m_cornerIndex[i + 1]) end0 = m_cornerIndex[i + 1];
                int start1 = theCorner - (int)Math.Ceiling((decimal)(theCorner - start0) / 3);
                int end1 = theCorner + (int)Math.Ceiling((decimal)(end0 - theCorner) / 3);

                // compute angle3
                double angle3 = GetAngle(m_resamplePoints[theCorner], m_resamplePoints[theCorner - 1], m_resamplePoints[theCorner + 1]);

                // check whether the stroke segment has self-intersection
                if (!SameDirection(m_resamplePoints[theCorner], m_resamplePoints[start0], m_resamplePoints[end0],
                            m_resamplePoints[start1], m_resamplePoints[end1]))
                {
                    start0 = theCorner - 4;
                    end0 = theCorner + 4;
                    if (start0 < m_cornerIndex[i - 1]) start0 = m_cornerIndex[i - 1];
                    if (end0 > m_cornerIndex[i + 1]) end0 = m_cornerIndex[i + 1];
                    start1 = theCorner - 1;
                    end1 = theCorner + 1;
                    // check whether the intersection is close to the corner candidate
                    if (!SameDirection(m_resamplePoints[theCorner], m_resamplePoints[start0], m_resamplePoints[end0],
                                m_resamplePoints[start1], m_resamplePoints[end1]))
                        continue;
                    hasCross = true;
                }
                else if (!IsLine(m_cornerIndex[i - 1], theCorner, 0.975) && !IsLine(theCorner, m_cornerIndex[i + 1], 0.975))
                {
                    // check whether the stroke segment has S shape
                    if (!SameDirection(m_resamplePoints[theCorner], m_resamplePoints[start0], m_resamplePoints[start1],
                                m_resamplePoints[end1], m_resamplePoints[end0]) && angle3 > 135)
                        notCorner = true;
                }

                // compute angle1 and angle2
                double angle1 = GetAngle(m_resamplePoints[theCorner], m_resamplePoints[start0], m_resamplePoints[end0]);
                double angle2 = GetAngle(m_resamplePoints[theCorner], m_resamplePoints[start1], m_resamplePoints[end1]);
                // add new threshold for angle3
                double threshold = 96.3;
                if (IsLine(m_cornerIndex[i - 1], theCorner, 0.975) || IsLine(theCorner, m_cornerIndex[i + 1], 0.975))
                    threshold = 128;
                // add new threshold for angle3
                if ((angle2 > 26.1 + 0.93 * angle1 && ((angle3 > 31 + angle1 && angle3 > threshold) || angle3 > 161))
                    || (hasCross && angle2 - angle1 > 15 && angle3 > 20))
                {
                    notCorner = true;
                }

                // delete the incorrect corner
                if (notCorner)
                {
                    for (int j = i; j < m_numC - 1; j++)
                    {
                        m_cornerIndex[j] = m_cornerIndex[j + 1];
                    }
                    m_numC--;
                    i--;
                }
            }
        }
       
        // check whether the rotate direction is the same
        private bool SameDirection(Point O, Point A, Point B, Point C, Point D)
        {
            // get each vector
            Vector d0 = new Vector(A.X - O.X, A.Y - O.Y);
            Vector d1 = new Vector(O.X - B.X, O.Y - B.Y);
            Vector d2 = new Vector(C.X - O.X, C.Y - O.Y);
            Vector d3 = new Vector(O.X - D.X, O.Y - D.Y);
            // get rotate direction
            double cross0 = Vector.CrossProduct(d0, d1);
            double cross1 = Vector.CrossProduct(d2, d3);
            // check whether the two direction are the same or not
            double result = cross0 * cross1;
            if (result > 0) return true;
            return false;
        }

        // function to get the angle of p1 to center to p2
        private double GetAngle(Point in_center, Point in_p1, Point in_p2)
        {
            double theAngle;
            // get two vector of the angle
            Vector d1 = new Vector(in_p1.X - in_center.X, in_p1.Y - in_center.Y);
            d1.Normalize();
            Vector d2 = new Vector(in_p2.X - in_center.X, in_p2.Y - in_center.Y);
            d2.Normalize();
            // compute the angle
            theAngle = Math.Acos(d1.X * d2.X + d1.Y * d2.Y);
            return theAngle * 180 / Math.PI;
        }

        // function to find the point with smallest straw
        private int HalfwayCorner(double[] in_straws, int x, int y)
        {
            // compute the quarter value
            int quarter = (y - x) / 4;
            if (quarter == 0)
            {
                quarter = 1;
            }
            // get the least straw
            double minValue = 10000;
            int minIndex = x + 1;
            for (int i = x + quarter; i < y - quarter; i++)
            {
                // compare with the least straw
                if (in_straws[i] < minValue)
                {
                    minValue = in_straws[i];
                    minIndex = i;
                }
            }
            return minIndex;
        }

        // function to check whether two points in a line
        private bool IsLine(int x, int y, double threshold)
        {
            // compute the distance between two points
            double distance = GetDistance(m_resamplePoints[x], m_resamplePoints[y]);
            double pathDist = 0;
            // compute the path between two points
            for (int i = x; i < y; i++)
            {
                pathDist += GetDistance(m_resamplePoints[i], m_resamplePoints[i+1]);
            }
            // compare with the threshold
            if (distance / pathDist > threshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // function to compute distance between two points
        private double GetDistance(Point px, Point py)
        {
            double deltaX = py.X - px.X;
            double deltaY = py.Y - px.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

    }
}