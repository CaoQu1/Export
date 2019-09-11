using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Common
{
    /// <summary>
    /// excel列特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }

        public int Index { get; set; }

        public string ColumnProcess { get; set; }

        public ColumnAttribute()
        {
        }

        public ColumnAttribute(string name, int index, string columnProcess = "")
        {
            this.Name = name;
            this.Index = index;
            this.ColumnProcess = columnProcess;
        }
    }

    /// <summary>
    /// excel行特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RowAttribute : Attribute
    {
        public int StartRow { get; set; }

        public RowAttribute()
        { }

        public RowAttribute(int startRow)
        {
            this.StartRow = startRow;
        }
    }

    /// <summary>
    /// excel页特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SheetAttribute : Attribute
    {
        public string Name { get; set; }

        public SheetAttribute() { }

        public SheetAttribute(string Name)
        {
            this.Name = Name;
        }
    }

    /// <summary>
    /// 数据库表特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }

        public string Comments { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        public TableAttribute(string name, string Comments = "")
        {
            this.Name = name;
            this.Comments = Comments;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public TableAttribute() { }
    }

    /// <summary>
    /// 数据库列特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TableColumnAttribute : Attribute
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 列类型
        /// </summary>
        public TableColumnType ColumnType { get; set; } = TableColumnType.None;

        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIdentity { get; set; } = false;

        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 列注释
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// 外键描述 eg:User(No)
        /// </summary>
        public string ForeignKeyDec { get; set; }

        /// <summary>
        /// 列处理
        /// </summary>
        public string TableColumnProcess { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tableColumnType"></param>
        /// <param name="isIdentity"></param>
        /// <param name="defaultValue"></param>
        public TableColumnAttribute(string name, TableColumnType tableColumnType = TableColumnType.None, bool isIdentity = false, string defaultValue = "", string Comments = "", string foreignKeyDec = "", string tableColumnProcess = "")
        {
            this.Name = name;
            this.ColumnType = tableColumnType;
            this.IsIdentity = isIdentity;
            this.DefaultValue = defaultValue;
            this.Comments = Comments;
            this.ForeignKeyDec = foreignKeyDec;
            this.TableColumnProcess = tableColumnProcess;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public TableColumnAttribute() { }
    }

    /// <summary>
    /// 列类型
    /// </summary>
    public enum TableColumnType
    {
        PrimaryKey,
        ForeignKey,
        None
    }
}
