namespace Radical.Ethernet.Client
{
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Transit;
    using Security.Identity;
    using Invoking;

    public sealed class EthernetClient : IEthernetClient
    {
        private readonly ManualResetEvent connectNotice = new ManualResetEvent(false);
        public IPEndPoint EndPoint;
        private ITransitContext context;
        private IPHostEntry host;
        private MemberIdentity identity;
        private IPAddress ip;
        private ushort port;
        private Socket socket;
        private int timeout = 50;

        public EthernetClient(MemberIdentity ConnectionIdentity)
        {
            Identity = ConnectionIdentity;

            if (Identity.Ip == null || Identity.Ip == "")
                Identity.Ip = "127.0.0.1";
            ip = IPAddress.Parse(Identity.Ip);
            port = Convert.ToUInt16(Identity.Port);
            host = Dns.GetHostEntry(
                (Identity.Ip != null && Identity.Ip != string.Empty) ? Identity.Ip : string.Empty
            );

            EndPoint = new IPEndPoint(ip, port);
        }

        public IInvoker Connected { get; set; }

        public ITransitContext Context
        {
            get { return context; }
            set { context = value; }
        }

        public IInvoker HeaderReceived { get; set; }

        public IInvoker HeaderSent { get; set; }

        public MemberIdentity Identity
        {
            get
            {
                return (identity != null)
                    ? identity
                    : identity = new MemberIdentity()
                    {
                        Id = 0,
                        Ip = "127.0.0.1",
                        Host = "localhost",
                        Port = 44004,
                        Limit = 0,
                        Scale = 0,
                        Site = ServiceSite.Client
                    };
            }
            set
            {
                if (value != null)
                {
                    value.Site = ServiceSite.Client;
                    identity = value;
                }
            }
        }

        public IInvoker MessageReceived { get; set; }

        public IInvoker MessageSent { get; set; }

        public void Connect()
        {
            ushort _port = port;
            string hostname = host.HostName;
            IPAddress _ip = ip;
            IPEndPoint endpoint = new IPEndPoint(_ip, _port);

            try
            {
                socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );
                context = new TransitContext(socket);
                socket.BeginConnect(endpoint, OnConnectCallback, context);
                connectNotice.WaitOne();

                Connected.Invoke(this);
            }
            catch (SocketException ex) { throw ex; }
        }

        public void Dispose()
        {
            connectNotice.Dispose();
            Close();
        }

        public bool IsConnected()
        {
            if (socket != null && socket.Connected)
                return !(
                    socket.Poll(timeout * 1000, SelectMode.SelectRead) && socket.Available == 0
                );
            return true;
        }

        public void Receive(TransitPart messagePart)
        {
            AsyncCallback callback = HeaderReceivedCallBack;
            if (messagePart != TransitPart.Header && context.ReceiveMessage)
            {
                callback = MessageReceivedCallBack;
                context.ObjectRemainds = context.Transfer.HeaderReceived.Context.ObjectsCount;
                context.Listener.BeginReceive(
                    context.MessageBuffer,
                    0,
                    context.BufferSize,
                    SocketFlags.None,
                    callback,
                    context
                );
            }
            else
                context.Listener.BeginReceive(
                    context.HeaderBuffer,
                    0,
                    context.BufferSize,
                    SocketFlags.None,
                    callback,
                    context
                );
        }

        public void Send(TransitPart messagePart)
        {
            if (!IsConnected())
                throw new Exception("Destination socket is not connected.");
            AsyncCallback callback = HeaderSentCallback;
            if (messagePart == TransitPart.Header)
            {
                callback = HeaderSentCallback;
                TransitOperation request = new TransitOperation(
                    Context.Transfer,
                    TransitPart.Header,
                    DirectionType.Send
                );
                request.Resolve();
            }
            else if (Context.SendMessage)
            {
                callback = MessageSentCallback;
                context.SerialBlockId = 0;
                TransitOperation request = new TransitOperation(
                    context.Transfer,
                    TransitPart.Message,
                    DirectionType.Send
                );
                request.Resolve();
            }
            else
                return;

            context.Listener.BeginSend(
                context.SerialBlock,
                0,
                context.SerialBlock.Length,
                SocketFlags.None,
                callback,
                context
            );
        }

        private void Close()
        {
            try
            {
                if (!IsConnected())
                {
                    context.Dispose();
                    return;
                }

                if (socket != null && socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }

                context.Dispose();
            }
            catch (SocketException) { }
        }

