using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Common
{
    /// <summary>
    /// 列处理基类
    /// </summary>
    public abstract class ColumnProcess : IColumnProcess
    {
        /// <summary>
        /// 列处理
        /// </summary>
        /// <param name="columnValue"></param>
        /// <returns></returns>
        public abstract object Process(string columnValue);
    }

    /// <summary>
    /// Excel列处理接口
    /// </summary>
    public interface IColumnProcess
    {
        /// <summary>
        /// 处理值
        /// </summary>
        /// <param name="columnValue"></param>
        /// <returns></returns>
        object Process(string columnValue);
    }
}
