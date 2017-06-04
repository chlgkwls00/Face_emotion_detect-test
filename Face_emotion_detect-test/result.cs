using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace faceemotion
{
    public partial class result : Form
    {
        public result()
        {
            InitializeComponent();
        }

        private void result_Load(object sender, EventArgs e)
        {

        }
        public void getresult(string result)
        {
            Console.WriteLine(result);
            label1.Text = result;
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
          this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
