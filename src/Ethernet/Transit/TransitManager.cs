using System;
using System.Linq;

namespace Radical.Ethernet.Transit
{
    using Security.Identity;

    public class TransitManager
    {
        private EthernetContext context;
        private ServiceSite site;
        private EthernetTransit transaction;
        private ITransitContext transferContext;
        private EthernetOperation operation;

        public TransitManager(EthernetTransit _transaction)
        {
            transaction = _transaction;
            transferContext = transaction.Context;
            context = transaction.MyHeader.Context;
            site = context.IdentitySite;
            operation = new EthernetOperation(_transaction);
        }

        public void HeaderContent(object data, object value, DirectionType _direction)
        {
            DirectionType direction = _direction;
            object _data = value;
            if (_data != null)
            {
                Type[] ifaces = _data.GetType().GetInterfaces();
                if (ifaces.Contains(typeof(ITransitFormatter)))
                {
                    transaction.MyHeader.Context.ContentType = _data.GetType();

                    if (direction == DirectionType.Send)
                        _data = ((ITransitFormatter)value).GetHeader();

                    object[] messages_ = null;
                    if (operation.Execute(_data, direction, out messages_))
                    {
                        if (messages_.Length > 0)
                        {
                            context.ObjectsCount = messages_.Length;
                            for (int i = 0; i < context.ObjectsCount; i++)
                            {
                                ITransitFormatter message = ((ITransitFormatter[])messages_)[i];
                                ITransitFormatter head = (ITransitFormatter)
                                    ((ITransitFormatter[])messages_)[i].GetHeader();
                                message.SerialCount = message.ItemsCount;
                                head.SerialCount = message.ItemsCount;
                            }

                            if (direction == DirectionType.Send)
                                transaction.MyMessage.Data = messages_;
                            else
                                transaction.MyMessage.Data = (
                                    (ITransitFormatter)_data
                                ).GetHeader();
                        }
                    }
                }
            }
            data = _data;
        }

        public void MessageContent(ref object content, object value, DirectionType _direction)
        {
            DirectionType direction = _direction;
            object _content = value;
            if (_content != null)
            {
                if (direction == DirectionType.Receive)
                {
                    Type[] ifaces = _content.GetType().GetInterfaces();
                    if (ifaces.Contains(typeof(ITransitFormatter)))
                    {
                        object[] messages_ = ((ITransitFormatter)value).GetMessage();
                        if (messages_ != null)
                        {
                            int length = messages_.Length;
                            for (int i = 0; i < length; i++)
                            {
                                ITransitFormatter message = ((ITransitFormatter[])messages_)[i];
                                ITransitFormatter head = (ITransitFormatter)
                                    ((ITransitFormatter[])messages_)[i].GetHeader();
                                message.SerialCount = head.SerialCount;
                                message.DeserialCount = head.DeserialCount;
                            }

                            _content = messages_;
                        }
                    }
                }
            }
            content = _content;
        }
    }
}
