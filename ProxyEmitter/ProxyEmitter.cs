using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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
            return (TInterface)CreateProxy(typeof(TBase), typeof(TInterface));
        }

        #endregion

        #region Emitting

        private static Type EmitProxyType(Type baseType, Type interfaceType)
        {
            AssemblyBuilder asmBuilder = Emitter.GetAssemblyBuilder("DynamicProxyAssembly");
            ModuleBuilder mBuilder = Emitter.GetModule(asmBuilder, "EmittedProxies");
            TypeBuilder tBuilder = Emitter.GetType(mBuilder, String.Format("{0}{1}Proxy", baseType.FullName, interfaceType), baseType, new[] { interfaceType });

            // TODO: emit parametrized ctor 
            var invokeMethod = baseType.GetMethod("Invoke");
            var convertMethod = baseType.GetMethod("ConvertReturnValue");
            // Create methods in interfaceType
            foreach (var method in interfaceType.GetMethods())
            {
                EmitInterfaceMethod(tBuilder, method, invokeMethod, convertMethod);
            }

            return tBuilder.CreateType();
        }

        private static void EmitInterfaceMethod(TypeBuilder tBuilder, MethodInfo method, MethodInfo invokeMethod, MethodInfo convertMethod)
        {
            #region Emit Signatue
            //  .method public hidebysig virtual instance void 
            //      MethodName(xxx) cil managed
            //  {
            var pTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            MethodBuilder builder = Emitter.GetMethod(
                tBuilder,
                method.Name,
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.Virtual,
                method.ReturnType,
                pTypes);
            #endregion

            var ilGen = builder.GetILGenerator();


            EmitLocals(ilGen, method, pTypes);

            EmitHead(ilGen, method);

            EmitInvokeArguments(ilGen, method, pTypes);

            EmitInvoke(ilGen, method, invokeMethod);

            EmitReturn(ilGen, method, convertMethod);
        }

        private static void EmitLocals(ILGenerator ilGen, MethodInfo method, Type[] pTypes)
        {
            // Has return value
            if (method.ReturnType != typeof(void))
            {
                ilGen.DeclareLocal(method.ReturnType);
            }
            // Has parameters
            if (pTypes.Length > 0)
                ilGen.DeclareLocal(typeof(object[]));

        }

        private static void EmitHead(ILGenerator ilGen, MethodInfo method)
        {
            // IL_0000: nop
            ilGen.Emit(OpCodes.Nop);
            // IL_0001: ldarg.0
            ilGen.Emit(OpCodes.Ldarg_0);
            // Twice if has return value
            // IL_0002: ldarg.0
            if (method.ReturnType != typeof(void))
                ilGen.Emit(OpCodes.Ldarg_0);
        }

        private static void EmitInvokeArguments(ILGenerator ilGen, MethodInfo method, Type[] pTypes)
        {
            // First one, method name
            // IL_0002: ldstr "Fn2"
            ilGen.Emit(OpCodes.Ldstr, method.Name);

            // Has arguments?
            if (pTypes.Length == 0)
            {
                // No? Easy
                // IL_0007: ldnull
                ilGen.Emit(OpCodes.Ldnull);
            }
            else
            {
                // Yes? So much work
                // Initialize array
                // IL_0006:  ldc.i4.x
                ilGen.Emit(OpCodes.Ldc_I4_S, pTypes.Length);
                // IL_0007:  newarr     [mscorlib]System.Object
                ilGen.Emit(OpCodes.Newarr, typeof(Object));

                // If has return value, array is local 1. Otherwise 0.
                var hasReturn = method.ReturnType != typeof(void);
                var set_op = hasReturn ? OpCodes.Stloc_1 : OpCodes.Stloc_0;
                var get_op = hasReturn ? OpCodes.Ldloc_1 : OpCodes.Stloc_0;
                // IL_000c:  stloc.1
                ilGen.Emit(set_op);

                // More work coming. 
                // Now fill the array
                for (int i = 0; i < pTypes.Length; i++)
                {
                    // Load the array first
                    // IL_000X + 00: ldloc.1
                    ilGen.Emit(get_op);

                    // Push the index
                    // IL_000X + 01: ldc_i4_x
                    ilGen.Emit(OpCodes.Ldc_I4_S, i);
                    // Load argument i + 1 (note that argument 0 is this pointer(?))
                    // IL_000X + 02: ldarg_X
                    ilGen.Emit(OpCodes.Ldarga_S, i + 1);
                    // Box value type
                    if (pTypes[i].IsValueType)
                    {
                        // IL_000X + 03: box pTypes[i]
                        ilGen.Emit(OpCodes.Box, pTypes[i]);
                    }
                    // Set arrary element
                    // IL_00X + ??: stelem.ref
                    ilGen.Emit(OpCodes.Stelem_Ref);
                }

                // Load array
                // IL_00XX: ldloc.1
                ilGen.Emit(get_op);
            }
        }

        private static void EmitInvoke(ILGenerator ilGen, MethodInfo method, MethodInfo invokeMethod)
        {
            // IL_0022: callvirt instance object ProxyEmitter.ProxyBase::Invoke(string, object[])
            ilGen.Emit(OpCodes.Callvirt, invokeMethod);
        }

        private static void EmitReturn(ILGenerator ilGen, MethodInfo method, MethodInfo convertMethod)
        {
            var hasReturn = method.ReturnType != typeof(void);
            if (hasReturn)
            {
                // IL_000e: callvirt instance !!0 ProxyEmitter.ProxyBase::ConvertReturnValue<int32>(object)
                ilGen.EmitCall(OpCodes.Callvirt, convertMethod, new[] { method.ReturnType });
                // IL_0013: stloc.0
                ilGen.Emit(OpCodes.Stloc_0);
                // IL_0016: ldloc.0
                ilGen.Emit(OpCodes.Ldloc_0);
            }
            else
            {
                // IL_000d: pop
                ilGen.Emit(OpCodes.Pop);
            }
            // IL_000e: ret   
            ilGen.Emit(OpCodes.Ret);
        }

        #endregion
    }
}