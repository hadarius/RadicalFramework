using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Radical.Uniques;

namespace Radical.Uniques
{
    public interface IUniqueOne<T>
    {
        IQueryable<T> Queryable { get; }
    }
}
