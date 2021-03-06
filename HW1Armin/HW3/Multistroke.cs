﻿using HW1Armin.HW3;
using System;
using System.Collections.Generic;

namespace HW1Armin
{
    public class Multistroke : IComparable
    {
        public string Name;
        public string User;
        public string Speed;
        public int NumStrokes = -1; // how many strokes this multistroke has
        public List<Gesture> Gestures;  // all possible orderings/directions of this multistroke gesture
        public Gesture OriginalGesture; // the original gesture used to instantiate this Multistroke

        public Multistroke()
        {
            this.Name = String.Empty;
            this.User = String.Empty;
            this.Speed = String.Empty;
            this.Gestures = null;
            this.OriginalGesture = null;
        }

        // when a new Multistroke is made, it handles pre-processing the points given
        // so that all possible orderings and directions of the points are handled.
        // this allows $N to receive 1 template for a multistroke gesture such as "="
        // without limiting future recognition to users writing that template with the
        // strokes in the same order and the same direction.
        public Multistroke(string name, string user, string spd, List<List<PointR>> strokes)
        {
            this.Name = name;
            this.User = user;
            this.Speed = spd;

            // Lisa 8/8/2009
            // combine the strokes into one unistroke gesture to save the original gesture
            List<PointR> points = new List<PointR>();
            foreach (List<PointR> pts in strokes)
            {
                points.AddRange(pts);
            }
            this.OriginalGesture = new Gesture(name, points);

            this.NumStrokes = strokes.Count;

            // if it's a unistroke and we are trying to emulate $1, don't process; Lisa 8/16/2009
            if (!true && this.NumStrokes == 1)
            {
                this.Gestures = new List<Gesture>(1);
                this.Gestures.Add(this.OriginalGesture);
            }
            else
            {
                // Computes all possible stroke orderings/stroke direction combinations of the
                List<int> defaultOrder = new List<int>(strokes.Count); // array of integer indices
                for (int i = 0; i < strokes.Count; i++)
                {
                    defaultOrder.Add(i); // initialize
                }

                List<List<int>> allOrderings = new List<List<int>>();
                // HeapPermute operates on the indices
                HeapPermute(this.NumStrokes, defaultOrder, allOrderings);

                List<List<PointR>> unistrokes = MakeUnistrokes(strokes, allOrderings);

                this.Gestures = new List<Gesture>(unistrokes.Count);
                foreach (List<PointR> entry in unistrokes)
                {
                    Gesture newG = new Gesture(this.Name, entry);
                    this.Gestures.Add(newG);
                }
            }
        }

        // sorts in descending order of Score
        public int CompareTo(object obj)
        {
            if (obj is Multistroke)
            {
                Multistroke ms = (Multistroke)obj;
                return Name.CompareTo(ms.Name);
            }
            else throw new ArgumentException("object is not a Multistroke");
        }
        
        public void HeapPermute(int n, List<int> currentOrder, List<List<int>> allOrders)
        {
            if (n == 1) // base case
            {
                // build return value to be an ArrayList containing 1 ArrayList (strokes) of ArrayLists (points)
                allOrders.Add(new List<int>(currentOrder)); // copy
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    // recurse here, building up set of lists
                    HeapPermute(n - 1, currentOrder, allOrders);
                    if ((n % 2) == 1)           // odd n
                    {
                        SwapStrokes(0, n - 1, currentOrder);
                    }
                    else                        // even n
                    {
                        SwapStrokes(i, n - 1, currentOrder);
                    }
                }
            }
        }

        // swap the strokes given by the indices "first" and "second" in the
        // "order" argument; this DOES change the ArrayList sent as an argument.
        private void SwapStrokes(int first, int second, List<int> order)
        {
            int temp = order[first];
            order[first] = order[second];
            order[second] = temp;
        }

        // now swap stroke directions within all possible permutations
        // this can be done by treating the strokes as binary variables (F=0, B=1)
        // therefore, for each ordering, iterate 2^(num strokes) and extract bits of 
        // that # to determine which stroke is forward and which is backward
        // allOrderings has indices in it
        public List<List<PointR>> MakeUnistrokes(List<List<PointR>> originalStrokes, List<List<int>> allOrderings)
        {
            List<List<PointR>> allUnistrokes = new List<List<PointR>>(); // will contain all possible orderings/direction enumerations of this gesture
            foreach (List<int> ordering in allOrderings)
            {
                for (int b = 0; b < Math.Pow(2d, ordering.Count); b++) // decimal value b
                {
                    List<PointR> unistroke = new List<PointR>(); // we're building a unistroke instead of multistroke now for ease of processing
                    for (int i = 0; i < ordering.Count; i++) // examine b's bits
                    {
                        // copy the correct unistroke
                        List<PointR> stroke = new List<PointR>(originalStrokes[(int)ordering[i]]);
                        if (((b >> i) & 1) == 1) // if (BitAt(b, i) == 1), i.e., is b's bit at index i on?
                        {
                            stroke.Reverse(); // reverse the strokes
                        }
                        unistroke.AddRange(stroke); // add stroke to current strokePermute
                    }
                    // add completed strokePermute to set of strokePermutes (aka Multistrokes)
                    allUnistrokes.Add(unistroke);
                }
            }
            return allUnistrokes;
        }

    }
}