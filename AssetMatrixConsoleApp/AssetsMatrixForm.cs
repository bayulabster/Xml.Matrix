using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace AssetMatrixConsoleApp
{
    public partial class AssetsMatrixForm : Form
    {
        public AssetsMatrixForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void OnTextBoxInput(object sender, KeyEventArgs e)
        {
            Debug.WriteLine(e.KeyCode.ToString());
            if(e.KeyCode == Keys.Return)
            {
                Debug.WriteLine("kampret");
                TextBox textObj = (TextBox)sender;
                Debug.WriteLine(textObj.Text);
            }
                
        }
        
    }
}
