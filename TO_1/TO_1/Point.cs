using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TO_1
{
    public class Point
    {
        public int x;
        public int y;
        public int id;
        public Point() { }
        public Point(string[] input)
        {
            x = int.Parse(input[1]);
            y = int.Parse(input[2]);
            id = int.Parse(input[0]);
        }

        public int Distance(Point other)
        {
            if (other == null) return 0;
            return (int)Math.Round(Math.Sqrt((this.x - other.x) * (this.x - other.x) + (this.y - other.y) * (this.y - other.y)), 0);
        }

        [Obsolete]
        public Point GetMassCenter(IList<Point> pointsList)
        {
            return null;
        }
    }
}
