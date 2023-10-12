namespace Radical.Instant
{
    using Radical.Extracting;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using Radical.Series;
    using Radical.Uniques;
    using Rubrics;

    public class InstantCompilerValueTypes : InstantCompiler
    {
        public InstantCompilerValueTypes(InstantCreator instantFigure, ISeries<RubricModel> rubricBuilders)
            : base(instantFigure, rubricBuilders) { }

        public override Type CompileInstantType(string typeName)
        {
            TypeBuilder tb = GetTypeBuilder(typeName);

            CreateFieldsAndProperties(tb);

            CreateSerialCodeProperty(tb, typeof(Uscn), "Code");

            CreateValueArrayProperty(tb);

            CreateItemByIntProperty(tb);

            CreateItemByStringProperty(tb);

            CreateKeyProperty(tb);

            CreateTypeKeyProperty(tb);

            CreateGetUniqueBytesMethod(tb);

            CreateGetBytesMethod(tb);

            CreateEqualsMethod(tb);

            CreateCompareToMethod(tb);

            return tb.CreateTypeInfo();
        }

        public override void CreateFieldsAndProperties(TypeBuilder tb)
        {
            int i = 0;
            rubricBuilders.ForEach(
                (fp) =>
                {
                    MemberRubric attributeAtMember = null;

                    if (fp.Field != null)
                    {
                        if (!(fp.Field.IsBackingField))
                            attributeAtMember = new MemberRubric(fp.Field);
                        else if (fp.Property != null)
                            attributeAtMember = new MemberRubric(fp.Property);
                    }
                    else
                    {
                        attributeAtMember = new MemberRubric(fp.Property);
                    }

                    if (fp.Type == typeof(string))
                        fp.FieldType = typeof(char[]);
                    else
                        fp.FieldType = fp.Type;

                    FieldBuilder fb = createField(
                        tb,
                        attributeAtMember,
                        fp.FieldType,
                        '_' + fp.Name
                    );

                    if (fb != null)
                    {
                        ResolveFigureAttributes(fb, attributeAtMember);

                        PropertyBuilder pi = null;
                        if (fp.Type != typeof(string))
                            pi = createProperty(tb, fb, fp.Type, fp.Name);
                        else
                            pi = createStringProperty(tb, fb, fp.Type, fp.Name);

                        pi.SetCustomAttribute(
                            new CustomAttributeBuilder(
                                dataMemberCtor,
                                new object[0],
                                dataMemberProps,
                                new object[2] { i++, fp.Name }
                            )
                        );

                        fp.SetMember(new MemberRubric(fb));
                        fp.SetMember(new MemberRubric(pi));
                    }
                }
            );
        }

        public override void CreateGetBytesMethod(TypeBuilder tb)
        {
            MethodInfo createArray = typeof(IUnique).GetMethod("GetBytes");

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
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Box, tb.UnderlyingSystemType);
            il.EmitCall(
                OpCodes.Call,
                typeof(ObjectExtractExtenstion).GetMethod(
                    "GetValueStructureBytes",
                    new Type[] { typeof(object) }
                ),
                null
            );
            il.Emit(OpCodes.Ret);
        }

        public override void CreateItemByIntProperty(TypeBuilder tb)
        {
            foreach (PropertyInfo prop in typeof(IInstant).GetProperties())
            {
                MethodInfo accessor = prop.GetGetMethod();
                if (accessor != null)
                {
                    ParameterInfo[] args = accessor.GetParameters();
                    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                    if (args.Length == 1 && argTypes[0] == typeof(int))
                    {
                        MethodBuilder method = tb.DefineMethod(
                            accessor.Name,
                            accessor.Attributes & ~MethodAttributes.Abstract,
                            accessor.CallingConvention,
                            accessor.ReturnType,
                            argTypes
                        );
                        tb.DefineMethodOverride(method, accessor);
                        ILGenerator il = method.GetILGenerator();

                        Label[] branches = new Label[length];
                        for (int i = 0; i < length; i++)
                        {
                            branches[i] = il.DefineLabel();
                        }
                        il.Emit(OpCodes.Ldarg_1);

                        il.Emit(OpCodes.Switch, branches);

                        il.ThrowException(typeof(ArgumentOutOfRangeException));
                        for (int i = 0; i < length; i++)
                        {
                            il.MarkLabel(branches[i]);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, rubricBuilders[i].Field.RubricInfo);
                            if (rubricBuilders[i].FieldType.IsValueType)
                            {
                                il.Emit(OpCodes.Box, rubricBuilders[i].FieldType);
                            }
                            else if (rubricBuilders[i].FieldType == typeof(char[]))
                            {
                                il.Emit(
                                    OpCodes.Newobj,
                                    typeof(string).GetConstructor(new Type[] { typeof(char[]) })
                                );
                            }
                            il.Emit(OpCodes.Ret);
                        }
                    }
                }

                MethodInfo mutator = prop.GetSetMethod();
                if (mutator != null)
                {
                    ParameterInfo[] args = mutator.GetParameters();
                    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                    if (
                        args.Length == 2
                        && argTypes[0] == typeof(int)
                        && argTypes[1] == typeof(object)
                    )
                    {
                        MethodBuilder method = tb.DefineMethod(
                            mutator.Name,
                            mutator.Attributes & ~MethodAttributes.Abstract,
                            mutator.CallingConvention,
                            mutator.ReturnType,
                            argTypes
                        );
                        tb.DefineMethodOverride(method, mutator);
                        ILGenerator il = method.GetILGenerator();

                        Label[] branches = new Label[length];
                        for (int i = 0; i < length; i++)
                        {
                            branches[i] = il.DefineLabel();
                        }
                        il.Emit(OpCodes.Ldarg_1);

                        il.Emit(OpCodes.Switch, branches);

                        il.ThrowException(typeof(ArgumentOutOfRangeException));
                        for (int i = 0; i < length; i++)
                        {
                            il.MarkLabel(branches[i]);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldarg_2);
                            if (rubricBuilders[i].FieldType.IsValueType)
                            {
                                il.Emit(OpCodes.Unbox_Any, rubricBuilders[i].FieldType);
                            }
                            else if (rubricBuilders[i].FieldType == typeof(char[]))
                            {
                                il.Emit(OpCodes.Castclass, typeof(string));
                                il.EmitCall(
                                    OpCodes.Call,
                                    typeof(string).GetMethod("ToCharArray", Type.EmptyTypes),
                                    null
                                );
                            }
                            else
                                il.Emit(OpCodes.Castclass, rubricBuilders[i].FieldType);
                            il.Emit(OpCodes.Stfld, rubricBuilders[i].Field.RubricInfo);
                            il.Emit(OpCodes.Ret);
                        }
                    }
                }
            }
        }

        public override void CreateItemByStringProperty(TypeBuilder tb)
        {
            foreach (PropertyInfo prop in typeof(IInstant).GetProperties())
            {
                MethodInfo accessor = prop.GetGetMethod();
                if (accessor != null)
                {
                    ParameterInfo[] args = accessor.GetParameters();
                    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                    if (args.Length == 1 && argTypes[0] == typeof(string))
                    {
                        MethodBuilder method = tb.DefineMethod(
                            accessor.Name,
                            accessor.Attributes & ~MethodAttributes.Abstract,
                            accessor.CallingConvention,
                            accessor.ReturnType,
                            argTypes
                        );
                        tb.DefineMethodOverride(method, accessor);
                        ILGenerator il = method.GetILGenerator();

                        il.DeclareLocal(typeof(string));

                        Label[] branches = new Label[length];

                        for (int i = 0; i < length; i++)
                        {
                            branches[i] = il.DefineLabel();
                        }

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Stloc_0);

                        for (int i = 0; i < length; i++)
                        {
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldstr, rubricBuilders[i].Name);
                            il.EmitCall(
                                OpCodes.Call,
                                typeof(string).GetMethod(
                                    "op_Equality",
                                    new Type[] { typeof(string), typeof(string) }
                                ),
                                null
                            );
                            il.Emit(OpCodes.Brtrue, branches[i]);
                        }

                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Ret);

                        for (int i = 0; i < length; i++)
                        {
                            il.MarkLabel(branches[i]);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, rubricBuilders[i].Field.RubricInfo);
                            if (rubricBuilders[i].FieldType.IsValueType)
                            {
                                il.Emit(OpCodes.Box, rubricBuilders[i].FieldType);
                            }
                            else if (rubricBuilders[i].FieldType == typeof(char[]))
                            {
                                il.Emit(
                                    OpCodes.Newobj,
                                    typeof(string).GetConstructor(new Type[] { typeof(char[]) })
                                );
                            }
                            il.Emit(OpCodes.Ret);
                        }
                    }
                }

                MethodInfo mutator = prop.GetSetMethod();
                if (mutator != null)
                {
                    ParameterInfo[] args = mutator.GetParameters();
                    Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

                    if (
                        args.Length == 2
                        && argTypes[0] == typeof(string)
                        && argTypes[1] == typeof(object)
                    )
                    {
                        MethodBuilder method = tb.DefineMethod(
                            mutator.Name,
                            mutator.Attributes & ~MethodAttributes.Abstract,
                            mutator.CallingConvention,
                            mutator.ReturnType,
                            argTypes
                        );
                        tb.DefineMethodOverride(method, mutator);
                        ILGenerator il = method.GetILGenerator();

                        il.DeclareLocal(typeof(string));

                        Label[] branches = new Label[length];
                        for (int i = 0; i < length; i++)
                        {
                            branches[i] = il.DefineLabel();
                        }

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Stloc_0);

                        for (int i = 0; i < length; i++)
                        {
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldstr, rubricBuilders[i].Name);
                            il.EmitCall(
                                OpCodes.Call,
                                typeof(string).GetMethod(
                                    "op_Equality",
                                    new[] { typeof(string), typeof(string) }
                                ),
                                null
                            );
                            il.Emit(OpCodes.Brtrue, branches[i]);
                        }

                        il.Emit(OpCodes.Ret);

                        for (int i = 0; i < length; i++)
                        {
                            il.MarkLabel(branches[i]);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldarg_2);
                            if (rubricBuilders[i].FieldType.IsValueType)
                            {
                                il.Emit(OpCodes.Unbox_Any, rubricBuilders[i].FieldType);
                            }
                            else if (rubricBuilders[i].FieldType == typeof(char[]))
                            {
                                il.Emit(OpCodes.Castclass, typeof(string));
                                il.EmitCall(
                                    OpCodes.Call,
                                    typeof(string).GetMethod("ToCharArray", Type.EmptyTypes),
                                    null
                                );
                            }
                            else
                                il.Emit(OpCodes.Castclass, rubricBuilders[i].FieldType);
                            il.Emit(OpCodes.Stfld, rubricBuilders[i].Field.RubricInfo);
                            il.Emit(OpCodes.Ret);
                        }
                    }
                }
            }
        }

        public override void CreateSerialCodeProperty(TypeBuilder tb, Type type, string name)
        {
            RubricModel fp = null;
            var field = rubricBuilders
                .AsValues()
                .FirstOrDefault(
                    p =>
                        p.Field != null
                        && p.Field.FieldName.Contains(
                            name,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                );
            if (field != null)
            {
                scodeField = field.Field.RubricInfo;
            }
            else
            {
                FieldBuilder fb = createField(tb, null, type, name.ToLower());
                scodeField = fb;
                fp = new RubricModel(new MemberRubric(fb));
            }

            PropertyBuilder prop = tb.DefineProperty(
                name,
                PropertyAttributes.HasDefault,
                type,
                new Type[] { type }
            );

            PropertyInfo iprop = typeof(IInstant).GetProperty(name);

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

            prop.SetGetMethod(getter);
            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, scodeField);
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

            prop.SetSetMethod(setter);
            il = setter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, scodeField);
            il.Emit(OpCodes.Ret);

            prop.SetCustomAttribute(
                new CustomAttributeBuilder(
                    dataMemberCtor,
                    new object[0],
                    dataMemberProps,
                    new object[2] { 0, name.ToUpper() }
                )
            );
            if (fp != null)
            {
                fp.SetMember(new MemberRubric(prop));
                rubricBuilders.Add(fp);
            }
        }

        public override void CreateValueArrayProperty(TypeBuilder tb)
        {
            PropertyInfo prop = typeof(IInstant).GetProperty("ValueArray");

            MethodInfo accessor = prop.GetGetMethod();

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
            il.DeclareLocal(typeof(object[]));

            il.Emit(OpCodes.Ldc_I4, length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc_0);

            for (int i = 0; i < length; i++)
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, rubricBuilders[i].Field.RubricInfo);
                if (rubricBuilders[i].FieldType.IsValueType)
                {
                    il.Emit(OpCodes.Box, rubricBuilders[i].FieldType);
                }
                else if (rubricBuilders[i].FieldType == typeof(char[]))
                {
                    il.Emit(
                        OpCodes.Newobj,
                        typeof(string).GetConstructor(new Type[] { typeof(char[]) })
                    );
                }
                il.Emit(OpCodes.Stelem, typeof(object));
            }
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            MethodInfo mutator = prop.GetSetMethod();

            args = mutator.GetParameters();
            argTypes = Array.ConvertAll(args, a => a.ParameterType);

            method = tb.DefineMethod(
                mutator.Name,
                mutator.Attributes & ~MethodAttributes.Abstract,
                mutator.CallingConvention,
                mutator.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, mutator);
            il = method.GetILGenerator();
            il.DeclareLocal(typeof(object[]));

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc_0);
            for (int i = 0; i < length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem, typeof(object));
                if (rubricBuilders[i].FieldType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, rubricBuilders[i].FieldType);
                }
                else if (rubricBuilders[i].FieldType == typeof(char[]))
                {
                    il.Emit(OpCodes.Castclass, typeof(string));
                    il.EmitCall(
                        OpCodes.Call,
                        typeof(string).GetMethod("ToCharArray", Type.EmptyTypes),
                        null
                    );
                }
                else
                    il.Emit(OpCodes.Castclass, rubricBuilders[i].FieldType);
                il.Emit(OpCodes.Stfld, rubricBuilders[i].Field.RubricInfo);
            }
            il.Emit(OpCodes.Ret);
        }

        public override TypeBuilder GetTypeBuilder(string typeName)
        {
            string typeSignature =
                (typeName != null && typeName != "") ? typeName : Unique.NewKey.ToString();
            AssemblyName an = new AssemblyName(typeSignature);

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                an,
                AssemblyBuilderAccess.RunAndCollect
            );
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                typeSignature + "Module"
            );

            TypeBuilder tb;

            tb = moduleBuilder.DefineType(
                typeSignature,
                TypeAttributes.Public
                    | TypeAttributes.Serializable
                    | TypeAttributes.Class
                    | TypeAttributes.AnsiClass
                    | TypeAttributes.SequentialLayout,
                typeof(ValueType)
            );

            tb.SetCustomAttribute(
                new CustomAttributeBuilder(
                    structLayoutCtor,
                    new object[] { LayoutKind.Sequential },
                    structLayoutFields,
                    new object[] { CharSet.Ansi, 1 }
                )
            );

            tb.SetCustomAttribute(
                new CustomAttributeBuilder(
                    typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes),
                    new object[0]
                )
            );

            tb.AddInterfaceImplementation(typeof(IInstant));

            return tb;
        }

        private FieldBuilder createField(
            TypeBuilder tb,
            MemberRubric mr,
            Type type,
            string fieldName
        )
        {
            if (type == typeof(string) || type.IsArray)
            {
                FieldBuilder fb = tb.DefineField(
                    fieldName,
                    type,
                    FieldAttributes.Private
                        | FieldAttributes.HasDefault
                        | FieldAttributes.HasFieldMarshal
                );

                if (type == typeof(string))
                    ResolveMarshalAsAttributeForString(fb, mr, type);
                else
                    ResolveMarshalAsAttributeForArray(fb, mr, type);

                return fb;
            }
            else
            {
                return tb.DefineField(fieldName, type, FieldAttributes.Private);
            }
        }

        private PropertyBuilder createProperty(
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

        private PropertyBuilder createStringProperty(
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
            if (type == typeof(string))
            {
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(
                    OpCodes.Newobj,
                    typeof(string).GetConstructor(new Type[] { typeof(char[]) })
                );
            }
            else
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
            if (type == typeof(string))
            {
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(
                    OpCodes.Call,
                    typeof(string).GetMethod("ToCharArray", Type.EmptyTypes),
                    null
                );
            }
            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);

            return prop;
        }
    }
}
