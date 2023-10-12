namespace Radical.Instant.Rubrics
{
    using System;
    using System.Reflection;
    using Radical.Uniques;

    public class RubricModel : IUnique
    {
        public RubricModel(MemberRubric member)
        {
            SetMember(member);
        }

        public MemberInfo SetMember(MemberRubric member)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                SetField((FieldRubric)member.RubricInfo);
                Member ??= member;
                Member.FigureField = member.FigureField;
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                SetProperty((PropertyRubric)member.RubricInfo);
                Member = member;
            }
            return null;
        }

        public PropertyRubric SetProperty(PropertyRubric property)
        {
            Property = property;
            Name ??= property.RubricName;
            Type = property.PropertyType;
            Getter = property.GetGetMethod();
            Setter = property.GetSetMethod();
            return property;
        }

        public FieldRubric SetField(FieldRubric field)
        {
            Field = field;
            Type ??= field.FieldType;
            FieldType = field.FieldType;
            FieldName = field.FieldName;
            Name ??= field.RubricName;
            return field;
        }

        public string Name { get; set; }
        public string FieldName { get; set; }
        public Type Type { get; set; }
        public MemberRubric Member { get; set; }
        public PropertyRubric Property { get; set; }
        public FieldRubric Field { get; set; }
        public MethodInfo Getter { get; set; }
        public MethodInfo Setter { get; set; }
        public Type FieldType { get; set; }

        public IUnique Empty => throw new NotImplementedException();
        public ulong Key
        {
            get => Name.UniqueKey64();
            set => throw new NotImplementedException();
        }
        public ulong TypeKey
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public int CompareTo(IUnique other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IUnique other)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        public byte[] GetKeyBytes()
        {
            throw new NotImplementedException();
        }
    }
}
