namespace Radical.Instant.Series
{
    using Radical.Extracting;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Radical.Series;
    using Radical.Series.Base;
    using Radical.Uniques;
    using Rubrics;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class InstantSeriesItem : SeriesItemBase<IInstant>, IInstant, IEquatable<IInstant>, IComparable<IInstant>
    {
        private ISeries<object> presets;

        public InstantSeriesItem(IInstantSeries series)
        {
            Figures = series;
        }

        public InstantSeriesItem(object key, IInstant value, IInstantSeries series) : base(key, value)
        {
            Figures = series;
        }

        public InstantSeriesItem(ulong key, IInstant value, IInstantSeries series) : base(key, value)
        {
            Figures = series;
        }

        public InstantSeriesItem(IInstant value, IInstantSeries series) : base(value)
        {
            Figures = series;
            CompactKey();
        }

        public InstantSeriesItem(ISeriesItem<IInstant> value, IInstantSeries series) : base(value)
        {
            Figures = series;
            CompactKey();
        }

        public object this[int fieldId]
        {
            get => GetPreset(fieldId);
            set => SetPreset(fieldId, value);
        }
        public object this[string propertyName]
        {
            get => GetPreset(propertyName);
            set => SetPreset(propertyName, value);
        }

        public override void Set(object key, IInstant value)
        {
            this.value = value;
            this.value.Key = key.UniqueKey();
        }

        public override void Set(IInstant value)
        {
            this.value = value;
        }

        public override void Set(ISeriesItem<IInstant> item)
        {
            this.value = item.Value;
        }

        public override bool Equals(ulong key)
        {
            return Key == key;
        }

        public override bool Equals(object y)
        {
            return Key.Equals(y.UniqueKey());
        }

        public bool Equals(IInstant other)
        {
            return Key == other.Key;
        }

        public override int GetHashCode()
        {
            return Value.GetKeyBytes().BitAggregate64to32().ToInt32();
        }

        public override int CompareTo(object other)
        {
            return (int)(Key - other.UniqueKey64());
        }

        public override int CompareTo(ulong key)
        {
            return (int)(Key - key);
        }

        public override int CompareTo(ISeriesItem<IInstant> other)
        {
            return (int)(Key - other.Key);
        }

        public int CompareTo(IInstant other)
        {
            return (int)(Key - other.Key);
        }

        public override byte[] GetBytes()
        {
            if (!Figures.Prime && presets != null)
            {
                IInstant f = Figures.NewInstant();
                f.ValueArray = ValueArray;
                f.Code = value.Code;
                byte[] ba = f.GetBytes();
                f = null;
                return ba;
            }
            else
                return value.GetBytes();
        }

        public unsafe override byte[] GetKeyBytes()
        {
            return value.GetKeyBytes();
        }

        public override int[] UniqueOrdinals()
        {
            return Figures.KeyRubrics.Ordinals;
        }

        public override object[] UniqueValues()
        {
            int[] ordinals = UniqueOrdinals();
            if (ordinals != null)
                return ordinals.Select(x => value[x]).ToArray();
            return null;
        }

        public override ulong CompactKey()
        {
            ulong key = value.Key;
            if (key == 0)
            {
                IRubrics r = Figures.KeyRubrics;
                var objs = r.Ordinals.Select(x => value[x]).ToArray();
                key = objs.Any() ? objs.UniqueKey64(r.BinarySizes, r.BinarySize) : Unique.NewKey;
                value.Key = key;
            }
            return key;
        }

        public override ulong Key
        {
            get => value.Key;
            set => this.value.Key = value;
        }       

        public object[] ValueArray
        {
            get
            {
                if (Figures.Prime || presets == null)
                    return value.ValueArray;
                object[] valarr = value.ValueArray;
                presets.AsItems().Select(x => valarr[x.Key] = x.Value).ToArray();
                return valarr;
            }
            set
            {
                int l = value.Length;
                for (int i = 0; i < l; i++)
                    SetPreset(i, value[i]);
            }
        }

        public Uscn Code
        {
            get => value.Code;
            set => this.value.Code = value;
        }

        public IInstantSeries Figures { get; set; }

        public object GetPreset(int fieldId)
        {
            if (presets != null && !Figures.Prime)
            {
                object val = presets.Get(fieldId);
                if (val != null)
                    return val;
            }
            return value[fieldId];
        }

        public object GetPreset(string propertyName)
        {
            if (presets != null && !Figures.Prime)
            {
                MemberRubric rubric = Figures.Rubrics[propertyName.UniqueKey()];
                if (rubric != null)
                {
                    object val = presets.Get(rubric.FieldId);
                    if (val != null)
                        return val;
                }
                else
                    throw new IndexOutOfRangeException("Field doesn't exist");
            }
            return value[propertyName];
        }

        public ISeriesItem<object>[] GetPresets()
        {
            return presets.AsItems().ToArray();
        }

        public void SetPreset(int fieldId, object value)
        {
            if (GetPreset(fieldId).Equals(value))
                return;
            if (!Figures.Prime)
            {
                if (presets == null)
                    presets = new Catalog<object>(9);
                presets.Put(fieldId, value);
            }
            else
                this.value[fieldId] = value;
        }

        public void SetPreset(string propertyName, object value)
        {
            MemberRubric rubric = Figures.Rubrics[propertyName.UniqueKey()];
            if (rubric != null)
                SetPreset(rubric.FieldId, value);
            else
                throw new IndexOutOfRangeException("Field doesn't exist");
        }

        public void WritePresets()
        {
            foreach (var c in presets.AsItems())
                value[(int)c.Key] = c.Value;
            presets = null;
        }

        public bool HavePresets => presets != null ? true : false;
    }
}
