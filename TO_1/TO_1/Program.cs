using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TO_1
{
    class Program
    {

        static void Main(string[] args)
        {
            var instance = new TspInstance();
            string fileName = args[0];
            if (!File.Exists(fileName))
            {
                Console.WriteLine("{0} does not exist.", fileName);
                return;
            }
            using (StreamReader sr = File.OpenText(fileName))
            {
                String input;
                while ((input = sr.ReadLine()) != null)
                {
                    instance.AddPoint(input.Split(';'));
                }
                Console.WriteLine("The end of the stream has been reached.");
            }

            instance.Calculate();

            

            //Console.ReadLine();

        }
    }

    

    

    

}
