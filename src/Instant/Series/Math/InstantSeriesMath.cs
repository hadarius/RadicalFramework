namespace Radical.Instant.Series.Math
{
    using Radical.Series;
    using Uniques;
    using System.Linq;
    using Rubrics;
    using Set;
    using Instant.Rubrics;

    public class InstantSeriesMath : IInstantSeriesMath
    {
        private MathRubrics computation;

        public InstantSeriesMath(IInstantSeries data)
        {
            computation = new MathRubrics(data);
            serialcode.Key = (ulong)DateTime.Now.ToBinary();
            if (data.Computations == null)
                data.Computations = new Catalog<IInstantSeriesMath>();
            data.Computations.Put(this);
        }

        public MathSet this[int id]
        {
            get { return GetMathset(id); }
        }
        public MathSet this[string name]
        {
            get { return GetMathset(name); }
        }
        public MathSet this[MemberRubric rubric]
        {
            get { return GetMathset(rubric); }
        }

        public MathSet GetMathset(int id)
        {
            MemberRubric rubric = computation.Rubrics[id];
            if (rubric != null)
            {
                MathRubric mathrubric = null;
                if (computation.MathsetRubrics.TryGet(rubric.Name, out mathrubric))
                    return mathrubric.GetMathset();
                return computation
                    .Put(rubric.Name, new MathRubric(computation, rubric))
                    .Value.GetMathset();
            }
            return null;
        }

        public MathSet GetMathset(string name)
        {
            MemberRubric rubric = null;
            if (computation.Rubrics.TryGet(name, out rubric))
            {
                MathRubric mathrubric = null;
                if (computation.MathsetRubrics.TryGet(name, out mathrubric))
                    return mathrubric.GetMathset();
                return computation
                    .Put(rubric.Name, new MathRubric(computation, rubric))
                    .Value.GetMathset();
            }
            return null;
        }

        public MathSet GetMathset(MemberRubric rubric)
        {
            return GetMathset(rubric.Name);
        }

        public bool ContainsFirst(MemberRubric rubric)
        {
            return computation.First.Value.RubricName == rubric.Name;
        }

        public bool ContainsFirst(string rubricName)
        {
            return computation.First.Value.RubricName == rubricName;
        }

        public IInstantSeries Compute()
        {
            computation.Combine();
            computation
                .AsValues()
                .Where(p => !p.PartialMathset)
                .OrderBy(p => p.ComputeOrdinal)
                .Select(p => p.Compute())
                .ToArray();
            return computation.Data;
        }

        private Uscn serialcode;
        public Uscn SerialCode
        {
            get => serialcode;
            set => serialcode = value;
        }
        public IUnique Empty => Uscn.Empty;
        public ulong Key
        {
            get => serialcode.Key;
            set => serialcode.Key = value;
        }

        public int CompareTo(IUnique other)
        {
            return serialcode.CompareTo(other);
        }

        public bool Equals(IUnique other)
        {
            return serialcode.Equals(other);
        }

        public byte[] GetBytes()
        {
            return serialcode.GetBytes();
        }

        public byte[] GetKeyBytes()
        {
            return serialcode.GetKeyBytes();
        }

        public ulong TypeKey
        {
            get => serialcode.TypeKey;
            set => serialcode.TypeKey = value;
        }
    }
}
