using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radical.Instant.Series.Stocking
{
    public interface IStocker
    {
        public object this[int index] { get; set; }
        public object this[int index, int field, Type type] { get; set; }

        void Write();
        void Read();
        void Open();
        void Close();
    }
}
