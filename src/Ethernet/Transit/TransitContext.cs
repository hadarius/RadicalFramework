using System.Collections;
using System.Collections.Generic;
using Radical.Extracting;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Radical.Series;
using System.Text;
using System.Threading;

namespace Radical.Ethernet.Transit
{
    using Security.Identity;
    using Ethernet.Server;

    public sealed class TransitContext : ITransitContext, IDisposable
    {
        private const int Buffer_Size = 4096;

        [NonSerialized]
        private readonly EthernetStream instream;

        [NonSerialized]
        private readonly EthernetStream outstream;

        [NonSerialized]
        public byte[] binReceive = new byte[0];

        [NonSerialized]
        public IntPtr binReceiveAddress;

        [NonSerialized]
        public IntPtr binReceiveHandler;

        [NonSerialized]
        public byte[] binSend = new byte[0];

        [NonSerialized]
        public IntPtr binSendAddress;

        [NonSerialized]
        public IntPtr binSendHandler;

        [NonSerialized]
        public IntPtr headerBufferAddress;

        [NonSerialized]
        public IntPtr headerBufferHandler;

        [NonSerialized]
        public Hashtable httpHeaders = new Hashtable();

        [NonSerialized]
        public Hashtable httpOptions = new Hashtable();

        [NonSerialized]
        public IntPtr messageBufferAddress;

        [NonSerialized]
        public IntPtr messageBufferHandler;

        [NonSerialized]
        public MemoryStream msReceive;

        [NonSerialized]
        public MemoryStream msSend;

        [NonSerialized]
        public StringBuilder requestBuilder = new StringBuilder();

        [NonSerialized]
        public StringBuilder responseBuilder = new StringBuilder();

        [NonSerialized]
        private Socket _listener;
        private int Block_Offset = 16;

        [NonSerialized]
        private bool disposed = false;

        [NonSerialized]
        private byte[] headerbuffer = new byte[Buffer_Size];

        [NonSerialized]
        private String http_method;

        [NonSerialized]
        private String http_protocol_version;

        [NonSerialized]
        private String http_url;

        [NonSerialized]
        private Dictionary<string, string> httpExtensions = new Dictionary<string, string>()
        {
            { ".html", "text/html" },
            { ".css", "text/css" },
            { ".less", "text/css" },
            { ".png", "image/png" },
            { ".ico", "image/ico" },
            { ".jpg", "image/jpg" },
            { ".bmp", "image/bmp" },
            { ".gif", "image/gif" },
            { ".js", "text/javascript" },
            { ".qjs", "text/javascript" },
            { ".bjs", "text/babel" },
            { ".json", "application/json" },
            { ".woff", "font/woff" },
            { ".woff2", "font/woff2" },
            { ".ttf", "font/ttf" },
            { ".svg", "image/svg" }
        };
        private int id;

        [NonSerialized]
        private byte[] messagebuffer = new byte[Buffer_Size];

        [NonSerialized]
        private ISeries<byte[]> resources;

        [NonSerialized]
        private StringBuilder sb = new StringBuilder();

        [NonSerialized]
        private EthernetTransit tr;

        public TransitContext(Socket listener, int _id = -1, bool withStream = false)
        {
            this._listener = listener;
            if (withStream)
            {
                this.instream = new EthernetStream(listener);
                this.outstream = new EthernetStream(listener);
            }

            GCHandle gc = GCHandle.Alloc(messagebuffer, GCHandleType.Pinned);
            messageBufferHandler = GCHandle.ToIntPtr(gc);
            messageBufferAddress = gc.AddrOfPinnedObject();
            gc = GCHandle.Alloc(headerbuffer, GCHandleType.Pinned);
            headerBufferHandler = GCHandle.ToIntPtr(gc);
            headerBufferAddress = gc.AddrOfPinnedObject();

            this.id = _id;
            this.Close = false;
            this.Denied = false;
            this.ObjectPosition = 0;
            this.ObjectRemainds = 0;
            this.DeserialBlockId = 0;
            this.BlockSize = 0;
            this.SendMessage = true;
            this.ReceiveMessage = true;
            this.disposed = true;

            HeaderSentNotice.Reset();
            HeaderReceivedNotice.Reset();
            MessageSentNotice.Reset();
            MessageReceivedNotice.Reset();
            BatchesReceivedNotice.Reset();
        }

        public ManualResetEvent BatchesReceivedNotice { get; set; } = new ManualResetEvent(false);

        public int BlockOffset
        {
            get { return Block_Offset; }
            set { Block_Offset = value; }
        }

        public long BlockSize { get; set; }

        public int BufferSize
        {
            get { return Buffer_Size; }
        }

