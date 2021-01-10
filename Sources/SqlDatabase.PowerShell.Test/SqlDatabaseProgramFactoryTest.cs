using System;
using System.Collections;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class SqlDatabaseProgramFactoryTest
    {
        [Test]
        [TestCase("Desktop", typeof(SqlDatabaseProgramNet452))]
        [TestCase(null, typeof(SqlDatabaseProgramNet452))]
        [TestCase("Core", typeof(SqlDatabaseProgramNetCore))]
        public void CreateProgram(string psEdition, Type expected)
        {
            var psVersionTable = new Hashtable();
            if (psEdition != null)
            {
                psVersionTable.Add("PSEdition", psEdition);
            }

            var owner = new Mock<ICmdlet>(MockBehavior.Strict);

            var actual = SqlDatabaseProgramFactory.CreateProgram(new PSVersionTable(psVersionTable), owner.Object);

            actual.ShouldBeOfType(expected);
        }
    }
}
