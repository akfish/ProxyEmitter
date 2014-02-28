namespace ProxyEmitter.Dummy
{
    class DummyService : DummyProxyBase, IDummyService
    {
        public void Fn1()
        {
            Invoke("Fn1", null);
        }

        public int Fn2()
        {
            return ConvertReturnValue<int>(Invoke("Fn2", null));
        }

        public void Fn3(int a, int b)
        {
            Invoke("Fn2", new object[] { a, b });
        }

        public int Fn4(int a, int b)
        {
            return ConvertReturnValue<int>(Invoke("Fn4", new object[] { a, b }));
        }

        public int Fn5(int a, int b, int c, int d)
        {
            return ConvertReturnValue<int>(Invoke("Fn5", new object[] { a, b, c, d }));
        }
    }
}