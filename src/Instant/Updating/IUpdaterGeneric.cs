namespace Radical.Instant.Updating
{
    public interface IUpdater<T> : IUpdater
    {
        T Devisor { get; }

        new T Clone();

        new T PatchSelf();
        T Patch(T item);

        new T PutSelf();
        T Put(T item);

        UpdaterItem[] Detect(T item);
    }
}
