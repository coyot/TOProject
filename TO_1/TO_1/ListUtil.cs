using System.Collections.Generic;

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
    }
}
