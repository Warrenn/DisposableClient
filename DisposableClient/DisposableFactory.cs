using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace DisposableClient
{
    public class DisposableFactory<T> :
        ChannelFactory<T> where T : class
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

        public DisposableFactory(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {

        }

        public DisposableFactory(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {

        }

        public DisposableFactory(Binding binding)
            : base(binding)
        {

        }

        public DisposableFactory(Binding binding, string remoteAddress)
            : base(binding, remoteAddress)
        {

        }

        public DisposableFactory(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {

        }

        public DisposableFactory(ServiceEndpoint endpoint)
            : base(endpoint)
        {

        }

        public static Action<T> CreateDisposeMethod()
        {
            return DisposeMethod;
        }

        public static void DisposeMethod(T instance)
        {
            var communicationObject = instance as ICommunicationObject;
            if (communicationObject == null) return;
            var state = communicationObject.State;
            if (state == CommunicationState.Closed) return;
            try
            {
                if (state == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                    return;
                }
                communicationObject.Close();
            }
            catch (CommunicationException ex)
            {
                Trace.TraceError(ex.ToString());
                if (state == CommunicationState.Closed) return;
                communicationObject.Abort();
            }
        }

        public static Type CreateDisposableType()
        {
            var contractType = typeof(T);
            var disposeType = typeof(Action<T>);
            var disposeFactoryType = typeof(DisposableFactory<T>);

            var newTypeName = contractType.Name + "_" + Guid.NewGuid().ToString("N");
            var currentDomain = AppDomain.CurrentDomain;
            var aname = Assembly.GetAssembly(disposeFactoryType).GetName();
            var asmBuilder = currentDomain.DefineDynamicAssembly(aname, AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule(newTypeName + "_Module");

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

            var disposeMethodInfo = disposeFactoryType.GetMethod("CreateDisposeMethod");
            var createChannelMethodInfo = disposeFactoryType.GetMethod("CreateChannelFromConfig");

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

        public static T CreateWrapper(T instance, Action<T> dispose = null)
        {
            var type = CreateDisposableType();
            if (dispose == null) dispose = DisposeMethod;
            return (T)Activator.CreateInstance(type, instance, dispose);
        }

        public static ChannelEndpointElement GetEndPointFromConfig()
        {
            var contractType = typeof (T);

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var sectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);
            if (sectionGroup == null)
            {
                throw new ConfigurationErrorsException();
            }

            var client = sectionGroup.Client;
            var endPoint = client.Endpoints.OfType<ChannelEndpointElement>()
                .FirstOrDefault(ep =>
                    (ep.Contract == contractType.Name) ||
                    (ep.Contract == contractType.AssemblyQualifiedName) ||
                    (ep.Contract == contractType.FullName));
            if (endPoint == null)
            {
                throw new ConfigurationErrorsException();
            }
            return endPoint;
        }

        public override T CreateChannel(EndpointAddress address, Uri via)
        {
            var instance = base.CreateChannel(address, via);
            return CreateWrapper(instance);
        }

        public static T CreateChannelFromConfig()
        {
            var element = GetEndPointFromConfig();
            var factory = new ChannelFactory<T>(element.Name);
            return factory.CreateChannel();
        }

        public static T CreateDisposableChannel()
        {
            var element = GetEndPointFromConfig();
            var factory = new DisposableFactory<T>(element.Name);
            return factory.CreateChannel();
        }
    }
}