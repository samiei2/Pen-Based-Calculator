using MathNet.Numerics.LinearAlgebra;
using System;
using System.Windows.Input;

namespace HW1Armin
{
    internal class LeastSquares
    {
        /**
	     * Perform a least squares fit with the input values and return a line
	     * within the given bounds
	     * 
	     * @param sumX
	     *            sum of the x values
	     * @param sumX2
	     *            sum of the x values squared
	     * @param sumY
	     *            sum of the y values
	     * @param sumXY
	     *            sum of the x*y values
	     * @param n
	     *            number of values
	     * @param bounds
	     *            rectangular bounds of best fit line
	     * @return best fit line of the least squares fit y = a + bx
	     */
        public static Line2D bestFitLine(double sumX, double sumX2, double sumY,
                double sumXY, int n, Rectangle2D bounds)
        {
            Matrix<double> result = Matrix<double>.Build.Random(2, 1);
            result = fit(sumX, sumX2, sumY, sumXY, n);
            double a = result[0, 0];
            double b = result[1, 0];
            double minX = bounds.getMinX();
            double maxX = bounds.getMaxX();
            double minY = a + b * minX;
            double maxY = a + b * maxX;
            return new Line2D.Double(minX, minY, maxX, maxY);
        }

        /**
	     * Perform a least squares fit with the input values
	     * 
	     * @param sumX
	     *            sum of the x values
	     * @param sumX2
	     *            sum of the x values squared
	     * @param sumY
	     *            sum of the y values
	     * @param sumXY
	     *            sum of the x*y values
	     * @param n
	     *            number of values
	     * @return 2x1 matrix containing the least squares fit y = a + bx; first
	     *         value in matrix will contain the y-intercept, the second will
	     *         contain the slope
	     */
        public static Matrix<double> fit(double sumX, double sumX2, double sumY,
                double sumXY, int n)
        {
            Matrix<double> A = Matrix<double>.Build.Random(2, 2);
            Matrix<double> b = Matrix<double>.Build.Random(2, 1);
            A[0, 0] = n;
            A[1, 0] = sumX;
            A[0, 1] = sumX;
            A[1, 1] = sumX2;
            b[0, 0] = sumY;
            b[1, 0] = sumXY;
            return A.Solve(b);
        }

        /**
	     * Return the total least squares error between the array of points and the
	     * input line
	     * 
	     * @param points
	     *            points
	     * @param line
	     *            line to find the LSE to
	     * @return total least squares error between the input points and line
	     */
        public static double error(StylusPointCollection points, Line2D line)
        {
            double err = 0.0;
            double err2 = 0.0;
            for (int i = 0; i < points.Count; i++)
            {
                err += line.ptSegDist(points[i].X, points[i].Y);
                err2 += line.ptSegDist2(points[i].X,points[i].Y);
            }
            return err;
        }
    }
}