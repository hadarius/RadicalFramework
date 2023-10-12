using System.Reflection;
using System.Reflection.Emit;
using Radical.Series;
using Radical.Uniques;

namespace Radical.Invoking
{
    #region Delegates

    public delegate object InvokerDelegate(object target, params object[] parameters);

    public delegate R InvokerDelegate<T, R>(T target, params object[] parameters);

    #endregion

    public class Invoker<T> : Invoker
    {
        public Invoker() : base(typeof(T))
        {
        }
        public Invoker(params object[] constructorParams) : base(typeof(T), constructorParams)
        {
        }
        public Invoker(Func<T, string> method) : base(typeof(T), method(typeof(T).New<T>()))
        {
        }
        public Invoker(Func<T, string> method, params object[] constructorParams) : base(typeof(T), method(typeof(T).New<T>(constructorParams)), constructorParams)
        {

        }
        public Invoker(Func<T, string> method, params Type[] parameterTypes) : base(typeof(T), method(typeof(T).New<T>()), parameterTypes)
        {

        }
        public Invoker(T targetObject, Func<T, string> method) : base(targetObject, method(targetObject))
        {
        }
        public Invoker(Type[] parameterTypes, params object[] constructorParams) : base(typeof(T), parameterTypes, constructorParams)
        {
        }
        public Invoker(string methodName) : base(typeof(T), methodName)
        {
        }
        public Invoker(string methodName, params Type[] parameterTypes) : base(typeof(T), methodName, parameterTypes)
        {
        }
        public Invoker(string methodName, Type[] parameterTypes, params object[] constructorParams) : base(typeof(T), methodName, parameterTypes, constructorParams)
        {
        }
        public Invoker(string methodName, params object[] constructorParams) : base(typeof(T), methodName, constructorParams)
        {
        }
    }

    public class Invoker : IInvoker
    {
        private Uscn serialcode;
        private event InvokerDelegate routine;

        public Invoker()
        {
        }
        public Invoker(Type targetType) : this(targetType.GetMethods().FirstOrDefault(m => m.IsPublic)) { }
        public Invoker(Type targetType, Type[] parameterTypes)
            : this(targetType.GetMethods().Where(m => m.IsPublic && m.GetParameters().All(p => parameterTypes.Contains(p.ParameterType))).FirstOrDefault()) { }
        public Invoker(Type targetType, Type[] parameterTypes, params object[] constructorParameters)
            : this(targetType.GetMethods()
                .FirstOrDefault(m => m.IsPublic && m.GetParameters().
                    All(p => parameterTypes
                        .Contains(p.ParameterType))),
                constructorParameters)
        { }
        public Invoker(Type targetType, params object[] constructorParameters)
            : this(targetType.GetMethods().FirstOrDefault(m => m.IsPublic), constructorParameters) { }
        public Invoker(Delegate targetMethod)
        {
            TargetObject = targetMethod.Target;
            Type t = TargetObject.GetType();
            MethodInfo m = targetMethod.Method;

            NumberOfArguments = m.GetParameters().Length;
            Info = m;
            Parameters = m.GetParameters();
            createUniqueKey();
        }
        public Invoker(object targetObject, string methodName)
            : this(targetObject, methodName, null) { }
        public Invoker(object targetObject, string methodName, params Type[] parameterTypes)
        {
            TargetObject = targetObject;
            Type t = TargetObject.GetType();

            MethodInfo m = parameterTypes != null ? t.GetMethod(methodName, parameterTypes) : t.GetMethod(methodName);

            NumberOfArguments = m.GetParameters().Length;
            Info = m;
            Parameters = m.GetParameters();
            createUniqueKey();
        }
        public Invoker(Type targetType, string methodName)
            : this(Instances.New(targetType), methodName, null) { }
        public Invoker(Type targetType, string methodName, params Type[] parameterTypes)
            : this(Instances.New(targetType), methodName, parameterTypes) { }
        public Invoker(Type targetType, string methodName, params object[] constructorParams)
            : this(Instances.New(targetType, constructorParams), methodName) { }
        public Invoker(Type targetType, string methodName, Type[] parameterTypes, params object[] constructorParams)
            : this(Instances.New(targetType, constructorParams), methodName, parameterTypes) { }
        public Invoker(string targetName, string methodName)
            : this(Instances.New(targetName), methodName, null) { }
        public Invoker(string targetName, string methodName, params Type[] parameterTypes)
            : this(Instances.New(targetName), methodName, parameterTypes) { }
        public Invoker(string targetName, string methodName, Type[] parameterTypes, params object[] constructorParams)
            : this(Instances.New(targetName, constructorParams), methodName, parameterTypes) { }
        public Invoker(object targetObject, MethodInfo methodInvokeInfo)
        {
            TargetObject = targetObject;
            MethodInfo m = methodInvokeInfo;

            NumberOfArguments = m.GetParameters().Length;
            Info = m;
            Parameters = m.GetParameters();
            createUniqueKey();
        }
        public Invoker(MethodInfo methodInvokeInfo)
            : this(methodInvokeInfo.DeclaringType.New(), methodInvokeInfo)
        {
        }
        public Invoker(MethodInfo methodInvokeInfo, params object[] constructorParams)
            : this(methodInvokeInfo.DeclaringType.New(constructorParams), methodInvokeInfo)
        {
        }

