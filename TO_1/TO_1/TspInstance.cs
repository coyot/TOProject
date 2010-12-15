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
        CenterOfMass centerOfMass;
        IList<Point> allPoints;
        public int NumberOfPoints;
        public IDictionary<byte, IList<Point>> pointsDict;
        public IDictionary<Point, byte> newPointDict;
        int[] distance = new int[4];
        private static int LS_REPEAT_VALUE = 30;
        private static int K_VALUE = 4;
        private static bool WRITE_PRE_SOLUTION = false;
        private static byte NUMBER_OF_GROUPS = 4;

        public Group[] groups = new Group[4];

        public TspInstance()
        {
            allPoints = new List<Point>();
            NumberOfPoints = 0;
            pointsDict = new Dictionary<byte, IList<Point>>();
        }

        public void AddPoint(string[] input)
        {
            allPoints.Add(new Point(input));
            NumberOfPoints++;
            centerOfMass = new CenterOfMass();
        }

        /// <summary>
        /// Random calculation - for comparison purposes
        /// </summary>
        public void CalculateRandom()
        {
            var res = new StreamWriter("res_rand.txt", true);
            var sol = new StreamWriter(@"D:\sol_rand.txt");
            var presol = new StreamWriter(@"D:\presol_rand.txt");

            using (new Timer("LocalSearch - with RANDOM groups"))
            {
                CreateRandomGroups();

                if (WRITE_PRE_SOLUTION)
                {
                    CalculateDistance();
                    WriteSolution(presol);
                    res.WriteLine(distance.Sum(p => p).ToString());
                }

                CalculateLocalSearch();
            }
            CalculateDistance();
            WriteSolution(sol);
            res.WriteLine(distance.Sum(p => p).ToString());

            sol.Close();
            res.Close();
            presol.Close();
        }

        public void Calculate()
        {
            var res = new StreamWriter("res.txt", true);
            var sol = new StreamWriter(@"D:\sol.txt");
            var presol = new StreamWriter(@"D:\presol.txt");
            using (new Timer("LocalSearch - with groups production"))
            {
                CreateGroups();
                CalculateGroups();

                if (WRITE_PRE_SOLUTION)
                {
                    CalculateDistance();
                    WriteSolution(presol);
                    res.WriteLine(distance.Sum(p => p).ToString());
                }

                CalculateLocalSearch();
            }
            CalculateDistance();
            WriteSolution(sol);
            res.WriteLine(distance.Sum(p => p).ToString());

            sol.Close();
            res.Close();
            presol.Close();
        }

        private void CalculateDistance()
        {
            for (byte i = 0; i < 4; i++)
            {
                distance[i] = 0;
                for (int k = 1; k < NumberOfPoints / 4; k++)
                {
                    distance[i] += pointsDict[i][k - 1].Distance(pointsDict[i][k]);
                }
            }
            for (byte i = 0; i < 4; i++)
            {
                distance[i] += pointsDict[i].First().Distance(pointsDict[i].Last());
            }
        }

        /// <summary>
        /// To be used in the HEA for recombination procces
        /// </summary>
        /// <param name="list_1">First result</param>
        /// <param name="list_2">Second result</param>
        /// <returns>Recombination of two results passed as arguments</returns>
        private IDictionary<byte, IList<Point>> Recombination(IDictionary<byte, IList<Point>> list_1, IDictionary<byte, IList<Point>> list_2)
        {
            return null;
        }

        private IDictionary<byte, IList<Point>> Mutate(IDictionary<byte, IList<Point>> result, IList<Point> leftPoints)
        {
            var rand = new Random();
            while (leftPoints.Count != 0)
            {
                var groupIndex = (byte)rand.Next(NUMBER_OF_GROUPS);
                // Choose a group where we can add something!s
                while (result[groupIndex].Count >= 25)
                {
                    groupIndex = (byte) rand.Next(NUMBER_OF_GROUPS);
                }

                // where to put the point? (Which empty place should we fill?)
                var skipSteps = rand.Next(NumberOfPoints/NUMBER_OF_GROUPS);
                // simple iterator
                var i = 0;
                // Id on the list
                var putItHereId = -1;

                while (skipSteps >= 0)
                {
                    if (result[groupIndex][i] == null)
                    {
                        skipSteps--;
                        putItHereId = i;
                    }

                    i = (i + 1) % 25;
                }

                // which point?
                var pointIndex = rand.Next(leftPoints.Count);

                result[groupIndex][putItHereId] = leftPoints[pointIndex];
                result[groupIndex][putItHereId].groupId = groupIndex;
                leftPoints.RemoveAt(pointIndex);
            }

            return result;
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
                for (int i = 0; i < LS_REPEAT_VALUE; i++)
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
                                pointsDict[currentGroup][(l + pos) % 25] = path.points.ElementAt(l);
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
            int k = K_VALUE;

            Random r = new Random();
            int id = r.Next(NumberOfPoints);
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
                    }
                    else if (path.points.Contains(item))
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
            return pointsDict.Values.SelectMany(item => item).ToList();
        }

        private void CreateRandomGroups()
        {
            pointsDict[0] = new List<Point>();
            pointsDict[1] = new List<Point>();
            pointsDict[2] = new List<Point>();
            pointsDict[3] = new List<Point>();

            for (var i = 0; i < allPoints.Count; i++)
            {
                allPoints[i].groupId = (byte)(i / 25);
                pointsDict[(byte)(i / 25)].Add(allPoints[i]);
            }

            var changes = 100;
            var rand = new Random();

            while (changes > 0)
            {
                var g1 = (byte)rand.Next(4);
                var g2 = (byte)rand.Next(4);
                var e1 = rand.Next(25);
                var e2 = rand.Next(25);

                var tmp = pointsDict[g1][e1];
                tmp.groupId = g2;

                pointsDict[g1][e1] = pointsDict[g2][e2];
                pointsDict[g1][e1].groupId = g1;
                pointsDict[g2][e2] = tmp;

                changes--;
            }
        }

        private void CreateGroups()
        {
            Random rand = new Random();
            int pos = rand.Next(NumberOfPoints);
            //int pos = 43;
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
                //int max = allPoints.Max(p => p.Distance(centerOfMass));
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
                pos = rand.Next(NumberOfPoints / 4);
                //pos = 0;
                tmpPoint = pointsDict[i][0];
                pointsDict[i][0] = pointsDict[i][pos];
                pointsDict[i][pos] = tmpPoint;

                for (int k = 1; k < NumberOfPoints / 4; k++)
                {
                    tmpPoint = pointsDict[i].Skip(k).OrderBy(p => p.Distance(pointsDict[i][k - 1])).First();
                    //TODO optimization
                    pos = pointsDict[i].IndexOf(tmpPoint);
                    pointsDict[i][pos] = pointsDict[i][k];
                    pointsDict[i][k] = tmpPoint;
                    distance[i] += pointsDict[i][k - 1].Distance(pointsDict[i][k]);
                }
            }

            for (byte i = 0; i < 4; i++)
            {
                distance[i] += pointsDict[i].First().Distance(pointsDict[i].Last());
            }
        }


    }
}