        public bool Close { get; set; }

        public bool Denied { get; set; }

        public byte[] DeserialBlock
        {
            get
            {
                byte[] result = null;
                lock (binReceive)
                {
                    disposed = false;
                    BlockSize = 0;
                    result = binReceive;
                    binReceive = new byte[0];
                }
                return result;
            }
        }

        public int DeserialBlockId { get; set; }

        public IntPtr DeserialBlockPtr => binReceiveAddress;

        public string Echo
        {
            get { return this.sb.ToString(); }
        }

        public byte[] HeaderBuffer
        {
            get { return this.headerbuffer; }
        }

        public ManualResetEvent HeaderReceivedNotice { get; set; } = new ManualResetEvent(false);

        public ManualResetEvent HeaderSentNotice { get; set; } = new ManualResetEvent(false);

        public Hashtable HttpHeaders
        {
            get { return httpHeaders; }
            set { httpHeaders = value; }
        }

        public Hashtable HttpOptions
        {
            get { return httpOptions; }
            set { httpOptions = value; }
        }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public Socket Listener
        {
            get { return this._listener; }
            set { this._listener = value; }
        }

        public byte[] MessageBuffer
        {
            get { return this.messagebuffer; }
        }

        public ManualResetEvent MessageReceivedNotice { get; set; } = new ManualResetEvent(false);

        public ManualResetEvent MessageSentNotice { get; set; } = new ManualResetEvent(false);

        public ProtocolMethod Method { get; set; } = ProtocolMethod.NONE;

        public int ObjectPosition { get; set; }

        public int ObjectRemainds { get; set; }

        public EthernetProtocol Protocol { get; set; } = EthernetProtocol.NONE;

        public bool ReceiveMessage { get; set; }

        public StringBuilder RequestBuilder
        {
            get { return requestBuilder; }
            set { requestBuilder = value; }
        }

        public ISeries<byte[]> Resources
        {
            get { return resources; }
            set { resources = value; }
        }

        public StringBuilder ResponseBuilder
        {
            get { return responseBuilder; }
            set { responseBuilder = value; }
        }

        public bool SendMessage { get; set; }

        public byte[] SerialBlock
        {
            get { return binSend; }
            set
            {
                if (value != null)
                {
                    lock (binSend)
                    {
                        disposed = false;
                        binSend = value;
                        if (Protocol != EthernetProtocol.HTTP)
                        {
                            long size = binSend.Length - BlockOffset;
                            new byte[] { (byte)'D', (byte)'E', (byte)'A', (byte)'L' }.CopyTo(
                                binSend,
                                0
                            );
                            BitConverter.GetBytes(size).CopyTo(binSend, 4);
                            BitConverter.GetBytes(ObjectPosition).CopyTo(binSend, 12);
                        }
                        value = null;
                    }
                }
            }
        }

        public int SerialBlockId { get; set; }

        public IntPtr SerialBlockPtr => binSendAddress;

        public ServiceSite Site
        {
            get { return Transfer.MyHeader.Context.IdentitySite; }
        }

        public bool Synchronic { get; set; }

        public EthernetTransit Transfer
        {
            get { return this.tr; }
            set
            {
                if (value.Context == null)
                {
                    value.Context = this;
                    if (value.Identity != null)
                        value.MyHeader.BindContext(value.Context);
                }
                if (value.MyMessage.Data != null)
                {
                    if (value.MyMessage.Data.GetType() == typeof(object[][]))
                        value.MyHeader.Context.ObjectsCount = (
                            (object[][])value.MyMessage.Data
                        ).Length;
                }
                this.tr = value;
            }
        }

        internal EthernetMethod SendEcho { get; set; }

        public void Append(string text)
        {
            this.sb.Append(text);
        }

        public void DirSearch(string dir, List<string> jspfiles)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                    jspfiles.Add(f);
                foreach (string d in Directory.GetDirectories(dir))
                    DirSearch(d, jspfiles);
            }
            catch (Exception ex) { }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                sb.Clear();
                msSend.Dispose();
                msReceive.Dispose();
                GCHandle gc;
                lock (binReceive)
                {
                    if (!binReceiveHandler.Equals(IntPtr.Zero))
                    {
                        gc = GCHandle.FromIntPtr(binReceiveHandler);
                        gc.Free();
                    }
                    binReceive = null;
                }
                lock (binSend)
                {
                    if (!binSendHandler.Equals(IntPtr.Zero))
                    {
                        gc = GCHandle.FromIntPtr(binSendHandler);
                        gc.Free();
                    }
                    binSend = null;
                }

