using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;

namespace HW1Armin
{
    public class Symbol : IComparable<Symbol>
    {
        public bool isOprator;
        public string name;
        public bool isFractionBar;
        public bool isSqrt;
        public bool isSeen;
        public bool isEmpty;
        public StrokeCollection strokes;
        public Rect boundingBox;
        private bool isParanthesis;
        private bool isNumber;

        public Symbol()
        {
            this.isEmpty = true;
        }

        public Symbol(string name, StrokeCollection strokes)
        {
            this.name = name;
            this.strokes = strokes;
            this.isSeen = false;
            this.isFractionBar = false;
            this.isEmpty = false;
            this.isParanthesis = false;
            this.isNumber = false;

            //if (name == "times" || name == "star") this.name = "*";

            if (name == "plus" 
                || name == "minus" 
                || name == "times" 
                || name == "star" 
                || name == "divide" 
                || name == "sqrt" 
                || name == "equals"
                || name == "+" 
                || name == "-" 
                || name == "*" 
                || name == "/" 
                || name == "√" 
                || name == "=")
            {
                isOprator = true;
            }

            if (char.IsNumber(name[0])) this.isNumber = true;

            if (name == "sqrt") this.isSqrt = true;

            if (name == "(" || name == ")") this.isParanthesis = true;
            
            this.boundingBox = getBoundBox(strokes);
        }

        public int CompareTo(Symbol other)
        {
            if (this.boundingBox.X < other.boundingBox.X) return -1;
            else if (this.boundingBox.X == other.boundingBox.X) return 0;
            else return 1;
        }

        public static Rect getBoundBox(StrokeCollection strokes)
        {

            double minX = strokes[0].StylusPoints[0].X;
            double minY = strokes[0].StylusPoints[0].Y;

            double maxX = strokes[0].StylusPoints[0].X;
            double maxY = strokes[0].StylusPoints[0].Y;
            for (int j = 0; j < strokes.Count; j++)
            {
                Stroke stroke = strokes[j];
                for (int i = 0; i < stroke.StylusPoints.Count; i++)
                {
                    StylusPoint p = stroke.StylusPoints[i];
                    if (p.X > maxX) maxX = p.X;
                    if (p.X < minX) minX = p.X;
                    if (p.Y > maxY) maxY = p.Y;
                    if (p.Y < minY) minY = p.Y;
                }
            }
            return new Rect(new Point(minX, minY), new Point(maxX, maxY));

        }
    }
}