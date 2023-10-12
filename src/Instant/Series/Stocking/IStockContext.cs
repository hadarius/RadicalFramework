namespace Radical.Instant.Series.Stocking
{
    using System;
    using Ethernet.Transit;

    public interface IStockContext : ITransitBuffer, IDisposable
    {
        long BufferSize { get; set; }

        int ClientCount { get; set; }

        int Items { get; set; }

        string File { get; set; }

        long FreeSize { get; set; }

        long ItemCapacity { get; set; }

        int ItemCount { get; set; }

        int ItemSize { get; set; }

        int NodeCount { get; set; }

        string Path { get; set; }

        ushort SectorId { get; set; }

        int ServerCount { get; set; }

        ushort StockId { get; set; }

        long UsedSize { get; set; }

    }
}