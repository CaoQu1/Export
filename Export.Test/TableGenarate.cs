using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Test
{
    public class TableGenarate
    {
        /// <summary>
        /// 生成数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public static DataTable GetTable(string tableName = "mstest", int rowCount = 10)
        {
            DataTable dataTable = new DataTable();
            dataTable.TableName = tableName;
            dataTable.Columns.AddRange(new DataColumn[] { new DataColumn("name", typeof(string)), new DataColumn("age", typeof(int)) });
            for (int i = 0; i < rowCount; i++)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["name"] = i + "name";
                dataRow["age"] = i;
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }
    }
}
