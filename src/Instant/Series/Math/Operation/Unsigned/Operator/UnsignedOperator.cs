namespace Radical.Instant.Series.Math.Operation.Unsigned.Operator
{
    using System;
    using Set;
    using Math.Formulas;
    using System.Reflection.Emit;

    [Serializable]
    public class UnsignedOperator : Formula
    {
        protected Formula e;

        public UnsignedOperator(Formula ee)
        {
            e = ee;
        }

        public override MathSetSize Size
        {
            get { return e.Size; }
        }

        public override void Compile(ILGenerator g, InstantSeriesMathCompilerContext cc)
        {
            e.Compile(g, cc);
        }
    }
}
