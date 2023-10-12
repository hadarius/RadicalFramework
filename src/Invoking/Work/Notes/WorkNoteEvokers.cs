namespace Radical.Invoking.Work.Notes
{
    using System.Collections.Generic;
    using System.Linq;
    using Series;

    public class WorkNoteEvokers : Registry<WorkNoteEvoker>
    {
        public bool Contains(IEnumerable<WorkItem> objectives)
        {
            return this.AsValues()
                .Any(t => t.RelatedWorks.Any(ro => objectives.All(o => ReferenceEquals(ro, o))));
        }

        public bool Contains(IEnumerable<string> relayNames)
        {
            return this.AsValues().Any(t => t.RelatedWorkNames.SequenceEqual(relayNames));
        }

        public WorkNoteEvoker this[string relatedWorkName]
        {
            get
            {
                return this.AsValues()
                    .FirstOrDefault(c => c.RelatedWorkNames.Contains(relatedWorkName));
            }
        }
        public WorkNoteEvoker this[WorkItem relatedWork]
        {
            get
            {
                return this.AsValues().FirstOrDefault(c => c.RelatedWorks.Contains(relatedWork));
            }
        }
    }
}
