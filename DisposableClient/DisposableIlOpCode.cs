using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DisposableClient
{
    public static class DisposableIlOpCode<T> where T : class
    {
        private const MethodAttributes MethodAttributes =
            System.Reflection.MethodAttributes.Public |
            System.Reflection.MethodAttributes.HideBySig |
            System.Reflection.MethodAttributes.Final |
            System.Reflection.MethodAttributes.Virtual |
            System.Reflection.MethodAttributes.NewSlot;

        private const CallingConventions CallingConventions =
            System.Reflection.CallingConventions.HasThis |
            System.Reflection.CallingConventions.ExplicitThis;

        public static T WrapInstance(T instance, Action<T> dispose = null)
        {
            var type = CreateType();
            if (dispose == null) dispose = DisposeMethod.DisposeCommunicationObject;
            return (T)Activator.CreateInstance(type, instance, dispose);
        }
        
        public static Action<T> CreateDisposeMethod()
        {
            return DisposeMethod.DisposeCommunicationObject;
        }

        public static Type CreateType()
        {
            var contractType = typeof(T);
            var disposeType = typeof(Action<T>);
            var disposableIlOpCodeType = typeof(DisposableIlOpCode<T>);
            var configType = typeof(ConfigChannelFactory<T>);


            var newTypeName = contractType.Name + "_IlOpCode" + Guid.NewGuid().ToString("N");
            var currentDomain = AppDomain.CurrentDomain;
            var aname = Assembly.GetAssembly(disposableIlOpCodeType).GetName();
            var asmBuilder = currentDomain.DefineDynamicAssembly(aname, AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule(newTypeName + "_IlOpCodeModule");

            var classType = (contractType.IsInterface) ? typeof(object) : contractType;
            var interfaceTypes = (contractType.IsInterface)
                ? new[] { contractType, typeof(IDisposable) }
                : new[] { typeof(IDisposable) };

            var tbuilder = modBuilder.DefineType(newTypeName,
                TypeAttributes.Class | TypeAttributes.Public,
                classType, interfaceTypes);

            var instanceBuilder = tbuilder.DefineField("instance", contractType, FieldAttributes.Private);
            var disposeMethodBuilder = tbuilder.DefineField("disposeMethod", disposeType, FieldAttributes.Private);
            var superConstructor = typeof(Object).GetConstructor(Type.EmptyTypes);

            var disposeMethodInfo = disposableIlOpCodeType.GetMethod("CreateDisposeMethod");
            var createChannelMethodInfo = configType.GetMethod("CreateChannel", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);

            var defaultCtor = tbuilder.DefineConstructor(
                MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var defaultCtorIl = defaultCtor.GetILGenerator();

            defaultCtorIl.Emit(OpCodes.Ldarg_0);
            defaultCtorIl.Emit(OpCodes.Call, superConstructor);

            defaultCtorIl.Emit(OpCodes.Ldarg_0);
            defaultCtorIl.EmitCall(OpCodes.Call, disposeMethodInfo, null);
            defaultCtorIl.Emit(OpCodes.Stfld, disposeMethodBuilder);

            defaultCtorIl.Emit(OpCodes.Ldarg_0);
            defaultCtorIl.EmitCall(OpCodes.Call, createChannelMethodInfo, null);
            defaultCtorIl.Emit(OpCodes.Stfld, instanceBuilder);
            defaultCtorIl.Emit(OpCodes.Nop);
            defaultCtorIl.Emit(OpCodes.Ret);

            var constructorArgs = new[] { contractType, disposeType };

            var paramCtor = tbuilder.DefineConstructor(
               MethodAttributes.Public, CallingConventions.Standard, constructorArgs);
            var paramCtorIl = paramCtor.GetILGenerator();

            paramCtorIl.Emit(OpCodes.Ldarg_0);
            paramCtorIl.Emit(OpCodes.Call, superConstructor);

            paramCtorIl.Emit(OpCodes.Ldarg_0);
            paramCtorIl.Emit(OpCodes.Ldarg_1);
            paramCtorIl.Emit(OpCodes.Stfld, instanceBuilder);
            paramCtorIl.Emit(OpCodes.Ldarg_0);
            paramCtorIl.Emit(OpCodes.Ldarg_2);
            paramCtorIl.Emit(OpCodes.Stfld, disposeMethodBuilder);
            paramCtorIl.Emit(OpCodes.Ret);

            foreach (var methodInfo in contractType.GetMethods())
            {
                var isVoid = (methodInfo.ReturnType == typeof(void));
                var parameterTypes = methodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray();
                var parameterTypesParam = (parameterTypes.Any()) ? parameterTypes : null;
                var returnType = (isVoid) ? null : methodInfo.ReturnType;

                var methodBuilder = tbuilder
                    .DefineMethod(
                        methodInfo.Name,
                        MethodAttributes,
                        CallingConventions,
                        returnType,
                        parameterTypesParam);
                var generator = methodBuilder.GetILGenerator();

                methodBuilder.SetImplementationFlags(MethodImplAttributes.Managed);
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, instanceBuilder);

                for (var index = 1; index <= parameterTypes.Length; index++)
                {
                    generator.Emit(OpCodes.Ldarg_S, index);
                }

                generator.EmitCall(OpCodes.Callvirt, methodInfo, null);
                if (isVoid) generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Ret);
            }

            var disposeBuilder = tbuilder.DefineMethod("Dispose", MethodAttributes, CallingConventions, typeof(void), null);
            var disposeGenerator = disposeBuilder.GetILGenerator();

            disposeBuilder.SetImplementationFlags(MethodImplAttributes.Managed);
            var invokeMethod = disposeType.GetMethod("Invoke");

            disposeGenerator.Emit(OpCodes.Nop);
            disposeGenerator.Emit(OpCodes.Ldarg_0);
            disposeGenerator.Emit(OpCodes.Ldfld, disposeMethodBuilder);
            disposeGenerator.Emit(OpCodes.Ldarg_0);
            disposeGenerator.Emit(OpCodes.Ldfld, instanceBuilder);
            disposeGenerator.EmitCall(OpCodes.Callvirt, invokeMethod, null);
            disposeGenerator.Emit(OpCodes.Nop);
            disposeGenerator.Emit(OpCodes.Ret);

            return tbuilder.CreateType();
        }
    }
}
