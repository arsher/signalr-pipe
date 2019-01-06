using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace SignalR.Pipes.Ipc.Common
{
    public static class InterfaceHelpers
    {
        public static void BuildConstructor(TypeBuilder type, FieldInfo proxyField, Type proxyType)
        {
            var method = type.DefineMethod(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig);

            var ctor = typeof(object).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[] { }, null);

            method.SetReturnType(typeof(void));
            method.SetParameters(proxyType);

            var generator = method.GetILGenerator();

            // Call object constructor
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, ctor);

            // Assign constructor argument to the proxyField
            generator.Emit(OpCodes.Ldarg_0); // type
            generator.Emit(OpCodes.Ldarg_1); // type proxyfield
            generator.Emit(OpCodes.Stfld, proxyField); // type.proxyField = proxyField
            generator.Emit(OpCodes.Ret);
        }

        public static IEnumerable<MethodInfo> GetAllInterfaceMethods(this Type interfaceType)
        {
            foreach (var parent in interfaceType.GetInterfaces())
            {
                foreach (var parentMethod in GetAllInterfaceMethods(parent))
                {
                    yield return parentMethod;
                }
            }

            foreach (var method in interfaceType.GetMethods())
            {
                yield return method;
            }
        }

        public static void VerifyInterface(this Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new InvalidOperationException("Type must be an interface.");
            }

            if (interfaceType.GetProperties().Length != 0)
            {
                throw new InvalidOperationException("Type must not contain properties.");
            }

            if (interfaceType.GetEvents().Length != 0)
            {
                throw new InvalidOperationException("Type must not contain events.");
            }

            foreach (var method in interfaceType.GetMethods())
            {
                VerifyMethod(interfaceType, method);
            }

            foreach (var parent in interfaceType.GetInterfaces())
            {
                VerifyInterface(parent);
            }
        }

        private static void VerifyMethod(Type interfaceType, MethodInfo interfaceMethod)
        {
            if (!typeof(Task).IsAssignableFrom(interfaceMethod.ReturnType))
            {
                throw new InvalidOperationException(
                    $"Cannot generate proxy implementation for '{interfaceType.FullName}.{interfaceMethod.Name}'. All client proxy methods must return '{typeof(Task).FullName}'.");
            }

            foreach (var parameter in interfaceMethod.GetParameters())
            {
                if (parameter.IsOut)
                {
                    throw new InvalidOperationException(
                        $"Cannot generate proxy implementation for '{interfaceType.FullName}.{interfaceMethod.Name}'. Client proxy methods must not have 'out' parameters.");
                }

                if (parameter.ParameterType.IsByRef)
                {
                    throw new InvalidOperationException(
                        $"Cannot generate proxy implementation for '{interfaceType.FullName}.{interfaceMethod.Name}'. Client proxy methods must not have 'ref' parameters.");
                }
            }
        }
    }
}
