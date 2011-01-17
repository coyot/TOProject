using System.Collections.Generic;
using System.Linq;

namespace TO_1
{
    public class Path
    {
        //byte groupId;
        public LinkedList<Point> points;
        public int Distance 
        { 
            get 
            {
                //if (_distance == 0)
                    return CalculateDistace();
                //else
                //    return _distance;
            }
            set { _distance = value; } 
        }

        private int CalculateDistace()
        {
            int toRet = 0;
            for (int i = 1; i < points.Count; i++)
            {
                toRet += points.ElementAt(i - 1).Distance(points.ElementAt(i));
            }
            return toRet;
        }
        public int _distance;
        public Path()
        { 
            points = new LinkedList<Point>();
            _distance = 0;
        }

        public Path(Point point)
        {
            points = new LinkedList<Point>();
            _distance = 0;
            this.points.AddFirst(point);
        }

        public Path(Path path)
        {
            points = new LinkedList<Point>();
            _distance = 0;
            foreach (var item in path.points)
            {
                this.points.AddLast(item);
            }
            
        }
    }
}
