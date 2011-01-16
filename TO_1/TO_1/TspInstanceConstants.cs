using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TO_1
{
    public static class TspInstanceConstants
    {
        public const int LS_REPEAT_VALUE = 50;
        public const int K_VALUE = 4;
        public const bool WRITE_PRE_SOLUTION = true;
        public const byte NUMBER_OF_GROUPS = 4;
        public static int NUMBER_OF_POINTS_PER_GROUP;
        public static string RES_FILE_PATH;
        public static string SOL_FILE_PATH;

        public const int NUMBER_OF_MUTATIONS = 10;
        public static int NUMBER_OF_PRE_MUTATIONS = 10;
        public const int NUMBER_OF_HEA_RUNS = 100;

        public const int HeaRunTime = 30000;
        public const int HeaPopulationSize = 150;
        public const int HeaNumberOfMutations = 5;
        public const int HeaNumberOfPreMutations = 5;
        public const int HeaNumberOfRecombinations = 5;
        public const int HeaNumberOfLsOptimizations = 50;
    }
}
