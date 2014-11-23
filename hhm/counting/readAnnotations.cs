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

        public string getAllTextWithoutFirstLine( string file ) {
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


        private formatValues parseVal( String s ) {
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


        private string[] splitLine( string line, char split = ' ' ) {
            return line.Split( new char[] { split }, StringSplitOptions.RemoveEmptyEntries );
        }


        #endregion


        //the worker function.. wee.
        public HMM getHmm() {

            InternalHMM hmmCalc = new InternalHMM( states, obs.Count );
            int startNode = getStartState();

            int currentNode = startNode;

            for ( int i = 0; i < annotatedData.Length; i++ ) {
                if ( annotatedData[i] == 'N' ) {
                    ////ASSERTION CURRENT NODE == START NODE (we should always end in the start node again, after a tour.).
                    //if ( currentNode != startNode ) {
                    //    throw new NotSupportedException( "ERROR DEBUG ME" );
                    //}
                    ////count the transition
                    //hmmCalc.transProbs[startNode].add_J_To_K( startNode );
                    ////count the char.
                    //hmmCalc.emProbs[startNode].addCount( getIndexFromObs( geneData[i] ) );
                    //continue;
                    List<int> path = statesFromCurrentLocation( currentNode, i, 1, 1 ); //we have designed the model to work in a depth of 1. thats how the N's works
                    foreach ( var item in path ) {
                        //add to the transistions.
                        hmmCalc.transProbs[currentNode].add_J_To_K( item ); ;
                        //then the ems
                        hmmCalc.emProbs[item].addCount( getIndexFromObs( geneData[i] ) );
                        currentNode = item;//update currentNode to item
                        i++;//we are going over the data. REMBEBER THIS!!!!!
                    }
                } else {//either R or C, but we do not care :P
                    List<int> path = statesFromCurrentLocation( currentNode, i, 3, 3 ); //we have designed the model to work in a depth of 3.
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

            //secound phase, convert the internal calulation into the HMM.


            return null;
        }

        private List<int> statesFromCurrentLocation( int currentNodeIndex, int indexInData, int depth, int startDepth ) {
            List<int> result;
            if ( depth <= 0 ) {
                return getPossibleNodes( currentNodeIndex, indexInData );
            } else {
                int foundPossiblePath = 0;
                result = new List<int>();
                var nodes = getPossibleNodes( currentNodeIndex, indexInData );
                int chosenPath = -1;
                foreach ( var item in nodes ) {
                    var temp = statesFromCurrentLocation( item, indexInData + 1, depth - 1, startDepth );
                    if ( temp.Count > 0 ) {
                        //possible.
                        foundPossiblePath++;
                        result = temp;
                        chosenPath = item;
                    }
                }
                if ( foundPossiblePath > 1 ) {
                    throw new NotSupportedException( "ERROR DEBUG ME" );
                }
                if ( chosenPath != -1 ) {
                    result.Insert( 0, chosenPath );
                }else {
                    return new List<int>();//we got no path.
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
                result.Add( (double)item / (double)allRefs );
            }
            return result;
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
                result.Add( (double)item / (double)totalCounts );
            }
            return result;
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
