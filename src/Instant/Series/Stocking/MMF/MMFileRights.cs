namespace Radical.Instant.Series.Stocking.MMF
{
#if !NET40Plus

    [Flags]
    public enum MMFileRights : uint
    {
        Write = 0x02,

        Read = 0x04,

        ReadWrite = MMFileRights.Write | MMFileRights.Read,
    }
#endif
}