using Export.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Model
{
    /// <summary>
    /// 导入模型
    /// </summary>
    [Table("caoqu", "自定义表注释")]
    [Sheet("Sheet1")]
    [Row(StartRow = 1)]
    public class ExportEntity : BaseEntity<Guid>
    {
        /// <summary>
        /// 序号
        /// </summary>
        [TableColumn(Name = "row_id", Comments = "序号")]
        [Column(Name = "序号", Index = 0)]
        public int RowId { get; set; }

        /// <summary>
        /// eolinker用户名
        /// </summary>
        [TableColumn(Name = "eolinker_name", Comments = "eolinker用户名")]
        [Column(Name = "eolinker用户名", Index = 1)]
        public string EolinkerName { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [TableColumn(Name = "real_name", Comments = "真实姓名")]
        [Column(Name = "真实姓名", Index = 2)]
        public string RealName { get; set; }

        /// <summary>
        /// 是否新注册
        /// </summary>
        [TableColumn(Name = "new_regist", Comments = "是否新注册")]
        [Column(Name = "是否新注册", Index = 3)]
        public string NewRegist { get; set; }

        /// <summary>
        /// 入职时间
        /// </summary>
        [TableColumn(Name = "time", Comments = "入职时间")]
        [Column(Name = "入职时间", Index = 4)]
        public DateTime Time { get; set; }

        /// <summary>
        /// 职称
        /// </summary>
        [TableColumn(Name = "level", Comments = "职称")]
        [Column(Name = "职称", Index = 5)]
        public string Level { get; set; }

        /// <summary>
        /// 组别
        /// </summary>
        [TableColumn(Name = "groups", Comments = "组别")]
        [Column(Name = "组别", Index = 6)]
        public string Groups { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        [TableColumn(Name = "age", Comments = "年龄")]
        [Column("年龄", 7, "Export.Model,AgeColumnProcess")]
        public int Age { get; set; }

        /// <summary>
        /// 处理age
        /// </summary>
        public override object Before(object fieldValue)
        {
            return fieldValue;
        }
    }

    /// <summary>
    /// age列处理
    /// </summary>
    public class AgeColumnProcess : ColumnProcess
    {
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="columnValue"></param>
        /// <returns></returns>
        public override object Process(string columnValue)
        {
            return columnValue.Replace("岁", "");
        }
    }

    /// <summary>
    /// uuid列处理
    /// </summary>
    public class UuidColumnProcess : ColumnProcess
    {
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="columnValue"></param>
        /// <returns></returns>
        public override object Process(string columnValue)
        {
            return Guid.NewGuid().ToString();
        }
    }
}
