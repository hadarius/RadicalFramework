using Radical.Instant.Series;
using System.Runtime.InteropServices;

namespace Radical.Instant.Series.Stocking.Options
{
    public class SectorOptions<T> : SectorOptions
    {
        public SectorOptions() : base(typeof(T)) { }

        public SectorOptions(IInstantSeries series, ushort clusterId, ushort sectorId)
            : base(series.FigureType, clusterId, sectorId, series.FigureSize)
        {
            this.series = series;
            this.Type = series.Type;
        }

        public SectorOptions(ushort clusterId, ushort sectorId)
            : base(typeof(T), sectorId, sectorId) { }

        public SectorOptions(ushort clusterId, ushort sectorId, int blockSize)
            : base(typeof(T), sectorId, sectorId, blockSize) { }
    }

    public class SectorOptions : StockOptions
    {
        public SectorOptions() { }

        public SectorOptions(IInstantSeries series, ushort clusterId, ushort sectorId)
            : this(series.FigureType, clusterId, sectorId, series.FigureSize)
        {
            this.series = series;
            this.Type = series.Type;
        }

        public SectorOptions(Type type)
        {
            this.ItemType = type;
            this.blocksize = Marshal.SizeOf(type);
        }

        public SectorOptions(Type type, ushort clusterId, ushort sectorId, int blockSize)
        {
            this.ItemType = type;
            this.clusterId = clusterId;
            this.sectorId = sectorId;
            this.blocksize = blockSize;
        }

        public SectorOptions(Type type, ushort clusterId, ushort sectorId) : this(type)
        {
            this.clusterId = clusterId;
            this.sectorId = sectorId;
        }

        protected ushort clusterId;
        protected ushort sectorId;

        public ushort ClusterId
        {
            get => clusterId;
            set => clusterId = value;
        }

        public ushort SectorId
        {
            get => sectorId;
            set => sectorId = value;
        }

        public override string FileName => $"{type.Name}.{ClusterId}.{SectorId}.std";

        public string SectorName => $"{BasePath}__{type.FullName}.{ClusterId}.{SectorId}.std";

        public string SectorPath => $"{BasePath}/{type.Name}/{FileName}";
    }
}
