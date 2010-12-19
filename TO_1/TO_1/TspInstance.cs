using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace TO_1
{
    public class TspInstance
    {
        CenterOfMass centerOfMass;
        IList<Point> allPoints;
        private IList<Point> _allPointConst;
        public int NumberOfPoints;
        public IDictionary<byte, IList<Point>> pointsDict;
        public IDictionary<Point, byte> newPointDict;
        //int[] distance = new int[4];


        public Group[] groups = new Group[4];

        public TspInstance()
        {
            allPoints = new List<Point>();
            _allPointConst = new List<Point>();
            NumberOfPoints = 0;
            pointsDict = new Dictionary<byte, IList<Point>>();
        }

        public void AddPoint(string[] input)
        {
            var point = new Point(input);
            allPoints.Add(point);
            _allPointConst.Add(point);
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

                if (TspInstanceConstants.WRITE_PRE_SOLUTION)
                {
                    WriteSolution(pointsDict, presol);
                    res.WriteLine(pointsDict.Distance(NumberOfPoints));
                }

                CalculateLocalSearch();
            }
            WriteSolution(pointsDict, sol);
            res.WriteLine(pointsDict.Distance(NumberOfPoints));

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

                if (TspInstanceConstants.WRITE_PRE_SOLUTION)
                {
                    WriteSolution(pointsDict, presol);
                    res.WriteLine(pointsDict.Distance(NumberOfPoints));
                }

                CalculateLocalSearch();
            }

            WriteSolution(pointsDict, sol);
            res.WriteLine(pointsDict.Distance(NumberOfPoints));

            sol.Close();
            res.Close();
            presol.Close();
        }

        public IDictionary<byte, IList<Point>> CalculateHeuristicEvolutionaryAlgorithm()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            // we need to add starting results!! 
            IList<IDictionary<byte, IList<Point>>> results = null;

            while (stopwatch.ElapsedMilliseconds < TspInstanceConstants.HeaRunTime)
            {
                var mutated = new List<IDictionary<byte, IList<Point>>>();

                for (var k = 0; k < TspInstanceConstants.HeaNumberOfRecombinations; k++)
                {
                    // recombinate two chosen results!;->
                    var firstIndex = 0;
                    var secondIndex = 0;

                    // draw two results to be recombinated
                    while (firstIndex == secondIndex)
                    {
                        var random = new Random();
                        firstIndex = random.Next(TspInstanceConstants.HeaPopulationSize);
                        secondIndex = random.Next(TspInstanceConstants.HeaPopulationSize);
                    }

                    IList<Point> left;
                    var commonPaths = Recombination(results[firstIndex], results[secondIndex], out left);
                    var preMutated = new List<IDictionary<byte, IList<Point>>>();

                    // Prepare start resolution...
                    for (var j = 0; j < TspInstanceConstants.HeaNumberOfPreMutations; j++)
                    {
                        preMutated.Add(PrepareForMutationByChance(commonPaths));
                    }

                    // now we mutate!!
                    foreach (var preMutation in preMutated)
                    {
                        for (var j = 0; j < TspInstanceConstants.HeaNumberOfMutations; j++)
                        {
                            mutated.Add(Mutate(preMutation, left.Select(p => p).ToList()));
                        }
                    }
                }

                foreach (var mutation in mutated)
                {
                    // find the local optimum for each of evolved restult
                    // CalculateLocalSearch(mutation);
                }

                // here we should run the LS on each of the results!
                results = results.Concat(mutated)
                                 .OrderBy(r => r.Distance(NumberOfPoints))
                                 .Take(TspInstanceConstants.HeaPopulationSize)
                                 .ToList();
            }
            stopwatch.Stop();

            return results.OrderBy(r => r.Distance(NumberOfPoints)).First();
        }

        /// <summary>
        /// To be used in the HEA for recombination procces
        /// </summary>
        /// <param name="outerResult">First result</param>
        /// <param name="innerResult">Second result</param>
        /// <param name="leftPoints">Which point are left - we will add them later, when Mutation will happen</param>
        /// <returns>Recombination of two results passed as arguments</returns>
        private IList<IList<Point>> Recombination(IDictionary<byte, IList<Point>> outerResult, IDictionary<byte, IList<Point>> innerResult, out IList<Point> leftPoints)
        {
            IList<IList<Point>> result = new List<IList<Point>>();
            byte outerGroupIndex = 0;
            var outerPointIndex = 0;
            leftPoints = _allPointConst.Select(p => p).ToList();

            while (outerGroupIndex < TspInstanceConstants.NUMBER_OF_GROUPS)
            {
                while (outerPointIndex < TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP)
                {
                    byte innerGroupIndex = 0;
                    var outerPoint = outerResult[outerGroupIndex][outerPointIndex];

                    while (innerResult[innerGroupIndex].Contains(outerPoint) == false)
                    {
                        innerGroupIndex++;
                        if (innerGroupIndex == TspInstanceConstants.NUMBER_OF_GROUPS)
                        {
                            throw new AmbiguousMatchException("Nie znaleziono punktu!?:O");
                        }
                    }
                    // we know in which group is our point.
                    var innerPointIndex = innerResult[innerGroupIndex].IndexOf(outerPoint);
                    var outerNextPointIndex = (outerPointIndex + 1) % TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP;
                    var innerNextPointIndex = (innerPointIndex + 1) % TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP;

                    // did we find similar path in two results?
                    if (outerResult[outerGroupIndex][outerNextPointIndex] == innerResult[innerGroupIndex][innerNextPointIndex])
                    {
                        var tmp = new List<Point>
                        { 
                            outerResult[outerGroupIndex][outerPointIndex]
                        };

                        // Remove that point from ALL POINTS LIST - we have used this point, so it is not "free"
                        leftPoints.Remove(outerResult[outerGroupIndex][outerPointIndex]);

                        while (outerResult[outerGroupIndex][outerNextPointIndex] == innerResult[innerGroupIndex][innerNextPointIndex])
                        {
                            tmp.Add(outerResult[outerGroupIndex][outerNextPointIndex]);

                            // Remove that point from ALL POINTS LIST - we have used this point, so it is not "free"
                            leftPoints.Remove(outerResult[outerGroupIndex][outerNextPointIndex]);

                            // skip points which we have already added
                            outerPointIndex++;

                            // moove to the next point
                            outerNextPointIndex = (outerPointIndex + 1) % TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP;
                            innerNextPointIndex = (innerPointIndex + 1) % TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP;
                        }

                        result.Add(tmp);
                    }

                    outerPointIndex++;
                }

                // reset indexes!
                outerGroupIndex++;
                outerPointIndex = 0;
            }

            return result;
        }

        /// <summary>
        /// To be used in the HEA algorithm - to build initial resolution based on intersected paths
        /// </summary>
        /// <param name="intersectedPaths">Result from the Recombination proces</param>
        /// <returns>Initial result consisting only of intersected paths - so it means not all points are in the result, yet</returns>
        private static IDictionary<byte, IList<Point>> PrepareForMutationByChance(IList<IList<Point>> intersectedPaths)
        {
            var random = new Random();
            var result = new Dictionary<byte, IList<Point>>();

            for (byte i = 0; i < TspInstanceConstants.NUMBER_OF_GROUPS; i++)
            {
                result.Add(i, new List<Point>(25));
            }

            var listIndex = random.Next(intersectedPaths.Count);
            var startIndex = random.Next(TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP);
            var groupIndex = (byte)random.Next(TspInstanceConstants.NUMBER_OF_GROUPS);

            while (intersectedPaths.Count > 0)
            {
                var choosen = intersectedPaths[listIndex];
                var possible = !choosen.Where((t, i) => result[groupIndex][startIndex + i] != null).Any();

                if (possible)
                {
                    for (var i = 0; i < choosen.Count; i++)
                    {
                        result[groupIndex][startIndex + i] = choosen[i];
                    }
                    intersectedPaths.Remove(choosen);
                }

                if (intersectedPaths.Count > 0)
                {
                    listIndex = random.Next(intersectedPaths.Count);
                    startIndex = random.Next(TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP);
                    groupIndex = (byte)random.Next(TspInstanceConstants.NUMBER_OF_GROUPS);
                }
            }

            return result;
        }

        /// <summary>
        /// Mutation proces - build the result from the initial result based on intersected paths
        /// and left points. This is done by the nondeterministic matter.
        /// </summary>
        /// <param name="result">Initial result from the PrepareForMutationByChance</param>
        /// <param name="leftPoints">Points which were not in the intersected paths</param>
        /// <returns>Result of the mutation proces</returns>
        private IDictionary<byte, IList<Point>> Mutate(IDictionary<byte, IList<Point>> result, IList<Point> leftPoints)
        {
            while (leftPoints.Count > 0)
            {
                var random = new Random();
                var groupIndex = (byte)random.Next(TspInstanceConstants.NUMBER_OF_GROUPS);
                // Choose a group where we can add something!
                while (result[groupIndex].Count >= 25)
                {
                    groupIndex = (byte)random.Next(TspInstanceConstants.NUMBER_OF_GROUPS);
                }

                // where to put the point? (Which empty place should we fill?)
                var skipSteps = random.Next(NumberOfPoints / TspInstanceConstants.NUMBER_OF_GROUPS);
                // simple iterator
                var i = 0;
                // Id on the list where we will put choosen point
                var putItHereIndex = -1;

                while (skipSteps >= 0)
                {
                    // empty place - we count it in..
                    if (result[groupIndex][i] == null)
                    {
                        skipSteps--;
                        putItHereIndex = i;
                    }

                    // NEEEXT!
                    i = (i + 1) % 25;
                }

                // which point?
                var pointIndex = random.Next(leftPoints.Count);

                result[groupIndex][putItHereIndex] = leftPoints[pointIndex];
                result[groupIndex][putItHereIndex].groupId = groupIndex;
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
                for (int i = 0; i < TspInstanceConstants.LS_REPEAT_VALUE; i++)
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
            int k = TspInstanceConstants.K_VALUE;

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

        internal void WriteSolution(IDictionary<byte, IList<Point>> result, StreamWriter sol)
        {
            for (byte k = 0; k < 4; k++)
            {
                int count = result[k].Count;
                for (var i = 0; i < count; i++)
                {
                    sol.WriteLine(result[k][i].id.ToString());// + " " + pointsDict[k][i].x.ToString() + " " + pointsDict[k][i].y.ToString() + " dist " + pointsDict[k][i].Distance(pointsDict[k][Math.Max(0,i-1)]));

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
                    //distance[i] += pointsDict[i][k - 1].Distance(pointsDict[i][k]);
                }
            }

            //for (byte i = 0; i < 4; i++)
            //{
            //    distance[i] += pointsDict[i].First().Distance(pointsDict[i].Last());
            //}
        }


    }
}
