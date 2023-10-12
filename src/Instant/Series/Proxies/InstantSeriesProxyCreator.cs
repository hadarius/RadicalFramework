namespace Radical.Instant.Series.Proxies
{
    using Radical.Uniques;
    using Rubrics;


    public class InstantSeriesProxyCreator : IInstantCreator
    {
        private Type compiledType;
        private ulong key;

        public InstantSeriesProxyCreator(IInstantSeries seriesObject) : this(seriesObject, null) { }

        public InstantSeriesProxyCreator(IInstantSeries seriesObject, string sleeveTypeName)
        {
            Name =
                (sleeveTypeName != null && sleeveTypeName != "")
                    ? sleeveTypeName
                    : seriesObject.Type.Name + "_S";
            series = seriesObject;
        }

        public Type BaseType { get; set; }

        public string Name { get; set; }

        public IRubrics Rubrics
        {
            get => series.Rubrics;
        }

        public int Size
        {
            get => series.FigureSize;
        }

        public Type Type { get; set; }

        private IInstantSeries series { get; set; }

        public IInstantSeriesProxy Create()
        {
            if (series != null)
            {
                if (this.Type == null)
                {
                    var rsb = new InstantSeriesProxyCompiler(this);
                    compiledType = rsb.CompileFigureType(Name);
                    this.Type = compiledType.New().GetType();
                    key = Name.UniqueKey64();
                }
                return newProxySeries();
            }
            return null;
        }

        public object New()
        {
            return newProxySeries();
        }

        private IInstantSeriesProxy newProxySeries()
        {
            IInstantSeriesProxy o = (IInstantSeriesProxy)(Type.New());
            o.Series = series;
            o.Proxies = (IInstantSeries)(series.Creator.New());
            o.Proxies.Prime = false;
            o.Creator = series.Creator;
            o.Key = key;
            o.TypeKey = Unique.NewKey;
            return o;
        }
    }
}
