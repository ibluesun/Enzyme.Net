using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestingConsole
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public bool OK = false;

        private void OKButton_Click(object sender, EventArgs e)
        {
            OK = true;
            this.Close();
        }
    }
}
