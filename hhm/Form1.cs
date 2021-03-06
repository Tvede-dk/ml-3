﻿using hhm.counting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace hhm {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            DirectoryInfo current = new DirectoryInfo( Directory.GetCurrentDirectory() );
            string dir = current.Parent.Parent.FullName;
            textBox1.Text = Directory.GetCurrentDirectory() + "/hhm.txt";
            textBox4.Text = dir + "/genome/genome1.fa";
            textBox2.Text = dir + "/pred/temp.fa";
        }

        private void button1_Click( object sender, EventArgs e ) {
            HMM h = HMM.readFromFile( textBox1.Text );
            useHmmForRun( h );
        }


        private void useHmmForRun( HMM hmm ) {
            try {
                var fa = getFaFromFile();
                var vit = new viterbi( hmm );
                var res = getViterbiPrediction( fa, vit );
                String result = hmm.convertUsingAnnotations( res.prediction );
                File.WriteAllText( textBox2.Text, fa.description + "\r\n" + result );
                MessageBox.Show( "sucess" );
            } catch ( Exception except ) {
                MessageBox.Show( "Error:" + except.Message );
            }
        }

        private FA getFaFromForm() {
            var fa = new FA();
            if ( textBox3.Text.Length > 0 ) {
                fa.description = ">DIRECT INPUT";
                fa.data = getOmegaFromBox();
            } else {
                fa = FA.readFromFile( textBox4.Text );
            }
            return fa;
        }

        private VitterbiResult getViterbiPrediction( FA f ) {
            viterbi vi = getVitterbiDefault();
            vi.setOmega( f.data );
            List<List<double>> omega = vi.getOmegaMulti();
            return new VitterbiResult { prediction = vi.backtrack(), omega = omega };
        }

        private VitterbiResult getViterbiPrediction( FA f, viterbi vi ) {
            vi.setOmega( f.data );
            List<List<double>> omega = vi.getOmegaMulti();
            return new VitterbiResult { prediction = vi.backtrack(), omega = omega };
        }

        private viterbi getVitterbiDefault() {
            HMM h = HMM.readFromFile( textBox1.Text );
            viterbi vi = new viterbi( h );
            return vi;
        }



        private indexInOmega testOmegas( List<List<double>> omega, string toCompareWith ) {
            var lines = toCompareWith.Split( '\r' );
            if ( lines.Length == 1 ) {
                lines = lines[0].Split( '\n' );
            }
            var newOmega = new double[omega.Count, omega[0].Count];
            for ( int i = 0; i < lines.Length; i++ ) {
                var split = lines[i].Replace( "\n", "" ).Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                int j = 0;
                foreach ( var item in split ) {
                    newOmega[i, j] = double.Parse( item, CultureInfo.InvariantCulture );
                    j++;
                }
            }
            for ( int i = 0; i < omega.Count; i++ ) {
                for ( int j = 0; j < omega[0].Count; j++ ) {
                    if ( omega[i][j] - newOmega[i, j] > 0.01d ) {
                        return new indexInOmega() { firstIndex = i, secoundIndex = j };
                    }
                }
            }
            return null;
        }

        private void textBox1_TextChanged( object sender, EventArgs e ) {

        }

        private void label1_Click( object sender, EventArgs e ) {

        }

        private void button2_Click( object sender, EventArgs e ) {
            var res = openFileDialog1.ShowDialog();
            if ( res == DialogResult.OK ) {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button3_Click( object sender, EventArgs e ) {
            var res = saveFileDialog1.ShowDialog();
            if ( res == DialogResult.OK ) {
                textBox2.Text = saveFileDialog1.FileName;
            }
        }

        private void button4_Click( object sender, EventArgs e ) {
            var res = openFileDialog2.ShowDialog();
            if ( res == DialogResult.OK ) {
                textBox4.Text = openFileDialog2.FileName;
                if ( textBox2.Text.Length > 0 ) {
                    string end = Path.GetFileName( openFileDialog2.FileName );
                    string path = Path.GetDirectoryName( textBox2.Text );
                    textBox2.Text = Path.Combine( path, end );

                }
            }
        }

        private void button5_Click( object sender, EventArgs e ) {
            if ( File.Exists( textBox2.Text ) ) {
                System.Diagnostics.Process.Start( textBox2.Text );
            }
        }

        private string getOmegaFromBox() {
            return textBox3.Text.Trim().Replace( "\r", "" ).Replace( "\n", "" );
        }



        private void button6_Click( object sender, EventArgs e ) {
            string validationText = textBox3.Text;
            VitterbiResult vres = getViterbiPrediction( getFaFromFile() );
            var test = testOmegas( vres.omega, validationText );
            if ( test == null ) {
                MessageBox.Show( "EQUAL" );
            } else {
                MessageBox.Show( "NOT EQUAL , THEY DIFFER AT: " + test.firstIndex + "," + test.secoundIndex );
            }

        }

        private FA getFaFromFile() {
            return FA.readFromFile( textBox4.Text );
        }

        private void openFileDialog2_FileOk( object sender, CancelEventArgs e ) {

        }

        private void saveFileDialog1_FileOk( object sender, CancelEventArgs e ) {

        }

        private void button7_Click( object sender, EventArgs e ) {
            //var count = new counting.Counting();
            //count.test();
            DirectoryInfo current = new DirectoryInfo( Directory.GetCurrentDirectory() );
            string dir = current.Parent.Parent.FullName;
			var hmmCust = new readAnnotations(dir + "/genome/genome1.fa" , dir + "/annotation/annotation1.fa" , textBox1.Text); //startStopReverse.txt  simple.txt
            var hmm = hmmCust.getHmm();
            var res = saveFileDialog1.ShowDialog();
            if ( res == DialogResult.OK ) {
                File.WriteAllLines( saveFileDialog1.FileName, hmm.saveToText() );
            }


        }

        private void button8_Click( object sender, EventArgs e ) {
            DirectoryInfo current = new DirectoryInfo( Directory.GetCurrentDirectory() );
            string dir = current.Parent.Parent.FullName;
			var hmmCust = new readAnnotations(dir + "/genome/genome1.fa" , dir + "/annotation/annotation1.fa" ,  textBox1.Text); //startStopReverse.txt  simple.txt
            var hmm = hmmCust.getHmm();
            useHmmForRun( hmm );
        }

        private void button9_Click( object sender, EventArgs e ) {
            //principle.
            List<string> geneFiles = new List<string>();
            List<string> annoFiles = new List<string>();

            DirectoryInfo current = new DirectoryInfo( Directory.GetCurrentDirectory() );
            string dir = current.Parent.Parent.FullName;

            for ( int i = 1; i < 6; i++ ) {
                geneFiles.Add( dir + "/genome/genome" + i + ".fa" );
                annoFiles.Add( dir + "/annotation/annotation" + i + ".fa" );
            }
			HMM best = readAnnotations.n_fold_cross_validation(geneFiles , annoFiles , textBox1.Text);
            string[] text = best.saveToText();
            var res = saveFileDialog1.ShowDialog();
            if ( res == DialogResult.OK ) {
                File.WriteAllLines( saveFileDialog1.FileName, text );
            }
        }
    }
    struct VitterbiResult {
        public List<List<double>> omega;
        public string prediction;
    }
    class indexInOmega {
        public int firstIndex { get; set; }
        public int secoundIndex { get; set; }
    }
}
