namespace Radical.Instant.Series.Math.Set
{
    public abstract class CompiledMathSet
    {
        public IInstantSeries[] DataParameters = new IInstantSeries[1];
        public int ParametersCount = 0;

        public abstract void Compute();

        public int GetColumnCount(int paramid)
        {
            return DataParameters[paramid].Rubrics.Count;
        }

        public int GetIndexOf(IInstantSeries v)
        {
            for (int i = 0; i < ParametersCount; i++)
                if (DataParameters[i] == v)
                    return 1 + i;
            return -1;
        }

        public int GetRowCount(int paramid)
        {
            return DataParameters[paramid].Count;
        }

        public int Put(IInstantSeries v)
        {
            int index = GetIndexOf(v);
            if (index < 0)
            {
                DataParameters[ParametersCount] = v;
                return 1 + ParametersCount++;
            }
            else
            {
                DataParameters[index] = v;
            }
            return index;
        }

        public void SetParams(IInstantSeries p)
        {
            Put(p);
        }

        public bool SetParams(IInstantSeries p, int index)
        {
            if (index < ParametersCount)
            {
                if (ReferenceEquals(DataParameters[index], p))
                    return false;
                else
                    DataParameters[index] = p;
            }
            return false;
        }

        public void SetParams(IInstantSeries[] p, int paramCount)
        {
            DataParameters = p;
            ParametersCount = paramCount;
        }
    }
}
