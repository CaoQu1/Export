using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Export.Common
{
    /// <summary>
    /// excel帮助类
    /// </summary>
    public class ExcelHelper
    {
        /// <summary>
        /// Execl文件转模型
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<T> ExcelToModel<T>(string path) where T : class, new()
        {
            try
            {
                if (!File.Exists(path))
                {
                    throw new Exception($"{path}路径的文件不存在！");
                }

                List<T> list = new List<T>();
                Type tType = typeof(T);
                string sheetName = tType.Name;
                SheetAttribute sheetAttribute = tType.GetCustomAttribute<SheetAttribute>();
                if (sheetAttribute != null)
                {
                    sheetName = sheetAttribute.Name;
                }
                int startIndex = 0;
                RowAttribute rowAttribute = tType.GetCustomAttribute<RowAttribute>();
                if (rowAttribute != null)
                {
                    startIndex = rowAttribute.StartRow;
                }
                PropertyInfo[] properties = tType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new HSSFWorkbook(fileStream);
                    ISheet sheet = workbook.GetSheet(sheetName);
                    for (int j = startIndex; j < sheet.LastRowNum; j++)
                    {
                        IRow row = sheet.GetRow(j);
                        if (row == null)
                        {
                            continue;
                        }
                        else
                        {
                            T tModel = new T();
                            foreach (PropertyInfo property in properties)
                            {
                                ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                                if (columnAttribute != null)
                                {
                                    ICell cell = row.GetCell(columnAttribute.Index);
                                    Type pType = property.PropertyType;
                                    object value = null;
                                    switch (pType.Name)
                                    {
                                        case "System.String":
                                            value = cell.StringCellValue;
                                            break;
                                        case "System.Decimal":
                                        case "System.Double":
                                        case "System.Int32":
                                            value = cell.NumericCellValue;
                                            break;
                                        case "System.DateTime":
                                            value = cell.DateCellValue;
                                            break;
                                        case "System.Boolean":
                                            value = cell.BooleanCellValue;
                                            break;
                                        default:
                                            value = cell.RichStringCellValue.String;
                                            break;
                                    }
                                    property.SetValue(tModel, value);
                                }
                            }
                            list.Add(tModel);
                        }
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Execl文件转内存表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DataTable ExcelToTable<T>(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    throw new Exception($"{path}路径的文件不存在！");
                }

                DataTable dataTable = new DataTable();
                Type tType = typeof(T);
                string tableName = tType.Name;
                TableAttribute tableAttribute = tType.GetCustomAttribute<TableAttribute>();
                if (tableAttribute != null)
                {
                    tableName = tableAttribute.Name;
                }
                int startIndex = 0;
                RowAttribute rowAttribute = tType.GetCustomAttribute<RowAttribute>();
                if (rowAttribute != null)
                {
                    startIndex = rowAttribute.StartRow;
                }
                PropertyInfo[] properties = tType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                dataTable.Columns.AddRange(properties.Select(x =>
                {
                    string columnName = x.Name;
                    TableColumnAttribute tableColumnAttribute = x.GetCustomAttribute<TableColumnAttribute>();
                    if (tableColumnAttribute != null)
                    {
                        columnName = tableColumnAttribute.Name;
                    }
                    return new DataColumn(columnName, x.PropertyType);
                }).ToArray());
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new HSSFWorkbook(fileStream);
                    ISheet sheet = workbook.GetSheet(tableName);
                    for (int j = startIndex; j < sheet.LastRowNum; j++)
                    {
                        IRow row = sheet.GetRow(j);
                        if (row == null)
                        {
                            continue;
                        }
                        else
                        {
                            DataRow newRow = dataTable.NewRow();
                            foreach (PropertyInfo property in properties)
                            {
                                ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                                if (columnAttribute != null)
                                {
                                    ICell cell = row.GetCell(columnAttribute.Index);
                                    Type pType = property.PropertyType;
                                    object value = null;
                                    switch (pType.Name)
                                    {
                                        case "System.String":
                                            value = cell.StringCellValue;
                                            break;
                                        case "System.Decimal":
                                        case "System.Double":
                                        case "System.Int32":
                                            value = cell.NumericCellValue;
                                            break;
                                        case "System.DateTime":
                                            value = cell.DateCellValue;
                                            break;
                                        case "System.Boolean":
                                            value = cell.BooleanCellValue;
                                            break;
                                        default:
                                            value = cell.RichStringCellValue.String;
                                            break;
                                    }
                                    newRow[property.Name] = value;
                                }
                            }
                            dataTable.Rows.Add(newRow);
                        }
                    }
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                return null;
            }
        }
    }
}
