using Microsoft.Ink;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HW1Armin
{
    /// <summary>
    /// Interaction logic for RepeatWindow.xaml
    /// </summary>
    public partial class RepeatWindow : Window
    {
        private string fileName;
        private int index = 0;

        public RepeatWindow()
        {
            InitializeComponent();
        }

        private void newGesture_Button_Click(object sender, RoutedEventArgs e)
        {
            ShowSaveDialog();
        }

        private void ShowSaveDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                fileName = dialog.FileName;
            }
        }

        private void save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (fileName == null)
                ShowSaveDialog();
            if (fileName == null)
                return;
            if (fileName.Equals(""))
                ShowSaveDialog();
            var nFileName = fileName + "_" + index++ + ".dat";
            var fs = new FileStream(nFileName, FileMode.Create);
            if (trainerMode_checkBox.IsChecked.Value)
            {
                //var trainerFName = fileName + "_trainer" + "_" + index++ + ".dat";
                //string content = "";
                //foreach (var item in InkCanvas.Strokes)
                //{
                //    foreach (var item2 in item.StylusPoints)
                //    {
                //        content += "("+item2.X+","+item2.Y+"),("+item2.PressureFactor+")" + "\n";
                //    }
                //    content += "\n/////////\n";
                //}
                //content += "!!!!!";
                //File.WriteAllText(trainerFName, content, Encoding.UTF8);
            }
            else
            {
                InkCanvas.Strokes.Save(fs);
            }
            Dataset.Instance.Save(nFileName, InkCanvas.Strokes);
            InkCanvas.Strokes.Clear();
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

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var drawingAttrib = new System.Windows.Ink.DrawingAttributes();
            drawingAttrib.Color = colorPicker.SelectedColor ?? default(Color);
            drawingAttrib.Width = size_Slider.Value;
            InkCanvas.DefaultDrawingAttributes = drawingAttrib;
        }

        private void clear_Button_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.Strokes.Clear();
        }
    }
}
