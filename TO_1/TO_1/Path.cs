using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TO_1
{
    public class Path
    {
        public LinkedList<Point> points;
        public int distance;
        public Path()
        { 
            points = new LinkedList<Point>();
            distance = 0;
        }
    }
}
