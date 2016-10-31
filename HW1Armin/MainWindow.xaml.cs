using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Microsoft.Ink;
using System.Text.RegularExpressions;

namespace HW1Armin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Recognizer _recognizer;

        Window repeatWindow;
        private static double ThreasholdNearbyDistance = 22;
        private List<System.Windows.Ink.Stroke> selectedStrokes = new List<System.Windows.Ink.Stroke>();
        private int touchCount;

        Segmentation segmenter = new Segmentation();
        MachineRecognizer _machineRecognizer = new MachineRecognizer();

        List<Symbol> addedSymbols = new List<Symbol>();
        List<int> strokesPerSymbolCount = new List<int>();
        static Dictionary<string, int[]> operators = new Dictionary<string, int[]>();
        private string lastFoundSymbol;

        public MainWindow()
        {
            InitializeComponent();
            LoadDataSet();
            segmenter.DoRecognition += DoRecognition;
            InkCanvas.Strokes.StrokesChanged += InkCanvas_StrokesChanged;
        }
        
        private void LoadDataSet()
        {
            string datasetDirectory = "../../../test";
            if (!Directory.Exists(datasetDirectory))
                datasetDirectory = "";
            if (datasetDirectory == "")
            {
                System.Windows.Forms.FolderBrowserDialog open_diag = new System.Windows.Forms.FolderBrowserDialog();
                open_diag.ShowDialog();
                datasetDirectory = open_diag.SelectedPath;
            }
            if (datasetDirectory == null || datasetDirectory == "")
                System.Environment.Exit(1);
            var directoryEnumerator = Directory.GetFiles(datasetDirectory);
            //StrokeCollection collection = new StrokeCollection(new FileStream(directoryEnumerator[0],FileMode.Open,FileAccess.Read));
            //Dataset.Instance.Save(new StrokeCollection(new FileStream(directoryEnumerator[0], FileMode.Open, FileAccess.Read)));
            foreach (var path in directoryEnumerator)
            {
                try {
                    Dataset.Instance.Save(path, new StrokeCollection(new FileStream(path, FileMode.Open, FileAccess.Read)));
                }
                catch (Exception)
                {

                }
            }
        }
        
        private void DoRecognition(object sender, EventArgs e)
        {
            if (segmenter.GetStrokes().Count == 0)
                return;
            var strokes = segmenter.GetStrokes();
            var nbest = _machineRecognizer.Recognize(strokes);
            var dollarNRes = nbest[nbest.Keys.ToArray()[0]].Name;
            var protractorRes = nbest[nbest.Keys.ToArray()[1]].Name;
            var pincherRes = nbest[nbest.Keys.ToArray()[2]].Name;

            var dollarNRes3 = nbest[nbest.Keys.ToArray()[3]].Name;
            var protractorRes3 = nbest[nbest.Keys.ToArray()[4]].Name;
            var pincherRes3 = nbest[nbest.Keys.ToArray()[5]].Name;

            var dollarNRes5 = nbest[nbest.Keys.ToArray()[6]].Name;
            var protractorRes5 = nbest[nbest.Keys.ToArray()[7]].Name;
            var pincherRes5 = nbest[nbest.Keys.ToArray()[8]].Name;

            var name = pincherRes5.Substring(0, pincherRes5.IndexOf("_"));
            lastFoundSymbol = name;
            if (nbest[nbest.Keys.ToArray()[6]].Score < 0.85)
            {
                Dispatcher.Invoke(() => { InkCanvas.Strokes.Remove(strokes); });
                return;
            }
            Symbol sm = new Symbol(name, strokes);
            addedSymbols.Add(sm);
            strokesPerSymbolCount.Add(strokes.Count);
            lastFoundSymbol = name;
            foreach (var item in addedSymbols)
            {
                Console.Write(item.name);
            }

            if (strokes.Count == 1 && (name == "-" || name == "divide"))
            {
                var length = GetStrokeLength(strokes);
                if (length > 50)
                    name = "-"; // For now let divide be -
                else
                {
                    name = "-";
                }
            }

            if (name == "=")
            {
                List<Symbol> symbols = addedSymbols.ToList<Symbol>();
                addedSymbols.Clear();
                strokesPerSymbolCount.Clear();
                symbols.Sort();
                string equation = "";
                equation = recognizeEquation2(symbols, getBoundingBoxOfSymbols(symbols), equation);
                //equation = postProcessEquation(equation);
                Console.WriteLine(equation);
                try
                {
                    double result = CalculateExpression(InfixToPostFix(equation.Substring(0, equation.Length - 1)));
                    System.Windows.MessageBox.Show(equation + result);
                    var text = equation + result;
                    Console.WriteLine(text);
                    //Rect equalBoundingBox = getBoundBox(strokes);
                    //TextBlock textBlock1 = new TextBlock();
                    //textBlock1.Text = "" + result;
                    //textBlock1.FontSize = 60;
                    //InkCanvas.SetLeft(textBlock1, getMaxXRect(equalBoundingBox) + 10);
                    //InkCanvas.SetTop(textBlock1, equalBoundingBox.Y - equalBoundingBox.Height / 2 - textBlock1.FontSize / 2);
                    //inkCan.Children.Add(textBlock1);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Error: Equation is not correct" + equation);
                    //throw;
                }
            }

            this.Dispatcher.Invoke(() => {
                textBlock.Text = "$N Dollar (1): " + dollarNRes + "\n" +
                    "Protractor (1): " + protractorRes + "\n" +
                    "Penny Pincher (1): " + pincherRes + "\n";
                textBlock1.Text = "$N Dollar (3): " + dollarNRes3 + "\n" +
                    "Protractor (3): " + protractorRes3 + "\n" +
                    "Penny Pincher (3): " + pincherRes3 + "\n";
                textBlock2.Text = "$N Dollar (5): " + dollarNRes5 + "\n" +
                    "Protractor (5): " + protractorRes5 + "\n" +
                    "Penny Pincher (5): " + pincherRes5 + "\n";
            });
        }

        private string recognizeEquation2(List<Symbol> symbols, Rect boundingBox, string equation)
        {
            List<Symbol> newSymbols = getSymbolsInBoundingBox(symbols, boundingBox);
            Symbol prevSymbol = new Symbol();
            bool inPower = false;
            for (int index = 0; index < newSymbols.Count; index++) 
            {
                Symbol symbol = newSymbols[index];
                // check if symbol seen before
                if (!symbol.isSeen)
                {
                    symbol.isSeen = true;
                    if (!prevSymbol.isEmpty) // the presymbol must be digit
                    {
                        if (checkIfPower(prevSymbol, symbol))
                        {
                            if (prevSymbol.isOprator || prevSymbol.name == "(")
                                goto checkpoint1; // in fact this is an error

                            List<Symbol> powerSymbols = new List<Symbol>();
                            symbol.isSeen = false;
                            powerSymbols.Add(symbol);
                            for (int i = index + 1; i < newSymbols.Count; i++)
                            {
                                if (checkIfPower(prevSymbol, newSymbols[i]))
                                {
                                    newSymbols[i].isSeen = false;
                                    powerSymbols.Add(newSymbols[i]);
                                }
                                else
                                    break;
                            }
                            string powerEquation = "";
                            powerSymbols.Sort();
                            powerEquation = 
                                recognizeEquation2(powerSymbols, getBoundingBoxOfSymbols(powerSymbols), powerEquation);

                            string powerBase = "";
                            for (int i = index - 1; i >= 0; i--) // reformat equation
                            {
                                if (newSymbols[i].isOprator)
                                    break;
                                powerBase = newSymbols[i].name + powerBase;
                                equation = equation.Substring(0, equation.Length - 1); // remove the last character
                                if (newSymbols[i].name == "(")
                                    break;
                            }
                            equation += "power(" + powerBase + "," + powerEquation + ")";
                            prevSymbol = powerSymbols[powerSymbols.Count - 1];
                            continue;
                        }

                    }
                checkpoint1:
                    if (symbol.name == "-")
                    {
                        Rect upperRect = new Rect(
                            new Point(symbol.boundingBox.X, boundingBox.Y),
                            new Point(getMaxXRect(symbol.boundingBox), getMaxYRect(symbol.boundingBox)));
                        if (getSymbolsInBoundingBox(newSymbols, upperRect).Count > 0)
                        {
                            equation += "(";
                            equation += "(";
                            equation = recognizeEquation2(newSymbols, upperRect, equation);
                            equation += ")";
                            equation += "/";
                            Rect lowerRect = new Rect(
                                new Point(symbol.boundingBox.X, symbol.boundingBox.Y),
                                new Point(getMaxXRect(symbol.boundingBox), getMaxYRect(boundingBox)));
                            equation += "(";
                            equation = recognizeEquation2(newSymbols, lowerRect, equation);
                            equation += ")";
                            equation += ")";

                            // drawRectangle(upperRect);
                            //drawRectangle(lowerRect);
                        }
                        else
                        {
                            equation += symbol.name;
                        }
                    }
                    else if (symbol.isSqrt)
                    {
                        equation += "sqrt(";
                        equation = recognizeEquation2(newSymbols, symbol.boundingBox, equation);
                        equation += ")";
                    }
                    else
                    {
                        equation += symbol.name;
                    }
                    prevSymbol = symbol;
                }
            }
            if (inPower)
            {
                equation += ")";
                inPower = false;
            }
            return equation;
        }
        
        private string recognizeEquation(List<Symbol> symbols, Rect boundingBox, string equation)
        {
            List<Symbol> newSymbols = getSymbolsInBoundingBox(symbols, boundingBox);
            Symbol prevSymbol = new Symbol();
            bool inPower = false;
            foreach (Symbol symbol in newSymbols)
            {
                // check if symbol seen before
                if (!symbol.isSeen)
                {
                    symbol.isSeen = true;
                    if (!prevSymbol.isEmpty) // the presymbol must be digit
                    {
                        if (checkIfPower(prevSymbol, symbol))
                        {
                            if (!char.IsNumber(equation[equation.Length - 1]))
                                goto checkpoint1;
                            if (inPower)
                            {
                                equation += ")";
                                inPower = false;
                            }
                            else if (!prevSymbol.isOprator)
                            {
                                int i;
                                for (i = equation.Length - 1; i >= 0; i--)
                                {
                                    if (!char.IsNumber(equation[i])) break;
                                }
                                string number = equation.Substring(i + 1, equation.Length - 1 - i);
                                equation = equation.Substring(0, i + 1);
                                equation += "power(" + number + ",";
                                inPower = true;
                            }
                        }

                    }
                    checkpoint1:
                    if (symbol.name == "-")
                    {
                        Rect upperRect = new Rect(
                            new Point(symbol.boundingBox.X, boundingBox.Y), 
                            new Point(getMaxXRect(symbol.boundingBox), getMaxYRect(symbol.boundingBox)));
                        if (getSymbolsInBoundingBox(newSymbols, upperRect).Count > 0)
                        {
                            equation += "(";
                            equation += "(";
                            equation = recognizeEquation(newSymbols, upperRect, equation);
                            equation += ")";
                            equation += "/";
                            Rect lowerRect = new Rect(
                                new Point(symbol.boundingBox.X, symbol.boundingBox.Y), 
                                new Point(getMaxXRect(symbol.boundingBox), getMaxYRect(boundingBox)));
                            equation += "(";
                            equation = recognizeEquation(newSymbols, lowerRect, equation);
                            equation += ")";
                            equation += ")";

                            // drawRectangle(upperRect);
                            //drawRectangle(lowerRect);
                        }
                        else
                        {
                            equation += symbol.name;
                        }
                    }
                    else if (symbol.isSqrt)
                    {
                        equation += "sqrt(";
                        equation = recognizeEquation(newSymbols, symbol.boundingBox, equation);
                        equation += ")";
                    }
                    else
                    {
                        equation += symbol.name;
                    }
                    prevSymbol = symbol;
                }
            }
            if (inPower)
            {
                equation += ")";
                inPower = false;
            }
            return equation;
        }
        

        #region UI Event Handlers
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

        private void repeat_Button_Click(object sender, RoutedEventArgs e)
        {
            if (repeatWindow == null)
            {
                repeatWindow = new RepeatWindow();
                repeatWindow.Show();
            }
            else
            {
                if (!repeatWindow.IsVisible)
                    repeatWindow.Visibility = Visibility.Visible;
            }
        }

        private void save_with_background_Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            StrokeCollection sc = InkCanvas.Strokes;
            byte[] inkData = null;
            using (MemoryStream inkMemStream = new MemoryStream())
            {
                sc.Save(inkMemStream);
                inkData = inkMemStream.ToArray();
            }
            byte[] gifData = null;
            using (Microsoft.Ink.Ink ink = new Microsoft.Ink.Ink())
            {
                ink.Load(inkData);
                gifData = ink.Save(PersistenceFormat.Gif);
            }
            File.WriteAllBytes("c://strokes.gif", gifData);
        }

        private void InkCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            touchCount++;
            //var res = InkCanvas.Strokes.HitTest(e.GetTouchPoint(this).Position);
            //InkCanvas.Strokes[0].DrawingAttributes.Color = Color.FromArgb(255, 50, 100, 255);
            int nearByTouched = 0;
            foreach (var item in InkCanvas.Strokes)
            {
                var isNearby = IsNearbyPoint(item, e.GetTouchPoint(this).Position);
                if (isNearby)
                {
                    if (selectedStrokes.Contains(item))
                    {
                        item.DrawingAttributes.Color = Color.FromArgb(255, 0, 0, 0);
                        this.selectedStrokes.Remove(item);
                    }
                    else
                    {
                        if (touchCount > 1)
                        {
                            item.DrawingAttributes.Color = Color.FromArgb(255, 50, 100, 255);
                            this.selectedStrokes.Add(item);
                        }
                        else
                        {
                            foreach (var item2 in InkCanvas.Strokes)
                            {
                                item2.DrawingAttributes.Color = Color.FromArgb(255, 0, 0, 0);
                            }
                            selectedStrokes.Clear();
                            item.DrawingAttributes.Color = Color.FromArgb(255, 50, 100, 255);
                            this.selectedStrokes.Add(item);
                        }
                    }
                    nearByTouched++;
                }
            }
            if (nearByTouched == 0 && touchCount == 0)
            {
                foreach (var item2 in InkCanvas.Strokes)
                {
                    item2.DrawingAttributes.Color = Color.FromArgb(255, 0, 0, 0);
                }
                this.selectedStrokes.Clear();
            }
        }

        private void InkCanvas_TouchUp(object sender, TouchEventArgs e)
        {
            touchCount--;
        }

        private void InkCanvas_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (selectedStrokes != null)
            {
                foreach (var item in selectedStrokes)
                {
                    var boundingRect = item.GetBounds();
                    Matrix transformMatrix = new Matrix();
                    transformMatrix.Translate(
                        e.DeltaManipulation.Translation.X,
                        e.DeltaManipulation.Translation.Y);
                    transformMatrix.RotateAt(
                        e.DeltaManipulation.Rotation,
                        boundingRect.TopLeft.X + boundingRect.Width,
                        boundingRect.TopLeft.Y + boundingRect.Height);
                    transformMatrix.ScaleAt(
                        e.DeltaManipulation.Scale.X,
                        e.DeltaManipulation.Scale.Y,
                        boundingRect.TopLeft.X + boundingRect.Width,
                        boundingRect.TopLeft.Y + boundingRect.Height);
                    item.Transform(transformMatrix, false);
                }
            }
        }

        private void erase_all_Button_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.Strokes.Clear();
            addedSymbols.Clear();
            strokesPerSymbolCount.Clear();
            textBlock.Text = "";
            textBlock1.Text = "";
            textBlock2.Text = "";
        }

        private void InkCanvas_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.None;
        }

        private void InkCanvas_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            _recognizer = new Recognizer(e.Stroke.StylusPoints);
            var results = _recognizer.Recognize();
            var ass =results.info.Contains("Scratch");
            if (ass)
            {
                var hitStrokes = HitTest(e.Stroke);

                if (hitStrokes.Count > 0)
                {
                    foreach (var item in hitStrokes)
                    {
                        InkCanvas.Strokes.Remove(item);
                    }
                }
            }
            else
                segmenter.Feed(e.Stroke);
        }

        private void group_Button_Click(object sender, RoutedEventArgs e)
        {
            StylusPointCollection groupedPoints = new StylusPointCollection();
            foreach (var item in selectedStrokes)
            {
                groupedPoints.Add(item.StylusPoints);
                InkCanvas.Strokes.Remove(item);
            }
            InkCanvas.Strokes.Add(new System.Windows.Ink.Stroke(groupedPoints));
        }

        private void train_Button_Click(object sender, RoutedEventArgs e)
        {
            //TrainerForm _form = new TrainerForm();
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

        private void InkCanvas_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (e.Removed.Count > 0)
            {
                if (addedSymbols.Count == 0)
                    return;
                if (lastFoundSymbol == addedSymbols[addedSymbols.Count - 1].name)
                {
                    if (strokesPerSymbolCount.Count == addedSymbols.Count)
                    {
                        strokesPerSymbolCount[strokesPerSymbolCount.Count - 1]--;
                        addedSymbols.RemoveAt(addedSymbols.Count - 1);
                        if (strokesPerSymbolCount[strokesPerSymbolCount.Count - 1] == 0)
                        {
                            strokesPerSymbolCount.RemoveAt(strokesPerSymbolCount.Count - 1);
                            if (addedSymbols.Count > 0)
                                lastFoundSymbol = addedSymbols[addedSymbols.Count - 1].name;
                            else
                                lastFoundSymbol = "";

                        }
                    }
                }
                else
                {
                    if (strokesPerSymbolCount.Count > addedSymbols.Count)
                    {
                        strokesPerSymbolCount[strokesPerSymbolCount.Count - 1]--;
                        if (strokesPerSymbolCount[strokesPerSymbolCount.Count - 1] == 0)
                        {
                            strokesPerSymbolCount.RemoveAt(strokesPerSymbolCount.Count - 1);
                            lastFoundSymbol = addedSymbols[addedSymbols.Count - 1].name;
                        }
                    }
                }
            }
        }
        #endregion

        #region Non-Math Helper Functions
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

        private static bool IsNearbyPoint(System.Windows.Ink.Stroke stroke, Point point)
        {
            return stroke.StylusPoints.Any(p => (Math.Abs(p.X - point.X) <= ThreasholdNearbyDistance) &&
                (Math.Abs(p.Y - point.Y) <= ThreasholdNearbyDistance));
        }

        public static bool hasIntersection(Rect rect1, Rect rect2)
        {
            System.Drawing.Rectangle r1 = new System.Drawing.Rectangle((int)rect1.X, (int)rect1.Y, (int)rect1.Width, (int)rect1.Height);
            System.Drawing.Rectangle r2 = new System.Drawing.Rectangle((int)rect2.X, (int)rect2.Y, (int)rect2.Width, (int)rect2.Height);
            return r1.IntersectsWith(r2);
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

        #endregion

        #region Math Helper Functions
        Rect getBoundingBoxOfSymbols(List<Symbol> symbols)
        {
            double minX = symbols[0].boundingBox.X;
            double minY = symbols[0].boundingBox.Y;

            double maxX = getMaxXRect(symbols[0].boundingBox);
            double maxY = getMaxYRect(symbols[0].boundingBox);

            foreach (Symbol symbol in symbols)
            {
                if (symbol.boundingBox.X < minX) minX = symbol.boundingBox.X;
                if (symbol.boundingBox.Y < minY) minY = symbol.boundingBox.Y;
                if (getMaxXRect(symbol.boundingBox) > maxX) maxX = getMaxXRect(symbol.boundingBox);
                if (getMaxYRect(symbol.boundingBox) > maxY) maxY = getMaxYRect(symbol.boundingBox);
            }
            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        public static double getMaxXRect(Rect rect)
        {
            return rect.X + rect.Width;
        }

        public static double getMaxYRect(Rect rect)
        {
            return rect.Y + rect.Height;
        }

        public List<Symbol> getSymbolsInBoundingBox(List<Symbol> symbols, Rect boundingBox)
        {
            List<Symbol> newSymbols = new List<Symbol>();

            foreach (Symbol symbol in symbols)
            {
                if (hasIntersection(boundingBox, symbol.boundingBox) && !symbol.isSeen)
                {
                    newSymbols.Add(symbol);
                }
            }

            return newSymbols;
        }

        private double GetStrokeLength(StrokeCollection strokes)
        {
            var points = strokes[0].StylusPoints;
            double Xdist = points[0].X - points[points.Count - 1].X;
            double Ydist = points[0].Y - points[points.Count - 1].Y;

            return Math.Sqrt(Xdist * Xdist + Ydist * Ydist);
        }

        bool checkIfPower(Symbol s1, Symbol s2)
        {
            double diffX = Math.Abs(s1.boundingBox.Y - s2.boundingBox.Y);
            var s1Bottom = s1.boundingBox.Bottom;
            var s2Bottom = s2.boundingBox.Bottom;

            var s1Middle = s1.boundingBox.Bottom - s1.boundingBox.Height / 2;
            var s2Middle = s2.boundingBox.Bottom - s2.boundingBox.Height / 2;

            if (s2Bottom < s1Middle)
                return true;
            //if (
            //    (s2.boundingBox.Y + s2.boundingBox.Height / 2) < s1.boundingBox.Y ||
            //    (s2.boundingBox.Y + s2.boundingBox.Height / 2) > getMaxYRect(s1.boundingBox)) return true;
            return false;
        }

        public static Queue<string> InfixToPostFix(string expression)
        {
            Queue<string> queue = new Queue<string>();
            Stack<string> stack = new Stack<string>();

            if (operators.Count == 0)
            {
                //populate operators
                //int format is {precedence, association -> 0=Left 1=Right}
                operators.Add("sin", new int[] { 4, 0 });
                operators.Add("cos", new int[] { 4, 0 });
                operators.Add("tan", new int[] { 4, 0 });
                operators.Add("cot", new int[] { 4, 0 });
                operators.Add("power", new int[] { 3, 1 });
                operators.Add("sqrt", new int[] { 3, 0 });
                operators.Add("*", new int[] { 2, 0 });
                operators.Add("/", new int[] { 2, 0 });
                operators.Add("+", new int[] { 1, 0 });
                operators.Add("-", new int[] { 1, 0 });

            }

            expression = expression.Replace(" ", "");
            string pattern = @"(?<=[-+*/(),^<>=&])(?=.)|(?<=.)(?=[-+*/(),^<>=&])";

            Regex regExPattern = new Regex(pattern);

            regExPattern.Split(expression).Where(s => !String.IsNullOrEmpty(s.Trim())).ToList().ForEach(s =>
            {
                //is our token one of our defined operators
                //deal with precedence and association
                if (operators.ContainsKey(s))
                {
                    //while the stack is not empty and the top of the stack is not an (
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        if ((GetAssociation(s) == 0 && GetPrecedence(s) <= GetPrecedence(stack.Peek())) ||
                            (GetAssociation(s) == 1 && GetPrecedence(s) < GetPrecedence(stack.Peek()))
                          )
                            queue.Enqueue(stack.Pop());
                        else
                            break;
                    }

                    //push operator onto the stack
                    stack.Push(s);
                }

                //handle opening parenthesis
                //simply push this on the stack
                else if (s == "(")
                {
                    stack.Push(s);
                }

                //handle closing parenthesis
                //pop all operators off the stack until the matching 
                //opening parenthesis is found and then discard the
                //opening parenthesis
                else if (s == ")")
                {
                    while (stack.Count != 0 && stack.Peek() != "(")
                        queue.Enqueue(stack.Pop());

                    //forget the (
                    stack.Pop();
                }
                else if (s != ",")
                    //none of the above so queue it
                    queue.Enqueue(s);
            });

            //pop off the rest of the stack
            while (stack.Count != 0)
                queue.Enqueue(stack.Pop());

            return queue;

        }

        private static int GetPrecedence(string s)
        {
            return operators[s][0];
        }

        //Get the association of the operator passed in
        private static int GetAssociation(string s)
        {
            return operators[s][1];
        }

        public static double CalculateExpression(Queue<String> postfix)
        {
            Stack<double> stack = new Stack<double>();

            postfix.ToList<String>().ForEach(token =>
            {
                if (operators.ContainsKey(token))
                {
                    if (stack.Count > 0)
                    {

                        double rhs;
                        double lhs;
                        switch (token)
                        {

                            case "sqrt":
                                rhs = stack.Pop();
                                stack.Push(Math.Sqrt(rhs));
                                break;
                            case "sin":
                                rhs = stack.Pop();
                                stack.Push(Math.Sin(rhs));
                                break;
                            case "cos":
                                rhs = stack.Pop();
                                stack.Push(Math.Cos(rhs));
                                break;
                            case "tan":
                                rhs = stack.Pop();
                                stack.Push(Math.Tan(rhs));
                                break;
                            case "cot":
                                rhs = stack.Pop();
                                stack.Push(1 / Math.Tan(rhs));
                                break;
                            case "+":
                                rhs = stack.Pop();
                                lhs = stack.Pop();
                                stack.Push(lhs + rhs);
                                break;
                            case "-":
                                rhs = stack.Pop();
                                lhs = stack.Pop();
                                stack.Push(lhs - rhs);
                                break;
                            case "*":
                                rhs = stack.Pop();
                                lhs = stack.Pop();
                                stack.Push(lhs * rhs);
                                break;
                            case "/":
                                rhs = stack.Pop();
                                lhs = stack.Pop();
                                stack.Push(lhs / rhs);
                                break;
                            case "power":
                                rhs = stack.Pop();
                                lhs = stack.Pop();
                                stack.Push(Math.Pow(lhs, rhs));
                                break;

                        }
                    }
                }
                else
                    stack.Push(Convert.ToDouble(token));
            });

            //everything has been evaluated and our result is on the stack
            //so return it
            return stack.Pop();
        }
        #endregion
    }
}
