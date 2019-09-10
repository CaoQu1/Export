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

        public ColumnAttribute()
        {
        }

        public ColumnAttribute(string name, int index)
        {
            this.Name = name;
            this.Index = index;
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
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }

        public TableAttribute(string name)
        {
            this.Name = name;
        }

        public TableAttribute() { }
    }

    /// <summary>
    /// 数据库列特性
    /// </summary>
    public class TableColumnAttribute : Attribute
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        public TableColumnAttribute(string name)
        {
            this.Name = name;
        }

        public TableColumnAttribute() { }
    }
}
