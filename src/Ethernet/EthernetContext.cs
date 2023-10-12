using System.Net;

namespace Radical.Ethernet;

using Security.Identity;

[Serializable]
public class EthernetContext
{
    public string MethodTypeName;
    public string MethodName;

    [NonSerialized]
    public IPEndPoint LocalEndPoint;

    [NonSerialized]
    public IPEndPoint RemoteEndPoint;

    [NonSerialized]
    private Type contentType;

    public TransitComplexity Complexity { get; set; } = TransitComplexity.Standard;

    public Type ContentType
    {
        get
        {
            if (contentType == null && ContentTypeName != null)
                ContentType = Assemblies.FindType(ContentTypeName);
            return contentType;
        }
        set
        {
            if (value != null)
            {
                ContentTypeName = value.FullName;
                contentType = value;
            }
        }
    }

    public string ContentTypeName { get; set; }

    public string Echo { get; set; }

    public int Errors { get; set; }

    public MemberIdentity Identity { get; set; }

    public ServiceSite IdentitySite { get; set; } = ServiceSite.Client;

    public int ObjectsCount { get; set; } = 0;

    public bool ReceiveMessage { get; set; } = true;

    public bool SendMessage { get; set; } = true;

    public bool Synchronic { get; set; } = false;
}
