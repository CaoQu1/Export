using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Common
{
    /// <summary>
    /// 去除重复行
    /// </summary>
    public class DataRowEqualityComparer : IEqualityComparer<DataRow>
    {
        /// <summary>
        /// 是否相等
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(DataRow x, DataRow y)
        {
            return x["real_name"].ToString().Equals(y["real_name"].ToString());
        }

        /// <summary>
        /// 返回hashcode
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(DataRow obj)
        {
            return obj["real_name"].ToString().GetHashCode();
        }
    }
}
