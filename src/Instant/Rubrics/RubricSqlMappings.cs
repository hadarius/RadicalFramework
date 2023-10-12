namespace Radical.Instant.Rubrics
{
    using Radical.Series;
    using Radical.Series.Base;
    using Radical.Uniques;

    [Serializable]
    public class RubricSqlMappings : RegistrySeries<RubricSqlMapping>
    {
        public override ISeriesItem<RubricSqlMapping>[] EmptyVector(int size)
        {
            return new SeriesItem<RubricSqlMapping>[size];
        }

        public override ISeriesItem<RubricSqlMapping> EmptyItem()
        {
            return new SeriesItem<RubricSqlMapping>();
        }

        public override ISeriesItem<RubricSqlMapping>[] EmptyTable(int size)
        {
            return new SeriesItem<RubricSqlMapping>[size];
        }

        public override ISeriesItem<RubricSqlMapping> NewItem(RubricSqlMapping value)
        {
            return new SeriesItem<RubricSqlMapping>(value.DbTableName.UniqueKey(), value);
        }

        public override ISeriesItem<RubricSqlMapping> NewItem(ISeriesItem<RubricSqlMapping> value)
        {
            return new SeriesItem<RubricSqlMapping>(value);
        }

        public override ISeriesItem<RubricSqlMapping> NewItem(object key, RubricSqlMapping value)
        {
            return new SeriesItem<RubricSqlMapping>(key, value);
        }

        public override ISeriesItem<RubricSqlMapping> NewItem(ulong key, RubricSqlMapping value)
        {
            return new SeriesItem<RubricSqlMapping>(key, value);
        }
    }
}
