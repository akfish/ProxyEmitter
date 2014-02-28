using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyEmitter.Test.Dummy;

namespace ProxyEmitter.Test
{
    [TestClass]
    public class EmitterTest
    {
        public TestContext TestContext { get; set; }
        [TestMethod]
        public void TestEmit()
        {
            var service = ProxyEmitter.CreateProxy<DummyProxyBase, IDummyService>(TestContext);
            service.Fn1();
            service.Fn3(0, 0);
            Assert.AreEqual(0, service.Fn2());
            Assert.AreEqual(3, service.Fn4(1, 2));
            Assert.AreEqual(10, service.Fn5(1, 2, 3, 4));
        }
    }
}
