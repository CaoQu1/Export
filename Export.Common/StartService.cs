using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace Export.Common
{
    /// <summary>
    /// 快捷启动接口
    /// </summary>
    public interface IStartService
    {
    }

    /// <summary>
    /// 启动服务类
    /// </summary>
    public class StartService : IStartService
    {
        /// <summary>
        /// 私有构造
        /// </summary>
        private StartService()
        {
            //ContainerBuilder containerBuilder = new ContainerBuilder();
            //var typeFinder = new AppDomainTypeFinder();
            //containerBuilder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();
            //Container = containerBuilder.Build();
            ObjectDI objectDI = new ObjectDI();
            var typeFinder = new AppDomainTypeFinder();
            objectDI.Register<ITypeFinder>(typeFinder);
            var drTypes = typeFinder.FindClassesOfType<IDependencyManager>();
            var drInstances = new List<IDependencyManager>();
            foreach (var item in drTypes)
            {
                drInstances.Add((IDependencyManager)Activator.CreateInstance(item));
            }
            drInstances = drInstances.AsQueryable().OrderBy(x => x.Order).ToList();
            foreach (var item in drInstances)
            {
                item.Register(objectDI, typeFinder);
            }
            Container = objectDI;
        }

        /// <summary>
        /// 惰性单例
        /// </summary>
        public static StartService Instance => new Lazy<StartService>(() => new StartService()).Value;

        /// <summary>
        /// 容器对象
        /// </summary>
        //public IContainer Container { get; private set; }

        /// <summary>
        /// 容器对象
        /// </summary>
        public ObjectDI Container { get; private set; }

        /// <summary>
        /// 开启服务
        /// </summary>
        public StartService Start()
        {
            return this;
        }
    }
}
