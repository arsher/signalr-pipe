using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Pipes.Ipc.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SignalR.Pipes.Ipc.Client.Internal
{
    internal static class TypedServerBuilder<T>
    {
        private const string ServerModuleName = "SignalR.Pipes.Ipc.Client.TypedServerBuilder";

        private static readonly Lazy<Func<HubConnection, T>> builder = new Lazy<Func<HubConnection, T>>(() => GenerateClientBuilder());

        public static T Build(HubConnection hubConnection)
        {
            return builder.Value(hubConnection);
        }

        private static Func<HubConnection, T> GenerateClientBuilder()
        {
            typeof(T).VerifyInterface();

            var assemblyName = new AssemblyName(ServerModuleName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(ServerModuleName);
            var clientType = GenerateInterfaceImplementation(moduleBuilder);

            return proxy => (T)Activator.CreateInstance(clientType, proxy);
        }

        private static Type GenerateInterfaceImplementation(ModuleBuilder moduleBuilder)
        {
            var type = moduleBuilder.DefineType(
                ServerModuleName + "." + typeof(T).Name + "Impl",
                TypeAttributes.Public,
                typeof(Object),
                new[] { typeof(T) });

            var proxyField = type.DefineField("_proxy", typeof(HubConnection), FieldAttributes.Private);

            InterfaceHelpers.BuildConstructor(type, proxyField, typeof(HubConnection));

            foreach (var method in typeof(T).GetAllInterfaceMethods())
            {
                BuildMethod(type, method, proxyField);
            }

            return type.CreateTypeInfo();
        }

        private static void BuildMethod(TypeBuilder type, MethodInfo interfaceMethodInfo, FieldInfo proxyField)
        {
            var methodAttributes =
                  MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.Final
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot;

            var parameters = interfaceMethodInfo.GetParameters();
            var paramTypes = parameters.Select(param => param.ParameterType).ToArray();

            var methodBuilder = type.DefineMethod(interfaceMethodInfo.Name, methodAttributes);

            var returnType = interfaceMethodInfo.ReturnType;

            Type underlyingReturnType = null;
            if (returnType.IsGenericType)
            {
                underlyingReturnType = returnType.GetGenericArguments().First();
            }

            methodBuilder.SetReturnType(returnType);
            methodBuilder.SetParameters(paramTypes);

            var genericTypeNames =
                paramTypes.Where(p => p.IsGenericParameter).Select(p => p.Name).Distinct().ToArray();

            if (genericTypeNames.Any())
            {
                methodBuilder.DefineGenericParameters(genericTypeNames);
            }

            var generator = methodBuilder.GetILGenerator();
            generator.DeclareLocal(typeof(object[]));

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, proxyField);

            generator.Emit(OpCodes.Ldstr, interfaceMethodInfo.Name);

            generator.Emit(OpCodes.Ldc_I4, parameters.Length);
            generator.Emit(OpCodes.Newarr, typeof(object));
            generator.Emit(OpCodes.Stloc_0);

            for (var i = 0; i < paramTypes.Length; i++)
            {
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Ldarg, i + 1);
                generator.Emit(OpCodes.Box, paramTypes[i]);
                generator.Emit(OpCodes.Stelem_Ref);
            }

            generator.Emit(OpCodes.Ldloc_0);

            //typeof(Microsoft.AspNetCore.SignalR.Client.HubConnectionExtensions)
            //    .GetMethod(nameof(Microsoft.AspNetCore.SignalR.Client.HubConnectionExtensions.InvokeCoreAsync),
            //        new Type[] { typeof(HubConnection), typeof(string), typeof(object[]), typeof(CancellationToken) },)


            //var methodInfo = typeof(InvokeClientProxyExtensions)
            //    .GetMethod(nameof(InvokeClientProxyExtensions.InvokeAsync),
            //        BindingFlags.Static | BindingFlags.Public)
            //    .MakeGenericMethod(underlyingReturnType);

          //  generator.Emit(OpCodes.Call, methodInfo);
            generator.Emit(OpCodes.Ret);
        }
    }
}
