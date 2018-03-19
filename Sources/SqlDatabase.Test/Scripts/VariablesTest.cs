using NUnit.Framework;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class VariablesTest
    {
        private Variables _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new Variables();
        }

        [Test]
        public void GetValueUsesEnvironmentVariables()
        {
            const string Key = "TEMP";

            Assert.IsNotNull(_sut.GetValue(Key));
            Assert.AreNotEqual("new value", _sut.GetValue(Key));

            _sut.SetValue("temp", "new value");

            Assert.AreEqual("new value", _sut.GetValue(Key));
        }

        [Test]
        public void NullValue()
        {
            const string Key = "some name";

            Assert.IsNull(_sut.GetValue(Key));

            _sut.SetValue(Key, "1");
            Assert.AreEqual("1", _sut.GetValue(Key));

            _sut.SetValue(Key, string.Empty);
            Assert.AreEqual(string.Empty, _sut.GetValue(Key));

            _sut.SetValue(Key, null);
            Assert.IsNull(_sut.GetValue(Key));
        }
    }
}
