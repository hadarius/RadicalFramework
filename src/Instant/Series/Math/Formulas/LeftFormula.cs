namespace Radical.Instant.Series.Math.Formulas
{
    using System;
    using System.Reflection.Emit;

    [Serializable]
    public abstract class LeftFormula : Formula
    {
        public abstract void CompileAssign(
            ILGenerator g,
            InstantSeriesMathCompilerContext cc,
            bool post,
            bool partial
        );
    }
}
