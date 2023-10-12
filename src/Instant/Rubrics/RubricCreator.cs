namespace Radical.Instant.Rubrics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Radical.Series;

    public class RubricBuilder
    {
        public ISeries<RubricModel> Create(Type modelType)
        {
            Registry<RubricModel> rubricBuilders = new Registry<RubricModel>();

            var memberRubrics = PrepareMembers(GetMembers(modelType));

            return Create(memberRubrics);
        }

        public ISeries<RubricModel> Create(MemberRubric[] memberRubrics)
        {
            Registry<RubricModel> rubricBuilders = new Registry<RubricModel>();

            memberRubrics.ForEach(
                (member) =>
                {
                    if (!rubricBuilders.TryGet(member.RubricName, out RubricModel fieldProperty))
                        rubricBuilders.Put(new RubricModel(member));
                    else
                        fieldProperty.SetMember(member);
                }
            );
            int order = 0;
            rubricBuilders.ForEach(
                (fp) =>
                {
                    order++;
                    fp.Member.RubricId = order;
                    fp.Member.FieldId = order;
                }
            );

            return rubricBuilders;
        }

        public MemberRubric[] PrepareMembers(IEnumerable<MemberInfo> membersInfo)
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

        public MemberInfo[] GetMembers(Type modelType)
        {
            return modelType
                .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(
                    m =>
                        m.Name != "SeriesItem"
                        && m.Name != "ValueArray"
                        && (
                            m.MemberType == MemberTypes.Field
                            || (
                                m.MemberType == MemberTypes.Property
                                && ((PropertyInfo)m).CanRead
                                && ((PropertyInfo)m).CanWrite
                            )
                        )
                )
                .ToArray();
        }
    }
}
