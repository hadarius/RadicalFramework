namespace Radical.Instant.Rubrics
{
    using System.Linq;
    using System.Reflection;
    using Uniques;

    public class MemberRubric : MemberInfo, IRubric
    {
        Uscn code;
        public object deltamark;

        public bool deltamarkset = false;

        public MemberRubric() { }

        public MemberRubric(FieldInfo field) : this((IMemberRubric)new FieldRubric(field)) { }

        public MemberRubric(FieldRubric field) : this((IMemberRubric)field) { }

        public MemberRubric(IMemberRubric member)
        {
            RubricInfo = (MemberInfo)member;
            RubricName = member.RubricName;
            RubricId = member.RubricId;
            Visible = member.Visible;
            Editable = member.Editable;
            if (RubricInfo.MemberType == MemberTypes.Method)
                code.Key = (
                    $"{RubricName}_{(new string(RubricParameterInfo.SelectMany(p => p.ParameterType.Name).ToArray()))}"
                ).UniqueKey64();
            else
                code.Key = RubricName.UniqueKey64();
        }

        public MemberRubric(MethodInfo method) : this((IMemberRubric)new MethodRubric(method)) { }

        public MemberRubric(MethodRubric method) : this((IMemberRubric)method) { }

        public MemberRubric(PropertyInfo property)
            : this((IMemberRubric)new PropertyRubric(property)) { }

        public MemberRubric(PropertyRubric property) : this((IMemberRubric)property) { }

        public MemberRubric(MemberRubric member, bool copyMode = false)
            : this(
                (!copyMode && (member.RubricInfo != null))
                    ? ((IMemberRubric)member.RubricInfo)
                    : member
            )
        {
            FigureType = member.FigureType;
            FigureField = member.FigureField;
            FieldId = member.FieldId;
            RubricOffset = member.RubricOffset;
            IsKey = member.IsKey;
            IsIdentity = member.IsIdentity;
            IsAutoincrement = member.IsAutoincrement;
            IdentityOrder = member.IdentityOrder;
            Required = member.Required;
            DisplayName = member.DisplayName;
            RubricSize = member.RubricSize;
        }

        public int CompareTo(IUnique other)
        {
            return (int)(Key - other.Key);
        }

        public bool Equals(IUnique other)
        {
            return Key == other.Key;
        }

        public byte[] GetBytes()
        {
            return code.GetBytes();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return RubricInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return RubricInfo.GetCustomAttributes(attributeType, inherit);
        }

        public byte[] GetKeyBytes()
        {
            return code.GetKeyBytes();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return RubricInfo.IsDefined(attributeType, inherit);
        }

        public MemberRubric ShalowCopy(MemberRubric dst)
        {
            if (dst == null)
                dst = new MemberRubric();

            dst.RubricInfo = this;
            dst.RubricName = RubricName;
            dst.RubricId = RubricId;
            dst.Visible = Visible;
            dst.Editable = Editable;
            dst.FigureType = FigureType;
            dst.FigureField = FigureField;
            dst.FieldId = FieldId;
            dst.RubricOffset = RubricOffset;
            dst.IsKey = IsKey;
            dst.IsIdentity = IsIdentity;
            dst.IsAutoincrement = IsAutoincrement;
            dst.IdentityOrder = IdentityOrder;
            dst.Required = Required;
            dst.DisplayName = DisplayName;
            dst.code.Key = RubricName.UniqueKey64();

            return dst;
        }

        public override Type DeclaringType =>
            (FigureType != null) ? FigureType : RubricInfo.DeclaringType;

        public object DeltaMark
        {
            get
            {
                if (!deltamarkset)
                {
                    deltamark = RubricType.Default();
                    deltamarkset = true;
                }

                return deltamark;
            }
        }

        public string DisplayName { get; set; }

        public bool Editable { get; set; }

        public IUnique Empty => Uscn.Empty;

        public int FieldId { get; set; }

        public FieldInfo FigureField { get; set; }

        public Type FigureType { get; set; }

        public MethodInfo Getter =>
            (MemberType == MemberTypes.Property) ? ((PropertyRubric)RubricInfo).GetMethod : null;

        public short IdentityOrder { get; set; }

        public bool IsAutoincrement { get; set; }

        public bool IsDBNull { get; set; }

        public bool IsExpandable { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsKey { get; set; }

        public bool IsUnique { get; set; }

        public MemberInfo MemberInfo => RubricInfo;

        public override MemberTypes MemberType => RubricInfo.MemberType;

        public override string Name => RubricInfo.Name;

        public override Type ReflectedType => RubricInfo.ReflectedType;

        public bool Required { get; set; }

        public object[] RubricAttributes
        {
            get => VirtualInfo.RubricAttributes;
            set => VirtualInfo.RubricAttributes = value;
        }

        public int RubricId { get; set; }

        public MemberInfo RubricInfo { get; set; }

        public Module RubricModule =>
            (MemberType == MemberTypes.Method) ? ((MethodRubric)RubricInfo).RubricModule : null;

        public string RubricName { get; set; }

        public int RubricOffset { get; set; }

        public ParameterInfo[] RubricParameterInfo =>
            (MemberType == MemberTypes.Method)
                ? ((MethodRubric)RubricInfo).RubricParameterInfo
                : null;

        public Type RubricReturnType =>
            (MemberType == MemberTypes.Method) ? ((MethodRubric)RubricInfo).RubricReturnType : null;

        public MemberRubrics Rubrics { get; set; }

        public int RubricSize
        {
            get => VirtualInfo.RubricSize;
            set => VirtualInfo.RubricSize = value;
        }

        public Type RubricType
        {
            get => VirtualInfo.RubricType;
            set => VirtualInfo.RubricType = value;
        }

        public Uscn Code
        {
            get => code;
            set => code = value;
        }

        public MethodInfo Setter =>
            (MemberType == MemberTypes.Property) ? ((PropertyRubric)RubricInfo).SetMethod : null;

        public AggregationOperand AggregationOperand { get; set; }

        public int AggregationOrdinal { get; set; }

        public IRubric AggregateRubric { get; set; }

        public ulong Key
        {
            get => code.Key;
            set => code.SetKey(value);
        }

        public ulong TypeKey
        {
            get => code.TypeKey;
            set => code.SetTypeKey(value);
        }

        public IMemberRubric VirtualInfo => (IMemberRubric)RubricInfo;

        public bool Visible { get; set; }
    }
}
