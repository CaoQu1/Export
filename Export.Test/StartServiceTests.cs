using Microsoft.VisualStudio.TestTools.UnitTesting;
using Export.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace Export.Common.Tests
{
    [TestClass()]
    public class StartServiceTests
    {
        public static IContainer Container { get; set; }

        /// <summary>
        /// 所有单元测试开始前
        /// </summary>
        /// <param name="testContext"></param>
        [AssemblyInitialize]
        public static void Begin(TestContext testContext)
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            var typeFinder = new AppDomainTypeFinder();
            containerBuilder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();
            //var drTypes = typeFinder.FindClassesOfType<IDependencyManager>();
            //var drInstances = new List<IDependencyManager>();
            //foreach (var item in drTypes)
            //{
            //    drInstances.Add((IDependencyManager)Activator.CreateInstance(item));
            //}
            //drInstances = drInstances.AsQueryable().OrderBy(x => x.Order).ToList();
            //foreach (var item in drInstances)
            //{
            //    item.Register(containerBuilder, typeFinder);
            //}
            IDependencyManager dependencyManager = new UIDependencyManager();
            dependencyManager.Register(containerBuilder, typeFinder: typeFinder);
            Container = containerBuilder.Build();
            Assert.IsTrue(true);
        }

        /// <summary>
        /// 启动测试
        /// </summary>

        [TestMethod()]
        public void StartTest()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// 所有单元测试结束后
        /// </summary>
        [AssemblyCleanup]
        public static void End()
        {

        }
    }
}