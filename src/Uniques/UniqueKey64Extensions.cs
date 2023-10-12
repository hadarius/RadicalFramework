namespace Radical.Uniques
{
    using System.Collections;
    using Radical.Extracting;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Hashing;

    public unsafe static class UniqueKey64Extensions
    {
        public static Double ComparableDouble(this Object obj, Type t = null)
        {
            if (t == null)
                t = obj.GetType();

            if (t.IsAssignableTo(typeof(IUnique)))
                return ((IUnique)obj).Key;
            if (t.IsValueType)
                return getSequentialValueTypeUniqueKey64((ValueType)obj);
            if (t == typeof(string))
                return (((string)obj)).UniqueKey64();
            if (t.IsAssignableTo(typeof(Type)))
                return UniqueKey64((Type)obj);
            if (t.IsAssignableTo(typeof(IList)))
            {
                if (t == typeof(Byte[]))
                    return Hasher64.ComputeKey((Byte[])obj);

                IList o = (IList)obj;
                if (o.Count == 1)
                    return UniqueKey64(o[0]);

                return UniqueKey64(o);
            }

            return UniqueKey64(obj);
        }

        public static UInt64 ComparableUInt64(this Object obj, Type type = null)
        {
            if (type == null)
                type = obj.GetType();

            if (obj is string)
            {
                if (type != typeof(string))
                {
                    if (type == typeof(IUnique))
                        return new Uscn((string)obj).UniqueKey();
                    if (type == typeof(DateTime))
                        return (ulong)((DateTime)Convert.ChangeType(obj, type)).ToBinary();
                    if (type == typeof(Enum))
                        return (ulong)(Enum.Parse(type, (string)obj));
                    return Convert.ToUInt64(Convert.ChangeType(obj, type));
                }
                return ((string)obj).UniqueKey64();
            }

            if (obj is IUnique)
                return ((IUnique)obj).UniqueKey();
            if (type == typeof(DateTime))
                return (ulong)((DateTime)obj).Ticks;
            if (type == typeof(Enum))
                return (ulong)((int)obj);
            if (obj is ValueType)
                return Convert.ToUInt64(obj);
            return obj.UniqueKey64();
        }

        public static UInt32 GetHashCode(this Byte[] obj, ulong seed = 0)
        {
            return obj.UniqueKey32(seed);
        }

        public static UInt32 GetHashCode<T>(this IEquatable<T> obj, ulong seed = 0)
        {
            return obj.UniqueKey32(seed);
        }

        public static UInt32 GetHashCode(this IList obj, ulong seed = 0)
        {
            return obj.UniqueKey32(seed);
        }

        public static UInt32 GetHashCode(this IntPtr ptr, int length, ulong seed = 0)
        {
            return ptr.UniqueKey32(length, seed);
        }

        public static UInt32 GetHashCode(this IUnique obj)
        {
            return obj.UniqueBytes32().ToUInt32();
        }

        public static UInt32 GetHashCode(this Object obj, ulong seed = 0)
        {
            return obj.UniqueKey32(seed);
        }

        public static UInt32 GetHashCode(this string obj, ulong seed = 0)
        {
            return obj.UniqueKey32(seed);
        }

        public static UInt32 GetHashCode(this Type obj, ulong seed = 0)
        {
            return obj.UniqueKey32(seed);
        }

        public static bool NullOrEquals(this ICollection obj, Object value)
        {
            if (obj != null)
            {
                if (obj.Count > 0)
                    return (obj.Equals(value));
                return true;
            }
            return (obj == null && value == null);
        }

        public static bool NullOrEquals(this Object obj, Object value)
        {
            if (obj != null)
            {
                if (obj is ICollection)
                    return NullOrEquals((ICollection)obj, value);
                return obj.Equals(value);
            }
            return (obj == null && value == null);
        }

        public static Byte[] UniqueBytes64(this Byte[] bytes, ulong seed = 0)
        {
            return Hasher64.ComputeBytes(bytes, seed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte[] UniqueBytes64(
            this IList obj,
            int[] sizes,
            int totalsize,
            ulong seed = 0
        )
        {
            byte[] bytes = new byte[totalsize];
            fixed (byte* buff = bytes)
            {
                byte* buffer = buff;
                int[] _sizes = sizes;
                int offset = 0;
                for (int i = 0; i < obj.Count; i++)
                {
                    object o = obj[i];
                    var t = o.GetType();
                    int s = _sizes[i];
                    if (t == typeof(string))
                    {
                        string str = ((string)o);
                        fixed (char* c = str)
                            Extract.CopyBlock(buffer, (byte*)c, offset, s);
                    }
                    else
                    {
                        if (t.IsAssignableTo(typeof(IUnique)))
                        {
                            s = 8;
                            *((ulong*)(buffer + offset)) = ((IUnique)o).Key;
                        }
                        else
                        {
                            Extract.StructureToPointer(o, buffer + offset);
                        }
                    }
                    offset += s;
                }

                return Hasher64.ComputeBytes(buffer, offset, seed);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte[] UniqueBytes64(this IList obj, ulong seed = 0)
        {
            int length = 256,
                offset = 0,
                postoffset = 0,
                count = obj.Count,
                s = 0;
            byte[] bytes = new byte[length];
            fixed (byte* buff = bytes)
            {
                byte* buffer = buff;
                bool toResize = false;

                for (int i = 0; i < count; i++)
                {
                    object o = obj[i];
                    var t = o.GetType();
                    if (t == typeof(string))
                    {
                        string str = ((string)o);
                        s = str.Length * sizeof(char);
                        postoffset = (s + offset);

                        if (postoffset > length)
                            toResize = true;
                        else
                            fixed (char* c = str)
                                Extract.CopyBlock(buffer, (byte*)c, offset, s);
                    }
                    else
                    {
                        if (t.IsAssignableTo(typeof(IUnique)))
                        {
                            s = 8;
                            postoffset = (s + offset);

                            if (postoffset > length)
                                toResize = true;
                            else
                                *((ulong*)(buffer + offset)) = ((IUnique)o).Key;
                        }
                        else
                        {
                            if (t.IsAssignableTo(typeof(Type)))
                            {
                                o = ((Type)o).FullName;
                                s = ((Type)o).FullName.Length * 2;
                            }
                            else
                            {
                                s = o.GetSize();
                            }

                            postoffset = (s + offset);

                            if (postoffset > length)
                                toResize = true;
                            else
                                Extract.StructureToPointer(o, buffer + offset);
                        }
                    }

                    if (toResize)
                    {
                        i--;
                        toResize = false;
                        byte[] b = new byte[postoffset];
                        fixed (byte* _buffer = b)
                        {
                            Extract.CopyBlock(_buffer, buffer, offset);
                            buffer = _buffer;
                            length = postoffset;
                        }
                    }
                    else
                        offset = postoffset;
                }

                return Hasher64.ComputeBytes(buffer, offset, seed);
            }
        }

        public static Byte[] UniqueBytes64(this IntPtr bytes, int length, ulong seed = 0)
        {
            return Hasher64.ComputeBytes((byte*)bytes.ToPointer(), length, seed);
        }

        public static Byte[] UniqueBytes64(this IUnique obj)
        {
            return obj.GetKeyBytes();
        }

        public static Byte[] UniqueBytes64(this Object obj, ulong seed = 0)
        {
            if (obj == null)
                return new byte[0];

            var t = obj.GetType();

            if (t.IsAssignableTo(typeof(IUnique)))
                return ((IUnique)obj).GetKeyBytes();
            if (t.IsValueType)
                return getValueTypeHashBytes64((ValueType)obj, seed);
            if (t == typeof(string))
                return (((string)obj)).UniqueBytes64(seed);
            if (t.IsAssignableTo(typeof(Type)))
                return UniqueBytes64((Type)obj, seed);
            if (t.IsAssignableTo(typeof(IList)))
            {
                if (t == typeof(Byte[]))
                    return Hasher64.ComputeBytes((Byte[])obj, seed);

                IList o = (IList)obj;
                if (o.Count == 1)
                    return UniqueBytes64(o[0], seed);

                return UniqueBytes64(o, seed);
            }
            return Hasher64.ComputeBytes(obj.GetBytes(true), seed);
        }

        public static Byte[] UniqueBytes64(
            this Object[] obj,
            int[] sizes,
            int totalsize,
            ulong seed = 0
        )
        {
            if (obj.Length == 1)
                return UniqueBytes64(obj[0], seed);
            return UniqueBytes64((IList)obj, sizes, totalsize, seed);
        }

        public static Byte[] UniqueBytes64(this Object[] obj, ulong seed = 0)
        {
            if (obj.Length == 1)
                return UniqueBytes64(obj[0], seed);
            return UniqueBytes64((IList)obj, seed);
        }

        public static Byte[] UniqueBytes64(this String obj, ulong seed = 0)
        {
            fixed (char* c = obj)
                return Hasher64.ComputeBytes((byte*)c, obj.Length * sizeof(char), seed);
        }

        public static Byte[] UniqueBytes64(this Type obj, ulong seed = 0)
        {
            fixed (char* b = obj.FullName)
            {
                return Hasher64.ComputeBytes((byte*)b, obj.FullName.Length * 2, seed);
            }
        }

        public static UInt64 UniqueKey(this Byte[] bytes, ulong seed = 0)
        {
            return UniqueKey64(bytes, seed);
        }

        public static UInt64 UniqueKey<T>(this IEquatable<T> obj, ulong seed = 0)
        {
            return UniqueKey64(obj, seed);
        }

        public static UInt64 UniqueKey(this IList obj, ulong seed = 0)
        {
            return UniqueKey64(obj, seed);
        }

        public static UInt64 UniqueKey(this IntPtr ptr, int length, ulong seed = 0)
        {
            return UniqueKey64(ptr, length, seed);
        }

        public static UInt64 UniqueKey(this IUnique obj)
        {
            return obj.Key;
        }

        public static UInt64 UniqueKey(this IUnique obj, ulong seed)
        {
            return Hasher64.ComputeKey(obj.GetKeyBytes(), seed);
        }

        public static UInt64 UniqueKey<V>(this IUnique<V> obj, ulong seed)
        {
            return UniqueKey64(obj.UniqueValues(), seed);
        }

        public static UInt64 UniqueKey(this Object obj, ulong seed = 0)
        {
            return UniqueKey64(obj, seed);
        }

        public static UInt64 UniqueKey(this Object[] obj, ulong seed = 0)
        {
            if (obj.Length == 1)
                return UniqueKey64(obj[0], seed);
            return UniqueKey64((IList)obj, seed);
        }

        public static UInt64 UniqueKey(this String obj, ulong seed = 0)
        {
            return UniqueKey64(obj, seed);
        }

        public static UInt64 UniqueKey(this Type obj, ulong seed = 0)
        {
            return UniqueKey64(obj, seed);
        }

        public static UInt64 UniqueKey64(this Byte[] bytes, ulong seed = 0)
        {
            return Hasher64.ComputeKey(bytes, seed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 UniqueKey64(this IList obj, int[] sizes, int totalsize, ulong seed = 0)
        {
            byte[] bytes = new byte[totalsize];
            fixed (byte* buff = bytes)
            {
                byte* buffer = buff;
                int[] _sizes = sizes;
                int offset = 0;
                for (int i = 0; i < obj.Count; i++)
                {
                    object o = obj[i];
                    int s = _sizes[i];
                    Type t = o.GetType();

                    if (t == typeof(string))
                    {
                        string str = ((string)o);
                        fixed (char* c = str)
                            Extract.CopyBlock(buffer, (byte*)c, offset, s);
                    }
                    else
                    {
                        if (t.IsAssignableTo(typeof(IUnique)))
                        {
                            s = 8;
                            *((ulong*)(buffer + offset)) = ((IUnique)o).Key;
                        }
                        else
                        {
                            if (t.IsAssignableTo(typeof(Type)))
                                o = ((Type)o).FullName;

                            Extract.StructureToPointer(o, buffer + offset);
                        }
                    }
                    offset += s;
                }

                return Hasher64.ComputeKey(buffer, offset, seed);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 UniqueKey64(this IList obj, ulong seed = 0)
        {
            int length = 256,
                offset = 0,
                postoffset = 0,
                count = obj.Count,
                s = 0;
            byte[] bytes = new byte[length];
            fixed (byte* buff = bytes)
            {
                byte* buffer = buff;
                bool toResize = false;

                for (int i = 0; i < count; i++)
                {
                    object o = obj[i];
                    Type t = o.GetType();

                    if (t == typeof(string))
                    {
                        string str = ((string)o);
                        s = str.Length * 2;
                        postoffset = (s + offset);
                        if (postoffset > length)
                            toResize = true;
                        else
                        {
                            fixed (char* c = str)
                                Extract.CopyBlock(buffer, (byte*)c, offset, s);
                        }
                    }
                    else
                    {
                        if (t.IsAssignableTo(typeof(IUnique)))
                        {
                            s = 8;
                            postoffset = (s + offset);

                            if (postoffset > length)
                                toResize = true;
                            else
                                *((ulong*)(buffer + offset)) = ((IUnique)o).Key;
                        }
                        else
                        {
                            if (t.IsAssignableTo(typeof(Type)))
                            {
                                o = ((Type)o).FullName;
                                s = ((Type)o).FullName.Length * 2;
                            }
                            else
                            {
                                s = o.GetSize();
                            }

                            postoffset = (s + offset);

                            if (postoffset > length)
                                toResize = true;
                            else
                                Extract.StructureToPointer(o, buffer + offset);
                        }
                    }

                    if (toResize)
                    {
                        i--;
                        toResize = false;
                        byte[] b = new byte[postoffset];
                        fixed (byte* _buffer = b)
                        {
                            Extract.CopyBlock(_buffer, buffer, offset);
                            buffer = _buffer;
                            length = postoffset;
                        }
                    }
                    else
                        offset = postoffset;
                }

                return Hasher64.ComputeKey(buffer, offset, seed);
            }
        }

        public static UInt64 UniqueKey64(this IntPtr ptr, int length, ulong seed = 0)
        {
            return Hasher64.ComputeKey((byte*)ptr.ToPointer(), length, seed);
        }

        public static UInt64 UniqueKey64(this IUnique obj)
        {
            return obj.Key;
        }

        public static UInt64 UniqueKey64(this IUnique obj, ulong seed)
        {
            return Hasher64.ComputeKey(obj.GetKeyBytes(), seed);
        }

        public static UInt64 UniqueKey64<V>(this IUnique<V> obj, ulong seed)
        {
            return UniqueKey64(obj.UniqueValues(), seed);
        }

        public static UInt64 UniqueKey64(this Object obj, ulong seed = 0)
        {
            if (obj == null)
                return 0;

            var t = obj.GetType();

            if (t.IsAssignableTo(typeof(IUnique)))
                return ((IUnique)obj).Key;

            if (t.IsValueType)
                return getValueTypeUniqueKey64((ValueType)obj, seed);

            if (t == typeof(string))
                return (((string)obj)).UniqueKey64(seed);

            if (t.IsAssignableTo(typeof(Type)))
                return UniqueKey64((Type)obj, seed);

            if (t.IsAssignableTo(typeof(IList)))
            {
                if (t == typeof(Byte[]))
                    return Hasher64.ComputeKey((Byte[])obj, seed);

                IList o = (IList)obj;
                if (o.Count == 1)
                    return UniqueKey64(o[0], seed);

                return UniqueKey64(o, seed);
            }

            return Hasher64.ComputeKey(obj.GetBytes(true), seed);
        }

        public static UInt64 UniqueKey64(
            this Object[] obj,
            int[] sizes,
            int totalsize,
            ulong seed = 0
        )
        {
            if (obj.Length == 1)
                return UniqueKey64(obj[0], seed);
            return UniqueKey64((IList)obj, sizes, totalsize, seed);
        }

        public static UInt64 UniqueKey64(this Object[] obj, ulong seed = 0)
        {
            if (obj.Length == 1)
                return UniqueKey64(obj[0], seed);
            return UniqueKey64((IList)obj, seed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 UniqueKey64(this String obj, ulong seed = 0)
        {
            fixed (char* c = obj)
            {
                return Hasher64.ComputeKey((byte*)c, obj.Length * sizeof(char), seed);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 UniqueKey64(this Type obj, ulong seed = 0)
        {
            fixed (char* b = obj.FullName)
            {
                return Hasher64.ComputeKey((byte*)b, obj.FullName.Length * sizeof(char), seed);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Byte[] getSequentialValueTypeHashBytes64(ValueType obj, ulong seed = 0)
        {
            int size = obj.GetSize();
            byte[] s = new byte[size];
            fixed (byte* ps = s)
            {
                Extract.StructureToPointer(obj, ps);
                return Hasher64.ComputeBytes(ps, size, seed);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 getSequentialValueTypeUniqueKey64(ValueType obj, ulong seed = 0)
        {
            int size = obj.GetSize();
            byte* ps = stackalloc byte[size];
            Extract.StructureToPointer(obj, ps);
            return Hasher64.ComputeKey(ps, size, seed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Byte[] getValueTypeHashBytes64(ValueType obj, ulong seed = 0)
        {
            byte[] s = new byte[8];
            fixed (byte* ps = s)
            {
                Extract.StructureToPointer(obj, ps);
                if (seed == 0)
                    return s;
                if (*(int*)ps == 0)
                    *(ulong*)ps = Unique.NewKey;
                return Hasher64.ComputeBytes(ps, 8, seed);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 getValueTypeUniqueKey64(ValueType obj, ulong seed = 0)
        {
            byte[] s = new byte[8];
            fixed (byte* ps = s)
            {
                Extract.StructureToPointer(obj, ps);
                ulong r = *(ulong*)ps;
                if (seed == 0)
                    return *(ulong*)ps;
                if (r == 0)
                    *(ulong*)ps = Unique.NewKey;
                return Hasher64.ComputeKey(ps, 8, seed);
            }
        }
    }
}
