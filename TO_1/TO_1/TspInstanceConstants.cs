using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TO_1
{
    public static class TspInstanceConstants
    {
        public const int LS_REPEAT_VALUE = 130;
        public const int K_VALUE = 4;
        public const bool WRITE_PRE_SOLUTION = true;
        public const byte NUMBER_OF_GROUPS = 4;
        public const int NUMBER_OF_POINTS_PER_GROUP = 25;

        public const int NUMBER_OF_MUTATIONS = 10;
        public const int NUMBER_OF_PRE_MUTATIONS = 10;
        public const int NUMBER_OF_HEA_RUNS = 100;

        public const int HeaRunTime = 900000;
        public const int HeaPopulationSize = 500;
        public const int HeaNumberOfMutations = 5;
        public const int HeaNumberOfPreMutations = 5;
        public const int HeaNumberOfRecombinations = 5;
    }
}
