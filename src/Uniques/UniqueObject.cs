using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Radical.Extracting;
using Radical.Instant;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Radical.Uniques
{
    using Instant.Attributes;
    using Instant.Rubrics.Attributes;
    using Instant.Proxies;

    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
    public class UniqueObject : InnerProxy, IUniqueObject
    {
        private Uscn code;

        public UniqueObject() : this(true) { }

        public UniqueObject(bool autoId)
        {
            if (!autoId)
                return;

            code.Key = Unique.NewKey;
            code.TypeKey = this.GetType().UniqueKey();
        }

        [Required]
        [StringLength(32)]
        [ConcurrencyCheck]
        [DataMember(Order = 0)]
        [InstantAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public virtual Uscn Code
        {
            get => code;
            set => code = value;
        }

        [KeyRubric]
        [DataMember(Order = 1)]
        public virtual long Id
        {
            get => (long)code.Key;
            set => code.Key = (ulong)value;
        }

        [DataMember(Order = 2)]
        public virtual int Ordinal
        {
            get => (int)code.ValueFromXYZ(10, 25 * 1000);
            set => code.ValueToXYZ(10, 25 * 1000, (ulong)value);
        }

        [IdentityRubric]
        [NotMapped]
        public virtual int TypeKey
        {
            get => (int)base.TypeKey;
            set => base.TypeKey = (uint)value;
        }

        [KeyRubric]
        [DataMember(Order = 3)]
        public virtual long SourceId { get; set; } = -1;

        [DataMember(Order = 4)]
        [InstantAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public virtual string SourceType { get; set; }

        [KeyRubric]
        [DataMember(Order = 5)]
        public virtual long TargetId { get; set; } = -1;

        [DataMember(Order = 6)]
        [InstantAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public virtual string TargetType { get; set; }

        [DataMember(Order = 7)]
        [InstantAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public virtual string Label { get; set; }

        [NotMapped]
        public override int OriginKey
        {
            get { return (int)code.OriginKey; }
            set { code.OriginKey = (uint)value; }
        }

        [KeyRubric]
        [DataMember(Order = 199)]
        [Column(Order = 199)]
        [InstantAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public virtual string CodeNumber { get; set; }

        public long AutoId()
        {
            ulong key = code.Key;
            if (key != 0)
                return (long)key;

            ulong id = Unique.NewKey;
            code.Key = id;
            code.TypeKey = this.GetType().UniqueKey();
            return (long)id;
        }
    }
}
