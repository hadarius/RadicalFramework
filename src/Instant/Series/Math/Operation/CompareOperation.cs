namespace Radical.Instant.Series.Math.Operation;

using System;
using System.Reflection.Emit;
using Formulas;
using Binary.Formulas;
using Binary.Operator;
using Set;

[Serializable]
public class CompareOperation : BinaryFormula
{
    internal BinaryOperator oper;

    public CompareOperation(Formula e1, Formula e2, BinaryOperator op) : base(e1, e2)
    {
        oper = op;
    }

    public override MathSetSize Size
    {
        get { return expr1.Size == MathSetSize.Scalar ? expr2.Size : expr1.Size; }
    }

    public override void Compile(ILGenerator g, InstantSeriesMathCompilerContext cc)
    {
        expr1.Compile(g, cc);
        expr2.Compile(g, cc);
        if (cc.IsFirstPass())
            return;
        oper.Compile(g);
    }
}