                gc = GCHandle.FromIntPtr(messageBufferHandler);
                gc.Free();
                messagebuffer = null;
                gc = GCHandle.FromIntPtr(headerBufferHandler);
                gc.Free();
                headerbuffer = null;

                HeaderSentNotice.Dispose();
                HeaderReceivedNotice.Dispose();
                MessageSentNotice.Dispose();
                MessageReceivedNotice.Dispose();
                BatchesReceivedNotice.Dispose();

                disposed = true;
            }
        }

        public string GetHttpExtensionType()
        {
            string extension = http_url.Substring(http_url.LastIndexOf('.'));
            string result = null;
            httpExtensions.TryGetValue(extension, out result);
            return result = (result == null) ? "text/html" : result;
        }

        public bool GetJavaScriptProject(MemoryStream ms)
        {
            string extension = http_url.Substring(http_url.LastIndexOf('.'));
            if (extension.Equals(".qjs") || extension.Equals(".bjs"))
            {
                string jspdir = "../../Web" + http_url.Substring(0, http_url.LastIndexOf('.'));
                List<string> jspfiles = new List<string>();
                DirSearch(jspdir, jspfiles);
                foreach (string jspfile in jspfiles)
                {
                    Stream fs = File.Open(jspfile, FileMode.Open);
                    fs.CopyTo(ms);

                    fs.Close();
                }
                return true;
            }
            return false;
        }

        public void HandleDeniedRequest()
        {
            writeDenied();
            writeClose();
            binSend = ResponseBuilder.ToString().ToBytes(CharEncoding.UTF8);
        }

        public void HandleGetRequest(string content_type = "text/html")
        {
            if (http_url.Equals("/") || http_url.Equals(""))
                http_url = "/Index.html";

            if (!Resources.TryGet(http_url, out binSend))
            {
                MemoryStream ms = new MemoryStream();
                if (GetJavaScriptProject(ms))
                {
                    content_type = GetHttpExtensionType();
                    writeSuccess(content_type);
                }
                else
                {
                    if (File.Exists("../../Web" + http_url))
                    {
                        content_type = GetHttpExtensionType();
                        Stream fs = File.Open("../../Web" + http_url, FileMode.Open);
                        fs.CopyTo(ms);
                        fs.Close();
                        writeSuccess(content_type);
                    }
                    else
                        writeFailure();
                }

                if (HttpHeaders.ContainsKey("Connection"))
                    writeClose(HttpHeaders["Connection"].ToString());
                else
                    writeClose();

                if (content_type.Contains("text") || content_type.Contains("json"))
                {
                    ResponseBuilder.Append((ms.ToArray().ToChars(CharEncoding.UTF8)));
                    binSend = ResponseBuilder.ToString().ToBytes(CharEncoding.UTF8);
                    Resources.Add(http_url, binSend);
                }
                else
                {
                    binSend = ResponseBuilder
                        .ToString()
                        .ToBytes(CharEncoding.UTF8)
                        .Concat(ms.ToArray())
                        .ToArray();
                    Resources.Add(http_url, binSend);
                }
                ms.Dispose();
            }
        }

        public void HandleOptionsRequest(string content_type = "text/html")
        {
            writeSuccess(content_type);
            writeOptions();
            writeClose();
            binSend = ResponseBuilder.ToString().ToBytes(CharEncoding.UTF8);
        }

        public void HandlePostRequest(string content_type = "text/html")
        {
            writeSuccess(content_type);
            writeOptions();
            writeClose();
            string requestBuilder = RequestBuilder.ToString();
            ResponseBuilder.AppendLine(requestBuilder);
            string responseString = ResponseBuilder.ToString();
            binSend = responseString.ToBytes(CharEncoding.UTF8);
        }

        public MarkupType HttpHeader(int received)
        {
            MarkupType noiseKind = MarkupType.None;

            lock (binReceive)
            {
                if (BlockSize == 0)
                {
                    BlockSize = 1;
                    msReceive = new MemoryStream();
                }

                msReceive.Write(HeaderBuffer, 0, received);

                if (received < BufferSize)
                {
                    BlockSize = 0;
                    msReceive.Position = 0;
                    ParseRequest(msReceive);
                    ReadHeaders(msReceive);
                    if (VerifyRequest())
                        binReceive = msReceive
                            .ToArray()
                            .Skip(Convert.ToInt32(msReceive.Position))
                            .ToArray();
                    else
                        Denied = true;
                }
            }
            return noiseKind;
        }

        public EthernetProtocol IdentifyProtocol()
        {
            StringBuilder sb = new StringBuilder();
            Protocol = EthernetProtocol.NONE;
            ProtocolMethod method = ProtocolMethod.NONE;
            for (int i = 0; i < HeaderBuffer.Length; i++)
            {
                MarkupType splitter = MarkupType.None;
                HeaderBuffer[i].IsSpliter(out splitter);
                if (
                    (splitter != MarkupType.Empty)
                    && (splitter != MarkupType.Space)
                    && (splitter != MarkupType.Line)
                )
                    sb.Append(HeaderBuffer[i].ToChars(CharEncoding.UTF8));
                if (sb.Length > 3)
                {
                    method = ProtocolMethod.NONE;
                    Enum.TryParse(sb.ToString().ToUpper(), out method);
                    if (method != ProtocolMethod.NONE)
                    {
                        switch (method)
                        {
                            case ProtocolMethod.DATA:
                                Protocol = EthernetProtocol.DOTP;
                                break;
                            case ProtocolMethod.SYNC:
                                Protocol = EthernetProtocol.HTTP;
                                break;
                            case ProtocolMethod.GET:
                                Protocol = EthernetProtocol.HTTP;
                                break;
                            case ProtocolMethod.POST:
                                Protocol = EthernetProtocol.HTTP;
                                break;
                            case ProtocolMethod.OPTIONS:
                                Protocol = EthernetProtocol.HTTP;
                                break;
                            default:
                                Protocol = EthernetProtocol.NONE;
                                break;
                        }
                    }
                    Method = method;
                    if (Protocol != EthernetProtocol.NONE)
                    {
                        sb = null;
                        return Protocol;
                    }
                }
            }
            sb = null;
            return Protocol;
        }

        public MarkupType IncomingHeader(int received)
        {
            disposed = false;
            MarkupType noiseKind = MarkupType.None;
            if (Protocol == EthernetProtocol.NONE)
                IdentifyProtocol();
            if (Protocol == EthernetProtocol.DOTP)
                return SyncHeader(received);
            else if (Protocol == EthernetProtocol.HTTP)
                return HttpHeader(received);
            return noiseKind;
        }

        public unsafe MarkupType IncomingMessage(int received)
        {
            disposed = false;
            MarkupType noiseKind = MarkupType.None;
            if (Protocol == EthernetProtocol.DOTP)
                return SyncMessage(received);
            return noiseKind;
        }

        public void ParseRequest(Stream ms)
        {
            String request = streamReadLine(ms);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            http_method = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_version = tokens[2];
        }

        public void ReadHeaders(Stream ms)
        {
            String line;
            while ((line = streamReadLine(ms)) != null)
            {
                if (line.Equals(""))
                    return;

                int separator = line.IndexOf(':');
                if (separator == -1)
                    throw new Exception("invalid http header line: " + line);

                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                    pos++;

                string value = line.Substring(pos, line.Length - pos);
                HttpHeaders[name] = value;
            }
        }

        public void Reset()
        {
            if (!disposed)
            {
                sb.Clear();
                sb = new StringBuilder();
                msSend.Dispose();
                msSend = new MemoryStream();
                msReceive.Dispose();
                msReceive = new MemoryStream();

                lock (binReceive)
                {
                    if (!binReceiveHandler.Equals(IntPtr.Zero))
                    {
                        GCHandle gc = GCHandle.FromIntPtr(binReceiveHandler);
                        gc.Free();
                        binReceive = new byte[0];
                    }
                }
                lock (binSend)
                    binSend = new byte[0];
            }
        }

        public unsafe MarkupType SyncHeader(int received)
        {
            MarkupType noiseKind = MarkupType.None;

            lock (binReceive)
            {
                int offset = 0,
                    length = received;
                bool inprogress = false;
                if (BlockSize == 0)
                {
                    BlockSize = *((int*)(headerBufferAddress + 4).ToPointer());
                    DeserialBlockId = *((int*)(headerBufferAddress + 12).ToPointer());

                    binReceive = new byte[BlockSize];
                    GCHandle gc = GCHandle.Alloc(binReceive, GCHandleType.Pinned);
                    binReceiveHandler = GCHandle.ToIntPtr(gc);
                    binReceiveAddress = gc.AddrOfPinnedObject();

                    offset = BlockOffset;
                    length -= BlockOffset;
                }

                if (BlockSize > 0)
                    inprogress = true;

                BlockSize -= length;

                if (BlockSize < 1)
                {
                    long endPosition = length;
                    noiseKind = HeaderBuffer.SeekMarkup(out endPosition, SeekDirection.Backward);
                }

                int destid = (int)(binReceive.Length - (BlockSize + length));

                if (inprogress)
                {
                    Extracting.Extract.CopyBlock(
                        binReceiveAddress,
                        destid,
                        headerBufferAddress,
                        offset,
                        length
                    );
                }
            }
            return noiseKind;
        }

        public unsafe MarkupType SyncMessage(int received)
        {
            MarkupType noiseKind = MarkupType.None;

            lock (binReceive)
            {
                int offset = 0,
                    length = received;
                bool inprogress = false;

                if (BlockSize == 0)
                {
                    BlockSize = *((int*)(messageBufferAddress + 4).ToPointer());
                    DeserialBlockId = *((int*)(messageBufferAddress + 12).ToPointer());

                    binReceive = new byte[BlockSize];
                    GCHandle gc = GCHandle.Alloc(binReceive, GCHandleType.Pinned);
                    binReceiveHandler = GCHandle.ToIntPtr(gc);
                    binReceiveAddress = gc.AddrOfPinnedObject();

                    offset = BlockOffset;
                    length -= BlockOffset;
                }

                if (BlockSize > 0)
                    inprogress = true;

                BlockSize -= length;

                if (BlockSize < 1)
                {
                    long endPosition = length;
                    noiseKind = MessageBuffer.SeekMarkup(out endPosition, SeekDirection.Backward);
                }

                int destid = (int)(binReceive.Length - (BlockSize + length));
                if (inprogress)
                {
                    Extracting.Extract.CopyBlock(
                        binReceiveAddress,
                        destid,
                        messageBufferAddress,
                        offset,
                        length
                    );
                }
            }
            return noiseKind;
        }

        public bool VerifyRequest()
        {
            bool verified = false;
            string ip = tr.MyHeader.Context.RemoteEndPoint.Address.ToString();

            if (HttpHeaders.ContainsKey("Authorization"))
                if (HttpHeaders["Authorization"].ToString() != "")
                {
                    string token = HttpHeaders["Authorization"].ToString();
                    MemberIdentity di = null;
                    if (EthernetServer.Security.Register(token, out di, ip))
                    {
                        verified = true;
                        HttpOptions["Authorization"] = di.Token;
                        HttpOptions["UserId"] = di.UserId;
                        HttpOptions["ClientId"] = di.ClientId;
                    }
                }

            if (!verified)
                if (HttpHeaders.ContainsKey("Authorization"))
                    if (HttpHeaders["Authorization"].ToString() != "")
                    {
                        string[] codes = HttpHeaders["Authorization"].ToString().Split(' ');
                        string decode64 = "";
                        string name = "";
                        string key = "";
                        if (codes.Length > 1)
                        {
                            decode64 = Encoding.UTF8.GetString(Convert.FromBase64String(codes[1]));
                            string[] namekey = decode64.Split(':');
                            name = namekey[0];
                            key = namekey[1];

                            MemberIdentity di = null;
                            if (EthernetServer.Security.Register(name, key, out di, ip))
                            {
                                verified = true;
                                HttpOptions["Authorization"] = di.Token;
                                HttpOptions["UserId"] = di.UserId;
                                HttpOptions["ClientId"] = di.ClientId;
                            }
                        }
                    }

            return verified;
        }

        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            StringBuilder data = new StringBuilder();
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n')
                {
                    break;
                }
                if (next_char == '\r')
                {
                    continue;
                }
                if (next_char == -1)
                {
                    Thread.Sleep(1);
                    continue;
                }
                ;
                data.Append(((byte)next_char).ToChars(CharEncoding.UTF8));
            }
            return data.ToString();
        }

        private void writeClose(string state = "close")
        {
            ResponseBuilder.AppendLine("Connection: " + state);
            ResponseBuilder.AppendLine("");
        }

        private void writeDenied()
        {
            ResponseBuilder.AppendLine("HTTP/1.1 401.7 Access denied");
        }

        private void writeFailure()
        {
            ResponseBuilder.AppendLine("HTTP/1.1 404 File not found");
        }

        private void writeOptions()
        {
            if (HttpOptions.Count > 0)
                foreach (DictionaryEntry option in HttpOptions)
                    ResponseBuilder.AppendLine(string.Format("{0}: {1}", option.Key, option.Value));

            ResponseBuilder.AppendLine("Accept: application/json");
            ResponseBuilder.AppendLine("Access-Control-Allow-Headers: data-type");
            ResponseBuilder.AppendLine(
                "Access-Control-Allow-Origin: " + HttpHeaders["Origin"].ToString()
            );
        }

        private void writeSuccess(string content_type = "text/html")
        {
            ResponseBuilder.AppendLine("HTTP/1.1 200 OK");
            ResponseBuilder.AppendLine("Data-Type: " + content_type);
        }
    }
}
