using System.Collections;
using System.Collections.Generic;

namespace Radical.Series
{
    public interface IFindableSeries<V> : IFindableSeries, IEnumerable<V>, IList<V>
    {
        new V this[object key] { get; set; }
    }

    public interface IFindableSeries : IEnumerable
    {
        object this[object key] { get; set; }
    }
}
