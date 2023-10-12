namespace Radical.Instant.Updating
{
    using Invoking;
    using System.Text.Json;

    public static class UpdaterExtensions
    {
        public static E PatchTo<T, E>(this T item, E target, IInvoker traceChanges = null)
            where T : class
            where E : class
        {
            return new Updater<T>(item, traceChanges).Patch(target);
        }

        public static object PatchTo(this object item, object target, IInvoker traceChanges = null)
        {
            return new Updater(item, traceChanges).Patch(target);
        }

        public static T PatchFrom<T, E>(this T item, E source, IInvoker traceChanges = null)
           where T : class
           where E : class
        {
            return new Updater<E>(source, traceChanges).Patch(item);
        }

        public static object PatchFrom(this object item, object source, IInvoker traceChanges = null)
        {
            return new Updater(source, traceChanges).Patch(item);
        }

        public static E PatchTo<T, E>(this T item, IInvoker traceChanges = null)
            where T : class
            where E : class
        {
            return new Updater<T>(item, traceChanges).Patch<E>();
        }

        public static object PatchSelf(this object item, IInvoker traceChanges = null)
        {
            return new Updater(item, traceChanges).PatchSelf();
        }

        public static E PutTo<T, E>(this T item, E target, IInvoker traceChanges = null)
            where T : class
            where E : class
        {
            return new Updater<T>(item, traceChanges).Put(target);
        }

        public static object PutTo(this object item, object target, IInvoker traceChanges = null)
        {
            return new Updater(item, traceChanges).Put(target);
        }

        public static E PutTo<T, E>(this T item, IInvoker traceChanges = null)
            where T : class
            where E : class
        {
            return new Updater<T>(item, traceChanges).Put<E>();
        }

        public static T PutFrom<T, E>(this T item, E source, IInvoker traceChanges = null)
         where T : class
         where E : class
        {
            return new Updater<E>(source, traceChanges).Put(item);
        }

        public static object PutFrom(this object item, object source, IInvoker traceChanges = null)
        {
            return new Updater(source, traceChanges).Put(item);
        }

        public static E PatchFromJson<T, E>(this E obj, string str) where T : class where E : class
        {
            return str.FromJson<T>().PatchTo<T, E>(obj);
        }

        public static E PutFromJson<T, E>(this E obj, string str) where T : class where E : class
        {
            return str.FromJson<T>().PutTo<T, E>(obj);
        }

        public static E PatchFromJson<T, E>(this E obj, byte[] bytes) where T : class where E : class
        {
            return bytes.FromJson<T>().PatchTo<T, E>(obj);
        }

        public static E PutFromJson<T, E>(this E obj, byte[] bytes) where T : class where E : class
        {
            return bytes.FromJson<T>().PutTo<T, E>(obj);
        }

        public static E PatchFromJson<T, E>(this E obj, Stream str) where T : class where E : class
        {
            return str.FromJson<T>().PatchTo<T, E>(obj);
        }

        public static E PutFromJson<T, E>(this E obj, Stream str) where T : class where E : class
        {
            return str.FromJson<T>().PutTo<T, E>(obj);
        }
    }
}
