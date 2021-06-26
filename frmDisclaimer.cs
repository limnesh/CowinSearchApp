using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CowinSearchApp
{
    public partial class frmDisclaimer : Form
    {
        public frmDisclaimer()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (chk6.Checked)
            {
                Program.bAccept = true;
                Program.strState = cmbState.SelectedItem.ToString();
                Close();
            }
            else
            {
                MessageBox.Show("Please accept all the terms and conditions to start using the application", "Terms & Conditions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmDisclaimer_Load(object sender, EventArgs e)
        {
            cmbState.DropDownStyle = ComboBoxStyle.DropDownList;
        }
    }
}
