using System.Runtime.InteropServices;

namespace Radical.Instant.Series.Stocking.Options
{
    using Instant;
    using Instant.Series;

    public class StockOptions<T> : StockOptions
    {
        public StockOptions() : base(new InstantSeriesCreator<T>().Combine()) { }

        public StockOptions(IInstantSeries series) : base(series) { }
    }

    public class StockOptions
    {
        public StockOptions() { }

        public StockOptions(IInstantSeries series) : this(series.FigureType, series.FigureSize)
        {
            this.series = series;
            Type = series.Type;
        }

        public StockOptions(Type type) : this(new InstantSeriesCreator(type, InstantType.Reference).Combine()) { }

        public StockOptions(Type type, int blockSize)
        {
            ItemType = type;
            blocksize = blockSize;
        }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        protected string stockname;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        protected string basepath;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        protected string stockpath;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        protected string sectorsuffix;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        protected string regitrysuffix;
        protected ushort sectorsize = 25 * 1000;
        protected ushort clustersize = 10;
        protected int blocksize = 0;
        protected bool oversized = false;
        protected bool isowner = true;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        protected string typestring;
        protected Type type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        protected string itemtypestring;
        protected Type itemtype;
        protected IInstantSeries series;

        public IInstantSeries Figures
        {
            get => series;
            set => series = value;
        }

        public Type Type
        {
            get =>
                type ??=
                    typestring == null
                        ? type = Type.GetType(typestring = typeof(byte[]).FullName)
                        : type = Type.GetType(typestring);
            set
            {
                typestring = value.FullName;
                type = value;
            }
        }
        public Type ItemType
        {
            get =>
                itemtype ??=
                    itemtypestring == null
                        ? itemtype = Type.GetType(itemtypestring = typeof(byte[]).FullName)
                        : itemtype = Type.GetType(itemtypestring);
            set
            {
                itemtypestring = value.FullName;
                itemtype = value;
            }
        }

        public virtual string RegistrySuffix
        {
            get => regitrysuffix ??= "str";
            set => regitrysuffix = value;
        }

        public virtual string SectorSuffix
        {
            get => sectorsuffix ??= "std";
            set => sectorsuffix = value;
        }

        public virtual string FileName => $"{type.Name}.{SectorSuffix}";

        public virtual string StockName => $"{BasePath}__{type.FullName}.{SectorSuffix}";

        public virtual string FilePath => $"{BasePath}/{type.Name}/{FileName}";

        public virtual string StockPath => $"{BasePath}/{type.Name}";

        public virtual string BasePath
        {
            get => basepath ??= ".mmfs";
            set => basepath = value;
        }

        public virtual ushort SectorSize
        {
            get => sectorsize;
            set => sectorsize = value;
        }

        public virtual ushort ClusterSize
        {
            get => clustersize;
            set => clustersize = value;
        }

        public virtual bool Oversized
        {
            get => oversized;
            set => oversized = value;
        }

        public virtual bool IsOwner
        {
            get => isowner;
            set => isowner = value;
        }

        public virtual int BlockSize
        {
            get => blocksize;
            set => blocksize = value;
        }
    }
}
