namespace Radical.Ethernet.Transit
{
    using Security.Identity;

    public interface ITransitBuffer : IDisposable
    {
        int BlockOffset { get; set; }

        long BlockSize { get; set; }

        byte[] DeserialBlock { get; }

        int DeserialBlockId { get; set; }

        IntPtr DeserialBlockPtr { get; }

        byte[] SerialBlock { get; set; }

        int SerialBlockId { get; set; }

        IntPtr SerialBlockPtr { get; }

        ServiceSite Site { get; }
    }
}
