namespace Radical.Instant.Series.Math.Rubrics
{
    using System;
    using Formulas;
    using Set;
    using Rubrics;
    using Instant.Rubrics;
    using Radical.Uniques;

    public class MathRubric : IUnique
    {
        [NonSerialized]
        private CombinedFormula formula;

        [NonSerialized]
        private MathRubrics formulaRubrics;

        [NonSerialized]
        private MathSet formuler;

        [NonSerialized]
        private MathRubrics mathlineRubrics;

        [NonSerialized]
        private MemberRubric memberRubric;

        [NonSerialized]
        private CompiledMathSet evaluator;

        [NonSerialized]
        private SubMathSet subFormuler;

        public MathRubric(MathRubrics rubrics, MemberRubric rubric)
        {
            memberRubric = rubric;
            mathlineRubrics = rubrics;
            SerialCode = rubric.Code;
        }

        public int ComputeOrdinal { get; set; }

        public IUnique Empty => Uscn.Empty;

        public CompiledMathSet Evaluator
        {
            get { return evaluator; }
            set { evaluator = value; }
        }

        public int FigureFieldId
        {
            get => memberRubric.FieldId;
        }

        public Formula Formula
        {
            get { return (!ReferenceEquals(formula, null)) ? formula : null; }
            set
            {
                if (!ReferenceEquals(value, null))
                {
                    formula = value.Prepare(Formuler[this.memberRubric.RubricName]);
                }
            }
        }

        public MathRubric FormulaRubric
        {
            get { return this; }
        }

        public MathRubrics FormulaRubrics
        {
            get { return formulaRubrics; }
            set { formulaRubrics = value; }
        }

        public MathSet Formuler
        {
            get { return formuler; }
            set { formuler = value; }
        }

        public MathRubrics MathsetRubrics
        {
            get { return mathlineRubrics; }
            set { mathlineRubrics = value; }
        }

        public bool PartialMathset { get; set; }

        public Formula RightFormula { get; set; }

        public string RubricName
        {
            get => memberRubric.RubricName;
        }

        public Type RubricType
        {
            get => memberRubric.RubricType;
        }

        public Uscn SerialCode { get; set; }

        public SubMathSet SubFormuler { get; set; }

        public ulong Key
        {
            get => SerialCode.Key;
            set => SerialCode.SetKey(value);
        }

        public ulong TypeKey
        {
            get => SerialCode.TypeKey;
            set => SerialCode.SetTypeKey(value);
        }

        public MathRubric AssignRubric(int ordinal)
        {
            if (FormulaRubrics == null)
                FormulaRubrics = new MathRubrics(mathlineRubrics);

            MathRubric erubric = null;
            MemberRubric rubric = MathsetRubrics.Rubrics[ordinal];
            if (rubric != null)
            {
                erubric = new MathRubric(MathsetRubrics, rubric);
                assignRubric(erubric);
            }
            return erubric;
        }

        public MathRubric AssignRubric(string name)
        {
            if (FormulaRubrics == null)
                FormulaRubrics = new MathRubrics(mathlineRubrics);

            MathRubric erubric = null;
            MemberRubric rubric = MathsetRubrics.Rubrics[name];
            if (rubric != null)
            {
                erubric = new MathRubric(MathsetRubrics, rubric);
                assignRubric(erubric);
            }
            return erubric;
        }

        public MathSet CloneMathset()
        {
            return formuler.Clone();
        }

        public CompiledMathSet CombineEvaluator()
        {
            if (evaluator == null)
                evaluator = formula.CompileMathSet(formula);

            return evaluator;
        }

        public int CompareTo(IUnique other)
        {
            return (int)(Key - other.Key);
        }

        public LeftFormula Compute()
        {
            if (evaluator != null)
            {
                Evaluator reckon = new Evaluator(evaluator.Compute);
                reckon();
            }
            return formula.lexpr;
        }

        public bool Equals(IUnique other)
        {
            return Key == other.Key;
        }

        public byte[] GetBytes()
        {
            return SerialCode.GetBytes();
        }

        public MathSet GetMathset()
        {
            if (!ReferenceEquals(Formuler, null))
                return Formuler;
            else
            {
                formulaRubrics = new MathRubrics(mathlineRubrics);
                return Formuler = new MathSet(this);
            }
        }

        public byte[] GetKeyBytes()
        {
            return SerialCode.GetKeyBytes();
        }

        public MathSet NewMathset()
        {
            return Formuler = new MathSet(this);
        }

        public MathRubric RemoveRubric(int ordinal)
        {
            MathRubric erubric = null;
            MemberRubric rubric = MathsetRubrics.Rubrics[ordinal];
            if (rubric != null)
            {
                erubric = MathsetRubrics[rubric];
                removeRubric(erubric);
            }
            return erubric;
        }

        public MathRubric RemoveRubric(string name)
        {
            MathRubric erubric = null;
            MemberRubric rubric = MathsetRubrics.Rubrics[name];
            if (rubric != null)
            {
                erubric = MathsetRubrics[rubric];
                removeRubric(erubric);
            }
            return erubric;
        }

        private MathRubric assignRubric(MathRubric erubric)
        {
            if (!FormulaRubrics.Contains(erubric))
            {
                if (!MathsetRubrics.MathsetRubrics.Contains(erubric))
                {
                    MathsetRubrics.MathsetRubrics.Add(erubric);
                }

                if (
                    erubric.FigureFieldId == FormulaRubric.FigureFieldId
                    && !MathsetRubrics.FormulaRubrics.Contains(erubric)
                )
                    MathsetRubrics.FormulaRubrics.Add(erubric);

                FormulaRubrics.Add(erubric);
            }
            return erubric;
        }

        private MathRubric removeRubric(MathRubric erubric)
        {
            if (!FormulaRubrics.Contains(erubric))
            {
                FormulaRubrics.Remove(erubric);

                if (!MathsetRubrics.MathsetRubrics.Contains(erubric))
                    MathsetRubrics.MathsetRubrics.Remove(erubric);

                if (
                    !ReferenceEquals(Formuler, null)
                    && !MathsetRubrics.FormulaRubrics.Contains(erubric)
                )
                {
                    MathsetRubrics.Rubrics.Remove(erubric);
                    Formuler = null;
                    Formula = null;
                }
            }
            return erubric;
        }
    }
}
