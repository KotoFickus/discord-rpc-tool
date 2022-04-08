using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace RPCtool
{
    public partial class LoadingForm : Form
    {
        Form1 frm;
        public LoadingForm()
        {
            InitializeComponent();
        }

        private void LoadingForm_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.LoadingGif;
            timer1.Enabled = true;
        }

        private void pictureBox1_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.Size = Properties.Resources.LoadingGif.Size;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            frm = new Form1();
            frm.Activate();
            frm.Show();
            this.Hide();
            timer1.Enabled = false;
        }
    }
}
