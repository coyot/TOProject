using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TO_1
{
    public static class DictionaryUtil
    {
        public static int Distance(this IDictionary<byte, IList<Point>> result, int numberOfPoints)
        {
            var distance = new int[4];
            for (byte i = 0; i < 4; i++)
            {
                distance[i] = 0;
                for (var k = 1; k < numberOfPoints / 4; k++)
                {
                    distance[i] += result[i][k - 1].Distance(result[i][k]);
                }
            }
            for (byte i = 0; i < 4; i++)
            {
                distance[i] += result[i].First().Distance(result[i].Last());
            }

            return distance.Sum(p => p);
        }
    }
}
