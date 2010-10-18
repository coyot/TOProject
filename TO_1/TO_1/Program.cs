using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TspImplementationPart1
{
    class Program
    {

        static void Main(string[] args)
        {
            var instance = new TspInstance();
            string fileName = args[0];
            if (!File.Exists(fileName))
            {
                Console.WriteLine("{0} does not exist.", fileName);
                return;
            }
            using (StreamReader sr = File.OpenText(fileName))
            {
                String input;
                while ((input = sr.ReadLine()) != null)
                {
                    instance.AddPoint(input.Split(';'));
                }
                Console.WriteLine("The end of the stream has been reached.");

                instance.Calculate();
                Console.ReadLine();
            }

        }
    }

    public class TspInstance
    {
        CenterOfMass centerOfMass;
        IList<Point> allPoints;
        int numberOfPoints;

        Group[] groups = new Group[4];

        public TspInstance()
        {
            allPoints = new List<Point>();
            numberOfPoints = 0;
        }

        public void AddPoint(string[] input)
        {
            allPoints.Add(new Point(input));
            numberOfPoints++;
            centerOfMass = new CenterOfMass();
        }

        public void Calculate()
        {
            CreateGroups();
        }

        private void CreateGroups()
        {
            Point point;
            Random rand = new Random();
            int pos = rand.Next(numberOfPoints);
            groups[0] = new Group();
            groups[0].points.Add(allPoints[pos]);
            centerOfMass.AddPoint(allPoints[pos]);
            allPoints.RemoveAt(pos);

            for (var groupIndex = 1; groupIndex < 4; groupIndex++)
            {
                groups[groupIndex] = new Group();
                point = allPoints.OrderBy(p => p.Distance(centerOfMass)).LastOrDefault();
                groups[groupIndex].AddPoint(point);
                centerOfMass.AddPoint(point);
                allPoints.Remove(point);
            }

            while (allPoints.Any())
            {
                // 4 | allPoints.Count()
                for (int groupIndex = 0; groupIndex < 4; groupIndex++)
                {
                    point = allPoints.OrderBy(p => p.Distance(groups[groupIndex].centerOfMass)).FirstOrDefault();
                    groups[groupIndex].AddPoint(point);
                    allPoints.Remove(point);
                }
            }
        }
    }

    public class Point
    {
        public int x;
        public int y;
        public int id;
        public Point(){}
        public Point(string[] input)
        {
            x = int.Parse(input[1]);
            y = int.Parse(input[2]);
            id = int.Parse(input[0]);
        }

        public int Distance(Point other)
        {
            return (int)Math.Round(Math.Sqrt(this.x * other.x + this.y * other.y), 0);
        }

        [Obsolete]
        public Point GetMassCenter(IList<Point> pointsList)
        {
            return null;
        }
    }

    public class Group
    {
        public CenterOfMass centerOfMass;
        public IList<Point> points;
        public Group()
        {
            points = new List<Point>();
            centerOfMass = new CenterOfMass();
        }

        public void AddPoint(Point point)
        {
            this.points.Add(point);
            centerOfMass.AddPoint(point);
        }
    }

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
            y += (point.x - y) / weight;            
        }
    }
}
