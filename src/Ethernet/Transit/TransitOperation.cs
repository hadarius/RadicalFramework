using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.Json;

namespace Radical.Ethernet.Transit;

using Formatters.Json;
using Server;
using Security.Identity;

public class TransitOperation
{
    private DirectionType direction;
    private ProtocolMethod method;
    private TransitPart part;
    private EthernetProtocol protocol;
    private ServiceSite site;
    private EthernetTransit transaction;
    private ITransitContext transferContext;
    private EthernetContext transportContext;

    public TransitOperation(
        EthernetTransit _transaction,
        TransitPart _part,
        DirectionType _direction
    )
    {
        transaction = _transaction;
        transferContext = transaction.Context;
        transportContext = transaction.MyHeader.Context;
        site = transportContext.IdentitySite;
        direction = _direction;
        part = _part;
        protocol = transferContext.Protocol;
        method = transferContext.Method;
    }

    public void Resolve(ITransitBuffer buffer = null)
    {
        switch (protocol)
        {
            case EthernetProtocol.DOTP:
                switch (site)
                {
                    case ServiceSite.Server:
                        switch (direction)
                        {
                            case DirectionType.Receive:
                                switch (part)
                                {
                                    case TransitPart.Header:
                                        ServerReceivedTcpTransitHeader(buffer);
                                        break;
                                    case TransitPart.Message:
                                        ServerReceivedTcpTransitMessage(buffer);
                                        break;
                                }
                                break;
                            case DirectionType.Send:
                                switch (part)
                                {
                                    case TransitPart.Header:
                                        ServerSendTcpTransitHeader();
                                        break;
                                    case TransitPart.Message:
                                        ServerSendTcpTransitMessage();
                                        break;
                                }
                                break;
                        }
                        break;
                    case ServiceSite.Client:
                        switch (direction)
                        {
                            case DirectionType.Receive:
                                switch (part)
                                {
                                    case TransitPart.Header:
                                        ClientReceivedTcpTransitHeader(buffer);
                                        break;
                                    case TransitPart.Message:
                                        ClientReceivedTcpTransitMessage(buffer);
                                        break;
                                }
                                break;
                            case DirectionType.Send:
                                switch (part)
                                {
                                    case TransitPart.Header:
                                        ClientSendTcpTransitHeader();
                                        break;
                                    case TransitPart.Message:
                                        ClientSendTcpTrnsitMessage();
                                        break;
                                }
                                break;
                        }
                        break;
                }
                break;

            case EthernetProtocol.NONE:
                switch (site)
                {
                    case ServiceSite.Server:
                        switch (direction)
                        {
                            case DirectionType.Receive:
                                switch (part)
                                {
                                    case TransitPart.Header:
                                        ServerReceivedTcpTransitHeader(buffer);
                                        break;
                                    case TransitPart.Message:
                                        ServerReceivedTcpTransitMessage(buffer);
                                        break;
                                }
                                break;
                            case DirectionType.Send:
                                switch (part)
                                {
                                    case TransitPart.Header:
                                        ServerSendTcpTransitHeader();
                                        break;
                                    case TransitPart.Message:
                                        ServerSendTcpTransitMessage();
                                        break;
                                }
                                break;
                        }
                        break;
                    case ServiceSite.Client:
                        switch (direction)
                        {
                            case DirectionType.Receive:
                                switch (part)
                                {
                                    case TransitPart.Header:
                                        ClientReceivedTcpTransitHeader(buffer);
                                        break;
                                    case TransitPart.Message:
                                        ClientReceivedTcpTransitMessage(buffer);
                                        break;
                                }
                                break;
                            case DirectionType.Send:
                                switch (part)
                                {
                                    case TransitPart.Header:
                                        ClientSendTcpTransitHeader();
                                        break;
                                    case TransitPart.Message:
                                        ClientSendTcpTrnsitMessage();
                                        break;
                                }
                                break;
                        }
                        break;
                }
                break;
            case EthernetProtocol.HTTP:
                switch (method)
                {
                    case ProtocolMethod.GET:
                        switch (site)
                        {
                            case ServiceSite.Server:
                                switch (direction)
                                {
                                    case DirectionType.Receive:
                                        switch (part)
                                        {
                                            case TransitPart.Header:
                                                ServerReceivedHttpGet(buffer);
                                                break;
                                        }
                                        break;
                                    case DirectionType.Send:
                                        switch (part)
                                        {
                                            case TransitPart.Header:
                                                ServerSendHttpGet();
                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case ProtocolMethod.POST:
                        switch (site)
                        {
                            case ServiceSite.Server:
                                switch (direction)
                                {
                                    case DirectionType.Receive:
                                        switch (part)
                                        {
                                            case TransitPart.Header:
                                                ServerReceivedHttpPost(buffer);
                                                break;
                                        }
                                        break;
                                    case DirectionType.Send:
                                        switch (part)
                                        {
                                            case TransitPart.Header:
                                                ServerSendHttpPost();
                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case ProtocolMethod.OPTIONS:
                        switch (site)
                        {
                            case ServiceSite.Server:
                                switch (direction)
                                {
                                    case DirectionType.Receive:
                                        switch (part)
                                        {
                                            case TransitPart.Header:
                                                ServerReceivedHttpOptions();
                                                break;
                                        }
                                        break;
                                    case DirectionType.Send:
                                        switch (part)
                                        {
                                            case TransitPart.Header:
                                                ServerSendHttpOptions();
                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                }
                break;
        }
    }

    private void ClientReceivedTcpTransitHeader(ITransitBuffer buffer)
    {
        TransitHeader headerObject = (TransitHeader)transaction.MyHeader.Deserialize(buffer);

        if (headerObject != null)
        {
            transaction.HeaderReceived = headerObject;

            transaction.MyHeader.Context.Identity.Key = null;
            transaction.MyHeader.Context.Identity.Name = null;
            transaction.MyHeader.Context.Identity.UserId = transaction
                .HeaderReceived
                .Context
                .Identity
                .UserId;
            transaction.MyHeader.Context.Identity.Token = transaction
                .HeaderReceived
                .Context
                .Identity
                .Token;
            transaction.MyHeader.Context.Identity.ClientId = transaction
                .HeaderReceived
                .Context
                .Identity
                .ClientId;

            object reciveContent = transaction.HeaderReceived.Data;

            Type[] ifaces = reciveContent.GetType().GetInterfaces();
            if (
                ifaces.Contains(typeof(ITransitFormatter))
                && ifaces.Contains(typeof(ITransitObject))
            )
            {
                if (transaction.MyHeader.Data == null)
                    transaction.MyHeader.Data = ((ITransitObject)reciveContent).Locate();

                object myContent = transaction.MyHeader.Data;

                ((ITransitObject)myContent).Merge(reciveContent);

                int objectCount = transaction.HeaderReceived.Context.ObjectsCount;
                if (objectCount == 0)
                    transferContext.ReceiveMessage = false;
                else
                    transaction.MessageReceived = new TransitMessage(
                        transaction,
                        DirectionType.Receive,
                        myContent
                    );
            }
            else if (reciveContent is Hashtable)
            {
                Hashtable hashTable = (Hashtable)reciveContent;
                if (hashTable.Contains("Register"))
                {
                    transferContext.Denied = !(bool)hashTable["Register"];
                    if (transferContext.Denied)
                    {
                        transferContext.Close = true;
                        transferContext.ReceiveMessage = false;
                        transferContext.SendMessage = false;
                    }
                }
            }
            else
                transferContext.SendMessage = false;
        }
    }

    private void ClientReceivedTcpTransitMessage(ITransitBuffer buffer)
    {
        object serialItemsObj = ((object[])transaction.MessageReceived.Data)[
            buffer.DeserialBlockId
        ];
        ITransitFormatter serialItems = (ITransitFormatter)serialItemsObj;

        object deserialItemsObj = serialItems.Deserialize(buffer);
        ITransitFormatter deserialItems = (ITransitFormatter)deserialItemsObj;
        if (
            deserialItems.DeserialCount <= deserialItems.ProgressCount
            || deserialItems.ProgressCount == 0
        )
        {
            transaction.Context.ObjectRemainds--;
            deserialItems.ProgressCount = 0;
        }
    }

    private void ClientSendTcpTransitHeader()
    {
        transaction.Manager.HeaderContent(
            transferContext.Transfer.MyHeader.Data,
            transferContext.Transfer.MyHeader.Data,
            DirectionType.Send
        );

        if (transaction.MyHeader.Context.ObjectsCount == 0)
            transferContext.SendMessage = false;

        transferContext.Transfer.MyHeader.Serialize(transferContext, 0, 0);
    }

    private void ClientSendTcpTrnsitMessage()
    {
        object serialitems = ((object[])transaction.MyMessage.Data)[
            transferContext.ObjectPosition
        ];

        int serialBlockId = ((ITransitFormatter)serialitems).Serialize(
            transferContext,
            transferContext.SerialBlockId,
            5000
        );
        if (serialBlockId < 0)
        {
            if (
                transferContext.ObjectPosition < (transaction.MyHeader.Context.ObjectsCount - 1)
            )
            {
                transferContext.ObjectPosition++;
                transferContext.SerialBlockId = 0;
                return;
            }
        }
        transferContext.SerialBlockId = serialBlockId;
    }

    private void ServerReceivedHttpGet(ITransitBuffer buffer)
    {
        transferContext.SendMessage = false;
        transferContext.ReceiveMessage = false;
        transaction.HeaderReceived = transaction.MyHeader;
        transferContext.HandleGetRequest();
    }

    private void ServerReceivedTcpTransitHeader(ITransitBuffer buffer)
    {
        bool isError = false;
        string errorMessage = "";
        try
        {
            TransitHeader headerObject = (TransitHeader)transaction.MyHeader.Deserialize(buffer);
            if (headerObject != null)
            {
                transaction.HeaderReceived = headerObject;

                if (
                    EthernetServer.Security.Register(
                        transaction.HeaderReceived.Context.Identity,
                        true
                    )
                )
                {
                    transaction.MyHeader.Context.Identity.UserId = transaction
                        .HeaderReceived
                        .Context
                        .Identity
                        .UserId;
                    transaction.MyHeader.Context.Identity.Token = transaction
                        .HeaderReceived
                        .Context
                        .Identity
                        .Token;
                    transaction.MyHeader.Context.Identity.ClientId = transaction
                        .HeaderReceived
                        .Context
                        .Identity
                        .ClientId;

                    if (transaction.HeaderReceived.Context.ContentType != null)
                    {
                        object _content = transaction.HeaderReceived.Data;

                        Type[] ifaces = _content.GetType().GetInterfaces();
                        if (
                            ifaces.Contains(typeof(ITransitFormatter))
                            && ifaces.Contains(typeof(ITransitObject))
                        )
                        {
                            int objectCount = transaction.HeaderReceived.Context.ObjectsCount;
                            transferContext.Synchronic = transaction
                                .HeaderReceived
                                .Context
                                .Synchronic;

                            object myheader = ((ITransitObject)_content).Locate();

                            if (myheader != null)
                            {
                                if (objectCount == 0)
                                {
                                    transferContext.ReceiveMessage = false;

                                    transaction.MyHeader.Data = myheader;
                                }
                                else
                                {
                                    transaction.MyHeader.Data = (
                                        (ITransitObject)myheader
                                    ).Merge(_content);
                                    transaction.MessageReceived = new TransitMessage(
                                        transaction,
                                        DirectionType.Receive,
                                        transaction.MyHeader.Data
                                    );
                                }
                            }
                            else
                            {
                                isError = true;
                                errorMessage += "Prime not exist - incorrect object target ";
                            }
                        }
                        else
                        {
                            isError = true;
                            errorMessage += "Incorrect DPOT object - deserialization error ";
                        }
                    }
                    else
                    {
                        transaction.MyHeader.Data = new Hashtable() { { "Register", true } };
                        transaction.MyHeader.Context.Echo +=
                            "Registration success - ContentType: null ";
                    }
                }
                else
                {
                    isError = true;
                    transaction.MyHeader.Data = new Hashtable() { { "Register", false } };
                    transaction.MyHeader.Context.Echo += "Registration failed - access denied ";
                }
            }
            else
            {
                isError = true;
                errorMessage += "Incorrect DPOT object - deserialization error ";
            }
        }
        catch (Exception ex)
        {
            isError = true;
            errorMessage += ex.ToString();
        }

        if (isError)
        {
            transaction.Context.Close = true;
            transaction.Context.ReceiveMessage = false;
            transaction.Context.SendMessage = false;

            if (errorMessage != "")
            {
                transaction.MyHeader.Data += errorMessage;
                transaction.MyHeader.Context.Echo += errorMessage;
            }
            transaction.MyHeader.Context.Errors++;
        }
    }

    private void ServerReceivedTcpTransitMessage(ITransitBuffer buffer)
    {
        object serialItemsObj = ((object[])transaction.MessageReceived.Data)[
            buffer.DeserialBlockId
        ];
        object deserialItemsObj = ((ITransitFormatter)serialItemsObj).Deserialize(buffer);
        ITransitFormatter deserialItems = (ITransitFormatter)deserialItemsObj;
        if (
            deserialItems.DeserialCount <= deserialItems.ProgressCount
            || deserialItems.ProgressCount == 0
        )
        {
            transaction.Context.ObjectRemainds--;
            deserialItems.ProgressCount = 0;
        }
    }

    private void ServerReceivedHttpOptions()
    {
        transaction.HeaderReceived = transaction.MyHeader;
        transferContext.SendMessage = false;
        transferContext.ReceiveMessage = false;
        transferContext.HandleOptionsRequest("application/json");
    }

    private void ServerReceivedHttpPost(ITransitBuffer buffer)
    {
        if (ServerReceivedHttpPostTransit(buffer))
            transaction.HeaderReceived = transaction.MyHeader;
        transferContext.SendMessage = false;
        transferContext.ReceiveMessage = false;
    }

    private bool ServerReceivedHttpPostTransit(ITransitBuffer buffer)
    {
        bool isError = false;
        string errorMessage = "";
        try
        {
            byte[] _array = buffer.DeserialBlock;
            StringBuilder sb = new StringBuilder();
            sb.Append(_array.ToChars(CharEncoding.UTF8));

            string dpttransx = sb.ToString();
            int msgid = dpttransx.IndexOf(",\"DealMessage\":");
            string dptheadx = dpttransx.Substring(0, msgid) + "}";
            string dptmsgx =
                "{" + dpttransx.Substring(msgid, dpttransx.Length - msgid).Trim(',');

            string[] msgcntsx = dptmsgx.Split(
                new string[] { "\"Data\":" },
                StringSplitOptions.RemoveEmptyEntries
            );
            string[] cntarrays =
                msgcntsx.Length > 0
                    ? msgcntsx[1].Split(new string[] { "\"Items\":" }, StringSplitOptions.None)
                    : null;
            int objectCount = 0;
            if (cntarrays != null)
                for (int i = 1; i < cntarrays.Length; i += 1)
                {
                    string[] itemarray = cntarrays[i].Split('[');
                    for (int x = 1; x < itemarray.Length; x += 1)
                    {
                        if (itemarray[x].IndexOf(']') > 0)
                            objectCount++;
                    }
                }

            string msgcntx = msgcntsx[1].Trim(' ').Substring(0, 6);

            object dptheadb = dptheadx;
            object dptmsgb = dptmsgx;

            isError = ServerReceivedHttpPostTransitHeader(buffer);
            if (objectCount > 0 && !isError)
                isError = ServerReceivedHttpPostTransitMessage(dptmsgb);
        }
        catch (Exception ex)
        {
            isError = true;
            errorMessage = ex.ToString();
        }

        if (isError)
        {
            transaction.Context.SendMessage = false;
            if (errorMessage != "")
            {
                transaction.MyHeader.Data += errorMessage;
                transaction.MyHeader.Context.Echo += errorMessage;
            }
            transaction.MyHeader.Context.Errors++;
        }
        return isError;
    }

    private bool ServerReceivedHttpPostTransitHeader(ITransitBuffer buffer)
    {
        bool isError = false;
        string errorMessage = "";
        try
        {
            TransitHeader headerObject = (TransitHeader)
                transaction.MyHeader.Deserialize(buffer);
            headerObject.Context.Identity.Ip =
                transaction.MyHeader.Context.RemoteEndPoint.Address.ToString();
            if (EthernetServer.Security.Register(headerObject.Context.Identity, true))
            {
                transaction.HeaderReceived = (headerObject != null) ? headerObject : null;
                transaction.MyHeader.Context.Complexity = headerObject.Context.Complexity;
                transaction.MyHeader.Context.Identity = headerObject.Context.Identity;

                if (headerObject.Context.ContentType != null)
                {
                    object instance = new object();
                    JsonParser.PrepareInstance(out instance, headerObject.Context.ContentType);
                    object content = headerObject.Data;
                    object result = ((ITransitFormatter)instance).Deserialize(
                        buffer
                    );
                    transaction.HeaderReceived.Data = result;
                    object _content = transaction.HeaderReceived.Data;

                    Type[] ifaces = _content.GetType().GetInterfaces();
                    if (
                        ifaces.Contains(typeof(ITransitFormatter))
                        && ifaces.Contains(typeof(ITransitObject))
                    )
                    {
                        int objectCount = buffer.DeserialBlockId;

                        object myheader = ((ITransitObject)_content).Locate();

                        if (myheader != null)
                        {
                            if (objectCount == 0)
                            {
                                transferContext.ReceiveMessage = false;

                                transaction.MyHeader.Data = myheader;
                            }
                            else
                            {
                                transaction.MyHeader.Data = ((ITransitObject)myheader).Merge(
                                    _content
                                );
                                transaction.MessageReceived = new TransitMessage(
                                    transaction,
                                    DirectionType.Receive,
                                    transaction.MyHeader.Data
                                );
                            }
                        }
                        else
                        {
                            isError = true;
                            errorMessage += "Prime not exist - incorrect object target ";
                        }
                    }
                    else
                    {
                        isError = true;
                        errorMessage += "Incorrect DPOT object - deserialization error ";
                    }
                }
                else
                {
                    transaction.MyHeader.Data = new Hashtable() { { "Register", true } };
                    transaction.MyHeader.Context.Echo +=
                        "Registration success - ContentType: null ";
                }
            }
            else
            {
                isError = true;
                transaction.MyHeader.Data = new Hashtable() { { "Register", false } };
                transaction.MyHeader.Context.Echo += "Registration failed - access denied ";
            }
        }
        catch (Exception ex)
        {
            isError = true;
            errorMessage += ex.ToString();
        }

        if (isError)
        {
            transaction.Context.SendMessage = false;
            if (errorMessage != "")
            {
                transaction.MyHeader.Data += errorMessage;
                transaction.MyHeader.Context.Echo += errorMessage;
            }
            transaction.MyHeader.Context.Errors++;
        }
        return isError;
    }

    private bool ServerReceivedHttpPostTransitMessage(object buffer)
    {
        bool isError = false;
        string errorMessage = "";
        try { }
        catch (Exception ex)
        {
            isError = true;
            errorMessage += ex.ToString();
        }

        if (isError)
        {
            transaction.Context.SendMessage = false;
            if (errorMessage != "")
            {
                transaction.MyHeader.Data = "Prime not exist - incorrect object path";
                transaction.MyHeader.Context.Echo =
                    "Error - Prime not exist - incorrect object path";
            }
            transaction.MyHeader.Context.Errors++;
        }
        return isError;
    }

    private void ServerSendHttpGet()
    {
        transferContext.SendMessage = false;
        transferContext.ReceiveMessage = false;
    }

    private void ServerSendTcpTransitHeader()
    {
        transaction.Manager.HeaderContent(
            transferContext.Transfer.MyHeader.Data,
            transferContext.Transfer.MyHeader.Data,
            DirectionType.Send
        );

        if (transaction.MyHeader.Context.ObjectsCount == 0)
            transferContext.SendMessage = false;

        transferContext.Transfer.MyHeader.Serialize(transferContext, 0, 0);
    }

    private void ServerSendTcpTransitMessage()
    {
        int serialBlockId = ((ITransitFormatter[])transaction.MyMessage.Data)[
            transferContext.ObjectPosition
        ].Serialize(transferContext, transferContext.SerialBlockId, 5000);

        if (serialBlockId < 0)
        {
            if (
                transferContext.ObjectPosition < (transaction.MyHeader.Context.ObjectsCount - 1)
            )
            {
                transferContext.ObjectPosition++;
                transferContext.SerialBlockId = 0;
                return;
            }
        }
        transferContext.SerialBlockId = serialBlockId;
    }

    private void ServerSendHttpOptions()
    {
        transferContext.SendMessage = false;
        transferContext.ReceiveMessage = false;
    }

    private void ServerSendHttpPost()
    {
        transferContext.SendMessage = false;
        transferContext.ReceiveMessage = false;
        transferContext.RequestBuilder.Clear();
        if (!transferContext.Denied)
        {
            ServerSendHttpPostTransit();
            transferContext.HandlePostRequest("application/json");
        }
        else
            transferContext.HandleDeniedRequest();
    }

    private void ServerSendHttpPostTransit()
    {
        ServerSendHttpPostTransitHeader();
        SrvSendPostDealMessage();
    }

    private void ServerSendHttpPostTransitHeader()
    {
        transaction.Manager.HeaderContent(
            transferContext.Transfer.MyHeader.Data,
            transferContext.Transfer.MyHeader.Data,
            DirectionType.Send
        );
        transferContext.RequestBuilder.Append(JsonSerializer.Serialize(transaction.MyHeader, typeof(TransitHeader)));
    }

    private void SrvSendPostDealMessage()
    {
        StringBuilder msgcnt = new StringBuilder();

        Type[] ifaces = transaction.MyMessage.Data.GetType().GetInterfaces();

        msgcnt.Append("null");

        transaction.MyMessage.Data = new object();
        transferContext.RequestBuilder.Append(JsonSerializer.Serialize(transaction.MyMessage, typeof(TransitMessage)));
        string msg = msgcnt.ToString().Replace("}\r\n{", ",").Trim(new char[] { '\n', '\r' });
        transferContext.RequestBuilder.Replace(
            "\"Data\":{}",
            "\"Data\":" + msgcnt.ToString()
        );
        transferContext.RequestBuilder.Replace("}\r\n{", ",");
        transferContext.SendMessage = false;
        transferContext.ReceiveMessage = false;
    }
}
