namespace Radical.Instant.Proxies;

using Radical.Series;
using Rubrics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Uniques;

public class ProxyCompiler : ProxyCompilerBase
{
    protected FieldInfo scodeField;
    protected FieldBuilder sobjectField;
    protected FieldBuilder rubricsField;
    public bool hasSerialCode;

    public ProxyCompiler(ProxyCreator instantSleeve, ISeries<RubricModel> rubricBuilders)
        : base(instantSleeve, rubricBuilders) { }

    public override Type CompileSleeveType(string typeName)
    {
        TypeBuilder tb = GetTypeBuilder(typeName);

        CreateInstanceProperty(tb, typeof(object), "Target");

        CreateRubricsProperty(tb, typeof(MemberRubrics), "Rubrics");

        CreateFieldsAndProperties(tb);

        CreateSerialCodeProperty(tb, typeof(Uscn), "Code");

        CreateValueArrayProperty(tb);

        CreateItemByIntProperty(tb);

        CreateItemByStringProperty(tb);

        CreateUniqueKeyProperty(tb);

        CreateUniqueSeedProperty(tb);

        CreateGetUniqueBytesMethod(tb);

        CreateGetBytesMethod(tb);

        CreateEqualsMethod(tb);

        CreateCompareToMethod(tb);

        return tb.CreateTypeInfo();
    }

    public override void CreateCompareToMethod(TypeBuilder tb)
    {
        MethodInfo mi = typeof(IComparable<IUnique>).GetMethod("CompareTo");

        ParameterInfo[] args = mi.GetParameters();
        Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

        MethodBuilder method = tb.DefineMethod(
            mi.Name,
            mi.Attributes & ~MethodAttributes.Abstract,
            mi.CallingConvention,
            mi.ReturnType,
            argTypes
        );
        tb.DefineMethodOverride(method, mi);

        ILGenerator il = method.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.Emit(OpCodes.Ldarg_1);
        il.EmitCall(
            OpCodes.Call,
            typeof(Uscn).GetMethod("CompareTo", new Type[] { typeof(IUnique) }),
            null
        );
        il.Emit(OpCodes.Ret);
    }

    public override void CreateEqualsMethod(TypeBuilder tb)
    {
        MethodInfo createArray = typeof(IEquatable<IUnique>).GetMethod("Equals");

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
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.Emit(OpCodes.Ldarg_1);
        il.EmitCall(
            OpCodes.Call,
            typeof(Uscn).GetMethod("Equals", new Type[] { typeof(IUnique) }),
            null
        );
        il.Emit(OpCodes.Ret);
    }

