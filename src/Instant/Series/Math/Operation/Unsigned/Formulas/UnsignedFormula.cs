namespace Radical.Instant.Series.Math.Operation.Unsigned.Formulas
{
    using System;
    using Set;
    using Math.Formulas;
    using System.Reflection.Emit;

    [Serializable]
    public class UnsignedFormula : Formula
    {
        internal double thevalue;

        public UnsignedFormula(double vv)
        {
            thevalue = vv;
        }

        public override MathSetSize Size
        {
            get { return MathSetSize.Scalar; }
        }

        public override void Compile(ILGenerator g, InstantSeriesMathCompilerContext cc)
        {
            if (cc.IsFirstPass())
                return;
            g.Emit(OpCodes.Ldc_R8, thevalue);
        }
    }
}
