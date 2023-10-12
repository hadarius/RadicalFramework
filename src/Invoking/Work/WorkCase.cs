namespace Radical.Invoking.Work
{
    using System.Collections.Generic;
    using System.Linq;
    using Invoking;
    using Uniques;
    using Notes;

    public class WorkCase<TRule> : Case where TRule : class
    {
        public WorkCase() : base(new WorkAspects(typeof(TRule).FullName)) { }

        public WorkAspect<TAspect> Aspect<TAspect>() where TAspect : class
        {
            if (!TryGet(typeof(TAspect).FullName, out WorkAspect aspect))
            {
                aspect = new WorkAspect<TAspect>();
                Add(aspect);
            }
            return aspect as WorkAspect<TAspect>;
        }
    }

    public class Case : WorkAspects
    {
        public Case(IEnumerable<IInvoker> methods, WorkAspects @case = null)
            : base(
                (@case == null) ? $"Case_{Unique.NewKey}" : @case.Name,
                (@case == null) ? new WorkNotes() : @case.Notes
            )
        {
            Add($"Aspect_{Unique.NewKey}", methods);
            Open();
        }

        public Case(WorkAspects @case) : base(@case.Name, @case.Notes)
        {
            Add(@case.AsValues());
        }

        public Case() : base($"Case_{Unique.NewKey}", new WorkNotes()) { }

        public WorkAspect Aspect(IInvoker method, WorkAspect aspect)
        {
            if (aspect != null)
            {
                if (!TryGet(aspect.Name, out WorkAspect _aspect))
                {
                    Add(_aspect);
                    _aspect.AddWork(method);
                }
                return aspect;
            }
            return null;
        }

        public WorkAspect Aspect(IInvoker method, string name)
        {
            if (!TryGet(name, out WorkAspect aspect))
            {
                aspect = new WorkAspect(name);
                Add(aspect);
                aspect.AddWork(method);
            }
            return aspect;
        }

        public WorkAspect Aspect(string name)
        {
            if (!TryGet(name, out WorkAspect aspect))
            {
                aspect = new WorkAspect(name);
                Add(aspect);
            }
            return aspect;
        }

        public void Open()
        {
            Setup();
        }

        public void Setup()
        {
            foreach (WorkAspect aspect in AsValues())
            {
                if (aspect.Workator == null)
                {
                    aspect.Workator = new Workspace(aspect);
                }
                if (!aspect.Workator.Ready)
                {
                    aspect.Allocate();
                }
            }
        }

        public void Run(string laborName, params object[] input)
        {
            WorkItem[] labors = AsValues()
                .Where(m => m.ContainsKey(laborName))
                .SelectMany(w => w.AsValues())
                .ToArray();

            foreach (WorkItem labor in labors)
                labor.Invoke(input);
        }

        public void Run(IDictionary<string, object[]> laborsAndInputs)
        {
            foreach (KeyValuePair<string, object[]> worker in laborsAndInputs)
            {
                object input = worker.Value;
                string workerName = worker.Key;
                WorkItem[] workerWorks = AsValues()
                    .Where(m => m.ContainsKey(workerName))
                    .SelectMany(w => w.AsValues())
                    .ToArray();

                foreach (WorkItem objc in workerWorks)
                    objc.Execute(input);
            }
        }
    }

    public class InvalidWorkException : Exception
    {
        #region Constructors


        public InvalidWorkException(string message) : base(message) { }

        #endregion
    }
}
