using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Common
{
    /// <summary>
    /// 依赖注入管理接口
    /// </summary>
    public interface IDependencyManager
    {
        /// <summary>
        /// 注入
        /// </summary>
        /// <param name="assemblies"></param>
        void Register(ContainerBuilder containerBuilder, ITypeFinder typeFinder);

        /// <summary>
        /// 注入
        /// </summary>
        /// <param name="objectDI"></param>
        /// <param name="typeFinder"></param>
        void Register(ObjectDI objectDI, ITypeFinder typeFinder);

        /// <summary>
        /// 顺序
        /// </summary>
        int Order { get; set; }
    }
}
