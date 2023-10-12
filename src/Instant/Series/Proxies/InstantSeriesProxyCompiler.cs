namespace Radical.Instant.Series.Proxies
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    public class InstantSeriesProxyCompiler
    {
        private readonly ConstructorInfo marshalAsCtor = typeof(MarshalAsAttribute).GetConstructor(
            new Type[] { typeof(UnmanagedType) }
        );
        private FieldBuilder multemicField = null;
        private FieldBuilder selectiveField = null;
        private InstantSeriesProxyCreator seriesProxyCreator;
        private Type seriesProxyType = typeof(InstantSeriesProxy);

        public InstantSeriesProxyCompiler(InstantSeriesProxyCreator instantSleeve)
        {
            seriesProxyCreator = instantSleeve;
        }

        public Type CompileFigureType(string typeName)
        {
            TypeBuilder tb = GetTypeBuilder(typeName);

            CreateSeriesField(tb);

            CreateProxiesField(tb);

            CreateItemByIntProperty(tb);

            CreateValueByIntProperty(tb);

            CreateValueByStringProperty(tb);

            return tb.CreateTypeInfo();
        }

        private void CreateArrayLengthField(TypeBuilder tb)
        {
            PropertyInfo iprop = seriesProxyType.GetProperty("Length");

            MethodInfo accessor = iprop.GetGetMethod();

            ParameterInfo[] args = accessor.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder getter = tb.DefineMethod(
                accessor.Name,
                accessor.Attributes & ~MethodAttributes.Abstract,
                accessor.CallingConvention,
                accessor.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(getter, accessor);

            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, selectiveField);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Ret);
        }

        private void CreateItemByIntProperty(TypeBuilder tb)
        {
            PropertyInfo prop = typeof(InstantSeriesProxy).GetProperty(
                "Item",
                new Type[] { typeof(int) }
            );
            MethodInfo accessor = prop.GetGetMethod();
            if (accessor != null)
            {
                ParameterInfo[] args = accessor.GetParameters();
                Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                MethodBuilder method = tb.DefineMethod(
                    accessor.Name,
                    accessor.Attributes & ~MethodAttributes.Abstract,
                    accessor.CallingConvention,
                    accessor.ReturnType,
                    argTypes
                );
                tb.DefineMethodOverride(method, accessor);
                ILGenerator il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, selectiveField);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(
                    OpCodes.Callvirt,
                    typeof(IInstantSeries).GetMethod("get_Item", new Type[] { typeof(int) }),
                    null
                );
                il.Emit(OpCodes.Ret);
            }

            MethodInfo mutator = prop.GetSetMethod();
            if (mutator != null)
            {
                ParameterInfo[] args = mutator.GetParameters();
                Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                MethodBuilder method = tb.DefineMethod(
                    mutator.Name,
                    mutator.Attributes & ~MethodAttributes.Abstract,
                    mutator.CallingConvention,
                    mutator.ReturnType,
                    argTypes
                );
                tb.DefineMethodOverride(method, mutator);
                ILGenerator il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, selectiveField);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.EmitCall(
                    OpCodes.Callvirt,
                    typeof(IInstantSeries).GetMethod(
                        "set_Item",
                        new Type[] { typeof(int), typeof(IInstant) }
                    ),
                    null
                );
                il.Emit(OpCodes.Ret);
            }
        }

        private FieldBuilder CreateField(TypeBuilder tb, Type type, string name)
        {
            return tb.DefineField("_" + name, type, FieldAttributes.Private);
        }

        private void CreateProxiesField(TypeBuilder tb)
        {
            FieldBuilder fb = CreateField(tb, typeof(object).MakeArrayType(), "Proxies");
            multemicField = fb;

            PropertyInfo iprop = seriesProxyType.GetProperty("Proxies");

            MethodInfo accessor = iprop.GetGetMethod();

            ParameterInfo[] args = accessor.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder getter = tb.DefineMethod(
                accessor.Name,
                accessor.Attributes & ~MethodAttributes.Abstract,
                accessor.CallingConvention,
                accessor.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(getter, accessor);

            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fb);
            il.Emit(OpCodes.Ret);

            MethodInfo mutator = iprop.GetSetMethod();

            args = mutator.GetParameters();
            argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder setter = tb.DefineMethod(
                mutator.Name,
                mutator.Attributes & ~MethodAttributes.Abstract,
                mutator.CallingConvention,
                mutator.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(setter, mutator);

            il = setter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fb);
            il.Emit(OpCodes.Ret);
        }

        private void CreateValueByIntProperty(TypeBuilder tb)
        {
            PropertyInfo prop = typeof(InstantSeriesProxy).GetProperty(
                "Item",
                new Type[] { typeof(int), typeof(int) }
            );
            MethodInfo accessor = prop.GetGetMethod();
            if (accessor != null)
            {
                ParameterInfo[] args = accessor.GetParameters();
                Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                MethodBuilder method = tb.DefineMethod(
                    accessor.Name,
                    accessor.Attributes & ~MethodAttributes.Abstract,
                    accessor.CallingConvention,
                    accessor.ReturnType,
                    argTypes
                );
                tb.DefineMethodOverride(method, accessor);
                ILGenerator il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, selectiveField);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.EmitCall(
                    OpCodes.Callvirt,
                    typeof(IInstantSeries).GetMethod("get_Item", new Type[] { typeof(int), typeof(int) }),
                    null
                );
                il.Emit(OpCodes.Ret);
            }

            MethodInfo mutator = prop.GetSetMethod();
            if (mutator != null)
            {
                ParameterInfo[] args = mutator.GetParameters();
                Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                MethodBuilder method = tb.DefineMethod(
                    mutator.Name,
                    mutator.Attributes & ~MethodAttributes.Abstract,
                    mutator.CallingConvention,
                    mutator.ReturnType,
                    argTypes
                );
                tb.DefineMethodOverride(method, mutator);
                ILGenerator il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, selectiveField);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);
                il.EmitCall(
                    OpCodes.Callvirt,
                    typeof(IInstantSeries).GetMethod(
                        "set_Item",
                        new Type[] { typeof(int), typeof(int), typeof(object) }
                    ),
                    null
                );
                il.Emit(OpCodes.Ret);
            }
        }

        private void CreateValueByStringProperty(TypeBuilder tb)
        {
            PropertyInfo prop = typeof(InstantSeriesProxy).GetProperty(
                "Item",
                new Type[] { typeof(int), typeof(string) }
            );
            MethodInfo accessor = prop.GetGetMethod();
            if (accessor != null)
            {
                ParameterInfo[] args = accessor.GetParameters();
                Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                MethodBuilder method = tb.DefineMethod(
                    accessor.Name,
                    accessor.Attributes & ~MethodAttributes.Abstract,
                    accessor.CallingConvention,
                    accessor.ReturnType,
                    argTypes
                );
                tb.DefineMethodOverride(method, accessor);
                ILGenerator il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, selectiveField);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.EmitCall(
                    OpCodes.Callvirt,
                    typeof(IInstantSeries).GetMethod(
                        "get_Item",
                        new Type[] { typeof(int), typeof(string) }
                    ),
                    null
                );
                il.Emit(OpCodes.Ret);
            }

            MethodInfo mutator = prop.GetSetMethod();
            if (mutator != null)
            {
                ParameterInfo[] args = mutator.GetParameters();
                Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);
                MethodBuilder method = tb.DefineMethod(
                    mutator.Name,
                    mutator.Attributes & ~MethodAttributes.Abstract,
                    mutator.CallingConvention,
                    mutator.ReturnType,
                    argTypes
                );
                tb.DefineMethodOverride(method, mutator);
                ILGenerator il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, selectiveField);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);
                il.EmitCall(
                    OpCodes.Callvirt,
                    typeof(IInstantSeries).GetMethod(
                        "set_Item",
                        new Type[] { typeof(int), typeof(string), typeof(object) }
                    ),
                    null
                );
                il.Emit(OpCodes.Ret);
            }
        }

        private void CreateMarshalAttribue(FieldBuilder field, MarshalAsAttribute attrib)
        {
            List<object> attribValues = new List<object>(1);
            List<FieldInfo> attribFields = new List<FieldInfo>(1);
            attribValues.Add(attrib.SizeConst);
            attribFields.Add(attrib.GetType().GetField("SizeConst"));
            field.SetCustomAttribute(
                new CustomAttributeBuilder(
                    marshalAsCtor,
                    new object[] { attrib.Value },
                    attribFields.ToArray(),
                    attribValues.ToArray()
                )
            );
        }

        private void CreateNewSleevesObject(TypeBuilder tb)
        {
            MethodInfo createArray = seriesProxyType.GetMethod("NewProxies");

            ParameterInfo[] args = createArray.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                createArray.Name,
                createArray.Attributes & ~MethodAttributes.Abstract,
                createArray.CallingConvention,
                createArray.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, createArray);

            ILGenerator il = method.GetILGenerator();
            il.DeclareLocal(typeof(IInstant).MakeArrayType());

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newarr, typeof(IInstant));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Stfld, selectiveField);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
        }

        private PropertyBuilder CreateProperty(
            TypeBuilder tb,
            FieldBuilder field,
            Type type,
            string name
        )
        {
            PropertyBuilder prop = tb.DefineProperty(
                name,
                PropertyAttributes.HasDefault,
                type,
                new Type[] { type }
            );

            MethodBuilder getter = tb.DefineMethod(
                "get_" + name,
                MethodAttributes.Public | MethodAttributes.HideBySig,
                type,
                Type.EmptyTypes
            );
            prop.SetGetMethod(getter);
            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ret);

            MethodBuilder setter = tb.DefineMethod(
                "set_" + name,
                MethodAttributes.Public | MethodAttributes.HideBySig,
                typeof(void),
                new Type[] { type }
            );
            prop.SetSetMethod(setter);
            il = setter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);

            return prop;
        }

        private void CreateSeriesField(TypeBuilder tb)
        {
            FieldBuilder fb = CreateField(tb, typeof(object).MakeArrayType(), "Series");
            selectiveField = fb;

            PropertyInfo iprop = seriesProxyType.GetProperty("Series");

            MethodInfo accessor = iprop.GetGetMethod();

            ParameterInfo[] args = accessor.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder getter = tb.DefineMethod(
                accessor.Name,
                accessor.Attributes & ~MethodAttributes.Abstract,
                accessor.CallingConvention,
                accessor.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(getter, accessor);

            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fb);
            il.Emit(OpCodes.Ret);

            MethodInfo mutator = iprop.GetSetMethod();

            args = mutator.GetParameters();
            argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder setter = tb.DefineMethod(
                mutator.Name,
                mutator.Attributes & ~MethodAttributes.Abstract,
                mutator.CallingConvention,
                mutator.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(setter, mutator);

            il = setter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fb);
            il.Emit(OpCodes.Ret);
        }

        private TypeBuilder GetTypeBuilder(string typeName)
        {
            string typeSignature = typeName;
            AssemblyName an = new AssemblyName(typeSignature);

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                an,
                AssemblyBuilderAccess.RunAndCollect
            );
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                typeSignature + "Module"
            );
            TypeBuilder tb = null;

            tb = moduleBuilder.DefineType(
                typeSignature,
                TypeAttributes.Class
                    | TypeAttributes.Public
                    | TypeAttributes.Serializable
                    | TypeAttributes.AnsiClass
            );

            tb.SetCustomAttribute(
                new CustomAttributeBuilder(
                    typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes),
                    new object[0]
                )
            );
            tb.SetParent(typeof(InstantSeriesProxy));
            return tb;
        }
    }
}
