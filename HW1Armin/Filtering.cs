using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW1Armin
{
    class Filtering
    {
        /**
	 * Perform median filtering on the input using the given window size. Repeat
	 * edge values when the window "falls off the edge" of the input data.
	 * 
	 * @param input
	 *            Data to filter, cannot be null
	 * @param windowSize
	 *            Must be null and in the range of input.length
	 * @return Data filtered using median filtering.
	 */
        public static List<double> medianFilter(List<double> input, int windowSize)
        {
            if (input == null)
            {
                throw new Exception("Input cannot be null");
            }
            if (input.Count <= 2)
                return input;
            if (windowSize < 1 || windowSize > input.Count || windowSize % 2 == 0)
            {
                throw new Exception(
                        "Window size must be odd and in the range 1 >= windowSize < input.length; window size = "
                                + windowSize + " input length = " + input.Count);
            }

            List<double> output = new List<double>(input.Count);

            // loop over each position in the output to calculate the filtered
            // results
            for (int i = 0; i < output.Count; i++)
            {
                List<double> window = getWindow(input, windowSize, i);

                // median
                output[i] = Statistics.median(window);
            }

            return output;
        }

        internal static List<double> medianFilter(List<double> input)
        {
            return medianFilter(input, getDefaultWindowSize(input));
        }

        protected static int getDefaultWindowSize(List<double> input)
        {
            double sqrt = Math.Sqrt(input.Count);
            int window = (int)Math.Round(sqrt);

            // window is even but we need odd window size; round to nearest odd
            if (window % 2 == 0)
            {
                if (window < sqrt)
                    window++;
                else
                    window--;
            }

            return window;
        }

        public static List<double> getWindow(List<double> data, int windowSize, int i)
        {
            if (data == null)
            {
                throw new Exception("Data cannot be null");
            }
            if (windowSize < 1 || windowSize >= data.Count || windowSize % 2 == 0)
            {
                throw new Exception(
                        "Window size must be ODD number >=1, < data.length; window size = "
                                + windowSize + " data length = " + data.Count);
            }
            if (i < 0 || i > data.Count)
            {
                throw new Exception(
                        "Given value for i is out of bounds");
            }

            // loop over each position in the window and fill it with the
            // correct values
            List<double> window = new List<double>(windowSize);

            // how much on either side of i do we want?
            int halfWindow = (int)Math.Floor(windowSize / 2.0);

            // we start half window width less than the center
            int start = i - halfWindow;

            // loop over window index
            for (int wIdx = 0; wIdx < window.Count; wIdx++)
            {
                // where in the original input is this portion of the sliding
                // window?
                int dataIdx = start + wIdx;

                // check for out of bounds, and repeat edge values if we're out
                if (dataIdx < 0)
                {
                    window[wIdx] = data[0];
                }
                else if (dataIdx >= data.Count)
                {
                    window[wIdx] = data[data.Count - 1];
                }
                // not out of bounds
                else
                {
                    window[wIdx] = data[dataIdx];
                }
            }

            return window;
        }

        //public static Line2D bestFitLine(double sumX, double sumX2, double sumY,
        //    double sumXY, int n, Rectangle2D bounds)
        //{
        //    Matrix result = new Matrix(1, 0);
        //    result = fit(sumX, sumX2, sumY, sumXY, n);
        //    double a = result.get(0, 0);
        //    double b = result.get(1, 0);
        //    double minX = bounds.getMinX();
        //    double maxX = bounds.getMaxX();
        //    double minY = a + b * minX;
        //    double maxY = a + b * maxX;
        //    return new Line2D.Double(minX, minY, maxX, maxY);
        //}

        //public static Matrix fit(double sumX, double sumX2, double sumY,
        //    double sumXY, int n)
        //{
        //    Matrix A = new Matrix(2, 2);
        //    Matrix b = new Matrix(2, 1);
        //    A.set(0, 0, n);
        //    A.set(1, 0, sumX);
        //    A.set(0, 1, sumX);
        //    A.set(1, 1, sumX2);
        //    b.set(0, 0, sumY);
        //    b.set(1, 0, sumXY);
        //    return A.solve(b);
        //}
    }
}
