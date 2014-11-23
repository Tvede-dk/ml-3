using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hhm.counting {
    public class Counting {

        public void test() {
            long states = 7;
            var hm = new HMM();
            Dictionary<int, char> observalbes;
            countStruct[,] table = new countStruct[states, states];
            String annotated = "NNCCCCCCNN";
            string data = "AATAGGTAAA";
            long length = annotated.Length;
            long initState = 0;
            long currentState = initState;
            for (int i = 0; i < length; i++) {
                if (annotated[i] == 'N') {
                    table[currentState, currentState].allTrans++;
                    table[currentState, currentState].fromJTrans++;
                    //stay => increase that one.
                } else if (annotated[i] == 'C') {
                    List<long> paths = new List<long>();

                    //run though list of allowed tranisints, and find the possible paths, if prob >1, or multiple x's for that char, then error.


                    long newCurrentState = 1;
                    table[currentState, newCurrentState].allTrans++;
                    //find what part we are performing.

                    //perform somehting
                } else {
                    //R 

                }
            }
        }

        private List<long> getPossiblePaths( long fromState, char data, List<String> rowParts, Dictionary<char,int> observalbes ) {
            var result = new List<long>();
            long end = rowParts.Count;
            bool haveSeen1 = false;
            for (int i = 0; i < end; i++) {
                if (rowParts[i].Contains( "1" )) {
                    result.Add( i );
                    haveSeen1 = true;
                } else if (rowParts[i].Contains( "x" )) {
                    result.Add( i );
                    //if we dont belive in validation, we could just stop when seeing 1.
                }
            }
            var statesObsValues = new List<char>();//we have every resulting states Observation value
            //validate our foundings.
            if (result.Count > 1 && haveSeen1) {
                //ERROR; INVALID MODEL.
            }
            List<char> resultingChar = new List<char>();
            //foreach (var item in result) {

            //    var state = observalbes[item];

            //    if (resultingChar.Contains(value)) {// the char
            //    }else {
            //        resultingChar.Add( value );//the char
            //    }
            //}
            return result;
        }

    }
    public struct countStruct {
        public long fromJTrans;
        public long allTrans;
    }


}
