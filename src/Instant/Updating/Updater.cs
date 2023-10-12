using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Radical.Series;
using Radical.Uniques;

namespace Radical.Instant.Updating;

using Invoking;
using Proxies;
using Instant.Rubrics;

public class Updater : IUpdater
{
    protected HashSet<int> trackset;
    protected ProxyCreator creator;
    protected IProxy entry;
    protected IProxy preset;
    protected Type type => creator.BaseType;
    protected int patchNotEqualTypeCount = 0;
    protected int otherMutationsCount = 0;
    protected int patchEqualTypeCount = 0;
    protected int sameMutationsCount = 0;
    protected bool traceable;

    public IProxy Entry
    {
        get => entry;
        set => entry = value;
    }

    public IProxy Preset
    {
        get => ((preset == null) ? preset = creator.Create(entry) : preset);
        set => preset = value;
    }

    public Action<Updater, object> ReferenceUpdater { get; set; }

    public IInvoker TraceEvent { get; set; }

    public Updater() { } 

    public Updater(object item, IInvoker traceChanges) : this(item.GetType())
    {
        if (traceChanges != null)
        {
            TraceEvent = traceChanges;
            traceable = true;
        }
        if (item.GetType().IsAssignableTo(typeof(IProxy)))
            Combine(item as IProxy);
        else
            Combine(item);
    }

    public Updater(object item) : this(item.GetType())
    {
        if (item.GetType().IsAssignableTo(typeof(IProxy)))
            Combine(item as IProxy);
        else
            Combine(item);
    }

    public Updater(IProxy sleeve) : this(sleeve.Target.GetType())
    {
        Combine(sleeve);
    }

    public Updater(Type type)
    {
        creator = ProxyFactory.GetCreator(type);
    }

    public void Combine(IProxy sleeve)
    {
        entry = sleeve;
    }

    public void Combine(object item)
    {
        entry = creator.Create(item);
    }

    protected void setDeltaMarks()
    {
        foreach (var rubric in Rubrics)
        {
            if (rubric.DeltaMark != null)
                preset[rubric.RubricId] = rubric.DeltaMark;
        }
    }

    protected void setBy(IProxy target, UpdaterItem[] changes, int count)
    {
        var _target = target;
        var _changes = changes;
        UpdaterItem vary;
        for (int i = 0; i < count; i++)
        {
            vary = _changes[i];
            if (vary.TargetType.IsAssignableTo(vary.OriginType))
            {
                _target[vary.TargetIndex] = vary.OriginValue;
            }
        }
    }

    protected void set(IProxy target, UpdaterItem[] changes, int count)
    {
        var _target = target;
        var _changes = changes;
        UpdaterItem vary;
        for (int i = 0; i < count; i++)
        {
            vary = _changes[i];
            _target[vary.TargetIndex] = vary.OriginValue;
        }
    }

    public object Patch(object item)
    {
        UpdaterItem[] changes;

        ReferenceUpdater = (o, t) => o.Patch(t);

        IProxy target = item.ToProxy();
        if (item.GetType() != type)
            setBy(target, changes = PatchNotEqualTypes(target), patchNotEqualTypeCount);
        else
            set(target, changes = PatchEqualTypes(target), patchEqualTypeCount);

        return item;
    }

    public E Patch<E>(E item) where E : class
    {
        UpdaterItem[] changes;

        ReferenceUpdater = (o, t) => o.Patch(t);

        IProxy target = item.ToProxy();
        if (typeof(E) != type)
            setBy(target, changes = PatchNotEqualTypes(target), patchNotEqualTypeCount);
        else
            set(target, changes = PatchEqualTypes(target), patchEqualTypeCount);

        return item;
    }

    public E Patch<E>() where E : class
    {
        return Patch(typeof(E).New<E>());
    }

    public object PatchSelf()
    {
        ReferenceUpdater = (o, s) => o.PatchSelf();

        set(entry, PatchEqualTypes(entry), patchEqualTypeCount);
        return Devisor;
    }

    public object Put(object item)
    {
        UpdaterItem[] changes;

        ReferenceUpdater = (o, t) => o.Put(t);

        IProxy target = item.ToProxy();
        if (item.GetType() != type)
            setBy(target, changes = putEqualTypes(target), otherMutationsCount);
        else
            set(target, changes = putNotEqualTypes(target), sameMutationsCount);

        return item;
    }

    public E Put<E>(E item) where E : class
    {
        UpdaterItem[] changes;

        ReferenceUpdater = (o, t) => o.Put(t);

        IProxy target = item.ToProxy();
        if (typeof(E) != type)
            setBy(target, changes = putEqualTypes(target), otherMutationsCount);
        else
            set(target, changes = putNotEqualTypes(target), sameMutationsCount);

        return item;
    }