        private void HeaderReceivedCallBack(IAsyncResult result)
        {
            ITransitContext context = (ITransitContext)result.AsyncState;
            int receive = context.Listener.EndReceive(result);

            if (receive > 0)
                context.IncomingHeader(receive);

            if (context.BlockSize > 0)
            {
                int buffersize =
                    (context.BlockSize < context.BufferSize)
                        ? (int)context.BlockSize
                        : context.BufferSize;
                context.Listener.BeginReceive(
                    context.HeaderBuffer,
                    0,
                    buffersize,
                    SocketFlags.None,
                    HeaderReceivedCallBack,
                    context
                );
            }
            else
            {
                TransitOperation request = new TransitOperation(
                    context.Transfer,
                    TransitPart.Header,
                    DirectionType.Receive
                );
                request.Resolve(context);

                if (!context.ReceiveMessage && !context.SendMessage)
                    context.Close = true;

                context.HeaderReceivedNotice.Set();
                HeaderReceived.Invoke(this);
            }
        }

        private void HeaderSentCallback(IAsyncResult result)
        {
            ITransitContext context = (ITransitContext)result.AsyncState;
            try
            {
                int sendcount = context.Listener.EndSend(result);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }

            context.HeaderSentNotice.Set();
            HeaderSent.Invoke(this);
        }

        private void MessageReceivedCallBack(IAsyncResult result)
        {
            ITransitContext context = (ITransitContext)result.AsyncState;
            MarkupType noiseKind = MarkupType.None;

            int receive = context.Listener.EndReceive(result);

            if (receive > 0)
                noiseKind = context.IncomingMessage(receive);

            if (context.BlockSize > 0)
            {
                int buffersize =
                    (context.BlockSize < context.BufferSize)
                        ? (int)context.BlockSize
                        : context.BufferSize;
                context.Listener.BeginReceive(
                    context.MessageBuffer,
                    0,
                    buffersize,
                    SocketFlags.None,
                    MessageReceivedCallBack,
                    context
                );
            }
            else
            {
                object readPosition = context.DeserialBlockId;

                if (
                    noiseKind == MarkupType.Block
                    || (
                        noiseKind == MarkupType.End
                        && (int)readPosition
                            < (context.Transfer.HeaderReceived.Context.ObjectsCount - 1)
                    )
                )
                    context.Listener.BeginReceive(
                        context.MessageBuffer,
                        0,
                        context.BufferSize,
                        SocketFlags.None,
                        MessageReceivedCallBack,
                        context
                    );

                TransitOperation request = new TransitOperation(
                    context.Transfer,
                    TransitPart.Message,
                    DirectionType.Receive
                );
                request.Resolve(context);

                if (
                    context.ObjectRemainds <= 0
                    && !context.BatchesReceivedNotice.SafeWaitHandle.IsClosed
                )
                    context.BatchesReceivedNotice.Set();

                if (
                    noiseKind == MarkupType.End
                    && (int)readPosition
                        >= (context.Transfer.HeaderReceived.Context.ObjectsCount - 1)
                )
                {
                    context.BatchesReceivedNotice.WaitOne();

                    if (context.SendMessage)
                        context.MessageSentNotice.WaitOne();

                    context.Close = true;

                    context.MessageReceivedNotice.Set();
                    MessageReceived.Invoke(this);
                }
            }
        }

        private void MessageSentCallback(IAsyncResult result)
        {
            ITransitContext context = (ITransitContext)result.AsyncState;
            try
            {
                int sendcount = context.Listener.EndSend(result);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }

            if (context.SerialBlockId >= 0)
            {
                TransitOperation request = new TransitOperation(
                    context.Transfer,
                    TransitPart.Message,
                    DirectionType.Send
                );
                request.Resolve();
                context.Listener.BeginSend(
                    context.SerialBlock,
                    0,
                    context.SerialBlock.Length,
                    SocketFlags.None,
                    MessageSentCallback,
                    context
                );
            }
            else
            {
                if (!context.ReceiveMessage)
                    context.Close = true;

                context.MessageSentNotice.Set();
                MessageSent.Invoke(this);
            }
        }

        private void OnConnectCallback(IAsyncResult result)
        {
            ITransitContext context = (ITransitContext)result.AsyncState;

            try
            {
                context.Listener.EndConnect(result);
                connectNotice.Set();
            }
            catch (SocketException ex) { }
        }
    }
}
