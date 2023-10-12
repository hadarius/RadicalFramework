namespace Radical.Uniques
{
    using System.Collections.Concurrent;
    using System.Threading;
    using Hashing;

    public static class Unique
    {
        private static readonly int CAPACITY = 75 * 1000;
        private static readonly int LOW_LIMIT = 50 * 1000;
        private static readonly uint NEXT_KEY_VECTOR = (uint)UniquePrimes.Get(4);
        private static readonly int WAIT_LOOPS = 500;
        private static UniqueKey32 bit32 = new UniqueKey32();
        private static UniqueKey64 bit64 = new UniqueKey64();
        private static bool generating;
        private static Thread generator;
        private static object holder = new object();
        private static ulong keyNumber = (ulong)DateTime.Now.Ticks;
        private static ConcurrentQueue<ulong> keys = new ConcurrentQueue<ulong>();
        private static Random randomSeed = new Random((int)(DateTime.Now.Ticks.UniqueKey32()));

        static Unique()
        {
            generator = startup();
        }

        public static UniqueKey32 Bit32
        {
            get => bit32;
        }

        public static UniqueKey64 Bit64
        {
            get => bit64;
        }

        public static ulong NewKey
        {
            get
            {
                ulong key = 0;
                int counter = 0;
                bool loop = false;
                while (counter < WAIT_LOOPS)
                {
                    if (!(loop = keys.TryDequeue(out key)))
                    {
                        if (!generating)
                            Start();

                        counter++;
                        Thread.Sleep(20);
                    }
                    else
                    {
                        int count = keys.Count;
                        if (count < LOW_LIMIT)
                            Start();
                        break;
                    }
                }
                return key;
            }
        }

        public static void Start()
        {
            lock (holder)
            {
                if (!generating)
                {
                    generating = true;
                    Monitor.Pulse(holder);
                }
            }
        }

        public static void Stop()
        {
            if (generating)
            {
                generating = false;
            }
        }

        private unsafe static void keyGeneration()
        {
            while (generating)
            {
                lock (holder)
                {
                    ulong seed = nextSeed();
                    int count = CAPACITY - keys.Count;
                    for (int i = 0; i < count; i++)
                    {
                        ulong keyNo = nextKeyNumber();
                        keys.Enqueue(Hasher64.ComputeKey(((byte*)&keyNo), 8, seed));
                    }
                    Stop();
                    Monitor.Wait(holder);
                }
            }
        }

        private static unsafe ulong nextKeyNumber()
        {
            return keyNumber += NEXT_KEY_VECTOR;
        }

        private static ulong nextSeed()
        {
            return (ulong)randomSeed.Next();
        }

        private static Thread startup()
        {
            generating = true;
            Thread _reffiler = new Thread(new ThreadStart(keyGeneration));
            _reffiler.Start();
            return _reffiler;
        }
    }
}
