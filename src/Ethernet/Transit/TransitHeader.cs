using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace Radical.Ethernet.Transit
{
    using Security.Identity;

    [Serializable]
    public class TransitHeader : ITransitFormatter, IDisposable
    {
        [NonSerialized]
        private EthernetTransit transaction;

        public TransitHeader()
        {
            Context = new EthernetContext();
            SerialCount = 0;
            DeserialCount = 0;
        }

        public TransitHeader(EthernetTransit _transaction)
        {
            Context = new EthernetContext();
            transaction = _transaction;
            SerialCount = 0;
            DeserialCount = 0;
        }

        public TransitHeader(EthernetTransit _transaction, ITransitContext context)
        {
            Context = new EthernetContext();
            Context.LocalEndPoint = (IPEndPoint)context.Listener.LocalEndPoint;
            Context.RemoteEndPoint = (IPEndPoint)context.Listener.RemoteEndPoint;
            transaction = _transaction;
            SerialCount = 0;
            DeserialCount = 0;
        }

        public TransitHeader(
            EthernetTransit _transaction,
            ITransitContext context,
            MemberIdentity identity
        )
        {
            Context = new EthernetContext();
            Context.LocalEndPoint = (IPEndPoint)context.Listener.LocalEndPoint;
            Context.RemoteEndPoint = (IPEndPoint)context.Listener.RemoteEndPoint;
            Context.Identity = identity;
            Context.IdentitySite = identity.Site;
            transaction = _transaction;
            SerialCount = 0;
            DeserialCount = 0;
        }

        public TransitHeader(EthernetTransit _transaction, MemberIdentity identity)
        {
            Context = new EthernetContext();
            Context.Identity = identity;
            Context.IdentitySite = identity.Site;
            transaction = _transaction;
            SerialCount = 0;
            DeserialCount = 0;
        }

        public object Data { get; set; }

        public EthernetContext Context { get; set; }

        public int DeserialCount { get; set; }

        public int ItemsCount
        {
            get { return Context.ObjectsCount; }
        }

        public int ProgressCount { get; set; }

        public int SerialCount { get; set; }

        public void BindContext(ITransitContext context)
        {
            Context.LocalEndPoint = (IPEndPoint)context.Listener.LocalEndPoint;
            Context.RemoteEndPoint = (IPEndPoint)context.Listener.RemoteEndPoint;
        }

        public object Deserialize(
            ITransitBuffer buffer
        )
        {          
                return null;
        }

        public object Deserialize(
            Stream fromstream
        )
        {
                return null;
        }

        public void Dispose()
        {
            Data = null;
        }

        public object GetHeader()
        {
            return this;
        }

        public object[] GetMessage()
        {
            return null;
        }

        public int Serialize(
    ITransitBuffer buffer,
    int offset,
    int batchSize
)
        {
            buffer.SerialBlock = JsonSerializer.SerializeToUtf8Bytes(this, typeof(TransitHeader));
            return (int)buffer.SerialBlock.Length;
        }

        public int Serialize(
            Stream tostream,
            int offset,
            int batchSize
        )
        {
            tostream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(this, typeof(TransitHeader)));
            return (int)tostream.Length;
        }
    }

  
}
