using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSRF_Tool_2.MDLB_Import
{
    public partial class Color : UserControl
    {
        public Color()
        {
            InitializeComponent();
        }
        /*
        public Color(int R, int G, int B, int A)
        {
            this.txtb_R.Text = R.ToString();
            this.txtb_G.Text = G.ToString();
            this.txtb_B.Text = B.ToString();
            this.txtb_A.Text = A.ToString();

        }
        */
        public void set_colors(int R, int G, int B, int A)
        {
            this.txtb_R.Text = R.ToString();
            this.txtb_G.Text = G.ToString();
            this.txtb_B.Text = B.ToString();
            this.txtb_A.Text = A.ToString();
        }


        public Materials.color get_colors()
        {
            return new Materials.color(Convert.ToByte(txtb_R.Text), Convert.ToByte(txtb_G.Text), Convert.ToByte(txtb_B.Text), Convert.ToByte(txtb_A.Text));
        }


        private void txtb_B_KeyPress(object sender, KeyPressEventArgs e)
        {

        }


        private void txtb_numeric_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void txtb_numeric_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (Convert.ToInt32(tb.Text) > 255)
            {
                tb.Text = "255";
            }

            if (Convert.ToInt32(tb.Text) < 0)
            {
                tb.Text = "0";
            }
        }
    }
}
