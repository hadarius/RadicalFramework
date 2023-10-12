namespace Radical.Instant.Series.Math.Operation
{
    using System;
    using Set;
    using Formulas;
    using Binary.Formulas;
    using System.Reflection.Emit;

    [Serializable]
    public class PowerOperation : BinaryFormula
    {
        public PowerOperation(Formula e1, Formula e2) : base(e1, e2) { }

        public override MathSetSize Size
        {
            get { return expr1.Size; }
        }

        public override void Compile(ILGenerator g, InstantSeriesMathCompilerContext cc)
        {
            if (cc.IsFirstPass())
            {
                expr1.Compile(g, cc);
                expr2.Compile(g, cc);
                if (!(expr2.Size == MathSetSize.Scalar))
                    throw new SizeMismatchException(
                        "Pow Operator requires a scalar second operand"
                    );
                return;
            }
            expr1.Compile(g, cc);
            expr2.Compile(g, cc);
            g.EmitCall(OpCodes.Call, typeof(Math).GetMethod("Pow"), null);
        }
    }
}
