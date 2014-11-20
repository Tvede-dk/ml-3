using System;
using System.ComponentModel;
using System.IO;

using System.Windows.Forms;

namespace hhm {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
			textBox1.Text = Directory.GetCurrentDirectory() + "/hhm.txt";
			textBox4.Text = Directory.GetCurrentDirectory() + "/genome/genome7.fa";
			textBox2.Text =  Directory.GetCurrentDirectory()+"/pred/temp.result";

        }

        private void button1_Click( object sender, EventArgs e ) {
            try {
                HHM h = HHM.readFromFile( textBox1.Text );
                viterbi vi = new viterbi( h );

                var fa = FA.readFromFile( textBox4.Text );

                if (textBox3.Text.Length > 0) {
                    vi.setOmega( textBox3.Text );
                } else {
                    vi.setOmega( fa.data );
                }
                var omega = vi.getOmegaMulti();
                // testOmegas( omega );
                String res = vi.backtrack();



                File.WriteAllText( textBox2.Text, fa.description + "\r\n" + res );
                MessageBox.Show( "sucess" );
            } catch (Exception except) {
                MessageBox.Show( "Error:" + except.Message );
            }
        }

        //private void testOmegas( List<List<double>> omega ) {

        //    string file = "omega.txt";



        //    var lines = File.ReadAllLines( file );
        //    var readOmgea = new double[1000, 7];
        //    for (int i = 0; i < 1000; i++) {
        //        var split = lines[i].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
        //        int j = 0;
        //        foreach (var item in split) {
        //            readOmgea[i, j] = double.Parse( item, CultureInfo.InvariantCulture );
        //            j++;
        //        }
        //    }
        //    for (int i = 0; i < 1000; i++) {
        //        for (int j = 0; j < 7; j++) {
        //            if (omega[i][j] - readOmgea[i, j] > 0.01d) {
        //                MessageBox.Show( "ERROR IN OMEGA" );
        //            }
        //        }
        //    }

        //}

        private void textBox1_TextChanged( object sender, EventArgs e ) {

        }

        private void label1_Click( object sender, EventArgs e ) {

        }

        private void button2_Click( object sender, EventArgs e ) {
            var res = openFileDialog1.ShowDialog();
            if (res == DialogResult.OK) {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button3_Click( object sender, EventArgs e ) {
            var res = saveFileDialog1.ShowDialog();
            if (res == DialogResult.OK) {
                textBox2.Text = saveFileDialog1.FileName;
            }
        }

        private void button4_Click( object sender, EventArgs e ) {
            var res = openFileDialog2.ShowDialog();
            if (res == DialogResult.OK) {
                textBox4.Text = openFileDialog2.FileName;
            }
        }

        private void button5_Click( object sender, EventArgs e ) {
            if (File.Exists( textBox2.Text )) {
                System.Diagnostics.Process.Start( textBox2.Text );
            }
        }

        private void button6_Click( object sender, EventArgs e ) {
            //TODO make me.
        }

        private void openFileDialog2_FileOk( object sender, CancelEventArgs e ) {

        }

        private void saveFileDialog1_FileOk( object sender, CancelEventArgs e ) {

        }
    }
}
