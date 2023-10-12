using System.Collections;
using System.IO;
using System.Net.Sockets;
using Radical.Series;
using System.Text;
using System.Threading;

namespace Radical.Ethernet.Transit
{
    public interface ITransitContext : ITransitBuffer
    {
        ManualResetEvent BatchesReceivedNotice { get; set; }

        int BufferSize { get; }

        bool Close { get; set; }

        bool Denied { get; set; }

        string Echo { get; }

        byte[] HeaderBuffer { get; }

        ManualResetEvent HeaderReceivedNotice { get; set; }

        ManualResetEvent HeaderSentNotice { get; set; }

        Hashtable HttpHeaders { get; set; }

        Hashtable HttpOptions { get; set; }

        int Id { get; set; }

        Socket Listener { get; set; }

        byte[] MessageBuffer { get; }

        ManualResetEvent MessageReceivedNotice { get; set; }

        ManualResetEvent MessageSentNotice { get; set; }

        ProtocolMethod Method { get; set; }

        int ObjectPosition { get; set; }

        int ObjectRemainds { get; set; }

        EthernetProtocol Protocol { get; set; }

        bool ReceiveMessage { get; set; }

        StringBuilder RequestBuilder { get; set; }

        ISeries<byte[]> Resources { get; set; }

        StringBuilder ResponseBuilder { get; set; }

        bool SendMessage { get; set; }

        bool Synchronic { get; set; }

        EthernetTransit Transfer { get; set; }

        void Append(string text);

        void HandleDeniedRequest();

        void HandleGetRequest(string content_type = "text/html");

        void HandleOptionsRequest(string content_type = "text/html");

        void HandlePostRequest(string content_type = "text/html");

        EthernetProtocol IdentifyProtocol();

        MarkupType IncomingHeader(int received);

        MarkupType IncomingMessage(int received);

        void Reset();
    }
}
