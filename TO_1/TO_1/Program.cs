using System;
using System.Linq;
using System.IO;

namespace TO_1
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Count() != 1)
            {
                Console.WriteLine("Wrong number of arguments!");
                Console.WriteLine("Usage: > TO.exe <input file path>");
                return;
            }

            var tspInstance = new TspInstance();
            var fileName = args[0];

            if (!File.Exists(fileName))
            {
                Console.WriteLine("{0} does not exist.", fileName);
                return;
            }
            var k = 0;
            String input;
            using (var sr = File.OpenText(fileName))
            {
                while ((input = sr.ReadLine()) != null)
                {
                    tspInstance.AddPoint(input.Split(';'));
                    k++;
                }
            }

            if (k % 4 != 0)
            {
                Console.WriteLine("Wrong number of nodes -> the condition shown below should be true, but is not!!!");
                Console.WriteLine("\n\tnumberOfNodes % 4 == 0");
                return;
            }

            TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP = k / 4;

            tspInstance.CalculateHea();
        }

    }
}
