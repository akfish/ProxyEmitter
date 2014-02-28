using System;

namespace ProxyEmitter
{
    /// <summary>
    /// Specifies the namespace of Proxy methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ProxyNamespaceAttribute : Attribute
    {
        public string Namespace { get; private set; }

        public ProxyNamespaceAttribute(string @namespace)
        {
            Namespace = @namespace;
        }
    }
}