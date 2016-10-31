using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace HW1Armin
{
    internal class MyStrokes : Stroke
    {
        public MyStrokes(StylusPointCollection stylusPoints) : base(stylusPoints)
        {
            this.DrawingAttributes.Color = Color.FromArgb(180, 100, 100, 100);
        }
        public MyStrokes(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes)
            : base(stylusPoints, drawingAttributes)
        {

        }
    }
}