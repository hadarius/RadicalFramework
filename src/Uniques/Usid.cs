namespace Radical.Uniques;

using System;
using Radical.Extracting;
using System.Runtime.InteropServices;
using Hashing;

[Serializable]
[ComVisible(true)]
[StructLayout(LayoutKind.Sequential, Size = 8)]
public unsafe struct Usid
    : IFormattable,
        IComparable,
        IComparable<IUnique>,
        IEquatable<IUnique>,
        IUnique
{
    private byte[] bytes;

    private ulong _KeyBlock
    {
        get
        {
            ulong block = Key;
            return (block << 32) | ((block >> 16) & 0xffff0000) | (block >> 48);
        }
        set { Key = (value >> 32) | (((value & 0x0ffff0000) << 16)) | (value << 48); }
    }

    public Usid(ulong l)
    {
        bytes = new byte[8];
        Key = l;
    }

    public Usid(long l) : this((ulong)l) { }

    public Usid(string ca)
    {
        bytes = new byte[8];
        this.FromTetrahex(ca.ToCharArray());
    }

    public Usid(byte[] b)
    {
        bytes = new byte[8];
        if (b != null)
        {
            int l = b.Length;
            if (l > 8)
                l = 8;
            fixed (byte* dbp = bytes)
            fixed (byte* sbp = b)
            {
                Extract.CopyBlock(dbp, sbp, l);
            }
        }
    }

    public Usid(ushort z, ushort y, uint x)
    {
        bytes = new byte[8];
        fixed (byte* pbytes = bytes)
        {
            *((uint*)pbytes) = x;
            *((ushort*)(pbytes + 4)) = y;
            *((ushort*)(pbytes + 6)) = z;
        }
    }

    public Usid(object key)
    {
        bytes = new byte[8];
        fixed (byte* n = bytes)
            *((ulong*)n) = key.UniqueKey64();
    }

    public byte[] this[int offset]
    {
        get
        {
            if (offset > 0 && offset < 8)
            {
                int l = (8 - offset);
                byte[] r = new byte[l];
                fixed (byte* pbyte = sbytes)
                fixed (byte* rbyte = r)
                    Extract.CopyBlock(rbyte, pbyte + offset, l);
                return r;
            }
            return GetBytes();
        }
        set
        {
            int l = value.Length;
            if (offset > 0 || l < 8)
            {
                int count = 8 - offset;
                if (l < count)
                    count = l;
                fixed (byte* pbyte = sbytes)
                fixed (byte* rbyte = value)
                {
                    Extract.CopyBlock(pbyte, rbyte, offset, l);
                }
            }
            else
            {
                fixed (byte* v = value)
                fixed (byte* b = sbytes)
                    *(ulong*)b = *(ulong*)v;
            }
        }
    }

    public byte[] GetBytes()
    {
        byte[] r = new byte[8];
        fixed (byte* rbyte = r)
        fixed (byte* pbyte = sbytes)
        {
            *((ulong*)rbyte) = *((ulong*)pbyte);
        }
        return r;
    }

    public byte[] GetKeyBytes()
    {
        return GetBytes();
    }

    public ulong Key
    {
        get
        {
            fixed (byte* pbyte = sbytes)
                return *((ulong*)pbyte);
        }
        set
        {

            fixed (byte* b = sbytes)
                *((ulong*)b) = value;
        }
    }

    public ushort BlockZ
    {
        get
        {
            fixed (byte* pbyte = sbytes)
                return *((ushort*)(pbyte + 6));
        }
        set
        {
            fixed (byte* pbyte = sbytes)
                *((ushort*)(pbyte + 6)) = value;
        }
    }

    public ushort BlockY
    {
        get
        {

            fixed (byte* pbyte = sbytes)
                return *((ushort*)(pbyte + 4));
        }
        set
        {
            fixed (byte* pbyte = sbytes)
                *((ushort*)(pbyte + 4)) = value;
        }
    }

    public uint BlockX
    {
        get
        {
            fixed (byte* pbyte = sbytes)
                return *((uint*)pbyte);
        }
        set
        {
            fixed (byte* pbyte = sbytes)
                *((uint*)pbyte) = value;
        }
    }

    private byte[] sbytes
    {
        get => bytes ??= new byte[8];
    }

    public bool IsNotEmpty
    {
        get { return (Key > 0); }
    }

    public bool IsEmpty
    {
        get { return (Key == 0); }
    }

    public override int GetHashCode()
    {
        fixed (byte* pbyte = sbytes)
        {
            return (int)Hasher32.ComputeKey(pbyte, 8);
        }
    }

    public void SetKey(ulong value)
    {
        Key = value;
    }

    public ulong GetKey()
    {
        return Key;
    }

    public int CompareTo(object value)
    {
        if (value == null)
            return -1;
        if (!(value is Usid))
            throw new Exception();

        return (int)(Key - value.UniqueKey64());
    }

    public int CompareTo(Usid g)
    {
        return (int)(Key - g.Key);
    }

    public int CompareTo(IUnique g)
    {
        return (int)(Key - g.UniqueKey());
    }

    public override bool Equals(object value)
    {
        if (value == null)
            return false;
        if ((value is string))
            return new Usid(value.ToString()).Key == Key;

        return (Key == ((Usid)value).Key);
    }

    public bool Equals(ulong g)
    {
        return (Key == g);
    }

    public bool Equals(Usid g)
    {
        return (Key == g.Key);
    }

    public bool Equals(IUnique g)
    {
        return (Key == g.Key);
    }

    public bool Equals(string g)
    {
        return (Key == new Usid(g).Key);
    }

    public override String ToString()
    {
        return new string(this.ToTetrahex());
    }

    public String ToString(String format)
    {
        return ToString(format, null);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return new string(this.ToTetrahex());
    }

    public static bool operator ==(Usid a, Usid b)
    {
        return (a.Key == b.Key);
    }

    public static bool operator !=(Usid a, Usid b)
    {
        return (a.Key != b.Key);
    }

    public static explicit operator Usid(String s)
    {
        return new Usid(s);
    }

    public static implicit operator String(Usid s)
    {
        return s.ToString();
    }

    public static explicit operator Usid(byte[] l)
    {
        return new Usid(l);
    }

    public static implicit operator byte[](Usid s)
    {
        return s.GetBytes();
    }

    public static explicit operator Usid(ulong l)
    {
        return new Usid(l);
    }

    public static implicit operator ulong(Usid s)
    {
        return s.Key;
    }

    public static explicit operator Usid(long l)
    {
        return new Usid(l);
    }

    public static implicit operator long(Usid s)
    {
        return (long)s.Key;
    }

    public static explicit operator Usid(Uscn l)
    {
        return l.Id;
    }

    public static implicit operator Uscn(Usid s)
    {
        return new Uscn(s.Key);
    }

    public static Usid Empty
    {
        get { return new Usid() { bytes = new byte[8] }; }
    }

    public static Usid New
    {
        get { return new Usid(Unique.NewKey); }
    }

    public char[] ToTetrahex()
    {
        char[] pchchar = new char[10];
        ulong pchulong;
        byte pchbyte;
        int pchlength = 0;
        ulong _ulongValue = _KeyBlock;

        pchulong =
            ((_ulongValue & 0x3fffffff00000000L) >> 6)
            | ((_ulongValue & 0xffff0000L) >> 6)
            | (_ulongValue & 0x03ffL);
        for (int i = 0; i < 5; i++)
        {
            pchbyte = (byte)(pchulong & 0x003fL);
            pchchar[i] = (pchbyte).ToTetrahexChar();
            pchulong = pchulong >> 6;
        }

        pchlength = 5;

        for (int i = 5; i < 10; i++)
        {
            pchbyte = (byte)(pchulong & 0x003fL);
            if (pchbyte != 0x00)
                pchlength = i + 1;
            pchchar[i] = (pchbyte).ToTetrahexChar();
            pchulong = pchulong >> 6;
        }

        char[] pchchartrim = new char[pchlength];
        Array.Copy(pchchar, 0, pchchartrim, 0, pchlength);

        return pchchartrim;
    }

    public void FromTetrahex(char[] pchchar)
    {
        ulong pchulong = 0;
        byte pchbyte;
        int pchlength = 0;

        pchlength = pchchar.Length;
        pchbyte = (pchchar[pchlength - 1]).ToTetrahexByte();
        pchulong = pchbyte & 0x3fUL;
        for (int i = pchlength - 2; i >= 0; i--)
        {
            pchbyte = (pchchar[i]).ToTetrahexByte();
            pchulong = pchulong << 6;
            pchulong = pchulong | (pchbyte & 0x3fUL);
        }
        _KeyBlock =
            ((pchulong << 6) & 0x3fffffff00000000L)
            | ((pchulong << 6) & 0xffff0000L)
            | (pchulong & 0x03ffL);
    }

    public ulong TypeKey
    {
        get => 0;
        set => throw new NotImplementedException();
    }

    public void SetTypeKey(ulong seed)
    {
        throw new NotImplementedException();
    }

    public ulong GetTypeKey()
    {
        return 0;
    }
}
