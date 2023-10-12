namespace Radical.Linking
{
    using Uniques;
    using Series;

    public static class Linker
    {
        public static TypedCache<IUnique> Links = new TypedCache<IUnique>();

        public static Registry<Type> Types = new Registry<Type>();

    }
}
