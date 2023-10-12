namespace Radical.Instant.Series.Querying
{
    using System;

    [Serializable]
    public class InstantSeriesSort
    {
        #region Fields

        public IInstantSeries Figures;
        public InstantSeriesSortTerms Terms;

        #endregion

        #region Constructors

        public InstantSeriesSort(IInstantSeries series)
        {
            this.Figures = series;
            Terms = new InstantSeriesSortTerms(series);
        }

        #endregion
    }
}