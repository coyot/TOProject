using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TO_1
{
    public class TspInstance
    {
        CenterOfMass centerOfMass;
        IList<Point> allPoints;
        public int numberOfPoints;
        public IDictionary<byte, IList<Point>> pointsDict;
        public IDictionary<Point, byte> newPointDict;
        int[] distance = new int[4];

        public Group[] groups = new Group[4];

        public TspInstance()
        {
            allPoints = new List<Point>();
            numberOfPoints = 0;
            pointsDict = new Dictionary<byte, IList<Point>>();
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

            StreamWriter res = new StreamWriter("res.txt", true);
            StreamWriter sol = new StreamWriter(@"D:\sol.txt");

            CalculateGroups();
            CalculateLocalSearch();
            WriteSolution(sol);
            res.WriteLine(distance.Sum(p => p).ToString());
            sol.Close();
            res.Close();
        }

        private void CalculateLocalSearch()
        {
            int k = 3;

            Random r = new Random();
            int id = r.Next(numberOfPoints);
            var pointsToBeMoved = new List<Point>();
            var allPoints = CreateAllPoints();

            //pointsDict.ke
            Point firstToMove = allPoints[id];
            pointsToBeMoved.Add(firstToMove);
            pointsToBeMoved.AddRange(allPoints.Except(pointsToBeMoved).OrderBy(p => p.Distance(firstToMove)).Take(k - 1));

            IList<Path> paths = new List<Path>();
            foreach (var item in pointsToBeMoved)
            {
                bool added = false;
                foreach (var path in paths)
                {
                    if(path.points.First().id == item.id)
                    {
                        int pos = allPoints.IndexOf(item);
                        if (pos % 25 == 0)
                            pos += 25;
                        path.points.AddFirst(allPoints[pos - 1]);
                        added = true;
                    }
                    else if(path.points.Last().id == item.id)
                    {
                        int pos = allPoints.IndexOf(item);
                        if (pos % 25 == 24)
                            pos -= 25;
                        path.points.AddFirst(allPoints[pos + 1]);
                        added = true;
                    }
                }
                if (!added)
                {
                    
                    Path newPath = new Path();
                    int pos = allPoints.IndexOf(item);
                    int next = pos + 1;
                    if (pos % 25 == 0)
                        pos += 25;
                    
                    newPath.points.AddFirst(allPoints[pos - 1]);
                    newPath.points.AddLast(item);

                    if (next % 25 == 0)
                        next -= 25;
                    newPath.points.AddFirst(allPoints[next + 1]); 

                    paths.Add(newPath);
                }
            }


        }

        private IList<Point> CreateAllPoints()
        {
            IList<Point> all = new List<Point>();

            foreach (var item in pointsDict.Values)
            {
                foreach (var element in item)
                {
                    all.Add(element);
                }
            }

            return all;
        }

        private void CreateGroups()
        {
            Random rand = new Random();
            int pos = rand.Next(numberOfPoints);
            //pos = 43;
            groups[0] = new Group(0);
            pointsDict[0] = new List<Point>();
            pointsDict[0].Add(allPoints[pos]);
            allPoints[pos].groupId = 0;
            centerOfMass.AddPoint(allPoints[pos]);
            allPoints.RemoveAt(pos);
            Point point;
            for (byte groupIndex = 1; groupIndex < 4; groupIndex++)
            {
                pointsDict[groupIndex] = new List<Point>();
                groups[groupIndex] = new Group(groupIndex);
                point = allPoints.OrderBy(p => p.Distance(centerOfMass)).Last();
                point.groupId = groupIndex;
                int max = allPoints.Max(p => p.Distance(centerOfMass));
                groups[groupIndex].AddPoint(point);
                pointsDict[groupIndex].Add(point);
                centerOfMass.AddPoint(point);
                allPoints.Remove(point);
            }

            while (allPoints.Any())
            {
                // 4 | allPoints.Count()
                for (byte groupIndex = 0; groupIndex < 4; groupIndex++)
                {
                    point = allPoints.OrderBy(p => p.Distance(groups[groupIndex].centerOfMass)).First();
                    point.groupId = groupIndex;
                    groups[groupIndex].AddPoint(point);
                    pointsDict[groupIndex].Add(point);
                    allPoints.Remove(point);
                }
            }
        }

        internal void WriteSolution(StreamWriter sol)
        {
            for (byte k = 0; k < 4; k++)
            {
                int count = pointsDict[k].Count;
                for (int i = 0; i < count; i++)
                {
                    sol.WriteLine(pointsDict[k][i].id.ToString());// + " " + pointsDict[k][i].x.ToString() + " " + pointsDict[k][i].y.ToString() + " dist " + pointsDict[k][i].Distance(pointsDict[k][Math.Max(0,i-1)]));

                }
                sol.WriteLine();
            }
        }

        public void CalculateGroups()
        {
            Point tmpPoint;

            int pos;
            for (byte i = 0; i < 4; i++)
            {
                Random rand = new Random();
                pos = rand.Next(numberOfPoints / 4);
                pos = 0;
                tmpPoint = pointsDict[i][0];
                pointsDict[i][0] = pointsDict[i][pos];
                pointsDict[i][pos] = tmpPoint;

                for (int k = 1; k < numberOfPoints / 4; k++)
                {
                    tmpPoint = pointsDict[i].Skip(k).OrderBy(p => p.Distance(pointsDict[i][k - 1])).First();
                    //TODO optimization
                    pos = pointsDict[i].IndexOf(tmpPoint);
                    pointsDict[i][pos] = pointsDict[i][k];
                    pointsDict[i][k] = tmpPoint;
                    distance[i] += tmpPoint.Distance(pointsDict[i][k]);
                }
            }

            for (byte i = 0; i < 4; i++)
            {
                distance[i] += pointsDict[i].First().Distance(pointsDict[i].Last());
            }
        }


    }
}
