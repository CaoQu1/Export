using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Export
{
    public partial class Export : Form
    {
        public Export()
        {
            InitializeComponent();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "*.xlsx|*.xls";
            fileDialog.Title = "选择Excel文件";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = fileDialog.FileName;

            }
        }
    }
}
