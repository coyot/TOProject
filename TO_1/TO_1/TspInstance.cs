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
            WriteSolution(sol);
            res.WriteLine(distance.Sum(p => p).ToString());
            sol.Close();
            res.Close();
        }

        private void CreateGroups()
        {
            Random rand = new Random();
            int pos = rand.Next(numberOfPoints);
            groups[0] = new Group(0);
            pointsDict[0] = new List<Point>();
            pointsDict[0].Add(allPoints[pos]);
            centerOfMass.AddPoint(allPoints[pos]);
            allPoints.RemoveAt(pos);
            Point point;
            for (byte groupIndex = 1; groupIndex < 4; groupIndex++)
            {
                pointsDict[groupIndex] = new List<Point>();
                groups[groupIndex] = new Group(groupIndex);
                point = allPoints.OrderBy(p => p.Distance(centerOfMass)).Last();
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
                    sol.WriteLine(pointsDict[k][i].id.ToString());
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
                pos = rand.Next(numberOfPoints/4);
                tmpPoint = pointsDict[i][0];
                pointsDict[i][0] = pointsDict[i][pos];
                pointsDict[i][pos] = tmpPoint;

                for (int k = 1; k < numberOfPoints/4; k++)
                {
                    tmpPoint = pointsDict[i].OrderBy(p => p.Distance(pointsDict[i][k-1])).AsParallel().First();
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
