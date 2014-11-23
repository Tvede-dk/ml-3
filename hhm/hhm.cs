using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;


namespace hhm {
    public class HMM {
        public int states;
        public List<int> statesList = new List<int>();

        public int observables = 0;
        public Dictionary<char, int> obsList = new Dictionary<char, int>();

        public List<char> obserablesList = new List<char>();

        public List<char> ObsAnnotation = new List<char>();


        public double[,] transProbs = new double[0, 0];

        public double[] initProbes = new double[0];

        public double[,] emprobes = new double[0, 0];

        public static HMM readFromFile( string fileName ) {
            HMM result = new HMM();

            string[] lines = File.ReadAllLines( fileName );
            int states = int.Parse( lines[1] );

            var stateslist = new List<int>();
            var split = lines[2].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            for ( int i = 0; i < states; i++ ) {
                stateslist.Add( int.Parse( split[i] ) );
            }

            List<char> obsAnn = new List<char>();
            split = lines[3].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            for ( int i = 0; i < states; i++ ) {
                obsAnn.Add( split[i][0] );
            }

            var obsListChar = new List<char>();
            var obsCount = int.Parse( lines[5] );
            var obsDict = new Dictionary<char, int>();
            split = lines[6].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            for ( int i = 0; i < obsCount; i++ ) {
                obsListChar.Add( split[i][0] );
                obsDict.Add( split[i][0], i );
            }
            //init probes

            var initpr = new List<double>();
            split = lines[8].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            for ( int i = 0; i < states; i++ ) {
                initpr.Add( double.Parse( split[i], CultureInfo.InvariantCulture ) );
            }


            var transProbes = new double[states, states];
            for ( int i = 0; i < states; i++ ) {
                split = lines[10 + i].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                int j = 0;
                foreach ( var item in split ) {
                    transProbes[i, j] = double.Parse( item, CultureInfo.InvariantCulture );
                    j++;
                }
            }

            var emProbes = new double[states, obsCount];
            for ( int i = 0; i < states; i++ ) {
                split = lines[11 + states + i].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                int j = 0;
                foreach ( var item in split ) {
                    emProbes[i, j] = double.Parse( item, CultureInfo.InvariantCulture );
                    j++;
                }
            }
            result.ObsAnnotation = obsAnn;
            result.emprobes = emProbes;
            result.initProbes = initpr.ToArray();
            result.states = states;
            result.statesList = stateslist;
            result.transProbs = transProbes;
            result.obsList = obsDict;
            result.observables = obsCount;
            result.obserablesList = obsListChar;
            return result;
        }

        public string convertUsingAnnotations( string input ) {
            string[] split = input.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            StringBuilder sb = new StringBuilder();
            foreach ( var item in split ) {
                int index = int.Parse( item ) - 1;
                sb.Append( ObsAnnotation[index] );
            }
            return sb.ToString();
        }


        public string[] saveToText() {
            List<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();
            result.Add( "states" );

            #region states and annotations


            result.Add( states + "" );

            for ( int i = 1; i < states + 1; i++ ) {
                sb.Append( i ).Append( " " );
            }
            result.Add( sb.ToString() );
            sb.Clear();

            for ( int i = 0; i < states; i++ ) {
                sb.Append( ObsAnnotation[i] ).Append( " " );
            }
            result.Add( sb.ToString() );
            sb.Clear();
            #endregion
            result.Add( "observables" );
            #region obserables
            result.Add( "" + observables );
            sb.Clear();
            for ( int i = 0; i < observables; i++ ) {
                sb.Append( obserablesList[i] ).Append( " " );
            }
            result.Add( sb.ToString() );

            #endregion
            result.Add( "initProbs" );
            #region init probs
            sb.Clear();
            for ( int i = 0; i < initProbes.Length; i++ ) {
                sb.Append( initProbes[i].ToString( CultureInfo.InvariantCulture ) ).Append( " " );
            }
            result.Add( sb.ToString() );
            #endregion
            result.Add( "transProbs" );

            for ( int i = 0; i < states; i++ ) {
                sb.Clear();
                for ( int j = 0; j < states; j++ ) {
                    sb.Append( transProbs[i, j].ToString( CultureInfo.InvariantCulture ) ).Append( " " );
                }
                result.Add( sb.ToString() );
            }

            result.Add( "emProbs" );
            for ( int i = 0; i < states; i++ ) {
                sb.Clear();
                for ( int j = 0; j < observables; j++ ) {
                    sb.Append( emprobes[i, j].ToString( CultureInfo.InvariantCulture ) ).Append( " " );
                }
                result.Add( sb.ToString() );
            }


            return result.ToArray();
        }

    }
}