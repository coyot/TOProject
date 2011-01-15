using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

namespace TO_1
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Count() != 3)
            {
                Console.WriteLine("Wrong number of arguments!");
                Console.WriteLine("Usage: prog.exe [input_file] [sol_path] [res_path]");
                return;
            }

            TspInstanceConstants.SOL_FILE_PATH = args[1];
            TspInstanceConstants.RES_FILE_PATH = args[2];

            for (var i = 0; i < 2; i++)
            {
                var tspInstance = new TspInstance();
                var tspInstance2 = new TspInstance();
                var fileName = args[0];

                if (!File.Exists(fileName))
                {
                    Console.WriteLine("{0} does not exist.", fileName);
                    return;
                }
                var k = 0;
                using (var sr = File.OpenText(fileName))
                {
                    String input;
                    while ((input = sr.ReadLine()) != null)
                    {
                        tspInstance.AddPoint(input.Split(';'));
                        tspInstance2.AddPoint(input.Split(';'));
                        k++;
                    }
                }

                TspInstanceConstants.NUMBER_OF_POINTS_PER_GROUP = k / 4;

                tspInstance2.CalculateHea();
            }
        }

    }







}
