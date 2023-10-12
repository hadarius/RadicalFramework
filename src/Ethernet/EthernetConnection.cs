namespace Radical.Ethernet
{
    using System;
    using Client;
    using Transit;
    using Invoking;
    using Security.Identity;
    using System.Threading;

    public interface IEthernetConnection
    {
        object Content { get; set; }

        void Close();

        ITransitContext Initiate(bool isAsync = true);

        void Reconnect();

        void SetCallback(IInvoker OnCompleteEvent);

        void SetCallback(string methodName, object classObject);
    }

    public class EthernetConnection : IEthernetConnection
    {
        private readonly ManualResetEvent completeNotice = new ManualResetEvent(false);
        public IInvoker CompleteMethod;
        public IInvoker EchoMethod;
        private IInvoker connected;
        private IInvoker headerReceived;
        private IInvoker headerSent;
        private IInvoker messageReceived;
        private IInvoker messageSent;
        private bool isAsync = true;

        public EthernetConnection(
            MemberIdentity ClientIdentity,
            IInvoker OnCompleteEvent = null,
            IInvoker OnEchoEvent = null
        )
        {
            MemberIdentity ci = ClientIdentity;
            ci.Site = ServiceSite.Client;
            EthernetClient client = new EthernetClient(ci);
            Transit = new EthernetTransit(ci);

            connected = new EthernetMethod(nameof(this.Connected), this);
            headerSent = new EthernetMethod(nameof(this.HeaderSent), this);
            messageSent = new EthernetMethod(nameof(this.MessageSent), this);
            headerReceived = new EthernetMethod(nameof(this.HeaderReceived), this);
            messageReceived = new EthernetMethod(nameof(this.MessageReceived), this);

            client.Connected = connected;
            client.HeaderSent = headerSent;
            client.MessageSent = messageSent;
            client.HeaderReceived = headerReceived;
            client.MessageReceived = messageReceived;

            CompleteMethod = OnCompleteEvent;
            EchoMethod = OnEchoEvent;

            Client = client;

            WriteEcho("Client Connection Created");
        }

        public object Content
        {
            get { return Transit.MyHeader.Data; }
            set { Transit.MyHeader.Data = value; }
        }

        public ITransitContext Context { get; set; }

        public EthernetTransit Transit { get; set; }

        private EthernetClient Client { get; set; }

        public void Close()
        {
            Client.Dispose();
        }

        public ITransitContext Connected(object inetdealclient)
        {
            WriteEcho("Client Connection Established");
            Transit.MyHeader.Context.Echo = "Client say Hello. ";
            Context = Client.Context;
            Client.Context.Transfer = Transit;

            IEthernetClient idc = (IEthernetClient)inetdealclient;

            idc.Send(TransitPart.Header);

            return idc.Context;
        }

        public ITransitContext HeaderReceived(object inetdealclient)
        {
            string serverEcho = Transit.HeaderReceived.Context.Echo;
            WriteEcho(string.Format("Server header received"));
            if (serverEcho != null && serverEcho != "")
                WriteEcho(string.Format("Server echo: {0}", serverEcho));

            IEthernetClient idc = (IEthernetClient)inetdealclient;

            if (idc.Context.Close)
                idc.Dispose();
            else
            {
                if (!idc.Context.Synchronic)
                {
                    if (idc.Context.SendMessage)
                        idc.Send(TransitPart.Message);
                }

                if (idc.Context.ReceiveMessage)
                    idc.Receive(TransitPart.Message);
            }

            if (!idc.Context.ReceiveMessage && !idc.Context.SendMessage)
            {
                if (CompleteMethod != null)
                    CompleteMethod.Invoke(idc.Context);
                if (!isAsync)
                    completeNotice.Set();
            }

            return idc.Context;
        }

        public ITransitContext HeaderSent(object inetdealclient)
        {
            WriteEcho("Client header sent");
            IEthernetClient idc = (IEthernetClient)inetdealclient;
            if (!idc.Context.Synchronic)
                idc.Receive(TransitPart.Header);
            else
                idc.Send(TransitPart.Message);

            return idc.Context;
        }

        public ITransitContext Initiate(bool IsAsync = true)
        {
            isAsync = IsAsync;
            Client.Connect();
            if (!isAsync)
            {
                completeNotice.WaitOne();
                return Context;
            }

            return null;
        }

        public ITransitContext MessageReceived(object inetdealclient)
        {
            WriteEcho(string.Format("Server message received"));

            ITransitContext context = ((IEthernetClient)inetdealclient).Context;
            if (context.Close)
                ((IEthernetClient)inetdealclient).Dispose();

            if (CompleteMethod != null)
                CompleteMethod.Invoke(context);
            if (!isAsync)
                completeNotice.Set();
            return context;
        }

        public ITransitContext MessageSent(object inetdealclient)
        {
            WriteEcho("Client message sent");

            IEthernetClient idc = (IEthernetClient)inetdealclient;
            if (idc.Context.Synchronic)
                idc.Receive(TransitPart.Header);

            if (!idc.Context.ReceiveMessage)
            {
                if (CompleteMethod != null)
                    CompleteMethod.Invoke(idc.Context);
                if (!isAsync)
                    completeNotice.Set();
            }

            return idc.Context;
        }

        public void Reconnect()
        {
            MemberIdentity ci = new MemberIdentity()
            {
                AuthId = Client.Identity.AuthId,
                Site = ServiceSite.Client,
                Name = Client.Identity.Name,
                Token = Client.Identity.Token,
                UserId = Client.Identity.UserId,
                ClientId = Client.Identity.ClientId,
                DataPath = Client.Identity.DataPath,
                Id = Client.Identity.Id,
                Ip = Client.EndPoint.Address.ToString(),
                Port = Client.EndPoint.Port,
                Key = Client.Identity.Key
            };
            Transit.Dispose();
            EthernetClient client = new EthernetClient(ci);
            Transit = new EthernetTransit(ci);
            client.Connected = connected;
            client.HeaderSent = headerSent;
            client.MessageSent = messageSent;
            client.HeaderReceived = headerReceived;
            client.MessageReceived = messageReceived;
            Client = client;
        }

        public void SetCallback(IInvoker OnCompleteEvent)
        {
            CompleteMethod = OnCompleteEvent;
        }

        public void SetCallback(string methodName, object classObject)
        {
            CompleteMethod = new EthernetMethod(methodName, classObject);
        }

        private void WriteEcho(string message)
        {
            if (EchoMethod != null)
                EchoMethod.Invoke(message);
        }
    }
}
