using Microsoft.VisualStudio.TestTools.UnitTesting;
using Export.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Export.Common.Tests;
using Autofac;
using System.Data;
using Export.Test;

namespace Export.Data.Tests
{
    [TestClass()]
    public class SqlHelperTests
    {
        private static ISqlExecute sqlExecute { get; set; }

        [ClassInitialize]
        public static void Begin(TestContext testContext)
        {
            sqlExecute = StartServiceTests.Container.Resolve<ISqlExecute>();
        }

        [TestMethod()]
        public void BatchInsertTestFalse()
        {
            Assert.ThrowsException<ArgumentNullException>(() => sqlExecute.BatchInsert(null));
        }

        [TestMethod()]
        public void BatchInsertTestTrue()
        {
            sqlExecute.BatchInsert(TableGenarate.GetTable());
            Assert.IsTrue(true);
        }
    }
}