    public E Put<E>() where E : class
    {
        return Put(typeof(E).New<E>());
    }

    public object PutSelf()
    {
        ReferenceUpdater = (c, h) => c.PutSelf();

        set(entry, putNotEqualTypes(entry), sameMutationsCount);
        return Devisor;
    }

    public UpdaterItem[] Detect(object item)
    {
        UpdaterItem[] changes;

        ReferenceUpdater = (o, t) => o.Detect(t);

        IProxy target = item.ToProxy();
        if (item.GetType() != type)
            changes = PatchNotEqualTypes(target);
        else
            changes = PatchEqualTypes(target);

        return changes;
    }

    public UpdaterItem[] Detect<E>(E item) where E : class
    {
        UpdaterItem[] changes;

        ReferenceUpdater = (o, t) => o.Detect(t);

        IProxy target = item.ToProxy();
        if (typeof(E) != type)
            changes = PatchNotEqualTypes(target);
        else
            changes = PatchEqualTypes(target);

        return changes;
    }

    public void MapPreset()
    {
        var presetShell = creator.Create(preset);
        Preset.ValueArray = presetShell.ValueArray;
    }

    public void MapEntry()
    {
        var presetShell = creator.Create(Preset);
        presetShell.ValueArray = preset.ValueArray;
    }

    public void MapDevisor()
    {
        Preset.ValueArray = entry.ValueArray;
    }

    public void SafeMapPreset()
    {
        var presetShell = creator.Create(preset);

        presetShell.Rubrics
            .Select(
                v =>
                    preset.Rubrics.ContainsKey(v.RubricName)
                        ? preset[v.RubricName] = presetShell[v.RubricId]
                        : null
            )
            .ToArray();
    }

    public void SafeMapEntry()
    {
        var presetShell = creator.Create(preset);

        preset.Rubrics
            .Select(
                v =>
                    presetShell.Rubrics.ContainsKey(v.RubricName)
                        ? presetShell[v.RubricName] = preset[v.RubricId]
                        : null
            )
            .ToArray();
    }

    public void SafeMapDevisor()
    {
        Rubrics
            .Select(
                v =>
                    preset.Rubrics.ContainsKey(v.RubricName)
                        ? preset[v.RubricName] = entry[v.RubricId]
                        : null
            )
            .ToArray();
    }

    public object Clone()
    {
        var clone = type.New();
        var _clone = creator.Create(clone);
        _clone.ValueArray = entry.ValueArray;
        return clone;
    }

    protected UpdaterItem[] PatchEqualTypes(IProxy target)
    {
        patchEqualTypeCount = 0;
        var _target = target;
        var _sameVariations = new UpdaterItem[Rubrics.Count];
        UpdaterItem vary;

        Rubrics.ForEach(
            (rubric) =>
            {
                if (!rubric.IsKey && !ExcludedRubrics.ContainsKey(rubric))
                {
                    var targetndex = rubric.RubricId;
                    var originValue = Entry[targetndex];
                    var targetValue = _target[targetndex];

                    if (
                        !originValue.NullOrEquals(rubric.DeltaMark)
                        && !originValue.Equals(targetValue)
                    )
                    {
                        if (
                            !RecursiveUpdate(originValue, targetValue, target, rubric, rubric)
                        )
                        {
                            vary = _sameVariations[patchEqualTypeCount++];
                            vary.TargetIndex = targetndex;
                            vary.OriginValue = originValue;
                        }
                    }
                }
            }
        );
        return _sameVariations;
    }

    protected UpdaterItem[] PatchNotEqualTypes(IProxy target)
    {
        patchNotEqualTypeCount = 0;
        var _target = target;
        var _customVariations = new UpdaterItem[Rubrics.Count];
        UpdaterItem vary;

        Rubrics.ForEach(
            (originRubric) =>
            {
                if (!originRubric.IsKey && !ExcludedRubrics.ContainsKey(originRubric))
                {
                    var name = originRubric.Name;
                    if (_target.Rubrics.TryGet(name, out MemberRubric targetRubric))
                    {
                        var originValue = Entry[originRubric.RubricId];
                        var targetIndex = targetRubric.RubricId;
                        var targetValue = _target[targetIndex];

                        if (
                            !originValue.NullOrEquals(originRubric.DeltaMark)
                            && !originValue.Equals(targetValue)
                        )
                        {
                            if (
                                !RecursiveUpdate(
                                    originValue,
                                    targetValue,
                                    target,
                                    originRubric,
                                    targetRubric
                                )
                            )
                            {
                                vary = _customVariations[patchNotEqualTypeCount++];
                                vary.TargetIndex = targetIndex;
                                vary.OriginValue = originValue;
                                vary.OriginType = originRubric.RubricType;
                                vary.TargetType = targetRubric.RubricType;
                            }
                        }
                    }
                }
            }
        );
        return _customVariations;
    }

