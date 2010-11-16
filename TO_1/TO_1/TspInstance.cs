using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace TO_1
{
    public class TspInstance
    {
        byte firstToBeMoved;
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
            firstToBeMoved = 1;
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
            StreamWriter presol = new StreamWriter(@"D:\presol.txt");

            CalculateGroups();

            for (byte i = 0; i < 4; i++)
            {
                distance[i] = 0;
                for (int k = 1; k < numberOfPoints / 4; k++)
                {
                    distance[i] += pointsDict[i][k - 1].Distance(pointsDict[i][k]);
                }
            }
            for (byte i = 0; i < 4; i++)
            {
                distance[i] += pointsDict[i].First().Distance(pointsDict[i].Last());
            }
            WriteSolution(presol);

            res.WriteLine(distance.Sum(p => p).ToString());
            CalculateLocalSearch();


            for (byte i = 0; i < 4; i++)
            {
                distance[i] = 0;
                for (int k = 1; k < numberOfPoints / 4; k++)
                {
                    distance[i] += pointsDict[i][k-1].Distance(pointsDict[i][k]);
                }
            }
            for (byte i = 0; i < 4; i++)
            {
                distance[i] += pointsDict[i].First().Distance(pointsDict[i].Last());
            }

            WriteSolution(sol);
            res.WriteLine(distance.Sum(p => p).ToString());
            sol.Close();
            res.Close();
            presol.Close();
        }

        private void CalculateLocalSearch()
        {
            List<Point> pointsToBeMoved;
            IList<Path> paths;
            IList<Path> target;
            GeneratePointsToBeMoved(out pointsToBeMoved, out paths, out target);


            while (true)
            {
                bool continueLS = false;
                for (int i = 0; i <3; i++)
                {
                    GeneratePointsToBeMoved(out pointsToBeMoved, out paths, out target);
                    long distance = paths.Sum(p => p.Distance);

                    paths = FindBestAllocation(paths, pointsToBeMoved, target, null, 0, 2);
                    int pathsSum = paths.Sum(p => p.Distance);

                    if (pathsSum < distance)
                    {
                        continueLS = true;
                        foreach (var path in paths)
                        {
                            byte currentGroup = path.points.First.Value.groupId;
                            Point p = path.points.First.Value;
                            int pos = pointsDict[currentGroup].IndexOf(p);

                            for (int l = 1; l < path.points.Count - 1; l++)
                            {
                                path.points.ElementAt(l).groupId = currentGroup;
                                pointsDict[currentGroup][(l + pos ) % 25] = path.points.ElementAt(l);
                            }
                        }
                    }
                }
                if (!continueLS)
                    break;
            }
            ;

        }

        private void GeneratePointsToBeMoved(out List<Point> pointsToBeMoved, out IList<Path> paths, out IList<Path> target)
        {
            int k = 9;

            Random r = new Random();
            int id = r.Next(numberOfPoints);
            //id = (firstToBeMoved++)%100;
            pointsToBeMoved = new List<Point>();
            var allPoints = CreateAllPoints();

            //pointsDict.ke
            Point firstToMove = allPoints[id];
            pointsToBeMoved.Add(firstToMove);
            pointsToBeMoved.AddRange(allPoints.Except(pointsToBeMoved).OrderBy(p => p.Distance(firstToMove)).Take(k - 1));

            paths = new List<Path>();
            target = new List<Path>();
            var pointsFromJoinedPaths = new List<Point>();
            foreach (var item in pointsToBeMoved)
            {
                bool added = false;
                foreach (var path in paths)
                {
                    if (path.points.First().id == item.id)
                    {
                        int pos = pointsDict[item.groupId].IndexOf(item);
                        if (pos == 0)
                            pos = 25;
                        path.points.AddFirst(pointsDict[item.groupId][pos - 1]);
                        added = true;
                    }
                    else if (path.points.Last().id == item.id)
                    {
                        int pos = pointsDict[item.groupId].IndexOf(item);
                        if (pos == 24)
                            pos = -1;
                        path.points.AddLast(pointsDict[item.groupId][pos + 1]);
                        added = true;
                    } else if (path.points.Contains(item))
                    {
                        added = true;
                    }
                }
                if (!added)
                {

                    Path newPath = new Path();
                    int prev = pointsDict[item.groupId].IndexOf(item);
                    int next = prev;
                    if (prev == 0)
                        prev = 25;

                    newPath.points.AddFirst(pointsDict[item.groupId][prev - 1]);
                    newPath.points.AddLast(item);

                    if (next == 24)
                        next = -1;
                    newPath.points.AddLast(pointsDict[item.groupId][next + 1]);
                    paths.Add(newPath);
                }
                while (true)
                {
                    Path toRemove = null;
                    foreach (var path in paths)
                    {
                        foreach (var innerPath in paths)
                            if (path.points.Last.Value.id == innerPath.points.First.Value.id)
                            {
                                pointsFromJoinedPaths.Add(path.points.Last.Value);
                                k++;
                                foreach (var point in innerPath.points.Skip(1))
                                {
                                    path.points.AddLast(point);
                                }
                                toRemove = innerPath;
                            }
                        if (toRemove != null)
                            break;
                    }

                    if (toRemove == null)
                        break;
                    else
                        paths.Remove(toRemove);

                }


            }


            foreach (var path in paths)
            {
                if (pointsToBeMoved.Contains(path.points.First.Value) || pointsToBeMoved.Contains(path.points.Last.Value))
                    throw new NotImplementedException();
            }

            pointsToBeMoved.AddRange(pointsFromJoinedPaths);




            foreach (var path in paths)
                target.Add(new Path(path.points.First.Value));

            //setting groupId for all points
            for (byte i = 0; i < 4; i++)
            {
                foreach (var point in pointsDict[i])
                {
                    point.groupId = i;
                }
            }
        }

        private IList<Path> FindBestAllocation(IList<Path> paths, List<Point> points, IList<Path> target, Point p, int pathNr, int posNr)
        {
            if (p != null)
            {
                if (target[pathNr].points.First.Value.id == p.id || target[pathNr].points.Last.Value.id == p.id)
                    throw new NotImplementedException("dupa");

                if (target[pathNr].points.Count >= posNr + 1)
                {
                    LinkedListNode<Point> point = target[pathNr].points.Find(target[pathNr].points.ElementAt(posNr)).Previous;

                    target[pathNr].points.AddAfter(point, p);
                    target[pathNr].points.Remove(target[pathNr].points.ElementAt(posNr - 1));
                }
                else if (paths[pathNr].points.Count > posNr)
                {
                    if (target[pathNr].points.Last.Value.id == p.id)
                        throw new NotImplementedException("dupa");
                    target[pathNr].points.AddLast(p);
                }

                if (paths[pathNr].points.Count == posNr + 1)
                {
                    if (paths[pathNr].points.Count > target[pathNr].points.Count)
                        target[pathNr].points.AddLast(paths[pathNr].points.Last.Value);
                    pathNr++;
                    posNr = 2;
                }
                else
                    posNr++;
            }
            //else
            //{
            //    target = new List<Path>();
            //    for (int i = 0; i < paths.Count; i++)
            //    {
            //        target.Add(new Path());
            //        target[i].points.AddFirst(paths[i].points.First.Value);
            //    }
            //}

            if (points.Any())
                foreach (Point item in points)
                {
                    var tmp = points.Except<Point>(new List<Point>() { item }).ToList();
                    var tmpResult = FindBestAllocation(paths, tmp, target, item, pathNr, posNr);
                    if (ComparePaths(paths, tmpResult))
                        paths = tmpResult;
                }
            else
            {
                //foreach (var item in target.First().points)
                //{
                //    Debug.Write(item.id + " -> ");
                //}
                //Debug.WriteLine(' ');
                paths = target;

            }
            return paths;
        }

        private bool ComparePaths(IList<Path> toRet, IList<Path> tmpResult)
        {
            return (toRet.Sum(p => p.Distance) > tmpResult.Sum(p => p.Distance));
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
            //int pos = rand.Next(numberOfPoints);
            int pos = 43;
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
                //pos = 0;
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
                    distance[i] += pointsDict[i][k-1].Distance(pointsDict[i][k]);
                }
            }

            for (byte i = 0; i < 4; i++)
            {
                distance[i] += pointsDict[i].First().Distance(pointsDict[i].Last());
            }
        }


    }
}