        public string Name { get; set; }

        public string QualifiedName { get; set; }

        public object this[int fieldId]
        {
            get => ParameterValues[fieldId];
            set => ParameterValues[fieldId] = value;
        }
        public object this[string propertyName]
        {
            get
            {
                for (int i = 0; i < Parameters.Length; i++)
                    if (Parameters[i].Name == propertyName)
                        return ParameterValues[i];
                return null;
            }
            set
            {
                for (int i = 0; i < Parameters.Length; i++)
                    if (Parameters[i].Name == propertyName)
                        ParameterValues[i] = value;
            }
        }

        public InvokerDelegate MethodInvoker
        {
            get
            {
                if (routine == null)
                    routine += (InvokerDelegate)Method;
                return routine;
            }
        }

        public Object TargetObject { get; set; }

        public Delegate Method { get; }

        public MethodInfo Info { get; set; }

        public ParameterInfo[] Parameters { get; set; }

        public object[] ParameterValues { get; set; }

        public int NumberOfArguments { get; set; }

        public StateOn StateOn { get; set; }

        public IUnique Empty => Uscn.Empty;

        public ulong Key
        {
            get => serialcode.Key;
            set => serialcode.Key = value;
        }

        public ulong TypeKey
        {
            get => serialcode.TypeKey;
            set => serialcode.TypeKey = value;
        }

        public object[] ValueArray { get => ParameterValues; set => ParameterValues = value; }

        public Uscn SerialCode { get => serialcode; set => serialcode = value; }