    protected UpdaterItem[] putNotEqualTypes(IProxy target)
    {
        sameMutationsCount = 0;
        var _target = target;
        var _sameMutations = new UpdaterItem[Rubrics.Count];
        UpdaterItem vary;

        Rubrics.ForEach(
            (rubric) =>
            {
                if (!rubric.IsKey && !ExcludedRubrics.ContainsKey(rubric))
                {
                    var index = rubric.RubricId;
                    var originValue = Entry[index];
                    var targetValue = _target[index];

                    if (
                        originValue != null
                        && !RecursiveUpdate(originValue, targetValue, target, rubric, rubric)
                    )
                    {
                        vary = _sameMutations[sameMutationsCount++];
                        vary.TargetIndex = index;
                        vary.OriginValue = originValue;
                    }
                }
            }
        );
        return _sameMutations;
    }

    protected UpdaterItem[] putEqualTypes(IProxy target)
    {
        otherMutationsCount = 0;
        var _target = target;
        var _customMutations = new UpdaterItem[Rubrics.Count];
        UpdaterItem item;

        Rubrics.ForEach(
            (originRubric) =>
            {
                if (!originRubric.IsKey && !ExcludedRubrics.ContainsKey(originRubric))
                {
                    var name = originRubric.Name;
                    if (_target.Rubrics.TryGet(name, out MemberRubric targetRubric))
                    {
                        var originValue = Entry[originRubric.RubricId];
                        var targetIndex = targetRubric.RubricId;
                        var targetValue = _target[targetIndex];

                        if (
                            originValue != null
                            && !RecursiveUpdate(
                                originValue,
                                targetValue,
                                target,
                                originRubric,
                                targetRubric
                            )
                        )
                        {
                            item = _customMutations[otherMutationsCount++];
                            item.TargetIndex = targetIndex;
                            item.OriginValue = originValue;
                            item.OriginType = originRubric.RubricType;
                            item.TargetType = targetRubric.RubricType;
                        }
                    }
                }
            }
        );
        return _customMutations;
    }

    private bool RecursiveUpdate(
        object originValue,
        object targetValue,
        IProxy target,
        IRubric originRubric,
        IRubric targetRubric
    )
    {
        var originType = originRubric.RubricType;
        var targetType = targetRubric.RubricType;

        if (originType.IsValueType || (originType == typeof(string)))
            return false;

        if (targetValue == null)
        {
            if (traceable)
                targetValue = TraceEvent.Invoke(
                    target.Target,
                    targetRubric.RubricName,
                    targetType
                );

            if (targetValue == null)
                target[targetRubric.RubricId] = targetValue = targetType.New();
        }

        if (originType.IsAssignableTo(typeof(ICollection)))
        {
            ICollection originItems = (ICollection)originValue;
            var originItemType = originType.GetEnumerableElementType();
            if (originItemType == null || !originItemType.IsValueType)
            {
                if (targetType.IsAssignableTo(typeof(ICollection)))
                {
                    ICollection targetItems = (ICollection)targetValue;
                    var targetItemType = targetType.GetEnumerableElementType();
                    if (targetItemType == null || !targetItemType.IsValueType)
                    {
                        if (
                            targetType.IsAssignableTo(typeof(IFindableSeries))
                            && originItemType.IsAssignableTo(typeof(IUnique))
                        )
                        {
                            IFindableSeries targetFindable = (IFindableSeries)targetValue;

                            foreach (var originItem in originItems)
                            {
                                var targetItem = targetFindable[originItem];
                                if (targetItem != null)
                                {
                                    if (traceable)
                                        targetItem = TraceEvent.Invoke(targetItem, null, null);
                                    ReferenceUpdater(
                                        new Updater(originItem, TraceEvent),
                                        targetItem
                                    );
                                }
                                else if (originItemType != targetItemType)
                                {
                                    targetItem = targetItemType.New();
                                    ((IUnique)targetItem).Key = ((IUnique)originItem).Key;
                                    originItem.PatchTo(targetItem, TraceEvent);
                                    if (traceable)
                                        targetItem = TraceEvent.Invoke(targetItem, null, null);
                                    ((IList)targetItems).Add(targetItem);
                                }
                                else
                                {
                                    if (traceable)
                                        targetItem = TraceEvent.Invoke(originItem, null, null);
                                    ((IList)targetItems).Add(originItem);
                                }
                            }

                            return true;
                        }

                        GreedyLookup(
                            originItems,
                            targetItems,
                            originItemType,
                            targetItemType
                        );
                    }
                }
            }
        }

        if (traceable)
            targetValue = TraceEvent.Invoke(targetValue, null, null);

        ReferenceUpdater(new Updater(originValue, TraceEvent), targetValue);

        return false;
    }

