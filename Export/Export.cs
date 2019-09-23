using Autofac;
using Export.Common;
using Export.Data;
using Export.Model;
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
    /// <summary>
    /// 导入窗体
    /// </summary>
    public partial class Export : Form
    {
        /// <summary>
        /// 数据上下文
        /// </summary>
        private IDbContext dbContext;

        /// <summary>
        /// ctor
        /// </summary>
        public Export()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 导入Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                txtFileName.Text = "";
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "xlsx文件(*.xlsx)|*.xlsx";
                fileDialog.Title = "选择Excel文件";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFileName.Text = fileDialog.FileName;
                    var correctionDate = DateTime.Now.AddMonths(-3);
                    DataTable dt = ExcelHelper.ExcelToTable<ExportEntity>(txtFileName.Text);
                    string tableName = dt.TableName;
                    string selectWhere = $"level ='P3' and time>'{correctionDate}'";
                    dt = dt.Select(selectWhere).Distinct(new DataRowEqualityComparer()).CopyToDataTable();
                    dt.TableName = tableName;
                    dbContext = StartService.Instance.Container.GetService<IDbContext>();
                    if (dbContext != null)
                    {
                        dbContext.BatchInsert<ExportEntity>(dt);
                    }
                }
                txtResult.Text += "导入成功！";
                MessageBox.Show("导入成功!", "导入状态");
            }
            catch (Exception ex)
            {
                txtResult.Text += "导入失败:" + ex.Message;
            }
        }
    }
}
