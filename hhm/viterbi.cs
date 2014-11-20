using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hhm {
	public class viterbi {
		private String omegaObs;
		private HHM hm;

		private List<List<double>> calculatedOmega = new List<List<double>>();

		public viterbi( HHM hm ) {
			this.hm = hm;
		}

		public void setOmega( String omg ) {
			omegaObs = omg;
		}

		public List<List<double>> getOmega() {
			if ( omegaObs == null || omegaObs == "" ) {
				throw new SystemException("Omega not initalized.");
			}
			var result = new List<List<double>>();
			foreach ( var item in omegaObs ) {
				if ( result.Count() == 0 ) {
					int upper = hm.emprobes.GetUpperBound(0) + 1;
					List<double> temp = new List<double>();
					for ( int i = 0 ; i < upper ; i++ ) {
						var val = hm.emprobes[i , hm.obsList[item]];
						temp.Add(Math.Log(val) + Math.Log(hm.initProbes[i]));
					}
					result.Add(temp);
				} else {
					//n -1 ..
					var last = result.Last();

					List<double> temp = new List<double>();
					for ( int i = 0 ; i < last.Count ; i++ ) {
						double maxInner = double.NegativeInfinity;
						for ( int u = 0 ; u < last.Count ; u++ ) {
							double d = last[u] + Math.Log(hm.transProbs[u , i]);
							maxInner = Math.Max(d , maxInner);
						}
						temp.Add(maxInner + Math.Log(hm.emprobes[i , hm.obsList[item]]));
					}
					result.Add(temp);
				}
			}
			calculatedOmega = result;
			return result;
		}

		public List<List<double>> getOmegaMulti() {

			double[,] precomputedTransprobs = new double[hm.states , hm.states];
			for ( int i = 0 ; i < hm.states ; i++ ) {
				for ( int j = 0 ; j < hm.states ; j++ ) {
					precomputedTransprobs[i , j] = Math.Log(hm.transProbs[i , j]);
				}
			}

			double[,] precompEmProbes = new double[hm.states , hm.observables];
			for ( int i = 0 ; i < hm.states ; i++ ) {
				for ( int j = 0 ; j < hm.observables ; j++ ) {
					precompEmProbes[i , j] = Math.Log(hm.emprobes[i , j]);
				}
			}


			if ( omegaObs == null || omegaObs == "" ) {
				throw new SystemException("Omega not initalized.");
			}
			List<double> lastInserted = null;
			var result = new List<List<double>>();
			//first itteration in loop
			int upper = hm.emprobes.GetUpperBound(0) + 1;
			List<double> first = new List<double>();
			for ( int i = 0 ; i < upper ; i++ ) {
				var val = hm.emprobes[i , hm.obsList[omegaObs[0]]];
				first.Add(Math.Log(val) + Math.Log(hm.initProbes[i]));
			}
			result.Add(first);
			lastInserted = first;
			for ( int j = 1 ; j < omegaObs.Length ; j++ ) {
				var item = omegaObs[j];
				//n -1 ..
				// var last = result.Last();
				List<double> temp = new List<double>();
				for ( int i = 0 ; i < lastInserted.Count ; i++ ) {
					double maxInner = double.NegativeInfinity;
					for ( int u = 0 ; u < lastInserted.Count ; u++ ) {
						double d = lastInserted[u] + precomputedTransprobs[u , i];
						maxInner = Math.Max(d , maxInner);
					}
					temp.Add(maxInner + precompEmProbes[i , hm.obsList[item]]);
				}
				result.Add(temp);
				lastInserted = temp;
			}
			calculatedOmega = result;
			return result;
		}

		public string backtrack() {
			if ( omegaObs == null || omegaObs == "" ) {
				throw new SystemException("Omega not initalized.");
			}

			var res = backtrack_itt(omegaObs.Length - 1 , new List<int>());

			StringBuilder sb = new StringBuilder();
			for ( int i = res.Count - 1 ; i >= 0 ; i-- ) {
				string valOfInt = (res[i] + 1).ToString();
				sb.Append(valOfInt);
			}
			return sb.ToString();
		}


		private List<int> backtrack_itt( int start , List<int> z ) {
			var current = z;
			for ( int i = start ; i >= 0 ; i -= 1 ) {
				if ( i < 0 && z.Count > 0 ) {
					return current;
				} else if ( current.Count == 0 ) {
					int largestIndex = 0;
					double largestVal = double.NegativeInfinity;
					for ( int j = 0 ; j < calculatedOmega[i].Count ; j++ ) {
						var val = calculatedOmega[i][j];
						if ( val > largestVal ) {
							largestVal = val;
							largestIndex = j;
						}
					}
					current.Add(largestIndex);
				} else {
					var last = current.Last();
					int largestIndex = 0;
					double largestSoFar = double.MinValue;
					for ( int j = 0 ; j < calculatedOmega[i].Count ; j++ ) {
						double val = (calculatedOmega[i][j] + Math.Log(hm.transProbs[j , last]));
						if ( val > largestSoFar ) {
							largestSoFar = val;
							largestIndex = j;
						}
					}
					current.Add(largestIndex);
				}
			}
			return z;
		}

		public HHM getHHM() {
			return hm;
		}
	}
}
