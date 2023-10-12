namespace Radical.Ethernet.Transit
{
    using System.IO;

    public interface ITransitFormatter
    {
        int DeserialCount { get; set; }

        int ItemsCount { get; }

        int ProgressCount { get; set; }

        int SerialCount { get; set; }

        object Deserialize(ITransitBuffer buffer);

        object Deserialize(Stream stream);

        object GetHeader();

        object[] GetMessage();

        int Serialize(
            ITransitBuffer buffer,
            int offset,
            int batchSize
        );

        int Serialize(
            Stream stream,
            int offset,
            int batchSize
        );
    }
}
