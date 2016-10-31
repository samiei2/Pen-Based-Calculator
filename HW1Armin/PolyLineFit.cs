using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace HW1Armin
{
    public class PolyLineFit
    {

        /**
	 * Flag denoting whether or not all sub-strokes passed a line test
	 */
        protected bool m_allLinesPassed;

        /**
         * Least squares error of polyline fit
         */
        protected double m_lsqe;

        /**
         * Number of sub-strokes that passed a line test
         */
        protected int m_numPassed;

        /**
         * Segmentation used for polyline fit
         */
        protected Segmentation m_segmentation;

        /**
         * List of subshapes for the polyline fit
         */
        //protected List<Shape> m_subshapes;

        /**
	     * Substrokes of polyline
	     */
        protected List<Stroke> m_subStrokes;
        private double m_err;
        private int m_fail;
        private bool m_passed;
        private bool OVERTRACED_LINE_COMBINE = true;

        public PolyLineFit(Recognizer features)
        {
            //PolylineCombinationSegmenter seg = new PolylineCombinationSegmenter();
            //seg.setStroke(features.getOrigPoints());
            //m_segmentation = seg.getSegmentations()[0];

            m_allLinesPassed = true;
            //m_err = 0;
            //m_lsqe = 0;
            //m_subStrokes = m_segmentation.getSegmentedStrokes();
            //m_subshapes = new List<Shape>();

            //// check to see if polyline is actually an overtraced line
            //if (OVERTRACED_LINE_COMBINE)
            //{
            //    bool overtracedLine = combineOvertracedLine(m_subStrokes,features);

            //    //// we have a line!
            //    //if (overtracedLine)
            //    //{
            //    //    try
            //    //    {
            //    //        computeBeautified();
            //    //        m_beautified.setShapes(m_subshapes);
            //    //        m_beautified.setLabel(Fit.POLYLINE + " (1)");
            //    //    }
            //    //    catch (Exception e)
            //    //    {
            //    //        log.debug("Could not create shape object: "
            //    //                + e.getMessage());
            //    //    }
            //    //    log.debug("PolylineFit: passed = " + m_passed
            //    //            + " OVERTRACED LINE lsqe = " + m_lsqe
            //    //            / m_features.getStrokeLength() + " err = " + m_err);
            //    //    return;
            //    //}
            //}

            ///*
            // * System.out.println("sizes:"); for (Stroke s : m_subStrokes)
            // * System.out.println(s.getNumPoints()); System.out.println("slopes:");
            // * for (Stroke s : m_subStrokes) System.out.println(getSlope(s));
            // */

            //// System.out.println("num substrokes = " + m_subStrokes.size());
            //// check for small, extraneous lines in interpretation
            //if (SMALL_POLYLINE_COMBINE
            //        && m_subStrokes.Count < 10)
            //{
            //    m_subStrokes = combineSmallLines(m_subStrokes);
            //}

            //// System.out.println("num substrokes = " + m_subStrokes.size());

            //// check for consecutive lines with similar slopes, combine if similar
            //if (SIM_SLOPE_POLYLINE_COMBINE)
            //{
            //    m_subStrokes = combineSimilarSlopes(m_subStrokes);
            //}

            //// System.out.println("num substrokes = " + m_subStrokes.size());

            //// check for consecutive lines that, when combined, pass a line test
            //if (LINE_TEST_COMBINE && m_subStrokes.Count > 2)
            //{
            //    m_subStrokes = combineLineTest(m_subStrokes);
            //}

            //// System.out.println("num substrokes = " + m_subStrokes.size());

            //// test 1: we need at least 2 substrokes
            //if (m_subStrokes.Count < 2)
            //{
            //    m_passed = false;
            //    m_fail = 0;
            //}
            //else
            //{
            //    // test 2: run line test on all sub strokes and get a cumulative
            //    // error
            //    for (int i = 0; i < m_subStrokes.Count; i++)
            //    {

            //        // if we have 2 or fewer points we automatically have a line
            //        if (m_subStrokes[i].getNumPoints() > 1)
            //        {
            //            LineFit lf = LineFit.getFit(m_subStrokes.get(i));
            //            m_err += lf.getError();
            //            m_lsqe += lf.getLSQE();
            //            m_subshapes.add(lf.getShape());
            //            if (!lf.passed())
            //            {
            //                m_allLinesPassed = false;
            //                m_passed = false;
            //                m_fail = 1;
            //            }
            //            else
            //                m_numPassed++;
            //        }
            //        else
            //            m_numPassed++;
            //    }
            //    m_err /= features.getStrokeLength();
            //    m_lsqe /= features.getStrokeLength();

            //    // dcr should be high
            //    if (features.getDCR() < M_DCR_TO_BE_POLYLINE)
            //    {
            //        m_passed = false;
            //        m_fail = 2;
            //    }

            //    // test 3.2: if total lsqe is low then accept even if not all
            //    // substrokes passed polyline test
            //    if (m_lsqe < M_POLYLINE_LS_ERROR)
            //    {// || m_allLinesPassed)
            //        m_passed = true;
            //        m_fail = 3;
            //    }
            //}

            //// calcular avg angular direction
            //double sum = 0.0;
            //foreach (Stroke s in m_subStrokes)
            //{
            //    sum += Math.Abs(angularDirection(s));
            //}
            //m_avgAngularDirection = (sum * 180.0) / (m_subStrokes.Count * Math.PI);

            //// generate beautified polyline
            //generatePolyline();
            ////try
            ////{
            ////    computeBeautified();
            ////    m_beautified.getAttributes().remove(IsAConstants.PRIMITIVE);
            ////    m_beautified.setShapes(m_subshapes);
            ////    m_beautified.setLabel(Fit.POLYLINE + " (" + m_subshapes.size()
            ////            + ")");
            ////}
            ////catch (Exception e)
            ////{
            ////    log.debug("Could not create shape object: " + e.getMessage());
            ////}
        }

      //  private bool combineOvertracedLine(List<Stroke> strokes,Recognizer features)
      //  {
      //      bool passed = true;
      //      LineFit lineFit = new LineFit();
      //      lineFit.FitTest(features, false);
      //      double err = lineFit.getLSQE();
      //      double fa = lineFit.getError();
      //      if (err / features.getStrokeLength() > 1.29)
      //          passed = false;
      //      // System.out.println("line fit: passed = " + passed + " err = " + m_err
      //      // + " lsqe = " + m_lsqe / m_features.getStrokeLength());

      //      // compute slopes
      //      if (passed)
      //      {
      //          double maxSlopeDiff = Double.MinValue;
      //          List<Double> slopes = new List<Double>();
      //          foreach (Stroke str in strokes)
      //              slopes.Add(getSlope(str));
      //          double diff;
      //          for (int i = 0; i < slopes.Count; i++)
      //          {
      //              for (int j = 0; j < slopes.Count; j++)
      //              {
      //                  if (i != j)
      //                  {
      //                      diff = angleBetween(slopes[i], slopes[j]);
      //                      if (diff > maxSlopeDiff)
      //                          maxSlopeDiff = diff;
      //                  }
      //              }
      //          }
      //          if (maxSlopeDiff > 90.0)
      //              maxSlopeDiff -= 90.0;
      //          // System.out.println("max slope diff = " + maxSlopeDiff);
      //          if (maxSlopeDiff > 10.0)
      //              passed = false;
      //      }

      //      // set values
      //      if (passed)
      //      {
      //          m_allLinesPassed = true;
      //          m_subStrokes.Clear();
      //          m_subStrokes.Add(new Stroke(features.getOrigStroke()));
      //          m_subshapes.Clear();
      //          m_subshapes.Add(lineFit.getShape());
      //          m_numPassed = 1;
      //          m_passed = true;
      //          m_shape = features.getMajorAxis();
      //          m_err = fa;
      //          m_lsqe = err;
      //      }
      //      return passed;
      //  }

      //  /**
	     //* Get the slope of a given Stroke
	     //* 
	     //* @param s
	     //*            stroke to find slope of
	     //* @return slope of stroke (assumed to be a line)
	     //*/
      //  private double getSlope(Stroke s)
      //  {
      //      Point p1 = s.getFirstPoint();
      //      Point p2 = s.getLastPoint();
      //      return (p2.getY() - p1.getY()) / (p2.getX() - p1.getX());
      //  }
    }
}
