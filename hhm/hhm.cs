using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hhm {
    public class HHM {
        public int states;
        public List<int> statesList = new List<int>();

        public int observables = 0;
        public Dictionary<char, int> obsList = new Dictionary<char, int>();


        public double[,] transProbs = new double[0, 0];

        public double[] initProbes = new double[0];

        public double[,] emprobes = new double[0, 0];

        public static HHM readFromFile( string fileName ) {
            HHM result = new HHM();

            string[] lines = File.ReadAllLines( fileName );
            int states = int.Parse( lines[1] );

            var stateslist = new List<int>();
            var split = lines[2].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            for (int i = 0; i < states; i++) {
                stateslist.Add( int.Parse( split[i] ) );
            }

            var obsCount = int.Parse( lines[4] );
            var obsDict = new Dictionary<char, int>();
            split = lines[5].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            for (int i = 0; i < obsCount; i++) {
                obsDict.Add( split[i][0], i );
            }
            //init probes

            var initpr = new List<double>();
            split = lines[7].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            for (int i = 0; i < states; i++) {
                initpr.Add( double.Parse( split[i], CultureInfo.InvariantCulture  ) );
            }


            var transProbes = new double[states, states];
            for (int i = 0; i < states; i++) {
                split = lines[9 + i].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                int j = 0;
                foreach (var item in split) {
                    transProbes[i, j] = double.Parse( item, CultureInfo.InvariantCulture );
                    j++;
                }
            }

            var emProbes = new double[states, obsCount];
            for (int i = 0; i < states; i++) {
                split = lines[9 + states + 1 + i].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                int j = 0;
                foreach (var item in split) {
                    emProbes[i, j] = double.Parse( item, CultureInfo.InvariantCulture );
                    j++;
                }
            }

            result.emprobes = emProbes;
            result.initProbes = initpr.ToArray();
            result.states = states;
            result.statesList = stateslist;
            result.transProbs = transProbes;
            result.obsList = obsDict;
            result.observables = obsCount;


            return result;
        }

    }
}
