namespace Radical.Uniques.Hashing
{
    using Algorithm;

    public static class Hasher32
    {
        public static unsafe Byte[] ComputeBytes(byte* ptr, int length, ulong seed = 0)
        {
            byte[] b = new byte[4];
            fixed (byte* pb = b)
            {
                *((uint*)pb) = xxHash32.UnsafeComputeHash(ptr, length, (uint)seed);
            }
            return b;
        }

        public static unsafe Byte[] ComputeBytes(byte[] bytes, ulong seed = 0)
        {
            byte[] b = new byte[4];
            fixed (
                byte* pb = b,
                    pa = bytes
            )
            {
                *((uint*)pb) = xxHash32.UnsafeComputeHash(pa, bytes.Length, (uint)seed);
            }
            return b;
        }

        public static unsafe uint ComputeKey(byte* ptr, int length, ulong seed = 0)
        {
            return xxHash32.UnsafeComputeHash(ptr, length, (uint)seed);
        }

        public static unsafe uint ComputeKey(byte[] bytes, ulong seed = 0)
        {
            fixed (byte* pa = bytes)
            {
                return xxHash32.UnsafeComputeHash(pa, bytes.Length, (uint)seed);
            }
        }
    }
}
