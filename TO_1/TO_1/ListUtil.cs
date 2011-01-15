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

        public static IList<IList<Point>> Clone(this IList<IList<Point>> source)
        {
            IList<IList<Point>> result = source.Select(list => list.Select(point => point.Clone()).ToList()).Cast<IList<Point>>().ToList();

            return result;
        }
    }
}
