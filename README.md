Machine Learning
================
In this project we are going to predict the gene structure of the 5 annotated genomes, i.e. genome6.fa, genome7.fa, genome8.fa, genome9.fa, and genome10.fa (Found in [genome/](https://github.com/Tvede-dk/ml-3/tree/master/hhm/genome). We use Hidden Markov Models to preduct the gene structure, thus the following steps are done:

* Deciding on an initial model structure, i.e. the number of hidden states and which transitions and emission should have a fixed probability (e.g. 0 for "not possible", or 1 for "always the case")
* Tune model parameters by training, i.e. set the non-fixed emission and transition probabilities.
* Use your best model to predict the gene structure for the 5 unannotated genomes using the Viterbi algorithm with subsequent backtracking. I.e. for each unannotated genome you must find the most likely sequence of states in your model generating it, and convert this sequence of states into a FASTA file giving the annotation of each nucleotide as N, C, or R. You should make files, annotation6.fa, ..., annotation10.fa, giving the annotation for the 5 unannotated genomes. 
 
Build and execute on Linux
----------------

1. Ensure that you have [mono installed](http://www.mono-project.com/download/#download-lin)
2. Run `xbuild` in root directory
3. Change into directory `hhm/bin/[Debug]/`
4. Run `mono hhm.exe`
