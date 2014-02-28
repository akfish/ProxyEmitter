using System;
using System.Collections.Generic;
using System.Reflection;

namespace ProxyEmitter
{
    public static class ProxyEmitter
    {
        #region Type Caches

        /// <summary>
        /// Internal cache of generated Proxy classes. First mapped by ProxyBase type, then interface type
        /// </summary>
        internal static Dictionary<Type, Dictionary<Type, Type>> ProxyTypes = new Dictionary<Type, Dictionary<Type, Type>>();

        /// <summary>
        /// Get cached Proxy class type
        /// </summary>
        /// <param name="baseType">Type of derived <see cref="ProxyBase"/> class</param>
        /// <param name="interfaceType">Type of interface</param>
        /// <returns>Null if the queried type is not present in cache</returns>
        internal static Type GetCachedType(Type baseType, Type interfaceType)
        {
            Dictionary<Type, Type> interfaceTypes;
            if (ProxyTypes.TryGetValue(baseType, out interfaceTypes))
            {
                if (interfaceTypes != null && interfaceTypes.ContainsKey(interfaceType))
                    return interfaceTypes[interfaceType];
            }
            return null;
        }

        /// <summary>
        /// Get cached Proxy class type
        /// </summary>
        /// <typeparam name="TBase">Type of derived <see cref="ProxyBase"/> class</typeparam>
        /// <typeparam name="TInterface">Type of interface</typeparam>
        /// <returns>Null if the queried type is not present in cache</returns>
        internal static Type GetCachedProxyType<TBase, TInterface>()
        {
            return GetCachedType(typeof(TBase), typeof(TInterface));
        }

        /// <summary>
        /// Cache generated Proxy class
        /// </summary>
        /// <param name="baseType">Type of derived <see cref="ProxyBase"/> class</param>
        /// <param name="interfaceType">Type of interface</param>
        /// <param name="proxyType">Generated Proxy class type</param>
        internal static void CacheProxyType(Type baseType, Type interfaceType, Type proxyType)
        {
            if (!ProxyTypes.ContainsKey(baseType))
                ProxyTypes.Add(baseType, new Dictionary<Type, Type>());
            ProxyTypes[baseType].Add(interfaceType, proxyType);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create Proxy class type
        /// </summary>
        /// <param name="baseType">Type of derived <see cref="ProxyBase"/> class</param>
        /// <param name="interfaceType">Type of interface</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="baseType"/> is not derived from <see cref="ProxyBase"/>,
        /// nor <paramref name="interfaceType"/> is not an interface.
        /// </exception>
        /// <returns>Type of Proxy class that drived from both <paramref name="baseType"/> and <paramref name="interfaceType"/></returns>
        public static Type CreateType(Type baseType, Type interfaceType)
        {
            // Validate
            if (!interfaceType.IsInterface)
                throw new ArgumentException("Invalid interface type");
            if (!baseType.IsSubclassOf(typeof(ProxyBase)))
                throw new ArgumentException("Base type is not derived from ProxyBase");
            var type = GetCachedType(baseType, interfaceType);
            // Look up cache first
            if (type != null)
                return type;
            // Not cached, create
            type = EmitProxyType(baseType, interfaceType);
            // Cache
            CacheProxyType(baseType, interfaceType, type);
            return type;
        }

        /// <summary>
        /// Create Proxy class type
        /// </summary>
        /// <typeparam name="TBase">Type of derived <see cref="ProxyBase"/> class</typeparam>
        /// <typeparam name="TInterface">Type of interface</typeparam>
        /// <exception cref="ArgumentException">
        /// Thrown if <typeparamref name="TInterface"/> is not an interface.
        /// </exception>
        /// <returns>Type of Proxy class that drived from both <typeparamref name="TBase"/> and <typeparamref name="TInterface"/></returns>
        public static Type CreateType<TBase, TInterface>()
            where TBase : ProxyBase
            where TInterface : class
        {
            return CreateType(typeof(TBase), typeof(TInterface));
        }

        /// <summary>
        /// Create Proxy class instance
        /// </summary>
        /// <param name="baseType">Type of derived <see cref="ProxyBase"/> class</param>
        /// <param name="interfaceType">Type of interface</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="baseType"/> is not derived from <see cref="ProxyBase"/>,
        /// nor <paramref name="interfaceType"/> is not an interface.
        /// </exception>
        /// <returns>Instance of Proxy class that drived from both <paramref name="baseType"/> and <paramref name="interfaceType"/></returns>
        public static object CreateProxy(Type baseType, Type interfaceType)
        {
            var type = CreateType(baseType, interfaceType);
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Create Proxy class instance
        /// </summary>
        /// <typeparam name="TBase">Type of derived <see cref="ProxyBase"/> class</typeparam>
        /// <typeparam name="TInterface">Type of interface</typeparam>
        /// <exception cref="ArgumentException">
        /// Thrown if <typeparamref name="TInterface"/> is not an interface.
        /// </exception>
        /// <returns>Instance of Proxy class that drived from both <typeparamref name="TBase"/> and <typeparamref name="TInterface"/></returns>
        public static TInterface CreateProxy<TBase, TInterface>()
            where TBase : ProxyBase
            where TInterface : class
        {
            return (TInterface) CreateProxy(typeof(TBase), typeof(TInterface));
        }

        #endregion

        #region Emitting

        private static Type EmitProxyType(Type baseType, Type interfaceType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}