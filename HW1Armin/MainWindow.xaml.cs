using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Microsoft.Ink;

namespace HW1Armin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void background_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            ofd.ShowDialog();
            if (ofd.FileName != "")
            {
                var ib = new ImageBrush
                {
                    ImageSource =
                        new BitmapImage(
                          new Uri(ofd.FileName, UriKind.RelativeOrAbsolute)
                        )
                };
                InkCanvas.Background = ib;
            }
        }

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var drawingAttrib = new System.Windows.Ink.DrawingAttributes();
            drawingAttrib.Color = colorPicker.SelectedColor ?? default(Color);
            drawingAttrib.Width = size_Slider.Value;
            InkCanvas.DefaultDrawingAttributes = drawingAttrib;
        }

        private void size_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var drawingAttrib = new System.Windows.Ink.DrawingAttributes();
            if (colorPicker != null)
                drawingAttrib.Color = colorPicker.SelectedColor ?? Color.FromRgb(0, 0, 0);
            drawingAttrib.Width = size_Slider.Value;
            drawingAttrib.StylusTip = StylusTip.Ellipse;

            InkCanvas.DefaultDrawingAttributes = drawingAttrib;
        }

        private void save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                var fs = new FileStream(dialog.FileName, FileMode.Create);
                InkCanvas.Strokes.Save(fs);
            }
        }

        private void load_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                var fs = new FileStream(dialog.FileName,
                FileMode.Open, FileAccess.Read);
                StrokeCollection strokes = new StrokeCollection(fs);
                InkCanvas.Strokes = strokes;
            }
        }

        private void backGround_ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            InkCanvas.Background = new SolidColorBrush(backGround_ColorPicker.SelectedColor ?? Color.FromRgb(0, 0, 0));
        }

        private void inkMode_RadioButton_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void selectMode_RadioButton_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.Select;
        }

        private void eraseStrokeMode_RadioButton_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        private void erasePointMode_RadioButton_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
        }

        private bool InterpretScratchoutGesture(System.Windows.Ink.Stroke stroke)
        {
            // Attempt to instantiate a recognizer for scratchout gestures.
            System.Windows.Ink.ApplicationGesture[] gestures = { System.Windows.Ink.ApplicationGesture.ScratchOut };
            GestureRecognizer recognizer = new GestureRecognizer(gestures);

            if (!recognizer.IsRecognizerAvailable)
                return false;

            // Determine if the stroke was a scratchout gesture.
            StrokeCollection gestureStrokes = new StrokeCollection();
            gestureStrokes.Add(stroke);

            ReadOnlyCollection<GestureRecognitionResult> results = recognizer.Recognize(gestureStrokes);

            if (results.Count == 0)
                return false;

            // Results are returned sorted in order strongest-to-weakest; 
            // we need only analyze the first (strongest) result.
            if (results[0].ApplicationGesture == System.Windows.Ink.ApplicationGesture.ScratchOut &&
                  results[0].RecognitionConfidence == System.Windows.Ink.RecognitionConfidence.Strong)
            {
                // Use the scratchout stroke to perform hit-testing and 
                // erase existing strokes, as necessary.
                return true;
            }
            else
            {
                // Not a gesture: display the stroke normally.
                return false;
            }
        }
        

        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var scratch = InterpretScratchoutGesture(e.Stroke);

            if (scratch)
            {
                var hitStrokes = HitTest(e.Stroke);

                if (hitStrokes.Count > 0)
                {
                    InkCanvas.Strokes.Remove(hitStrokes);
                }
            }
        }

        private StrokeCollection HitTest(System.Windows.Ink.Stroke e)
        {
            StrokeCollection hitList = new StrokeCollection();
            for (int i = 0; i < e.StylusPoints.Count - 1; i++)
            {
                var ps1 = GetVector2(e.StylusPoints[i]);
                var pe1 = GetVector2(e.StylusPoints[i + 1]);
                foreach (var stroke in InkCanvas.Strokes)
                {
                    if (hitList.Contains(stroke))
                        continue;
                    for (int j = 0; j < stroke.StylusPoints.Count - 1; j++)
                    {
                        var ps2 = GetVector2(stroke.StylusPoints[j]);
                        var pe2 = GetVector2(stroke.StylusPoints[j + 1]);
                        Vector result;
                        if (LineSegementsIntersect(ps1, pe1, ps2, pe2, out result))
                        {
                            hitList.Add(stroke);
                            break;
                        }
                    }
                }
            }
            return hitList;
        }

        private Vector GetVector2(StylusPoint stylusPoint)
        {
            return new Vector(stylusPoint.X, stylusPoint.Y);
        }

        public static bool LineSegementsIntersect(Vector p, Vector p2, Vector q, Vector q2,
    out Vector intersection, bool considerCollinearOverlapAsIntersect = false)
        {
            intersection = new Vector();

            var r = p2 - p;
            var s = q2 - q;
            var rxs = r.Cross(s);
            var qpxr = (q - p).Cross(r);

            // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            if (rxs.IsZero() && qpxr.IsZero())
            {
                // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
                // then the two lines are overlapping,
                if (considerCollinearOverlapAsIntersect)
                    if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
                        return true;

                // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return false;
            }

            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            if (rxs.IsZero() && !qpxr.IsZero())
                return false;

            // t = (q - p) x s / (r x s)
            var t = (q - p).Cross(s) / rxs;

            // u = (q - p) x r / (r x s)

            var u = (q - p).Cross(r) / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point p + t r = q + u s.
            if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                // We can calculate the intersection point using either t or u.
                intersection = p + t * r;

                // An intersection was found.
                return true;
            }

            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }

        private void InkCanvas_SelectionChanged(object sender, EventArgs e)
        {
            var selectedStrokes = InkCanvas.GetSelectedStrokes();

            using (MemoryStream ms = new MemoryStream())
            {
                selectedStrokes.Save(ms);
                var myInkCollector = new InkCollector();
                var ink = new Ink();
                ink.Load(ms.ToArray());

                using (RecognizerContext myRecoContext = new RecognizerContext())
                {
                    RecognitionStatus status = RecognitionStatus.ProcessFailed;
                    myRecoContext.Strokes = ink.Strokes;
                    try
                    {
                        var recoResult = myRecoContext.Recognize(out status);

                        if (status == RecognitionStatus.NoError)
                        {
                            textBlock.Text = recoResult.TopString;
                            //InkCanvas.Strokes.Clear();
                        }
                        else
                        {
                            MessageBox.Show("ERROR: " + status.ToString());
                        }
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("ERROR: " + status.ToString());
                    }
                }
            }
        }
    }
}
