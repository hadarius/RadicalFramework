using System.ComponentModel.DataAnnotations;

namespace Radical.Instant
{
    using Attributes;
    using Radical.Series;
    using Rubrics;
    using Rubrics.Attributes;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using Uniques;

    public enum InstantType
    {
        Derived,
        ValueType,
        Reference
    }

    public abstract class InstantCompiler : InstantCompilerConstructors
    {
        protected InstantCreator figure;
        protected int length;
        protected InstantType mode;
        protected FieldInfo scodeField;
        public SortedList<short, MemberRubric> Identities = new SortedList<short, MemberRubric>();

        public ISeries<RubricModel> rubricBuilders;

        public InstantCompiler(InstantCreator instantFigure, ISeries<RubricModel> rubricBuilders)
        {
            this.rubricBuilders = rubricBuilders;
            figure = instantFigure;
            length = rubricBuilders.Count;
        }

        void resolveDisplayRubricAttributes(FieldBuilder fb, MemberInfo mi, MemberRubric mr)
        {
            object[] o = mi.GetCustomAttributes(typeof(DisplayRubricAttribute), false);
            if ((o != null) && o.Any())
            {
                DisplayRubricAttribute fda = (DisplayRubricAttribute)o.First();
                ;
                mr.DisplayName = fda.Name;

                if (fb != null)
                    CreateDisplayRubricAttribute(fb, fda);
            }
            else if (mr.DisplayName != null)
            {
                CreateDisplayRubricAttribute(fb, new DisplayRubricAttribute(mr.DisplayName));
            }
        }

        void resolveIdentityRubricAttributes(FieldBuilder fb, MemberInfo mi, MemberRubric mr)
        {
            if (!mr.IsKey)
            {
                object[] o = mi.GetCustomAttributes(typeof(IdentityRubricAttribute), false);
                if ((o != null) && o.Any())
                {
                    IdentityRubricAttribute fia = (IdentityRubricAttribute)o.First();
                    mr.IsIdentity = true;
                    mr.IsAutoincrement = fia.IsAutoincrement;

                    if (Identities.ContainsKey(fia.Order))
                        fia.Order = (short)(Identities.LastOrDefault().Key + 1);

                    mr.IdentityOrder = fia.Order;
                    Identities.Add(mr.IdentityOrder, mr);

                    if (fb != null)
                        CreateIdentityRubricAttribute(fb, fia);
                }
                else if (mr.IsIdentity)
                {
                    if (Identities.ContainsKey(mr.IdentityOrder))
                        mr.IdentityOrder += (short)(Identities.LastOrDefault().Key + 1);

                    Identities.Add(mr.IdentityOrder, mr);

                    if (fb != null)
                        CreateIdentityRubricAttribute(
                            fb,
                            new IdentityRubricAttribute
                            {
                                IsAutoincrement = mr.IsAutoincrement,
                                Order = mr.IdentityOrder
                            }
                        );
                }
            }
        }

        void resolveKeyRubricAttributes(FieldBuilder fb, MemberInfo mi, MemberRubric mr)
        {
            object[] o = mi.GetCustomAttributes(typeof(KeyAttribute), false);
            if ((o == null) || !o.Any())
                o = mi.GetCustomAttributes(typeof(KeyRubricAttribute), false);
            else
                o[0] = new KeyRubricAttribute();

            if ((o != null) && o.Any())
            {
                KeyRubricAttribute fka = (KeyRubricAttribute)o.First();
                mr.IsKey = true;
                mr.IsIdentity = true;
                mr.IsAutoincrement = fka.IsAutoincrement;

                if (Identities.ContainsKey(fka.Order))
                    fka.Order = (short)(Identities.LastOrDefault().Key + 1);

                mr.IdentityOrder = fka.Order;
                Identities.Add(mr.IdentityOrder, mr);
                mr.Required = true;

                if (fb != null)
                    CreateKeyRubricAttribute(fb, fka);
            }
            else if (mr.IsKey)
            {
                mr.IsIdentity = true;
                mr.Required = true;

                if (Identities.ContainsKey(mr.IdentityOrder))
                    mr.IdentityOrder += (short)(Identities.LastOrDefault().Key + 1);

                Identities.Add(mr.IdentityOrder, mr);

                if (fb != null)
                    CreateKeyRubricAttribute(
                        fb,
                        new KeyRubricAttribute
                        {
                            IsAutoincrement = mr.IsAutoincrement,
                            Order = mr.IdentityOrder
                        }
                    );
            }
        }