        public virtual Task Publish(params object[] parameters)
        {
            try
            {

                return Task.Run(() => (Method ?? invoking(Info)).DynamicInvoke(TargetObject, parameters));
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }

        public virtual Task Publish(bool withTarget, object target, params object[] parameters)
        {
            try
            {
                if (!withTarget)
                {
                    parameters = new[] { target }.Concat(parameters).ToArray();
                    target = TargetObject;
                }

                return Task.Run(() => (Method ?? invoking(Info)).DynamicInvoke(target, parameters));
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }

        public virtual object Invoke(params object[] parameters)
        {
            try
            {
                return (Method ?? invoking(Info)).DynamicInvoke(TargetObject, parameters);
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }
        public virtual object Invoke(bool withTarget, object target, params object[] parameters)
        {
            try
            {
                if (!withTarget)
                {
                    parameters = new[] { target }.Concat(parameters).ToArray();
                    target = TargetObject;
                }
                return (Method ?? invoking(Info)).DynamicInvoke(target, parameters);
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }

        public virtual T Execute<T>(params object[] parameters)
        {
            try
            {
                return (T)(Method ?? invoking(Info)).DynamicInvoke(TargetObject, parameters);
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }
        public virtual T Execute<T>(bool withTarget, object target, params object[] parameters)
        {
            try
            {
                if (!withTarget)
                {
                    parameters = new[] { target }.Concat(parameters).ToArray();
                    target = TargetObject;
                }
                return (T)(Method ?? invoking(Info)).DynamicInvoke(target, parameters);
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }

        public virtual async Task<object> InvokeAsync(params object[] parameters)
        {
            try
            {
                return await Task.Run<object>(() => Invoke(parameters));
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }
        public virtual async Task<object> InvokeAsync(bool withTarget, object target, params object[] parameters)
        {
            try
            {
                if (!withTarget)
                {
                    parameters = new[] { target }.Concat(parameters).ToArray();
                    target = TargetObject;
                }
                return await Task.Run<object>(() => Invoke(target, parameters));
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }

        public virtual async Task<T> InvokeAsync<T>(params object[] parameters)
        {
            try
            {
                return await Task.Run<T>(() => Execute<T>(parameters));
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }
        public virtual async Task<T> InvokeAsync<T>(bool withTarget, object target, params object[] parameters)
        {
            try
            {
                if (!withTarget)
                {
                    parameters = new[] { target }.Concat(parameters).ToArray();
                    target = TargetObject;
                }
                return await Task.Run<T>(() => Execute<T>(target, parameters));
            }
            catch (Exception e)
            {
                throw new TargetInvocationException(e);
            }
        }

        public object ConvertType(object source, Type destination)
        {
            object newobject = Convert.ChangeType(source, destination);
            return (newobject);
        }

        private void createUniqueKey()
        {
            Name = getFullName();
            QualifiedName = getQualifiedName();
            ulong seed = Info.DeclaringType.UniqueKey();
            serialcode.Key = QualifiedName.UniqueKey64();
            serialcode.TypeKey = seed;
            serialcode.Time = DateTime.Now.ToBinary();
        }

        private static ulong getUniqueKey(MethodInfo info, ParameterInfo[] parameters)
        {
            var qualifiedName = getQualifiedName(info, parameters);
            var key = qualifiedName.UniqueKey64();
            return key;
        }
        private static ulong getUniqueSeed(MethodInfo info)
        {
            var name = getFullName(info);
            ulong seed = info.DeclaringType.UniqueKey();
            return seed;
        }

        private string getFullName()
        {
            return $"{Info.DeclaringType.FullName}." +
                   $"{Info.Name}";
        }
        private static string getFullName(MethodInfo info)
        {
            return $"{info.DeclaringType.FullName}." +
                   $"{info.Name}";
        }

        private string getQualifiedName()
        {
            return $"{Info.DeclaringType.FullName}." +
                   $"{Info.Name}" +
                   $"{new String(Parameters.SelectMany(p => "." + p.ParameterType.Name).ToArray())}";
        }
        private static string getQualifiedName(MethodInfo info, ParameterInfo[] parameters)
        {
            return $"{info.DeclaringType.FullName}." +
                   $"{info.Name}" +
                   $"{new String(parameters.SelectMany(p => "." + p.ParameterType.Name).ToArray())}";
        }

        public byte[] GetBytes()
        {
            return serialcode.GetBytes();
        }

        public byte[] GetKeyBytes()
        {
            return serialcode.GetKeyBytes();
        }

        public bool Equals(IUnique other)
        {
            return serialcode.Equals(other);
        }

        public int CompareTo(IUnique other)
        {
            return serialcode.CompareTo(other);
        }

        public static string GetName(Type type, string methodName, params Type[] parameterTypes)
        {
            MethodInfo m = parameterTypes != null ? type.GetMethod(methodName, parameterTypes) : type.GetMethod(methodName);
            return getFullName(m);
        }
        public static string GetName(Type type, params Type[] parameterTypes)
        {
            if (parameterTypes != null && parameterTypes.Any())
            {
                return getFullName(type.GetMethods()
                     .FirstOrDefault(m => m.IsPublic && m.GetParameters().
                         All(p => parameterTypes
                             .Contains(p.ParameterType))));
            }
            else
            {
                return getFullName(type.GetMethods()
                     .FirstOrDefault(m => m.IsPublic));
            }
        }
        public static string GetName<T>(string methodName, params Type[] parameterTypes)
        {
            return GetName(typeof(T), methodName, parameterTypes);
        }
        public static string GetName<T>(params Type[] parameterTypes)
        {
            return GetName(typeof(T), parameterTypes);
        }

        public static string GetQualifiedName(Type type, string methodName, params Type[] parameterTypes)
        {
            var m = type.GetMethods()
                 .FirstOrDefault(m => m.GetParameters().
                     All(p => parameterTypes
                         .Contains(p.ParameterType)) && m.IsPublic && m.Name == methodName);
            return getQualifiedName(m, m.GetParameters());
        }
        public static string GetQualifiedName(Type type, params Type[] parameterTypes)
        {
            var m = type.GetMethods()
                 .FirstOrDefault(m => m.IsPublic && m.GetParameters().
                     All(p => parameterTypes
                         .Contains(p.ParameterType)));
            return getQualifiedName(m, m.GetParameters());
        }
        public static string GetQualifiedName<T>(params Type[] parameterTypes)
        {
            return GetQualifiedName(typeof(T), parameterTypes);
        }
        public static string GetQualifiedName<T>(string methodName, params Type[] parameterTypes)
        {
            return GetQualifiedName(typeof(T), methodName, parameterTypes);
        }

        private Delegate invoking(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                          typeof(object), new Type[] { typeof(object),
                          typeof(object[]) },
                          methodInfo.DeclaringType.Module);

            ILGenerator il = dynamicMethod.GetILGenerator();
            ParameterInfo[] ps = methodInfo.GetParameters();

            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }

            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                directint(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                casting(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }

            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }

            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);

            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                boxing(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    directint(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);

            Delegate invoker = (InvokerDelegate)dynamicMethod.CreateDelegate(typeof(InvokerDelegate));

            return invoker;
        }
        private static void casting(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }
        private static void boxing(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }
        private static void directint(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }

    public class Deputies : Registry<Invoker>
    {
    }


    public class ModificationEventArgs : EventArgs
    {
        #region Fields

        public readonly object Original;
        public readonly StateOn StateOn;
        public readonly object Current;

        #endregion

        #region Constructors

        public ModificationEventArgs(StateOn eventon, object oldItem, object newItem)
        {
            StateOn = eventon;
            Original = oldItem;
            Current = newItem;
        }

        #endregion
    }

    public enum ChangeState
    {
        Unchanged,

        Adding,

        Added,

        Modifying,

        Modified,

        Deleting,

        Deleted,

        Upserting,

        Upserted,

        Patching,

        Patched
    };

    public enum StateOn
    {
        Adding,
        AddComplete,
        Deleting,
        DeleteComplete,
        Getting,
        Patching,
        PatchComplete,
        Upsert,
        UpsertComplete,
        GetComplete,
        Setting,
        SetComplete,
        Saving,
        SaveComplete,
        Filtering,
        FilterComplete,
        Finding,
        FindComplete,
        Mapping,
        MapComplete,
        Exist,
        ExistComplete,
        NonExist,
        NonExistComplete,
        Validating,
        ValidateComplete,
    }
}
