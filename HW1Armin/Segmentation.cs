using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Ink;

namespace HW1Armin
{
    public class Segmentation
    {
        private List<StrokeStruct> strokes = new List<StrokeStruct>();
        private List<StrokeStruct> segmentStrokes = new List<StrokeStruct>();
        private List<StrokeStruct> temp = new List<StrokeStruct>();
        private Timer _timer = new Timer(1500);

        public event EventHandler DoRecognition;

        public Segmentation()
        {
            _timer.Elapsed += TimePassed;
        }

        private void TimePassed(object sender, ElapsedEventArgs e)
        {
            segmentStrokes.Clear();
            segmentStrokes.AddRange(temp);
            temp.Clear();
            _timer.Stop();
            DoRecognition(sender, e);
        }

        public System.Collections.Generic.List<StrokeStruct> GetRawStrokes()
        {
            return strokes;
        }

        public void Feed(Stroke stroke)
        {
            _timer.Stop();
            _timer.Start();
            strokes.Add(new StrokeStruct(stroke));
            temp.Add(new StrokeStruct(stroke));
        }

        internal StrokeCollection GetStrokes()
        {
            StrokeCollection collection = new StrokeCollection();
            foreach (var item in segmentStrokes)
            {
                collection.Add(item.Stroke);
            }
            return collection;
        }
    }

    public struct StrokeStruct
    {
        private Stroke stroke;
        private long timeStamp;

        public StrokeStruct(Stroke stroke)
        {
            this.stroke = stroke;
            this.timeStamp = Util.CurrentTimeMillis();
        }

        public Stroke Stroke
        {
            get
            {
                return stroke;
            }

            set
            {
                stroke = value;
            }
        }

        public long TimeStamp
        {
            get
            {
                return timeStamp;
            }

            set
            {
                timeStamp = value;
            }
        }
    }
}