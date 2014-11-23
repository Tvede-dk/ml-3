using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace hhm.counting {
    public class readAnnotations {

        #region HMM file structures  / data.



        private int states = 0;
        private List<char> statesAnnotations = new List<char>();

        private List<char> obs = new List<char>();

        private Dictionary<char, int> lookupObsToIndex = new Dictionary<char, int>();

        private List<formatValues> initProbs = new List<formatValues>();

        private List<List<formatValues>> transProbs = new List<List<formatValues>>();


        private List<List<formatValues>> emProbs = new List<List<formatValues>>();

        #endregion

        #region gene and annotated files

        private string geneData;
        private string annotatedData;

        #endregion

        public readAnnotations( string geneFile, string annotatedFile, string hmmFile ) {

            geneData = getAllTextWithoutFirstLine( geneFile );
            annotatedData = getAllTextWithoutFirstLine( annotatedFile );
            parseHmmfile( File.ReadAllLines( hmmFile ) );
        }

        public static string getAllTextWithoutFirstLine( string file ) {
            String[] lines = File.ReadAllLines( file );
            StringBuilder sb = new StringBuilder();
            for ( int i = 1; i < lines.Length; i++ ) {
                sb.Append( lines[i] );
            }
            return sb.ToString();
        }

        #region parsing

        private void parseHmmfile( string[] data ) {
            states = int.Parse( data[1] );
            var annotations = splitLine( data[3] );
            foreach ( var item in annotations ) {
                statesAnnotations.Add( item.FirstOrDefault() );
            }

            var obserables = splitLine( data[6] );
            int innerCounter = 0;
            foreach ( var item in obserables ) {
                var ch = item.FirstOrDefault();
                lookupObsToIndex.Add( ch, innerCounter );
                obs.Add( ch );
                innerCounter++;
            }

            var ip = splitLine( data[8] );
            foreach ( var item in ip ) {
                initProbs.Add( parseVal( item ) );
            }


            for ( int i = 10; i < 10 + states; i++ ) {
                var transLine = splitLine( data[i] );
                var tempList = new List<formatValues>();
                foreach ( var item in transLine ) {
                    tempList.Add( parseVal( item ) );
                }
                transProbs.Add( tempList );
            }

            for ( int i = 11 + states; i < (11 + states + states); i++ ) {
                var emLine = splitLine( data[i] );
                var tempList = new List<formatValues>();
                foreach ( var item in emLine ) {
                    tempList.Add( parseVal( item ) );
                }
                emProbs.Add( tempList );
            }

        }


        private static formatValues parseVal( String s ) {
            formatValues result = formatValues.zero;
            switch ( s ) {
                case "0":
                    result = formatValues.zero;
                    break;
                case "1":
                    result = formatValues.one;
                    break;
                case "?":
                    result = formatValues.x;
                    break;
            }
            return result;
        }


        private static string[] splitLine( string line, char split = ' ' ) {
            return line.Split( new char[] { split }, StringSplitOptions.RemoveEmptyEntries );
        }


        #endregion


        //the worker function.. wee.
        public HMM getHmm() {

            InternalHMM hmmCalc = new InternalHMM( states, obs.Count );
            int startNode = getStartState();

            int currentNode = startNode;

            for ( int i = 0; i < annotatedData.Length; i++ ) {
                List<int> path;
                if ( annotatedData[i] == 'N' ) {
                    path = statesFromCurrentLocationNoRec( currentNode, i, 0 ); //we have designed the model to work in a depth of 1. thats how the N's works
                    foreach ( var item in path ) {
                        //add to the transistions.
                        hmmCalc.transProbs[currentNode].add_J_To_K( item );
                        //then the ems
                        hmmCalc.emProbs[item].addCount( getIndexFromObs( geneData[i] ) );
                        currentNode = item;//update currentNode to item
                        if ( item != 0 ) {
                            throw new NotSupportedException( "ERROR DEBUG ME" );
                        }
                    }
                } else {//either R or C, but we do not care :P
                    int end = annotatedData.IndexOf( 'N', i );
                    int count = end - i;
                    path = statesFromCurrentLocationNoRec( currentNode, i, count ); //we have designed the model to work in a depth of 3.
                    foreach ( var item in path ) {
                        //add to the transistions.
                        hmmCalc.transProbs[currentNode].add_J_To_K( item ); ;
                        //then the ems
                        hmmCalc.emProbs[item].addCount( getIndexFromObs( geneData[i] ) );
                        currentNode = item;//update currentNode to item
                        i++;//we are going over the data. REMBEBER THIS!!!!!
                    }
                }


            }

            HMM result = new HMM();

            //second phase, convert the internal calculation into the HMM.

            //result.ObsAnnotation = obsAnn;
            //result.emprobes = emProbes;
            //result.initProbes = initpr.ToArray();
            //result.states = states;
            //result.statesList = stateslist;
            //result.transProbs = transProbes;
            //result.obsList = obsDict;
            //result.observables = obsCount;

            //plumming
            result.states = states;
            result.statesList = new List<int>();
            for ( int i = 1; i < states + 1; i++ ) {
                result.statesList.Add( i );
            }
            result.observables = obs.Count;//4
            result.ObsAnnotation = statesAnnotations;
            result.obserablesList = obs;

            Console.WriteLine( "" );
            Console.WriteLine( "--------------INIT PROBS--------------" );
            Console.WriteLine( "" );
            //assumtion:  there is only one start node. otherwise most of the code will need more work (actually).
            result.initProbes = new double[initProbs.Count];
            for ( int i = 0; i < initProbs.Count; i++ ) {
                double val = 0;
                if ( initProbs[i] == formatValues.one ) {
                    val = 1;
                }
                Console.Write( val + " " );
                result.initProbes[i] = val;
            }
            Console.WriteLine( "" );

            //char to int. A C G T  = 0, 1,2,3
            result.obsList = lookupObsToIndex;



            //transition probabilities
            Console.WriteLine( "" );
            Console.WriteLine( "--------------trans probs--------------" );
            Console.WriteLine( "" );
            result.transProbs = new double[states, states];
            for ( int i = 0; i < transProbs.Count; i++ ) {
                var lst = transProbs[i];
                var calced = hmmCalc.transProbs[i].getTransProbs();
                for ( int j = 0; j < lst.Count; j++ ) {
                    switch ( lst[j] ) {
                        case formatValues.one:
                            //if we have not taken the path, then, although it is a legit path, it might not even exits in this data set.
                            if ( 1 != calced[j] && hmmCalc.transProbs[i].getRefs() != 0 ) {
                                throw new NotSupportedException( "ERROR DEBUG ME" );
                            }
                            Console.Write( 1 + " " );
                            result.transProbs[i, j] = 1;
                            break;
                        case formatValues.x:
                            Console.Write( calced[j] + " " );
                            result.transProbs[i, j] = calced[j];
                            break;
                        case formatValues.zero:
                            //validation:
                            if ( 0 != calced[j] ) {
                                throw new NotSupportedException( "ERROR DEBUG ME" );
                            }
                            Console.Write( 0 + " " );
                            result.transProbs[i, j] = 0;
                            break;
                    }
                }
                Console.WriteLine( "" );
            }

            Console.WriteLine( "" );
            Console.WriteLine( "--------------emprobes--------------" );
            Console.WriteLine( "" );

            result.emprobes = new double[states, obs.Count];
            for ( int i = 0; i < emProbs.Count; i++ ) {
                var lst = emProbs[i];
                var calced = hmmCalc.emProbs[i].getRatios();
                for ( int j = 0; j < lst.Count; j++ ) {
                    switch ( lst[j] ) {
                        case formatValues.one:
                            //if we have not taken the path, then, although it is a legit path, it might not even exits in this data set.
                            if ( 1 != calced[j] && hmmCalc.emProbs[i].getRefs() != 0 ) {
                                throw new NotSupportedException( "ERROR DEBUG ME" );
                            }
                            Console.Write( 1 + " " );
                            result.emprobes[i, j] = 1;
                            break;
                        case formatValues.x:
                            Console.Write( calced[j] + " " );
                            result.emprobes[i, j] = calced[j];
                            break;
                        case formatValues.zero:
                            //validation:
                            if ( 0 != calced[j] ) {
                                throw new NotSupportedException( "ERROR DEBUG ME" );
                            }
                            Console.Write( 0 + " " );
                            result.emprobes[i, j] = 0;
                            break;
                    }
                }
                Console.WriteLine( "" );
            }
            return result;
        }


        private List<int> statesFromCurrentLocationNoRec( int currentNodeIndex, int indexInData, int depth ) {

            var stack = new List<List<int>>();
            var nodes = getPossibleNodes( currentNodeIndex, indexInData );
            foreach ( var item in nodes ) {
                var lst = new List<int>();
                lst.Add( item );
                stack.Add( lst );
            }
            List<int> result = new List<int>();
            while ( stack.Count > 0 ) {

                //pop an element
                List<int> currentSteps = stack[stack.Count - 1];
                stack.RemoveAt( stack.Count - 1 );

                bool haveFoundPath =  (currentSteps.Count >= depth); ;
                for ( int i = currentSteps.Count; i < depth + 1; i++ ) {


                    int currentNode = currentSteps.Last();
                    int currentIndex = indexInData + currentSteps.Count;
                    var innerNodes = getPossibleNodes( currentNode, currentIndex );
                    if ( innerNodes.Count == 0 ) {
                        break;

                    } else {
                        for ( int nn = 1; nn < innerNodes.Count; nn++ ) {
                            List<int> newPossiblePath = new List<int>( currentSteps );
                            newPossiblePath.Add( innerNodes[nn] );
                            stack.Add( newPossiblePath );
                        }
                        currentSteps.Add( innerNodes[0] );
                    }
                    haveFoundPath = (i == depth);
                }
                if ( haveFoundPath == true ) {
                    result = currentSteps;
                    break;
                }
            }
            return result;
        }



        private List<int> getPossibleNodes( int currentNodeIndex, int indexInData ) {
            var possibleNodes = new List<int>();
            int itteratingIndex = 0;
            var dataChar = geneData[indexInData];
            foreach ( var item in transProbs[currentNodeIndex] ) {
                if ( item != formatValues.zero ) {//either 1 or x.
                    if ( emProbs[itteratingIndex][getIndexFromObs( dataChar )] != formatValues.zero ) {
                        if ( annotatedData[indexInData] == statesAnnotations[itteratingIndex] ) {
                            possibleNodes.Add( itteratingIndex ); //we can transit, and the current value is legit, and it is the right order.
                        }
                    }
                }
                itteratingIndex++;
            }
            return possibleNodes;
        }

        private int getIndexFromObs( char obs ) {
            return lookupObsToIndex[obs];
        }


        /// <summary>
        /// index
        /// </summary>
        /// <returns>index (0 based)</returns>
        private int getStartState() {
            for ( int i = 0; i < initProbs.Count; i++ ) {
                var val = initProbs[i];
                if ( val == formatValues.one ) {
                    return i;
                }
            }
            return 0;
        }


    }

    public struct InternalTransProbCalc {
        private int[] jToK;
        private int allRefs;

        public InternalTransProbCalc( int states ) {
            jToK = new int[states];
            allRefs = 0;
        }

        public void add_J_To_K( int index ) {
            jToK[index]++;
            allRefs++;
        }

        public List<double> getTransProbs() {
            var result = new List<double>();
            foreach ( var item in jToK ) {
                if ( allRefs == 0 ) {
                    result.Add( 0 );
                } else {
                    result.Add( (double)item / (double)allRefs );
                }
            }
            return result;
        }

        public int getRefs() {
            return allRefs;
        }
    }

    public struct EmProbcalculation {
        int[] counts;
        int totalCounts;

        public EmProbcalculation( int countsSize ) {
            counts = new int[countsSize];
            totalCounts = 0;
        }

        public void addCount( int index ) {
            addCount( 1, index );
        }

        public void addCount( int count, int index ) {
            counts[index] += count;
            totalCounts += count;
        }

        public List<double> getRatios() {
            var result = new List<double>();
            foreach ( var item in counts ) {
                if ( totalCounts == 0 ) {
                    result.Add( 0 );
                } else {
                    result.Add( (double)item / (double)totalCounts );
                }
            }
            return result;
        }

        public int getRefs() {
            return totalCounts;
        }
    }

    public class InternalHMM {
        public InternalTransProbCalc[] transProbs;
        public EmProbcalculation[] emProbs;


        public InternalHMM( int states, int obserables ) {
            transProbs = new InternalTransProbCalc[states];
            for ( int i = 0; i < states; i++ ) {
                for ( int j = 0; j < states; j++ ) {
                    transProbs[i] = new InternalTransProbCalc( states );
                }
            }
            emProbs = new EmProbcalculation[states];
            for ( int i = 0; i < states; i++ ) {
                emProbs[i] = new EmProbcalculation( obserables );
            }

        }
    }

    public enum formatValues {
        zero, one, x
    }
}
