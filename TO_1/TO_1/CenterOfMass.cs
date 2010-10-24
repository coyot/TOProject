using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TO_1
{
    public class CenterOfMass : Point
    {
        public int weight;

        public CenterOfMass()
            : base()
        {
            weight = 0;
        }

        public void AddPoint(Point point)
        {
            weight++;
            x += (point.x - x) / weight;
            y += (point.y - y) / weight;
        }
    }
}
