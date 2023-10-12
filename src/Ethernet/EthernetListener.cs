namespace Radical.Ethernet;

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Series;
using System.Threading;
using Uniques;
using Invoking;
using Security.Identity;
using Transit;

public sealed class EthernetListener : IEthernetListener
{
    private readonly Registry<ITransitContext> clients = new Registry<ITransitContext>();
    private readonly ManualResetEvent connectingNotice = new ManualResetEvent(false);
    private readonly Registry<byte[]> resources = new Registry<byte[]>();
    private static IMemberSecurity security;
    private MemberIdentity identity;
    private bool shutdown = false;
    private int timeout = 50;

    private EthernetListener() { }

    public static IEthernetListener Instance { get; } = new EthernetListener();

    public static IMemberSecurity Security
    {
        get { return security; }
        set { security = value; }
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
                    Port = 28465,
                    Limit = 200,
                    Scale = 1,
                    Site = ServiceSite.Server
                };
        }
        set
        {
            if (value != null)
            {
                value.Site = ServiceSite.Server;
                identity = value;
            }
        }
    }

    public IInvoker MessageReceived { get; set; }

    public IInvoker MessageSent { get; set; }

    public IInvoker WriteEcho { get; set; }

    public void ClearClients()
    {
        foreach (ITransitContext closeContext in clients.AsValues())
        {
            ITransitContext context = closeContext;

            if (context == null)
            {
                throw new Exception("Client does not exist.");
            }

            try
            {
                context.Listener.Shutdown(SocketShutdown.Both);
                context.Listener.Close();
            }
            catch (SocketException sx)
            {
                Echo(sx.Message);
            }
            finally
            {
                context.Dispose();
                Echo(string.Format("Client disconnected with Id {0}", context.Id));
            }
        }
        clients.Clear();
    }

    public void ClearResources()
    {
        resources.Clear();
    }

    public void CloseClient(ISeriesItem<ITransitContext> item)
    {
        ITransitContext context = item.Value;

        if (context == null)
        {
            Echo(string.Format("Client {0} does not exist.", context.Id));
        }
        else
        {
            try
            {
                if (context.Listener != null && context.Listener.Connected)
                {
                    context.Listener.Shutdown(SocketShutdown.Both);
                    context.Listener.Close();
                }
            }
            catch (SocketException sx)
            {
                Echo(sx.Message);
            }
            finally
            {
                ITransitContext contextRemoved = clients.Remove(context.Id);
                contextRemoved.Dispose();
                Echo(string.Format("Client disconnected with Id {0}", context.Id));
            }
        }
    }

    public void CloseClient(int id)
    {
        CloseClient(GetClient(id));
    }

    public void CloseListener()
    {
        foreach (ITransitContext closeContext in clients.AsValues())
        {
            ITransitContext context = closeContext;

            if (context == null)
            {
                Echo(string.Format("Client  does not exist."));
            }
            else
            {
                try
                {
                    if (context.Listener != null && context.Listener.Connected)
                    {
                        context.Listener.Shutdown(SocketShutdown.Both);
                        context.Listener.Close();
                    }
                }
                catch (SocketException sx)
                {
                    Echo(sx.Message);
                }
                finally
                {
                    context.Dispose();
                    Echo(string.Format("Client disconnected with Id {0}", context.Id));
                }
            }
        }
        clients.Clear();
        shutdown = true;
        connectingNotice.Set();
        GC.Collect();
    }

    public void EthHeaderReceived(ITransitContext context)
    {
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
                HeaderReceivedCallback,
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

            context.HeaderReceivedNotice.Set();

            try
            {
                HeaderReceived.Invoke(context);
            }
            catch (Exception ex)
            {
                Echo(ex.Message);
                CloseClient(context.Id);
            }
        }
    }

    public void Dispose()
    {
        foreach (var item in clients.AsItems())
        {
            CloseClient(item);
        }

        connectingNotice.Dispose();
    }

    public void Echo(string message)
    {
        if (WriteEcho != null)
            WriteEcho.Invoke(message);
    }

    public void HeaderReceivedCallback(IAsyncResult result)
    {
        ITransitContext context = (ITransitContext)result.AsyncState;
        int receive = context.Listener.EndReceive(result);

        if (receive > 0)
            context.IncomingHeader(receive);

        if (context.Protocol == EthernetProtocol.DOTP)
            EthHeaderReceived(context);
        else if (context.Protocol == EthernetProtocol.HTTP)
            HttpHeaderReceived(context);
    }

    public void HeaderSentCallback(IAsyncResult result)
    {
        ITransitContext context = (ITransitContext)result.AsyncState;
        try
        {
            int sendcount = context.Listener.EndSend(result);
        }
        catch (SocketException) { }
        catch (ObjectDisposedException) { }

        if (!context.ReceiveMessage && !context.SendMessage)
        {
            context.Close = true;
        }

        context.HeaderSentNotice.Set();

        try
        {
            HeaderSent.Invoke(context);
        }
        catch (Exception ex)
        {
            Echo(ex.Message);
            CloseClient(context.Id);
        }
    }

    public void HttpHeaderReceived(ITransitContext context)
    {
        if (context.BlockSize > 0)
        {
            context.Listener.BeginReceive(
                context.HeaderBuffer,
                0,
                context.BufferSize,
                SocketFlags.None,
                HeaderReceivedCallback,
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

            context.HeaderReceivedNotice.Set();

            try
            {
                HeaderReceived.Invoke(context);
            }
            catch (Exception ex)
            {
                Echo(ex.Message);
                CloseClient(context.Id);
            }
        }
    }

    public bool IsConnected(int id)
    {
        ITransitContext context = GetClient(id).Value;
        if (context != null && context.Listener != null && context.Listener.Connected)
            return !(
                context.Listener.Poll(timeout * 100, SelectMode.SelectRead)
                && context.Listener.Available == 0
            );
        else
            return false;
    }

    public void MessageReceivedCallback(IAsyncResult result)
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
                MessageReceivedCallback,
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
                    MessageReceivedCallback,
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
                context.MessageReceivedNotice.Set();

                try
                {
                    MessageReceived.Invoke(context);
                }
                catch (Exception ex)
                {
                    Echo(ex.Message);
                    CloseClient(context.Id);
                }
            }
        }
    }

    public void MessageSentCallback(IAsyncResult result)
    {
        ITransitContext context = (ITransitContext)result.AsyncState;
        try
        {
            int sendcount = context.Listener.EndSend(result);
        }
        catch (SocketException) { }
        catch (ObjectDisposedException) { }

        if (
            context.SerialBlockId >= 0
            || context.ObjectPosition < (context.Transfer.MyHeader.Context.ObjectsCount - 1)
        )
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
            if (context.ReceiveMessage)
                context.MessageReceivedNotice.WaitOne();

            context.Close = true;

            context.MessageSentNotice.Set();

            try
            {
                MessageSent.Invoke(context);
            }
            catch (Exception ex)
            {
                Echo(ex.Message);
                CloseClient(context.Id);
            }
        }
    }

    public void OnConnectCallback(IAsyncResult result)
    {
        try
        {
            if (!shutdown)
            {
                ITransitContext context;
                int id = -1;
                id = (int)Unique.NewKey.UniqueKey32();
                context = new TransitContext(
                    ((Socket)result.AsyncState).EndAccept(result),
                    id
                );
                context.Transfer = new EthernetTransit(identity, null, context);
                context.Resources = resources;
                while (true)
                {
                    if (!clients.Add(id, context))
                    {
                        id = (int)Unique.NewKey.UniqueKey32();
                        context.Id = id;
                    }
                    else
                        break;
                }
                Echo("Client connected. Get Id " + id);
                context.Listener.BeginReceive(
                    context.HeaderBuffer,
                    0,
                    context.BufferSize,
                    SocketFlags.None,
                    HeaderReceivedCallback,
                    clients[id]
                );
            }
            connectingNotice.Set();
        }
        catch (SocketException sx)
        {
            Echo(sx.Message);
        }
    }

    public void Receive(TransitPart messagePart, int id)
    {
        ITransitContext context = GetClient(id).Value;

        AsyncCallback callback = HeaderReceivedCallback;

        if (messagePart != TransitPart.Header && context.ReceiveMessage)
        {
            callback = MessageReceivedCallback;
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

    public void Send(TransitPart messagePart, int id)
    {
        ITransitContext context = GetClient(id).Value;
        if (!IsConnected(context.Id))
            throw new Exception("Destination socket is not connected.");

        AsyncCallback callback = HeaderSentCallback;

        if (messagePart == TransitPart.Header)
        {
            callback = HeaderSentCallback;
            TransitOperation request = new TransitOperation(
                context.Transfer,
                TransitPart.Header,
                DirectionType.Send
            );
            request.Resolve();
        }
        else if (context.SendMessage)
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

    public void StartListening()
    {
        ushort port = Convert.ToUInt16(Identity.Port),
            limit = Convert.ToUInt16(Identity.Limit);
        IPAddress address = IPAddress.Parse(Identity.Ip);
        IPEndPoint endpoint = new IPEndPoint(address, port);
        shutdown = false;
        try
        {
            using (
                Socket socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                )
            )
            {
                socket.Bind(endpoint);
                socket.Listen(limit);
                while (!shutdown)
                {
                    connectingNotice.Reset();
                    socket.BeginAccept(OnConnectCallback, socket);
                    connectingNotice.WaitOne();
                }
            }
        }
        catch (SocketException sx)
        {
            Echo(sx.Message);
        }
    }

    private ISeriesItem<ITransitContext> GetClient(int id)
    {
        return clients.GetItem(id);
    }
}