    private bool GreedyLookup(
        ICollection originItems,
        ICollection targetItems,
        Type originItemType,
        Type targetItemType
    )
    {
        if (
            !originItemType.IsAssignableTo(typeof(IUnique))
            || !targetItemType.IsAssignableTo(typeof(IUnique))
        )
            return false;

        foreach (var originItem in originItems)
        {
            bool founded = false;
            foreach (var targetItem in targetItems)
            {
                if (((IUnique)originItem).Equals((IUnique)targetItem))
                {
                    var _targetItem = targetItem;
                    if (traceable)
                        _targetItem = TraceEvent.Invoke(_targetItem, null, null);

                    ReferenceUpdater(new Updater(originItem, TraceEvent), _targetItem);
                    
                    founded = true;
                    break;
                }
            }

            if (!founded)
            {
                object targetItem = null;
                if (originItemType != targetItemType)
                {
                    targetItem = targetItemType.New();
                    ((IUnique)targetItem).Key = ((IUnique)originItem).Key;
                    if (traceable)
                        targetItem = TraceEvent.Invoke(targetItem, null, null);
                    ((IList)targetItems).Add(originItem.PatchTo(targetItem, TraceEvent));
                }
                else
                {
                    if (traceable)
                        targetItem = TraceEvent.Invoke(originItem, null, null);
                    ((IList)targetItems).Add(originItem);
                }
            }
        }

        return true;
    }

    private static IRubrics excludedRubrics;

    public IRubrics ExcludedRubrics
    {
        get =>
            excludedRubrics ??= new MemberRubrics(
                new MemberRubric[]
                {
                    Rubrics["TypeKey"],
                    Rubrics["TypeKey"],
                    Rubrics["OriginKey"],
                    Rubrics["Priority"],
                    Rubrics["Flags"],
                    Rubrics["Time"],
                    Rubrics["GUID"],
                    Rubrics["Code"],
                    Rubrics["SSN"],
                    Rubrics["proxy"]
                }
            );
    }

    public bool Equals(IUnique other)
    {
        return entry.Equals(other);
    }

    public int CompareTo(IUnique? other)
    {
        return entry.CompareTo(other);
    }

    public ulong Key
    {
        get => entry.Key;
        set => entry.Key = value;
    }

    public ulong TypeKey
    {
        get => entry.TypeKey;
        set => entry.TypeKey = value;
    }

    public byte[] GetBytes()
    {
        return entry.GetBytes();
    }

    public byte[] GetKeyBytes()
    {
        return entry.GetKeyBytes();
    }

    public object this[string propertyName]
    {
        get => GetPreset(propertyName);
        set => SetPreset(propertyName, value);
    }

    public object this[int fieldId]
    {
        get => GetPreset(fieldId);
        set => SetPreset(fieldId, value);
    }

    public object[] ValueArray
    {
        get => entry.ValueArray;
        set => entry.ValueArray = value;
    }

    public Uscn Code
    {
        get => entry.Code;
        set => entry.Code = value;
    }

    public IRubrics Rubrics
    {
        get => entry.Rubrics;
        set => entry.Rubrics = value;
    }

    public object Devisor
    {
        get => entry.Target;
        set => entry.Target = value;
    }

    public object GetPreset(int fieldId)
    {
        if (trackset != null && trackset.Contains(fieldId))
        {
            return preset[fieldId];
        }
        return entry[fieldId];
    }

    public object GetPreset(string propertyName)
    {
        if (trackset != null)
        {
            int id = 0;
            var rubric = Rubrics[propertyName];
            if (rubric != null && trackset.Contains(id = Rubrics[propertyName].RubricId))
            {
                return preset[id];
            }
            else
                return null;
        }

        return entry[propertyName];
    }

    public int[] GetPresets()
    {
        return trackset.ToArray();
    }

    public void SetPreset(int fieldId, object value)
    {
        if (GetPreset(fieldId).Equals(value))
            return;

        if (trackset == null)
            trackset = new HashSet<int>();

        trackset.Add(fieldId);
        preset[fieldId] = value;
    }

    public void SetPreset(string propertyName, object value)
    {
        if (GetPreset(propertyName).Equals(value))
            return;

        if (trackset == null)
            trackset = new HashSet<int>();

        int id = Rubrics[propertyName].RubricId;
        trackset.Add(id);
        preset[id] = value;
    }

    public void WritePresets()
    {
        trackset.ForEach((id) => entry[id] = preset[id]).Commit();
        trackset.Clear();
    }

    public bool HavePresets => trackset != null;
}
