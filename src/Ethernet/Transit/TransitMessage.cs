using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Radical.Ethernet.Transit
{
  
    [Serializable]
    public class TransitMessage : ITransitFormatter, IDisposable
    {
        private object data;
        private DirectionType direction;

        [NonSerialized]
        private EthernetTransit transaction;

        public TransitMessage()
        {
            data = new object();
            SerialCount = 0;
            DeserialCount = 0;
            direction = DirectionType.Receive;
        }

        public TransitMessage(EthernetTransit _transaction, DirectionType _direction, object message = null)
        {
            transaction = _transaction;
            direction = _direction;

            if (message != null)
                Data = message;
            else
                data = new object();

            SerialCount = 0;
            DeserialCount = 0;
        }

        public object Data
        {
            get { return data; }
            set { transaction.Manager.MessageContent(ref data, value, direction); }
        }

        public int DeserialCount { get; set; }

        public int ItemsCount
        {
            get
            {
                return (data != null) ? ((ITransitFormatter[])data).Sum(t => t.ItemsCount) : 0;
            }
        }

        public string Notice { get; set; }

        public int ObjectsCount
        {
            get { return (data != null) ? ((ITransitFormatter[])data).Length : 0; }
        }

        public int ProgressCount { get; set; }

        public int SerialCount { get; set; }

        public object Deserialize(
            ITransitBuffer buffer
        )
        {          
                return -1;
        }

        public object Deserialize(
            Stream fromstream
        )
        {           
                return -1;
        }

        public void Dispose()
        {
            data = null;
        }

        public object GetHeader()
        {
            if (direction == DirectionType.Send)
                return transaction.MyHeader.Data;
            else
                return transaction.HeaderReceived.Data;
        }

        public object[] GetMessage()
        {
            if (data != null)
                return (ITransitFormatter[])data;
            return null;
        }

        public int Serialize(
            ITransitBuffer buffer,
            int offset,
            int batchSize
        )
        {
            buffer = transaction.Context;
            buffer.SerialBlock = JsonSerializer.SerializeToUtf8Bytes(this, typeof(TransitMessage));
            return (int)buffer.SerialBlock.Length;
        }

        public int Serialize(
            Stream tostream,
            int offset,
            int batchSize
        )
        {
            tostream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(this, typeof(TransitMessage)));
            return (int)tostream.Length;
        }
    }

   
}
