namespace Radical.Instant.Series
{
    using Instant.Proxies;
    using Radical.Uniques;
    using Rubrics;
    using System.Linq;

    public enum InstantSeriesMode
    {
        Sleeve,
        Figure
    }

    public class InstantProxiesCreator<T> : InstantSeriesCreator
    {
        public InstantProxiesCreator() : base(typeof(T)) { }

        public InstantProxiesCreator(string seriesName) : base(typeof(T), seriesName) { }
    }

    public class InstantSeriesCreator<T> : InstantSeriesCreator
    {
        public InstantSeriesCreator(InstantType mode = InstantType.Reference) : base(typeof(T), mode) { }

        public InstantSeriesCreator(string seriesName, InstantType mode = InstantType.Reference)
            : base(typeof(T), seriesName, mode) { }
    }

    public class InstantSeriesCreator : IInstantCreator
    {
        private Type compiledType;
        private InstantCreator figure;
        private ProxyCreator sleeve;
        private ulong key;
        private bool safeThread;
        private InstantSeriesMode mode;

        public InstantSeriesCreator(
            ProxyCreator sleeveGenerator,
            string seriesTypeName = null,
            bool safeThread = true
        )
        {
            mode = InstantSeriesMode.Sleeve;
            if (sleeveGenerator.Type == null)
                sleeveGenerator.Create();
            this.safeThread = safeThread;
            this.sleeve = sleeveGenerator;
            Name =
                (seriesTypeName != null && seriesTypeName != "")
                    ? seriesTypeName
                    : sleeve.Name + "_F";
        }

        public InstantSeriesCreator(IProxy sleeveObject, bool safeThread = true)
            : this(
                new ProxyCreator(sleeveObject.GetType(), sleeveObject.GetType().Name),
                null,
                safeThread
            )
        { }

        public InstantSeriesCreator(Type sleeveModelType, bool safeThread = true)
            : this(new ProxyCreator(sleeveModelType), null, safeThread) { }

        public InstantSeriesCreator(Type sleeveModelType, string seriesName, bool safeThread = true)
            : this(new ProxyCreator(sleeveModelType), seriesName, safeThread) { }

        public InstantSeriesCreator(
            InstantCreator figureGenerator,
            string seriesTypeName = null,
            bool safeThread = true
        )
        {
            mode = InstantSeriesMode.Figure;
            if (figureGenerator.Type == null)
                figureGenerator.Create();
            this.safeThread = safeThread;
            this.figure = figureGenerator;
            Name =
                (seriesTypeName != null && seriesTypeName != "")
                    ? seriesTypeName
                    : figure.Name + "s";
        }

        public InstantSeriesCreator(IInstant figureObject, bool safeThread = true)
            : this(
                new InstantCreator(
                    figureObject.GetType(),
                    figureObject.GetType().Name,
                    InstantType.Reference
                ),
                null,
                safeThread
            )
        { }

        public InstantSeriesCreator(
            IInstant figureObject,
            string seriesTypeName,
            InstantType modeType = InstantType.Reference,
            bool safeThread = true
        )
            : this(
                new InstantCreator(figureObject.GetType(), figureObject.GetType().Name, modeType),
                seriesTypeName,
                safeThread
            )
        { }

        public InstantSeriesCreator(
            MemberRubrics figureRubrics,
            string seriesTypeName = null,
            string figureTypeName = null,
            InstantType modeType = InstantType.Reference,
            bool safeThread = true
        ) : this(new InstantCreator(figureRubrics, figureTypeName, modeType), seriesTypeName, safeThread)
        { }

        public InstantSeriesCreator(Type figureModelType, InstantType modeType, bool safeThread = true)
            : this(new InstantCreator(figureModelType, null, modeType), null, safeThread) { }

        public InstantSeriesCreator(
            Type figureModelType,
            string seriesTypeName,
            InstantType modeType,
            bool safeThread = true
        ) : this(new InstantCreator(figureModelType, null, modeType), seriesTypeName, safeThread) { }

        public InstantSeriesCreator(
            Type figureModelType,
            string seriesTypeName,
            string figureTypeName,
            InstantType modeType = InstantType.Reference,
            bool safeThread = true
        ) : this(new InstantCreator(figureModelType, figureTypeName, modeType), seriesTypeName, safeThread)
        { }

        public Type BaseType { get; set; }

        public string Name { get; set; }

        public IRubrics Rubrics
        {
            get => (sleeve != null) ? sleeve.Rubrics : figure.Rubrics;
        }

        public int Size
        {
            get => (sleeve != null) ? sleeve.Size : figure.Size;
        }

        public Type Type { get; set; }

        public IInstantSeries Combine()
        {
            if (this.Type == null)
            {
                var ifc = new InstantSeriesCompiler(this, safeThread);
                compiledType = ifc.CompileFigureType(Name);
                this.Type = compiledType.New().GetType();
                key = Unique.NewKey;
            }
            if (mode == InstantSeriesMode.Figure)
                return newFigures();
            else
                return newSleeves();
        }

        public object New()
        {
            return (mode == InstantSeriesMode.Sleeve) ? newSleeves() : newFigures();
        }

        private MemberRubrics CloneRubrics()
        {
            var rubrics = new MemberRubrics(Rubrics.Select(r => r.ShalowCopy(null)));
            rubrics.KeyRubrics = new MemberRubrics(
                Rubrics.KeyRubrics.Select(r => r.ShalowCopy(null))
            );
            rubrics.Update();
            return rubrics;
        }

        private IInstantSeries newFigures()
        {
            IInstantSeries newseries = newFigures((IInstantSeries)(Type.New()));
            newseries.Rubrics = CloneRubrics();
            newseries.KeyRubrics = newseries.Rubrics.KeyRubrics;
            newseries.View = newseries.AsQueryable();
            return newseries;
        }

        private IInstantSeries newFigures(IInstantSeries newseries)
        {
            newseries.FigureType = figure.Type;
            newseries.FigureSize = figure.Size;
            newseries.Type = this.Type;
            newseries.Creator = this;
            newseries.Prime = true;
            newseries.Key = key;
            newseries.TypeKey = Name.UniqueKey64();

            return newseries;
        }

        private IInstantSeries newSleeves()
        {
            IInstantSeries newsleeves = newSleeves((IInstantSeries)(this.Type.New()));
            newsleeves.Rubrics = CloneRubrics();
            newsleeves.KeyRubrics = newsleeves.Rubrics.KeyRubrics;
            newsleeves.View = newsleeves.AsQueryable();
            return newsleeves;
        }

        private IInstantSeries newSleeves(IInstantSeries newsleeves)
        {
            newsleeves.FigureType = sleeve.Type;
            newsleeves.FigureSize = sleeve.Size;
            newsleeves.Type = this.Type;
            newsleeves.Creator = this;
            newsleeves.Prime = true;
            newsleeves.Key = key;
            newsleeves.TypeKey = Name.UniqueKey64();

            return newsleeves;
        }

    }
}
