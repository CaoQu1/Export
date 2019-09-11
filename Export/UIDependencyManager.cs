using Autofac;
using Export.Common;
using Export.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Export
{
    /// <summary>
    /// 注入窗体类
    /// </summary>
    public class UIDependencyManager : IDependencyManager
    {
        /// <summary>
        /// 注入顺序
        /// </summary>
        public int Order { get; set; } = 1;

        /// <summary>
        /// 注入接口
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <param name="assemblies"></param>
        public void Register(ContainerBuilder containerBuilder, ITypeFinder typeFinder)
        {
            containerBuilder.RegisterType<SqlHelper>().As<ISqlExecute>().InstancePerDependency();
            containerBuilder.RegisterType<DbContext>().As<IDbContext>().InstancePerDependency();
            containerBuilder.RegisterType<Export>().As<Form>().InstancePerDependency();
        }
    }
}
