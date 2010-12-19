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

        private static Point CalculateCenterOfMass(IList<Point> list)
        {
            return null;
        }

        public static IList<IList<Point>> Clone(this IList<IList<Point>> source)
        {
            IList<IList<Point>> result = source.Select(list => list.Select(point => point.Clone()).ToList()).Cast<IList<Point>>().ToList();

            return result;
        }
    }
}
