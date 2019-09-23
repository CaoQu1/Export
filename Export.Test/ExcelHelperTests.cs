using Microsoft.VisualStudio.TestTools.UnitTesting;
using Export.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Export.Model;
using System.Data;

namespace Export.Common.Tests
{
    [TestClass()]
    public class ExcelHelperTests
    {
        [DataRow(@"C:\Users\caoqu\Desktop\转正考核题目\net\eolinker信息.xlsx", true)]
        [TestMethod]
        public void ExcelToTableTestTrue(string filePath, bool result)
        {
            DataTable dataTable = ExcelHelper.ExcelToTable<ExportEntity>(filePath);
            Assert.AreEqual(dataTable != null, result);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExcelToTableTestThrow()
        {
            ExcelHelper.ExcelToTable<ExportEntity>(null);
        }
    }
}