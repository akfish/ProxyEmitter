using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyEmitter.Test.Dummy
{
    /// <summary>
    /// Dummy ProxyBase implementations for reference
    /// Nothing is implemented.
    /// </summary>
    public class DummyProxyBase : ProxyBase
    {
        #region ProxyBase

        protected override object Invoke(string methodName, object[] arguments)
        {
            if (arguments != null)
                TestContext.WriteLine("{0}({1})", methodName, string.Join(", ", arguments));
            else
                TestContext.WriteLine("{0}()", methodName);
            switch (methodName)
            {
                case "Fn1":
                case "Fn3":
                    return null;
            }
            if (arguments == null)
                return 0;
            int sum = arguments.Cast<int>().Sum();
            return sum;
        }

        protected override TRet ConvertReturnValue<TRet>(object returnValue)
        {
            if (typeof(TRet) != typeof(int))
                return default(TRet);
            return (TRet)returnValue;
        }
        #endregion

        #region For Test

        public TestContext TestContext { get; set; }

        public DummyProxyBase(TestContext testContext)
        {
            TestContext = testContext;
        }
        #endregion
    }
}