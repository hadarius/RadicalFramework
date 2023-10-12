namespace Radical.Invoking.Work
{
    public class Workflow<TCase> : Case where TCase : class
    {
        public Workflow() : base(new WorkAspects(typeof(TCase).FullName)) { }

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
}
