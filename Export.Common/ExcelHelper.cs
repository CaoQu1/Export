using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
    public static class ExcelHelper
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
                    throw new ArgumentNullException($"{path}路径的文件不存在！");
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
                        if (row == null || IsRowEmpty(row))
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
                                    property.SetValue(tModel, GetCellValue(cell, property));
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
        /// 获取一行的值
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="row"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static object GetCellValue(ICell cell, PropertyInfo property)
        {
            Type pType = property.PropertyType;
            object value = null;
            switch (pType.FullName)
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
            return value;
        }

        /// <summary>
        /// 获取列值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="property"></param>
        /// <param name="tableColumnAttribute"></param>
        /// <param name="columnAttribute"></param>
        /// <returns></returns>
        private static object GetCellValue(IRow row, PropertyInfo property, TableColumnAttribute tableColumnAttribute, ColumnAttribute columnAttribute)
        {
            object value = null;
            if (!string.IsNullOrEmpty(tableColumnAttribute.TableColumnProcess))
            {
                ColumnProcess(tableColumnAttribute.TableColumnProcess, "", ref value);
            }
            else if (columnAttribute != null)
            {
                ICell cell = row.GetCell(columnAttribute.Index);
                if (cell != null && cell.CellType != CellType.Blank)
                {
                    if (!string.IsNullOrEmpty(columnAttribute.ColumnProcess))
                    {
                        ColumnProcess(columnAttribute.ColumnProcess, cell.StringCellValue, ref value);
                    }
                    else
                    {
                        value = GetCellValue(cell, property);
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Execl文件转内存表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DataTable ExcelToTable<T>(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException($"{path}路径为空！");
            }
            if (!File.Exists(path))
            {
                throw new ArgumentNullException($"{path}路径的文件不存在！");
            }
            try
            {
                DataTable dataTable = new DataTable();
                Type tType = typeof(T);
                string tableName = tType.Name;
                TableAttribute tableAttribute = tType.GetCustomAttribute<TableAttribute>();
                SheetAttribute sheetAttribute = tType.GetCustomAttribute<SheetAttribute>();
                if (tableAttribute != null)
                {
                    tableName = tableAttribute.Name;
                }
                dataTable.TableName = tableName;
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
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                IWorkbook workbook = null;
                HSSFWorkbook hSSFWorkbook2007 = null;
                XSSFWorkbook xSSFWorkbook2003 = null;
                try
                {
                    hSSFWorkbook2007 = new HSSFWorkbook(fileStream);
                    workbook = (IWorkbook)hSSFWorkbook2007;
                }
                catch (Exception ex)
                {
                    fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    xSSFWorkbook2003 = new XSSFWorkbook(fileStream);
                    workbook = (IWorkbook)xSSFWorkbook2003;
                    System.Diagnostics.Trace.TraceError(ex.Message);
                }
                if (sheetAttribute != null)
                {
                    tableName = sheetAttribute.Name;
                }
                ISheet sheet = workbook.GetSheet(tableName);
                for (int j = startIndex; j < sheet.LastRowNum; j++)
                {
                    IRow row = sheet.GetRow(j);
                    if (row == null || IsRowEmpty(row))
                    {
                        continue;
                    }
                    else
                    {
                        DataRow newRow = dataTable.NewRow();
                        foreach (PropertyInfo property in properties)
                        {
                            ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                            TableColumnAttribute tableColumnAttribute = property.GetCustomAttribute<TableColumnAttribute>();
                            if (tableColumnAttribute != null)
                            {
                                newRow[tableColumnAttribute.Name] = GetCellValue(row, property, tableColumnAttribute, columnAttribute);
                            }
                        }
                        dataTable.Rows.Add(newRow);
                    }
                }
                workbook.Close();
                fileStream.Close();
                return dataTable;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 判断一行是否为空
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static Boolean IsRowEmpty(IRow row)
        {
            if (row is XSSFRow)
            {
                for (int c = row.FirstCellNum; c < row.LastCellNum; c++)
                {
                    XSSFCell cell = (XSSFCell)row.GetCell(c);
                    if (cell != null && cell.CellType != CellType.Blank)
                    {
                        return false;
                    }
                }
            }
            else if (row is HSSFRow)
            {
                for (int c = row.FirstCellNum; c < row.LastCellNum; c++)
                {
                    HSSFCell cell = (HSSFCell)row.GetCell(c);
                    if (cell != null && cell.CellType != CellType.Blank)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 列处理
        /// </summary>
        /// <param name="ColumnProcess"></param>
        /// <returns></returns>
        public static void ColumnProcess(string columnProcess, string value, ref object returnValue)
        {
            var processStr = columnProcess.Split(',');
            if (processStr.Length > 1)
            {
                string processPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, processStr[0] + ".dll");
                Type type = Assembly.LoadFile(processPath).GetTypes().FirstOrDefault(x => x.Name.Equals(processStr[1]));
                var columnProcessInstance = (IColumnProcess)Activator.CreateInstance(type);
                returnValue = columnProcessInstance.Process(value);
            }
        }
    }
}
