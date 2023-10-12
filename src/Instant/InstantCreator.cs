namespace Radical.Instant
{
    using Radical.Series;
    using Radical.Uniques;
    using Rubrics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class InstantCreator<T> : InstantCreator
    {
        public InstantCreator(InstantType modeType = InstantType.Reference)
            : base(typeof(T), modeType) { }

        public InstantCreator(string figureTypeName, InstantType modeType = InstantType.Reference)
            : base(typeof(T), figureTypeName, modeType) { }
    }

    public class InstantCreator : IInstantCreator
    {
        public RubricBuilder instantBuilder = new RubricBuilder();
        public ISeries<RubricModel> rubricBuilders = new Registry<RubricModel>();
        private MemberRubric[] memberRubrics;
        private Type compiledType;

        public InstantCreator(
            IList<MemberInfo> figureMembers,
            InstantType modeType = InstantType.Reference
        ) : this(figureMembers.ToArray(), null, modeType) { }

        public InstantCreator(
            IList<MemberInfo> figureMembers,
            string figureTypeName,
            InstantType modeType = InstantType.Reference
        )
        {
            Name =
                (figureTypeName != null && figureTypeName != "")
                    ? figureTypeName
                    : DateTime.Now.ToBinary().ToString();

            Name += "Instant";

            mode = modeType;

            instantBuilder = new RubricBuilder();

            rubricBuilders = instantBuilder.Create(instantBuilder.PrepareMembers(figureMembers));

            Rubrics = new MemberRubrics(rubricBuilders.Select(m => m.Member).ToArray());
            Rubrics.KeyRubrics = new MemberRubrics();
        }

        public InstantCreator(
            MemberRubrics figureRubrics,
            string figureTypeName,
            InstantType modeType = InstantType.Reference
        ) : this(figureRubrics.ToArray(), figureTypeName, modeType) { }

        public InstantCreator(Type figureModelType, InstantType modeType = InstantType.Reference)
            : this(figureModelType, null, modeType) { }

        public InstantCreator(
            Type figureModelType,
            string figureTypeName,
            InstantType modeType = InstantType.Reference
        )
        {
            BaseType = figureModelType;

            if (modeType == InstantType.Derived)
                IsDerived = true;

            Name = figureTypeName == null ? figureModelType.Name : figureTypeName;
            Name += "Instant";
            mode = modeType;

            instantBuilder = new RubricBuilder();
            rubricBuilders = instantBuilder.Create(figureModelType);

            Rubrics = new MemberRubrics(rubricBuilders.Select(m => m.Member).ToArray());
            Rubrics.KeyRubrics = new MemberRubrics();
        }

        public Type BaseType { get; set; }

        public bool IsDerived { get; set; }

        public string Name { get; set; }

        public IRubrics Rubrics { get; set; }

        public int Size { get; set; }

        public Type Type { get; set; }

        private InstantType mode { get; set; }

        private ulong? _seed = null;
        private ulong seed => _seed ??= Type.UniqueKey64();

        public IInstant Create()
        {
            if (this.Type == null)
            {
                try
                {
                    switch (mode)
                    {
                        case InstantType.Reference:
                            compileInstantType(
                                new InstantCompilerReferenceTypes(this, rubricBuilders)
                            );
                            break;
                        case InstantType.ValueType:
                            compileInstantType(new InstantCompilerValueTypes(this, rubricBuilders));
                            break;
                        case InstantType.Derived:
                            compileDerivedType(
                                new InstantCompilerDerivedTypes(this, rubricBuilders)
                            );
                            break;
                        default:
                            break;
                    }

                    Rubrics.Update();
                }
                catch (Exception ex)
                {
                    throw new InstantTypeCompilerException(
                        "Instant compilation at runtime failed see inner exception",
                        ex
                    );
                }
            }
            return create();
        }

        public object New()
        {
            if (this.Type == null)
                return Create();
            return this.Type.New();
        }

        private IInstant create()
        {
            if (this.Type == null)
                return Create();

            var figure = (IInstant)this.Type.New();
            figure.Key = Unique.NewKey;
            figure.TypeKey = seed;
            return figure;
        }

        private void compileDerivedType(InstantCompiler compiler)
        {
            var fcdt = compiler;
            compiledType = fcdt.CompileInstantType(Name);
            Rubrics.KeyRubrics.Add(fcdt.Identities.Values);
            Type = compiledType.New().GetType();

            if (!(Rubrics.AsValues().Any(m => m.Name == "Code")))
            {
                var f = this.Type.GetField("code", BindingFlags.NonPublic | BindingFlags.Instance);

                if (!Rubrics.TryGet("code", out MemberRubric mr))
                {
                    mr = new MemberRubric(f);
                    mr.FigureField = f;
                    Rubrics.Insert(0, mr);
                }
                mr.RubricName = "Code";
            }
        }

        private void compileInstantType(InstantCompiler compiler)
        {
            var fcvt = compiler;
            compiledType = fcvt.CompileInstantType(Name);
            Rubrics.KeyRubrics.Add(fcvt.Identities.Values);
            Type = compiledType.New().GetType();
            Size = Marshal.SizeOf(Type);
        }

        private MemberRubric[] createMemberRurics(IList<MemberInfo> membersInfo)
        {
            return membersInfo
                .Select(
                    m =>
                        !(m is MemberRubric rubric)
                            ? m.MemberType == MemberTypes.Field
                                ? new MemberRubric((FieldInfo)m)
                                : m.MemberType == MemberTypes.Property
                                    ? new MemberRubric((PropertyInfo)m)
                                    : null
                            : rubric
                )
                .Where(p => p != null)
                .ToArray();
        }
    }

    public class InstantTypeCompilerException : Exception
    {
        public InstantTypeCompilerException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
