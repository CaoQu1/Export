using Microsoft.VisualStudio.TestTools.UnitTesting;
using Export.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Export.Common.Tests;
using Autofac;
using Export.Model;
using System.Data;
using Export.Test;

namespace Export.Data.Tests
{
    [TestClass()]
    public class DbContextTests
    {
        private static IDbContext DbContext { get; set; }

        [ClassInitialize]
        public static void Begin(TestContext testContext)
        {
            DbContext = StartServiceTests.Container.Resolve<IDbContext>();
        }

        [TestMethod]
        public void SetTestTrue()
        {
            Assert.IsTrue(DbContext.Set<TestEntity>() != null);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BatchInsertTestFalse()
        {
            DbContext.BatchInsert<TestEntity>(null);
        }


        [TestMethod()]
        public void BatchInsertTestThrow()
        {
            ArgumentNullException argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => DbContext.BatchInsert<TestEntity>(new DataTable()));
            Assert.IsInstanceOfType(argumentNullException, typeof(ArgumentNullException));
        }

        [TestMethod()]
        public void BatchInsertTestTrue()
        {
            IDbContext dbContext = DbContext.BatchInsert<TestEntity>(TableGenarate.GetTable());
            Assert.IsNotNull(dbContext);
        }
    }
}