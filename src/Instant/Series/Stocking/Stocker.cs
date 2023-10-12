using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Radical.Instant.Series.Stocking
{
    using Radical.Series;
    using Uniques;
    using Options;

    public class Stocker<T> : Stocker
    {
        public Stocker() : this(new InstantSeriesCreator<T>().Combine()) { }

        public Stocker(IInstantSeries series) : base(new StockOptions<T>(series)) { }

        public Stocker(StockOptions<T> options) : base(options) { }

        public Stocker(Action<StockOptions<T>> options)
        {
            var _options = new StockOptions<T>();
            _options.Type = typeof(T);
            options.Invoke((StockOptions<T>)_options);
            _options.BlockSize = Marshal.SizeOf(_options.Type);
            clustersize = _options.ClusterSize;
            sectorsize = _options.SectorSize;
            this.options = _options;
        }
    }

    public class Stocker : IStocker
    {
        protected StockOptions options;
        protected IInstantSeries series;
        protected ISeries<IStock[]> clusters;
        protected ITypedSeries<IStock> registries;
        protected ushort clustersize;
        protected ushort sectorsize;
        protected int counter;

        public virtual object this[int index, string propertyName]
        {
            get
            {
                var rubric = series.Rubrics[propertyName];
                return this[index, rubric.RubricId, rubric.RubricType];
            }
            set
            {
                var rubric = series.Rubrics[propertyName];
                this[index, rubric.RubricId, rubric.RubricType] = value;
            }
        }
        public virtual object this[int index, int fieldId]
        {
            get
            {
                var rubric = series.Rubrics[fieldId];
                return this[index, fieldId, rubric.RubricType];
            }
            set
            {
                var rubric = series.Rubrics[fieldId];
                this[index, fieldId, rubric.RubricType] = value;
            }
        }
        public virtual object this[int index, int field, Type type]
        {
            get
            {
                var sector = GetSector((uint)index, out ushort[] zyx);
                return sector[zyx[2], field, type];
            }
            set
            {
                var sector = GetSector((uint)index, out ushort[] zyx);
                sector[zyx[2], field, type] = value;
            }
        }
        public virtual object this[int index]
        {
            get
            {
                var sector = GetSector((uint)index, out ushort[] zyx);
                return sector[zyx[2]];
            }
            set
            {
                var sector = GetSector((uint)index, out ushort[] zyx);
                sector[zyx[2]] = value;
            }
        }
        public virtual object this[Uscn serialcode]
        {
            get { return this.Get(serialcode); }
            set
            {
                if (!serialcode.GetFlagBit(0))
                {
                    var sector = GetSector(serialcode);
                    if (sector != null)
                        sector[serialcode.BlockX] = value;
                }
            }
        }

        public Stocker() { }

        public Stocker(IInstantSeries series) : this(new StockOptions(series)) { }

        public Stocker(Type type) : this(new InstantSeriesCreator(type, InstantType.Reference).Combine()) { }

        public Stocker(StockOptions options)
        {
            this.options = options;
            clustersize = options.ClusterSize;
            sectorsize = options.SectorSize;
            series = options.Figures;
            if (options.Type != null && options.BlockSize == 0)
                options.BlockSize = Marshal.SizeOf(options.ItemType);
        }

        public Stocker(Action<StockOptions> options)
        {
            var _options = new StockOptions();
            options.Invoke(_options);
            if (_options.Type != null && _options.BlockSize == 0)
                _options.BlockSize = Marshal.SizeOf(_options.ItemType);
            clustersize = _options.ClusterSize;
            sectorsize = _options.SectorSize;
            series = _options.Figures;
            this.options = _options;
        }

        public void SetFigures(IInstantSeries series)
        {
            options.Figures = series;
            options.Type = series.Type;
            options.BlockSize = series.FigureSize;
        }

        public void Write() { }

        public void Read() { }

        public void Open()
        {
            if (clusters == null)
                clusters = new Registry<IStock[]>();

            if (registries == null)
                registries = new TypedRegistry<IStock>();

            string[] files = null;
            if (!Directory.Exists(options.StockPath))
                Directory.CreateDirectory(options.StockPath);
            else
                files = Directory.GetFiles(options.StockPath);
            if (files is null)
            {
                foreach (string file in files)
                    if (file.Contains($".{options.SectorSuffix}"))
                    {
                        string[] ids = file.Split('.');
                        int length = ids.Length;
                        var _options = new SectorOptions(
                            series,
                            UInt16.Parse(ids[length - 3]),
                            UInt16.Parse(ids[length - 2])
                        );
                        IStock sector = new ArrayStock(_options);
                        IStock[] cluster = GetCluster(sector.ClusterId);
                        cluster[sector.SectorId] = sector;
                    }
                    else if (file.Contains($".{options.RegistrySuffix}")) { }
            }
            else
            {
                var _options = new SectorOptions(series, 0, 0);
                IStock sector = new ArrayStock(_options);
                IStock[] cluster = GetCluster(sector.ClusterId);
                cluster[sector.SectorId] = sector;
            }
        }

        public void Close()
        {
            if (clusters != null)
                foreach (IStock[] cluster in clusters)
                    foreach (IStock sector in cluster)
                        if (sector != null)
                            sector.Close();
            clusters = null;
        }

        public virtual object Get(Uscn serialcode)
        {
            if (!serialcode.GetFlagBit(0))
            {
                var sector = GetSector(serialcode);
                if (sector != null)
                    return sector[serialcode.BlockX];
            }
            return null;
        }

        public virtual object Set(IInstant figure)
        {
            var serialcode = figure.Code;
            if (!serialcode.GetFlagBit(0))
            {
                var sector = GetSector(serialcode);
                if (sector != null)
                    return sector[serialcode.BlockX] = figure;
            }
            return null;
        }

        public IStock GetSector(ulong index, out ushort[] zyx)
        {
            ulong vectorYZ = (uint)(sectorsize * clustersize);
            ulong blockZdiv = (index / vectorYZ);
            ulong blockYsub = index - (blockZdiv * vectorYZ);
            ulong blockYdiv = blockYsub / sectorsize;
            ulong blockZ = (blockZdiv > 0 && (index % vectorYZ) > 0) ? blockZdiv + 1 : blockZdiv;
            ulong blockY = (blockYdiv > 0 && (index % sectorsize) > 0) ? blockYdiv + 1 : blockYdiv;
            ulong blockX = index % sectorsize;
            zyx = new ushort[] { (ushort)blockZ, (ushort)blockY, (ushort)blockX };
            return GetSector(zyx[0], zyx[1]);
        }

        public IStock GetSector(Uscn serialcode)
        {
            return GetSector(serialcode.BlockZ, serialcode.BlockY);
        }

        public IStock GetSector(IInstant figure)
        {
            return GetSector(figure.Code);
        }

        private IStock[] GetCluster(ushort clusterId)
        {
            if (clusters == null)
                Open();

            if (!clusters.TryGet(clusterId, out IStock[] cluster))
            {
                cluster = new IStock[options.ClusterSize];
                clusters.Add(clusterId, cluster);
            }
            return cluster;
        }

        private IStock GetSector(ushort clusterId, ushort sectorId)
        {
            IStock[] _cluster = GetCluster(clusterId);
            IStock _sector = _cluster?[sectorId];
            if (_sector == null)
            {
                _sector = new ArrayStock(new SectorOptions(series, clusterId, sectorId));
                if (!_sector.Exists)
                {
                    _sector.ClusterId = clusterId;
                    _sector.SectorId = sectorId;
                    _sector.ItemSize = series.FigureSize;
                    _sector.ItemCapacity = sectorsize;
                    _sector.WriteHeader();
                }
                _cluster[sectorId] = _sector;
            }
            return _sector;
        }
    }
}