    public override void CreateFieldsAndProperties(TypeBuilder tb)
    {
        rubricBuilders
            .AsValues()
            .ForEach(
                (m) =>
                {
                    if (m.Field != null)
                    {
                        if (!(m.Field.IsBackingField))
                            ResolveFigureAttributes(null, new MemberRubric(m.Field));
                        else if (m.Property != null)
                            ResolveFigureAttributes(null, new MemberRubric(m.Property));
                    }
                    else if (m.Property != null)
                    {
                        ResolveFigureAttributes(null, new MemberRubric(m.Property));
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
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.EmitCall(OpCodes.Call, typeof(IUnique).GetMethod("GetBytes"), null);
        il.Emit(OpCodes.Ret);
    }

    public override void CreateGetEmptyProperty(TypeBuilder tb)
    {
        PropertyBuilder prop = tb.DefineProperty(
            "Empty",
            PropertyAttributes.HasDefault,
            typeof(IUnique),
            Type.EmptyTypes
        );

        PropertyInfo iprop = typeof(IUnique).GetProperty("Empty");

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
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("get_Empty"), null);
        il.Emit(OpCodes.Ret);
    }

    public override void CreateGetUniqueBytesMethod(TypeBuilder tb)
    {
        MethodInfo createArray = typeof(IUnique).GetMethod("GetKeyBytes");

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
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("GetKeyBytes"), null);
        il.Emit(OpCodes.Ret);
    }

    public override void CreateInstanceProperty(TypeBuilder tb, Type type, string name)
    {
        sobjectField = tb.DefineField(name.ToLower(), type, FieldAttributes.Private);

        PropertyBuilder prop = tb.DefineProperty(
            name,
            PropertyAttributes.HasDefault,
            type,
            new Type[] { type }
        );

        PropertyInfo iprop = typeof(IProxy).GetProperty(name);

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
        il.Emit(OpCodes.Ldfld, sobjectField);
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
        il.Emit(OpCodes.Stfld, sobjectField);
        il.Emit(OpCodes.Ret);

        prop.SetCustomAttribute(
            new CustomAttributeBuilder(
                dataMemberCtor,
                new object[0],
                dataMemberProps,
                new object[2] { -1, name.ToUpper() }
            )
        );
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
                        il.Emit(OpCodes.Ldfld, sobjectField);
                        if (
                            rubricBuilders[i].Field == null
                            || rubricBuilders[i].Field.IsBackingField
                        )
                        {
                            il.EmitCall(OpCodes.Call, rubricBuilders[i].Getter, null);
                        }
                        else
                            il.Emit(OpCodes.Ldfld, rubricBuilders[i].Field.RubricInfo);

                        if (rubricBuilders[i].Type.IsValueType)
                        {
                            il.Emit(OpCodes.Box, rubricBuilders[i].Type);
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
                        il.Emit(OpCodes.Ldfld, sobjectField);
                        il.Emit(OpCodes.Ldarg_2);

                        il.Emit(
                            rubricBuilders[i].Type.IsValueType
                                ? OpCodes.Unbox_Any
                                : OpCodes.Castclass,
                            rubricBuilders[i].Type
                        );

                        if (
                            rubricBuilders[i].Field == null
                            || rubricBuilders[i].Field.IsBackingField
                        )
                        {
                            il.EmitCall(OpCodes.Call, rubricBuilders[i].Setter, null);
                        }
                        else
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
                        il.Emit(OpCodes.Ldfld, sobjectField);

                        if (
                            rubricBuilders[i].Field == null
                            || rubricBuilders[i].Field.IsBackingField
                        )
                        {
                            il.EmitCall(OpCodes.Call, rubricBuilders[i].Getter, null);
                        }
                        else
                            il.Emit(OpCodes.Ldfld, rubricBuilders[i].Field.RubricInfo);

                        if (rubricBuilders[i].Type.IsValueType)
                        {
                            il.Emit(OpCodes.Box, rubricBuilders[i].Type);
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
                        il.Emit(OpCodes.Ldfld, sobjectField);
                        il.Emit(OpCodes.Ldarg_2);

                        il.Emit(
                            rubricBuilders[i].Type.IsValueType
                                ? OpCodes.Unbox_Any
                                : OpCodes.Castclass,
                            rubricBuilders[i].Type
                        );

                        if (
                            rubricBuilders[i].Field == null
                            || rubricBuilders[i].Field.IsBackingField
                        )
                        {
                            il.EmitCall(OpCodes.Call, rubricBuilders[i].Setter, null);
                        }
                        else
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

        ILGenerator il = getter.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
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

        il = setter.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, scodeField);
        il.Emit(OpCodes.Ret);

        prop.SetCustomAttribute(
            new CustomAttributeBuilder(
                dataMemberCtor,
                new object[0],
                dataMemberProps,
                new object[2] { length, name.ToUpper() }
            )
        );
        if (fp != null)
        {
            fp.SetMember(new MemberRubric(prop));
            rubricBuilders.Add(fp);
        }
    }

    public override void CreateRubricsProperty(TypeBuilder tb, Type type, string name)
    {
        rubricsField = tb.DefineField(name.ToLower(), type, FieldAttributes.Private);

        PropertyBuilder prop = tb.DefineProperty(
            name,
            PropertyAttributes.HasDefault,
            type,
            new Type[] { type }
        );

        PropertyInfo iprop = typeof(IProxy).GetProperty(name);

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
        il.Emit(OpCodes.Ldfld, rubricsField);
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
        il.Emit(OpCodes.Stfld, rubricsField);
        il.Emit(OpCodes.Ret);

        prop.SetCustomAttribute(
            new CustomAttributeBuilder(
                dataMemberCtor,
                new object[0],
                dataMemberProps,
                new object[2] { -3, name.ToUpper() }
            )
        );
    }

    public override void CreateUniqueKeyProperty(TypeBuilder tb)
    {
        PropertyBuilder prop = tb.DefineProperty(
            "Key",
            PropertyAttributes.HasDefault,
            typeof(ulong),
            new Type[] { typeof(ulong) }
        );

        PropertyInfo iprop = typeof(IUnique).GetProperty("Key");

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
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("GetKey"), null);
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
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.Emit(OpCodes.Ldarg_1);
        il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("SetKey"), null);
        il.Emit(OpCodes.Ret);
    }

    public override void CreateUniqueSeedProperty(TypeBuilder tb)
    {
        PropertyBuilder prop = tb.DefineProperty(
            "TypeKey",
            PropertyAttributes.HasDefault,
            typeof(ulong),
            new Type[] { typeof(ulong) }
        );

        PropertyInfo iprop = typeof(IUnique).GetProperty("TypeKey");

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
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("GetTypeKey"), null);
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
        if (hasSerialCode)
            il.Emit(OpCodes.Ldfld, sobjectField);
        il.Emit(OpCodes.Ldflda, scodeField);
        il.Emit(OpCodes.Ldarg_1);
        il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("SetTypeKey"), null);
        il.Emit(OpCodes.Ret);
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
            il.Emit(OpCodes.Ldfld, sobjectField);

            if (rubricBuilders[i].Field == null || rubricBuilders[i].Field.IsBackingField)
            {
                il.EmitCall(OpCodes.Call, rubricBuilders[i].Getter, null);
            }
            else
                il.Emit(OpCodes.Ldfld, rubricBuilders[i].Field.RubricInfo);

            if (rubricBuilders[i].Type.IsValueType)
            {
                il.Emit(OpCodes.Box, rubricBuilders[i].Type);
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
            il.Emit(OpCodes.Ldfld, sobjectField);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem, typeof(object));
            il.Emit(
                rubricBuilders[i].Type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass,
                rubricBuilders[i].Type
            );

            if (rubricBuilders[i].Field == null || rubricBuilders[i].Field.IsBackingField)
            {
                il.EmitCall(OpCodes.Call, rubricBuilders[i].Setter, null);
            }
            else
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
        TypeBuilder tb = null;

        tb = moduleBuilder.DefineType(
            typeSignature,
            TypeAttributes.AnsiClass
                | TypeAttributes.Public
                | TypeAttributes.Class
                | TypeAttributes.Serializable
        );

        tb.SetCustomAttribute(
            new CustomAttributeBuilder(
                typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes),
                new object[0]
            )
        );

        tb.AddInterfaceImplementation(typeof(IProxy));

        tb.SetParent(sleeve.BaseType);

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
}
