using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Radical.Instant.Attributes;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Radical.Origins
{
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
    public abstract class Origin : IOrigin
    {
        [JsonIgnore]
        public virtual int OriginKey { get; set; }

        [StringLength(32)]
        [DataMember(Order = 8)]
        [InstantAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public virtual string OriginName { get; set; }

        [Column(TypeName = "timestamp")]
        [DataMember(Order = 9)]
        [InstantAs(UnmanagedType.I8, SizeConst = 8)]
        public virtual DateTime Modified { get; set; } = DateTime.Now;

        [StringLength(32)]
        [DataMember(Order = 10)]
        [InstantAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public virtual string Modifier { get; set; }

        [Column(TypeName = "timestamp")]
        [DataMember(Order = 11)]
        [InstantAs(UnmanagedType.I8, SizeConst = 8)]
        public virtual DateTime Created { get; set; }

        [StringLength(32)]
        [DataMember(Order = 12)]
        [InstantAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public virtual string Creator { get; set; }
    }
}
