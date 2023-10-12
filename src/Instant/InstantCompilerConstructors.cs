namespace Radical.Instant
{
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;    
    using Rubrics.Attributes;

    public class InstantCompilerConstructors
    {
        protected readonly ConstructorInfo dataMemberCtor =
            typeof(DataMemberAttribute).GetConstructor(Type.EmptyTypes);
        protected readonly PropertyInfo[] dataMemberProps = new[]
        {
            typeof(DataMemberAttribute).GetProperty("Order"),
            typeof(DataMemberAttribute).GetProperty("Name")
        };
        protected readonly ConstructorInfo figureDisplayCtor =
            typeof(DisplayRubricAttribute).GetConstructor(new Type[] { typeof(string) });
        protected readonly ConstructorInfo figureIdentityCtor =
            typeof(IdentityRubricAttribute).GetConstructor(Type.EmptyTypes);
        protected readonly ConstructorInfo figureKeyCtor =
            typeof(KeyRubricAttribute).GetConstructor(Type.EmptyTypes);
        protected readonly ConstructorInfo keyCtor = typeof(KeyAttribute).GetConstructor(
            Type.EmptyTypes
        );
        protected readonly ConstructorInfo figureRequiredCtor =
            typeof(RequiredRubricAttribute).GetConstructor(Type.EmptyTypes);
        protected readonly ConstructorInfo figureLinkCtor =
            typeof(LinkRubricAttribute).GetConstructor(Type.EmptyTypes);
        protected readonly ConstructorInfo requiredCtor = typeof(RequiredAttribute).GetConstructor(
            Type.EmptyTypes
        );
        protected readonly ConstructorInfo seriesTreatmentCtor =
            typeof(RubricAggregateAttribute).GetConstructor(Type.EmptyTypes);
        protected readonly ConstructorInfo marshalAsCtor =
            typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });
        protected readonly ConstructorInfo structLayoutCtor =
            typeof(StructLayoutAttribute).GetConstructor(new Type[] { typeof(LayoutKind) });
        protected readonly FieldInfo[] structLayoutFields = new[]
        {
            typeof(StructLayoutAttribute).GetField("CharSet"),
            typeof(StructLayoutAttribute).GetField("Pack")
        };
    }
}
