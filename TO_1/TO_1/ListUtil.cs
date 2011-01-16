using System.Collections.Generic;
using System.Linq;

namespace TO_1
{
    public static class ListUtil
    {
        public static int Distance(this IList<Point> first, IList<Point> second)
        {
            return CalculateCenterOfMass(first).Distance(CalculateCenterOfMass(second));
        }

        public static int Distance(this IList<Point> list, Point point)
        {
            return CalculateCenterOfMass(list).Distance(point);
        }

        public static int Distance(this Point point, IList<Point> list)
        {
            return point.Distance(CalculateCenterOfMass(list));
        }

        private static Point CalculateCenterOfMass(IEnumerable<Point> list)
        {
            var massCenter = new CenterOfMass();

            foreach (var point in list.Where(point => point != null))
            {
                massCenter.AddPoint(point);
            }

            return massCenter;
        }

        /// <summary>
        /// Divide the path into list of points - the divide point is the null entries.
        /// </summary>
        /// <param name="list">List ot be divided</param>
        /// <returns>List of paths</returns>
        public static IList<IList<Point>> Divide(this IList<Point> list)
        {
            var result = new List<IList<Point>>();
            var path = new List<Point>();
            var count = list.Count;
            var index = 0;
            var skipped = new List<Point>();

            while (index < count)
            {
                if (index == 0)
                {
                    if (list[TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP - 1] != null)
                    {
                        while (list[index] != null)
                        {
                            skipped.Add(list[index]);
                            index++;
                        }
                    }
                }

                var t = list[index];
                if (t != null)
                {
                    path.Add(t);
                    if (index == TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP - 1)
                    {
                        // if we have some leftovers from the beggining of the list
                        if (skipped.Count != 0)
                        {
                            path.AddRange(skipped);
                        }
                        result.Add(path);
                        path = new List<Point>();
                    }
                }
                else if (path.Count != 0)
                {
                    result.Add(path);
                    path = new List<Point>();
                }
                index++;
            }

            return result;
        }

        public static IList<IList<Point>> Clone(this IList<IList<Point>> source)
        {
            IList<IList<Point>> result = source.Select(list => list.Select(point => point.Clone()).ToList()).Cast<IList<Point>>().ToList();

            return result;
        }
    }
}
