using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enzyme;

namespace TestingConsole
{
    public class TestingContext : EnzymeDynamicContext
    {

        public object MsgBox(object msg)
        {
            System.Windows.Forms.MessageBox.Show(msg.ToString());
            return null;
        }


        public double Sin(double angle)
        {
            return Math.Sin(angle);
        }

        public object InputBox()
        {
            InputBox ib = new InputBox();
            ib.ShowDialog();
            if (ib.OK)
                return ib.InputTextBox.Text;
            else
                return string.Empty;
        }
    }
}