        void resolveRquiredRubricAttributes(FieldBuilder fb, MemberInfo mi, MemberRubric mr)
        {
            object[] o = mi.GetCustomAttributes(typeof(RequiredRubricAttribute), false);
            if ((o != null) && o.Any())
            {
                mr.Required = true;

                if (fb != null)
                    CreateRequiredRubricAttribute(fb);
            }
            else if (mr.Required)
            {
                if (fb != null)
                    CreateRequiredRubricAttribute(fb);
            }
        }

        void resolveTreatmentRubricAttributes(FieldBuilder fb, MemberInfo mi, MemberRubric mr)
        {
            object[] o = mi.GetCustomAttributes(typeof(RubricAggregateAttribute), false);
            if ((o != null) && o.Any())
            {
                RubricAggregateAttribute fta = (RubricAggregateAttribute)o.First();
                ;
                mr.AggregationOperand = fta.SummaryOperand;

                if (fb != null)
                    CreateTreatmentRubricAttribute(fb, fta);
            }
            else if (mr.AggregationOperand != AggregationOperand.None)
            {
                CreateTreatmentRubricAttribute(
                    fb,
                    new RubricAggregateAttribute { SummaryOperand = mr.AggregationOperand }
                );
            }
        }

        protected bool IsDerived => figure.IsDerived;

        public abstract Type CompileInstantType(string typeName);

        public virtual void CreateCompareToMethod(TypeBuilder tb)
        {
            MethodInfo mi = typeof(IComparable<IUnique>).GetMethod("CompareTo");

            ParameterInfo[] args = mi.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                mi.Name,
                mi.Attributes & (~MethodAttributes.Abstract),
                mi.CallingConvention,
                mi.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, mi);

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(
                OpCodes.Call,
                typeof(Uscn).GetMethod("CompareTo", new Type[] { typeof(IUnique) }),
                null
            );
            il.Emit(OpCodes.Ret);
        }

