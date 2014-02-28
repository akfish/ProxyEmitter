using System;

namespace ProxyEmitter.Dummy
{
    /// <summary>
    /// Dummy ProxyBase implementations for reference
    /// Nothing is implemented.
    /// </summary>
    class DummyProxyBase : ProxyBase
    {
        protected override object Invoke(string methodName, object[] arguments)
        {
            throw new NotImplementedException();
        }

        protected override TRet ConvertReturnValue<TRet>(object returnValue)
        {
            throw new NotImplementedException();
        }
    }
}