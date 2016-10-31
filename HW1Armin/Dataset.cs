using HW1Armin.HW3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace HW1Armin
{
    class Dataset
    {
        public static Dataset Instance = new Dataset();
        public Hashtable dataSet = new Hashtable();
        public Hashtable _NDOLLARDataset = new Hashtable();
        public Hashtable _PennyPincherDataset = new Hashtable();
        private Dataset()
        {

        }

        private void Save(StrokeCollection strokes)
        {
            dataSet.Add("",strokes);
        }

        Random rand = new Random(100);
        public void Save(string path, StrokeCollection strokeCollection)
        {
            while (true)
            {
                var name = GetName(path);
                name += "_";
                if (_NDOLLARDataset.ContainsKey(name))
                    name += rand.Next(100);
                try
                {
                    _NDOLLARDataset.Add(name, GetMultiStroke(name, strokeCollection));
                    dataSet.Add(name,strokeCollection);
                    _PennyPincherDataset.Add(name,GetPennyStroke(name, strokeCollection));
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
        

        private Multistroke GetMultiStroke(string name, StrokeCollection strokeCollection)
        {
            List<List<PointR>> pointCollection = new List<List<PointR>>();
            foreach (var stroke in strokeCollection)
            {
                List<PointR> points = new List<PointR>();
                foreach (var point in stroke.StylusPoints)
                {
                    points.Add(new PointR(point.X, point.Y));
                }
                pointCollection.Add(points);
            }
            Multistroke mStroke = new Multistroke(name,"test","test",pointCollection);
            return mStroke;
        }

        private PennyGesture GetPennyStroke(string name, StrokeCollection strokes)
        {
            PennyGesture gest = new PennyGesture();
            List<PointR> points = new List<PointR>();
            foreach (var pts in strokes)
            {
                points.AddRange(GetPointRs(pts));
            }
            gest.name = name;
            gest.Points = PennyPincher.pennyPincherResample(points);
            return gest;
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

        private string GetName(string path)
        {
            var fileName = ParseName(path);

            return fileName.Contains("_") ? fileName.Substring(0,fileName.IndexOf("_")):fileName;
        }

        public static string ParseName(string filename)
        {
            int start = filename.LastIndexOf('\\');
            int end = filename.LastIndexOf('.');
            return filename.Substring(start + 1, end - start - 1);
        }

        public Hashtable GetSampleCount(int count = 1)
        {
            Hashtable table = new Hashtable();
            Dictionary<string, int> addedKeys = new Dictionary<string, int>();
            
            foreach (string item in _NDOLLARDataset.Keys)
            {
                var key = item.Substring(0,item.IndexOf("_"));
                if(!addedKeys.ContainsKey(key.ToString()))
                {
                    addedKeys.Add(key, count);
                }
                if (addedKeys[key] != 0)
                {
                    table.Add(item, _NDOLLARDataset[item]);
                    addedKeys[key]--;
                }
            }
            return table;
        }

        public Hashtable GetPennyPincherSampleCount(int count = 1)
        {
            Hashtable table = new Hashtable();
            Dictionary<string, int> addedKeys = new Dictionary<string, int>();

            foreach (string item in _PennyPincherDataset.Keys)
            {
                var key = item.Substring(0, item.IndexOf("_"));
                if (!addedKeys.ContainsKey(key.ToString()))
                {
                    addedKeys.Add(key, count);
                }
                if (addedKeys[key] != 0)
                {
                    table.Add(item, _PennyPincherDataset[item]);
                    addedKeys[key]--;
                }
            }
            return table;
        }
    }
}
