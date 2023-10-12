using System.Threading;

namespace Radical.Ethernet.Server
{
    using Transit;
    using Security.Identity;
    using Invoking;

    public class EthernetServer : IEthernetServer
    {
        public static IMemberSecurity Security;
        private IEthernetListener server;

        public void ClearClients()
        {
            Echo("Client registry cleaned");
            if (server != null)
                server.ClearClients();
        }

        public void ClearResources()
        {
            Echo("Resource buffer cleaned");
            if (server != null)
                server.ClearResources();
        }

        public void Close()
        {
            if (server != null)
            {
                Echo("Server instance shutdown ");
                server.CloseListener();
                server = null;
            }
            else
            {
                Echo("Server instance doesn't exist ");
            }
        }

        public ITransitContext HeaderReceived(object inetdealcontext)
        {
            string clientEcho = ((ITransitContext)inetdealcontext)
                .Transfer
                .HeaderReceived
                .Context
                .Echo;
            Echo(string.Format("Client header received"));
            if (clientEcho != null && clientEcho != "")
                Echo(string.Format("Client echo: {0}", clientEcho));

            EthernetContext trctx = ((ITransitContext)inetdealcontext).Transfer.MyHeader.Context;
            if (trctx.Echo == null || trctx.Echo == "")
                trctx.Echo = "Server say Hello";
            if (!((ITransitContext)inetdealcontext).Synchronic)
                server.Send(TransitPart.Header, ((ITransitContext)inetdealcontext).Id);
            else
                server.Receive(TransitPart.Message, ((ITransitContext)inetdealcontext).Id);

            return ((ITransitContext)inetdealcontext);
        }

        public ITransitContext HeaderSent(object inetdealcontext)
        {
            Echo("Server header sent");

            ITransitContext context = (ITransitContext)inetdealcontext;
            if (context.Close)
            {
                context.Transfer.Dispose();
                server.CloseClient(context.Id);
            }
            else
            {
                if (!context.Synchronic)
                {
                    if (context.ReceiveMessage)
                        server.Receive(TransitPart.Message, context.Id);
                }
                if (context.SendMessage)
                    server.Send(TransitPart.Message, context.Id);
            }
            return context;
        }

        public bool IsActive()
        {
            if (server != null)
            {
                Echo("Server Instance Is Active");
                return true;
            }
            else
            {
                Echo("Server Instance Doesn't Exist");
                return false;
            }
        }

        public ITransitContext MessageReceived(object inetdealcontext)
        {
            Echo(string.Format("Client message received"));
            if (((ITransitContext)inetdealcontext).Synchronic)
                server.Send(TransitPart.Header, ((ITransitContext)inetdealcontext).Id);
            return (ITransitContext)inetdealcontext;
        }

        public ITransitContext MessageSent(object inetdealcontext)
        {
            Echo("Server message sent");
            ITransitContext result = (ITransitContext)inetdealcontext;
            if (result.Close)
            {
                result.Transfer.Dispose();
                server.CloseClient(result.Id);
            }
            return result;
        }

        public void Start(
            MemberIdentity ServerIdentity,
            IMemberSecurity security = null,
            IInvoker echoMethod = null
        )
        {
            server = EthernetListener.Instance;
            server.Identity = ServerIdentity;
            Security = security;

            new Thread(new ThreadStart(server.StartListening)).Start();

            server.HeaderSent = new EthernetMethod(nameof(this.HeaderSent), this);
            server.MessageSent = new EthernetMethod(nameof(this.MessageSent), this);
            server.HeaderReceived = new EthernetMethod(nameof(this.HeaderReceived), this);
            server.MessageReceived = new EthernetMethod(nameof(this.MessageReceived), this);
            server.WriteEcho = echoMethod;

            Echo("Server instance started");
        }

        public void Echo(string message)
        {
            if (server != null)
                server.Echo(message);
        }
    }
}
