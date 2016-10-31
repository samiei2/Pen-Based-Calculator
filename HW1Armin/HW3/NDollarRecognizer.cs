using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace HW1Armin.HW3
{
    class NDollarRecognizer
    {
        private Hashtable _gestures;
        private static readonly double Phi = 0.5 * (-1 + Math.Sqrt(5));
        private static readonly double _RotationBound = 45.0;
        private const double DX = 250.0;
        public static readonly SizeR ResampleScale = new SizeR(DX, DX);
        public static readonly double Diagonal = Math.Sqrt(DX * DX + DX * DX);
        public static readonly double HalfDiagonal = 0.5 * Diagonal;

        public NDollarRecognizer()
        {
            _gestures = Dataset.Instance._NDOLLARDataset;
        }

        public NBestList Recognize(StrokeCollection strokes,int sampleCount = 1)
        {
            List<PointR> points = new List<PointR>();
            foreach (var pts in strokes)
            {
                points.AddRange(GetPointRs(pts));
            }
            return Recognize(points, strokes.Count, sampleCount);
        }

        private IEnumerable<PointR> GetPointRs(Stroke pts)
        {
            List<PointR> points = new List<PointR>();
            foreach (var item in pts.StylusPoints)
            {
                points.Add(new PointR(item.X, item.Y));
            }
            return points;
        }

        public NBestList Recognize(List<PointR> points, int numStrokes, int sampleCount) // candidate points
        {
            Gesture candidate = new Gesture(points);
            NBestList nbest = new NBestList();

            int totalComparisons = 0;
            int actualComparisons = 0;
            _gestures = Dataset.Instance.GetSampleCount(sampleCount);
            foreach (Multistroke ms in _gestures.Values)
            {
                if (!NDollarParameters.Instance.MatchOnlyIfSameNumberOfStrokes || numStrokes == ms.NumStrokes) // optional -- only attempt match when number of strokes is same
                {
                    NBestList thisMSnbest = new NBestList(); // store the best list for just this MS
                    foreach (Gesture p in ms.Gestures)
                    {
                        totalComparisons++;

                        if (!NDollarParameters.Instance.DoStartAngleComparison ||
                            (NDollarParameters.Instance.DoStartAngleComparison && Utils.AngleBetweenUnitVectors(candidate.StartUnitVector, p.StartUnitVector) <= NDollarParameters.Instance.StartAngleThreshold))
                        {
                            actualComparisons++;

                            double score = -1;
                            double[] best = new double[3] { -1, -1, -1 };

                            if (NDollarParameters.Instance.SearchMethod == NDollarParameters.PossibleSearchMethods.GSS)
                            {
                                best = GoldenSectionSearch(
                                    candidate.Points,
                                    p.Points,
                                    Utils.Deg2Rad(-_RotationBound),
                                    Utils.Deg2Rad(+_RotationBound),
                                    Utils.Deg2Rad(2.0)
                                );

                                score = 1d - best[0] / HalfDiagonal;
                            }
                            else if (NDollarParameters.Instance.SearchMethod == NDollarParameters.PossibleSearchMethods.Protractor)
                            {
                                best = OptimalCosineDistance(p.VectorVersion, candidate.VectorVersion); //candidate.Points, p.Points);
                                score = 1 / best[0]; // distance
                            }
                            
                            thisMSnbest.AddResult(p.Name, score, best[0], best[1]); // name, score, distance, angle
                        }
                    }
                    thisMSnbest.SortDescending();
                    nbest.AddResult(thisMSnbest.Name, thisMSnbest.Score, thisMSnbest.Distance, thisMSnbest.Angle); // name, score, distance, angle
                }
            }
            nbest.SortDescending(); // sort so that nbest[0] is best result
            nbest.setTotalComparisons(totalComparisons);
            nbest.setActualComparisons(actualComparisons);
            return nbest;
        }

        private double[] GoldenSectionSearch(List<PointR> pts1, List<PointR> pts2, double a, double b, double threshold)
        {
            double x1 = Phi * a + (1 - Phi) * b;
            List<PointR> newPoints = Utils.RotateByRadians(pts1, x1);
            double fx1 = Utils.PathDistance(newPoints, pts2);

            double x2 = (1 - Phi) * a + Phi * b;
            newPoints = Utils.RotateByRadians(pts1, x2);
            double fx2 = Utils.PathDistance(newPoints, pts2);

            double i = 2.0; // calls
            while (Math.Abs(b - a) > threshold)
            {
                if (fx1 < fx2)
                {
                    b = x2;
                    x2 = x1;
                    fx2 = fx1;
                    x1 = Phi * a + (1 - Phi) * b;
                    newPoints = Utils.RotateByRadians(pts1, x1);
                    fx1 = Utils.PathDistance(newPoints, pts2);
                }
                else
                {
                    a = x1;
                    x1 = x2;
                    fx1 = fx2;
                    x2 = (1 - Phi) * a + Phi * b;
                    newPoints = Utils.RotateByRadians(pts1, x2);
                    fx2 = Utils.PathDistance(newPoints, pts2);
                }
                i++;
            }
            return new double[3] { Math.Min(fx1, fx2), Utils.Rad2Deg((b + a) / 2.0), i }; // distance, angle, calls to pathdist
        }

        private double[] OptimalCosineDistance(List<Double> v1, List<Double> v2)
        {
            double a = 0;
            double b = 0;

            for (int i = 0; i < v1.Count; i = i + 2)
            {
                a = a + v1[i] * v2[i] + v1[i + 1] * v2[i + 1];
                b = b + v1[i] * v2[i + 1] - v1[i + 1] * v2[i];
            }

            double angle = Math.Atan(b / a);
            return new double[3] { Math.Acos(a * Math.Cos(angle) + b * Math.Sin(angle)), Utils.Rad2Deg(angle), 0d }; // distance, angle, calls to path dist (n/a)
        }
    }
}