        public virtual void CreateEqualsMethod(TypeBuilder tb)
        {
            MethodInfo createArray = typeof(IEquatable<IUnique>).GetMethod("Equals");

            ParameterInfo[] args = createArray.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                createArray.Name,
                createArray.Attributes & (~MethodAttributes.Abstract),
                createArray.CallingConvention,
                createArray.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, createArray);

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(
                OpCodes.Call,
                typeof(Uscn).GetMethod("Equals", new Type[] { typeof(IUnique) }),
                null
            );
            il.Emit(OpCodes.Ret);
        }

        public abstract void CreateFieldsAndProperties(TypeBuilder tb);

        public virtual void CreateRubricAsAttribute(FieldBuilder field, InstantAsAttribute attrib)
        {
            field.SetCustomAttribute(
                new CustomAttributeBuilder(
                    marshalAsCtor,
                    new object[] { attrib.Value },
                    new FieldInfo[] { typeof(MarshalAsAttribute).GetField("SizeConst") },
                    new object[] { attrib.SizeConst }
                )
            );
        }

        public virtual void CreateDisplayRubricAttribute(
            FieldBuilder field,
            DisplayRubricAttribute attrib
        )
        {
            field.SetCustomAttribute(
                new CustomAttributeBuilder(figureDisplayCtor, new object[] { attrib.Name })
            );
        }

        public virtual void CreateIdentityRubricAttribute(
            FieldBuilder field,
            IdentityRubricAttribute attrib
        )
        {
            field.SetCustomAttribute(
                new CustomAttributeBuilder(
                    figureIdentityCtor,
                    Type.EmptyTypes,
                    new FieldInfo[]
                    {
                        typeof(IdentityRubricAttribute).GetField("Order"),
                        typeof(IdentityRubricAttribute).GetField("IsAutoincrement")
                    },
                    new object[] { attrib.Order, attrib.IsAutoincrement }
                )
            );
        }

        public virtual void CreateKeyRubricAttribute(FieldBuilder field, KeyRubricAttribute attrib)
        {
            field.SetCustomAttribute(
                new CustomAttributeBuilder(
                    figureKeyCtor,
                    Type.EmptyTypes,
                    new FieldInfo[]
                    {
                        typeof(KeyRubricAttribute).GetField("Order"),
                        typeof(KeyRubricAttribute).GetField("IsAutoincrement")
                    },
                    new object[] { attrib.Order, attrib.IsAutoincrement }
                )
            );
        }

        public virtual void CreateRequiredRubricAttribute(FieldBuilder field)
        {
            field.SetCustomAttribute(
                new CustomAttributeBuilder(figureRequiredCtor, Type.EmptyTypes)
            );
        }

        public virtual void CreateTreatmentRubricAttribute(
            FieldBuilder field,
            RubricAggregateAttribute attrib
        )
        {
            field.SetCustomAttribute(
                new CustomAttributeBuilder(
                    seriesTreatmentCtor,
                    Type.EmptyTypes,
                    new FieldInfo[]
                    {
                        typeof(RubricAggregateAttribute).GetField("AggregationOperand"),
                        typeof(RubricAggregateAttribute).GetField("AggregationOperand")
                    },
                    new object[] { attrib.SummaryOperand }
                )
            );
        }

        public abstract void CreateGetBytesMethod(TypeBuilder tb);

        public virtual void CreateGetEmptyProperty(TypeBuilder tb)
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
                accessor.Attributes & (~MethodAttributes.Abstract),
                accessor.CallingConvention,
                accessor.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(getter, accessor);

            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("get_Empty"), null);
            il.Emit(OpCodes.Ret);
        }

        public virtual void CreateGetGenericByIntMethod(TypeBuilder tb)
        {
            string[] typeParameterNames = { "V" };
            GenericTypeParameterBuilder[] typeParameters = tb.DefineGenericParameters(
                typeParameterNames
            );

            GenericTypeParameterBuilder V = typeParameters[0];

            MethodInfo mi = typeof(IInstant)
                .GetMethod("Get", new Type[] { typeof(int) })
                .MakeGenericMethod(typeParameters);

            ParameterInfo[] args = mi.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                mi.Name,
                mi.Attributes & (~MethodAttributes.Abstract),
                mi.CallingConvention,
                mi.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, mi);

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(
                OpCodes.Call,
                typeof(Uscn).GetMethod("CompareTo", new Type[] { typeof(IUnique) }),
                null
            );
            il.Emit(OpCodes.Ret);
        }

        public virtual void CreateGetUniqueBytesMethod(TypeBuilder tb)
        {
            MethodInfo createArray = typeof(IUnique).GetMethod("GetKeyBytes");

            ParameterInfo[] args = createArray.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                createArray.Name,
                createArray.Attributes & (~MethodAttributes.Abstract),
                createArray.CallingConvention,
                createArray.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, createArray);

            ILGenerator il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("GetKeyBytes"), null);
            il.Emit(OpCodes.Ret);
        }

        public virtual void CreateGetUniqueKeyMethod(TypeBuilder tb)
        {
            MethodInfo createArray = typeof(IUnique).GetMethod("GetKey");

            ParameterInfo[] args = createArray.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                createArray.Name,
                createArray.Attributes & (~MethodAttributes.Abstract),
                createArray.CallingConvention,
                createArray.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, createArray);

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("GetKey"), null);
            il.Emit(OpCodes.Ret);
        }

        public virtual void CreateGetUniqueSeedMethod(TypeBuilder tb)
        {
            MethodInfo createArray = typeof(IUnique).GetMethod("GetTypeKey");

            ParameterInfo[] args = createArray.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                createArray.Name,
                createArray.Attributes & (~MethodAttributes.Abstract),
                createArray.CallingConvention,
                createArray.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, createArray);

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("GetTypeKey"), null);
            il.Emit(OpCodes.Ret);
        }

        public abstract void CreateItemByIntProperty(TypeBuilder tb);

        public abstract void CreateItemByStringProperty(TypeBuilder tb);

        public virtual void CreateMarshaAslAttribute(FieldBuilder field, MarshalAsAttribute attrib)
        {
            field.SetCustomAttribute(
                new CustomAttributeBuilder(
                    marshalAsCtor,
                    new object[] { attrib.Value },
                    new FieldInfo[] { typeof(MarshalAsAttribute).GetField("SizeConst") },
                    new object[] { attrib.SizeConst }
                )
            );
        }

        public abstract void CreateSerialCodeProperty(TypeBuilder tb, Type type, string name);

        public virtual void CreateSetUniqueKeyMethod(TypeBuilder tb)
        {
            MethodInfo createArray = typeof(IUnique).GetMethod("SetKey");

            ParameterInfo[] args = createArray.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                createArray.Name,
                createArray.Attributes & (~MethodAttributes.Abstract),
                createArray.CallingConvention,
                createArray.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, createArray);

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("SetKey"), null);
            il.Emit(OpCodes.Ret);
        }

        public virtual void CreateSetTypeKeyMethod(TypeBuilder tb)
        {
            MethodInfo createArray = typeof(IUnique).GetMethod("SetTypeKey");

            ParameterInfo[] args = createArray.GetParameters();
            Type[] argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder method = tb.DefineMethod(
                createArray.Name,
                createArray.Attributes & (~MethodAttributes.Abstract),
                createArray.CallingConvention,
                createArray.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(method, createArray);

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetMethod("SetTypeKey"), null);
            il.Emit(OpCodes.Ret);
        }

        public virtual void CreateKeyProperty(TypeBuilder tb)
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
                accessor.Attributes & (~MethodAttributes.Abstract),
                accessor.CallingConvention,
                accessor.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(getter, accessor);

            prop.SetGetMethod(getter);
            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetProperty("Key").GetGetMethod(), null);
            il.Emit(OpCodes.Ret);

            MethodInfo mutator = iprop.GetSetMethod();

            args = mutator.GetParameters();
            argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder setter = tb.DefineMethod(
                mutator.Name,
                mutator.Attributes & (~MethodAttributes.Abstract),
                mutator.CallingConvention,
                mutator.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(setter, mutator);

            prop.SetSetMethod(setter);
            il = setter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetProperty("Key").GetSetMethod(), null);
            il.Emit(OpCodes.Ret);
        }

        public virtual void CreateTypeKeyProperty(TypeBuilder tb)
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
                accessor.Attributes & (~MethodAttributes.Abstract),
                accessor.CallingConvention,
                accessor.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(getter, accessor);

            prop.SetGetMethod(getter);
            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetProperty("TypeKey").GetGetMethod(), null);
            il.Emit(OpCodes.Ret);

            MethodInfo mutator = iprop.GetSetMethod();

            args = mutator.GetParameters();
            argTypes = Array.ConvertAll(args, a => a.ParameterType);

            MethodBuilder setter = tb.DefineMethod(
                mutator.Name,
                mutator.Attributes & (~MethodAttributes.Abstract),
                mutator.CallingConvention,
                mutator.ReturnType,
                argTypes
            );
            tb.DefineMethodOverride(setter, mutator);

            prop.SetSetMethod(setter);
            il = setter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, scodeField);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, typeof(Uscn).GetProperty("TypeKey").GetSetMethod(), null);
            il.Emit(OpCodes.Ret);
        }

        public abstract void CreateValueArrayProperty(TypeBuilder tb);

        public abstract TypeBuilder GetTypeBuilder(string typeName);

        public virtual void ResolveFigureAttributes(FieldBuilder fb, MemberRubric mr)
        {
            MemberInfo mi = mr.RubricInfo;
            if (
                !(((IMemberRubric)mi).MemberInfo is FieldBuilder)
                && !(((IMemberRubric)mi).MemberInfo is PropertyBuilder)
            )
            {
                resolveKeyRubricAttributes(fb, mi, mr);

                resolveIdentityRubricAttributes(fb, mi, mr);

                resolveRquiredRubricAttributes(fb, mi, mr);

                resolveDisplayRubricAttributes(fb, mi, mr);

                resolveTreatmentRubricAttributes(fb, mi, mr);
            }
        }

        public virtual void ResolveMarshalAsAttributeForArray(
            FieldBuilder field,
            MemberRubric member,
            Type type
        )
        {
            MemberInfo _member = member.RubricInfo;
            if ((member is MemberRubric) && (member.FigureField != null))
            {
                _member = member.FigureField;
            }

            object[] o = _member.GetCustomAttributes(typeof(MarshalAsAttribute), false);
            if ((o == null) || !o.Any())
            {
                o = _member.GetCustomAttributes(typeof(InstantAsAttribute), false);
                if ((o != null) && o.Any())
                {
                    InstantAsAttribute faa = (InstantAsAttribute)o.First();
                    CreateRubricAsAttribute(
                        field,
                        new InstantAsAttribute(UnmanagedType.ByValArray)
                        {
                            SizeConst = (faa.SizeConst < 1) ? 64 : faa.SizeConst
                        }
                    );
                }
                else
                {
                    int size = 64;
                    if (member.RubricSize > 0)
                        size = member.RubricSize;
                    CreateRubricAsAttribute(
                        field,
                        new InstantAsAttribute(UnmanagedType.ByValArray) { SizeConst = size }
                    );
                }
            }
            else
            {
                MarshalAsAttribute maa = (MarshalAsAttribute)o.First();
                CreateMarshaAslAttribute(
                    field,
                    new MarshalAsAttribute(UnmanagedType.ByValArray)
                    {
                        SizeConst = (maa.SizeConst < 1) ? 64 : maa.SizeConst
                    }
                );
            }
        }

        public void ResolveMarshalAsAttributeForString(
            FieldBuilder field,
            MemberRubric member,
            Type type
        )
        {
            MemberInfo _member = member.RubricInfo;
            if ((member is MemberRubric) && (member.FigureField != null))
            {
                _member = member.FigureField;
            }

            object[] o = _member.GetCustomAttributes(typeof(MarshalAsAttribute), false);
            if ((o == null) || !o.Any())
            {
                o = _member.GetCustomAttributes(typeof(InstantAsAttribute), false);
                if ((o != null) && o.Any())
                {
                    InstantAsAttribute maa = (InstantAsAttribute)o.First();
                    CreateRubricAsAttribute(
                        field,
                        new InstantAsAttribute(UnmanagedType.ByValTStr)
                        {
                            SizeConst = (maa.SizeConst < 1) ? 64 : maa.SizeConst
                        }
                    );
                }
                else
                {
                    int size = 64;
                    if (member.RubricSize > 0)
                        size = member.RubricSize;
                    CreateRubricAsAttribute(
                        field,
                        new InstantAsAttribute(UnmanagedType.ByValTStr) { SizeConst = size }
                    );
                }
            }
            else
            {
                MarshalAsAttribute maa = (MarshalAsAttribute)o.First();
                CreateMarshaAslAttribute(
                    field,
                    new MarshalAsAttribute(UnmanagedType.ByValTStr)
                    {
                        SizeConst = (maa.SizeConst < 1) ? 64 : maa.SizeConst
                    }
                );
            }
        }
    }
}
