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
    [Table("mstest", "自定义表注释")]
    public class TestEntity
    {
        /// <summary>
        /// 姓名
        /// </summary>
        [TableColumn(Name = "name", Comments = "姓名")]
        public string Name { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        [TableColumn(Name = "age", Comments = "年龄")]
        public int Age { get; set; }

    }
}
