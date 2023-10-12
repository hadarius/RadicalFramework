namespace Radical.Instant.Series.Math.Set
{
    using System;

    public class MathSetData
    {
        public int RowCount;
        public int RowOffset;

        public MathSetData() { }

        public MathSetData(IInstantSeries table)
        {
            Data = table;
        }

        public MathSetData(IInstantSeries table, int rowOffset, int rowCount)
        {
            RowCount = rowCount;
            RowOffset = rowOffset;
            Data = table;
        }

        public double this[int rowid, int cellid]
        {
            get { return Convert.ToDouble(Data[rowid, cellid]); }
            set { Data[rowid, cellid] = value; }
        }

        public IInstantSeries Data;

        public bool Enabled = false;
        public int Count { get; set; }
    }
}
