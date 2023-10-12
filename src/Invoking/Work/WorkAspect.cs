namespace Radical.Invoking.Work
{
    using System.Collections.Generic;
    using System.Linq;
    using Series;

    public class WorkAspect : Registry<WorkItem>, IWorkspace
    {
        public WorkAspects Case { get; set; }

        public WorkAspect(string Name)
        {
            this.Name = Name;
            WorkersCount = 1;
        }

        public WorkAspect(string Name, IEnumerable<WorkItem> WorkList) : this(Name)
        {
            foreach (WorkItem labor in WorkList)
            {
                labor.Case = Case;
                labor.Aspect = this;
                Put(labor);
            }
        }

        public WorkAspect(string Name, IEnumerable<IInvoker> MethodList)
            : this(Name, MethodList.Select(m => new WorkItem(m))) { }

        public int WorkersCount { get; set; }

        public Workspace Workator { get; set; }

        public string Name { get; set; }

        public override WorkItem Get(object key)
        {
            TryGet(key, out WorkItem result);
            return result;
        }

        public WorkItem AddWork(WorkItem labor)
        {
            labor.Case = Case;
            labor.Aspect = this;
            Put(labor);
            return labor;
        }

        public WorkItem AddWork(IInvoker deputy)
        {
            WorkItem labor = new WorkItem(deputy);
            labor.Case = Case;
            labor.Aspect = this;
            Put(labor);
            return labor;
        }

        public WorkAspect AddWork(IEnumerable<WorkItem> labors)
        {
            foreach (WorkItem labor in labors)
            {
                labor.Case = Case;
                labor.Aspect = this;
                Put(labor);
            }
            return this;
        }

        public WorkAspect AddWork(IEnumerable<IInvoker> methods)
        {
            foreach (IInvoker method in methods)
            {
                WorkItem labor = new WorkItem(method);
                labor.Case = Case;
                labor.Aspect = this;
                Put(labor);
            }
            return this;
        }

        public virtual WorkAspect AddWork<T>() where T : class
        {
            var deputy = new Invoker<T>();
            AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            return this;
        }

        public virtual WorkAspect AddWork<T>(Type[] arguments) where T : class
        {
            var deputy = new Invoker<T>(arguments);
            AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            return this;
        }

        public virtual WorkAspect AddWork<T>(params object[] consrtuctorParams) where T : class
        {
            var deputy = new Invoker<T>(consrtuctorParams);
            AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            return this;
        }

        public virtual WorkAspect AddWork<T>(Type[] arguments, params object[] consrtuctorParams)
            where T : class
        {
            var deputy = new Invoker<T>(arguments, consrtuctorParams);
            AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            return this;
        }

        public virtual WorkAspect AddWork<T>(Func<T, string> method) where T : class
        {
            var deputy = new Invoker<T>(method);
            AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            return this;
        }

        public virtual WorkAspect AddWork<T>(Func<T, string> method, params Type[] arguments)
            where T : class
        {
            var deputy = new Invoker<T>(method, arguments);
            AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            return this;
        }

        public virtual WorkAspect AddWork<T>(Func<T, string> method, params object[] consrtuctorParams)
            where T : class
        {
            var deputy = new Invoker<T>(method, consrtuctorParams);
            AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            return this;
        }

        public virtual WorkItem Work<T>() where T : class
        {
            if (!TryGet(Invoker.GetName<T>(), out WorkItem labor))
            {
                var deputy = new Invoker<T>();
                return AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            }
            return labor;
        }

        public virtual WorkItem Work<T>(Type[] arguments) where T : class
        {
            if (!TryGet(Invoker.GetName<T>(arguments), out WorkItem labor))
            {
                var deputy = new Invoker<T>();
                return AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            }
            return labor;
        }

        public virtual WorkItem Work<T>(params object[] consrtuctorParams) where T : class
        {
            if (!TryGet(Invoker.GetName<T>(), out WorkItem labor))
            {
                var deputy = new Invoker<T>(consrtuctorParams);
                return AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            }
            return labor;
        }

        public virtual WorkItem Work<T>(Type[] arguments, params object[] constructorParams)
            where T : class
        {
            if (!TryGet(Invoker.GetName<T>(arguments), out WorkItem labor))
            {
                var deputy = new Invoker<T>(arguments, constructorParams);
                return AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            }
            return labor;
        }

        public virtual WorkItem Work<T>(Func<T, string> method) where T : class
        {
            var deputy = new Invoker<T>(method);
            if (!TryGet(deputy.Name, out WorkItem labor))
            {
                return AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            }
            return labor;
        }

        public virtual WorkItem Work<T>(Func<T, string> method, params Type[] arguments)
            where T : class
        {
            var deputy = new Invoker<T>(method, arguments);
            if (!TryGet(deputy.Name, out WorkItem labor))
            {
                return AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            }
            return labor;
        }

        public virtual WorkItem Work<T>(Func<T, string> method, params object[] consrtuctorParams)
            where T : class
        {
            var deputy = new Invoker<T>(method, consrtuctorParams);
            if (!TryGet(deputy.Name, out WorkItem labor))
            {
                return AddWork(Case.Methods.EnsureGet(deputy, k => deputy).Value);
            }
            return labor;
        }

        public override WorkItem this[object key]
        {
            get => base[key];
            set
            {
                value.Case = Case;
                value.Aspect = this;
                base[key] = value;
            }
        }

        public void Close(bool SafeClose)
        {
            Workator.Close(SafeClose);
        }

        public WorkAspect Allocate(int workersCount = 1)
        {
            Workator.Allocate(workersCount);
            return this;
        }

        public void Run(WorkItem labor)
        {
            Workator.Run(labor);
        }

        public void Reset(int workersCount = 1)
        {
            Workator.Reset(workersCount);
        }
    }
}
