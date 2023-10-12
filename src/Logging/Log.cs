namespace Radical.Logging
{
    using NLog;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;

    public static partial class Log
    {
        private static readonly int BACK_LOG_DAYS = -1;
        private static readonly int BACK_LOG_HOURS = -1;
        private static readonly int BACK_LOG_MINUTES = -1;
        private static readonly JsonSerializerOptions jsonOptions;
        private static int _logLevel = 2;
        private static bool cleaningEnabled = false;
        private static DateTime clearLogTime;
        private static Thread logger;
        private static ILogHandler handler { get; set; }

        private static ConcurrentQueue<Starlog> logQueue = new ConcurrentQueue<Starlog>();

        private static bool threadLive;

        public static DateTime Clock = DateTime.Now;

        static Log()
        {
            jsonOptions = JsonOptionsBuilder();
            handler = new LogHandler(jsonOptions, LogLevel.Info);
            clearLogTime = DateTime.Now
                .AddDays(BACK_LOG_DAYS)
                .AddHours(BACK_LOG_HOURS)
                .AddMinutes(BACK_LOG_MINUTES);
            threadLive = true;
            logger = new Thread(new ThreadStart(logging));
            logger.Start();
        }

        public static void Add(LogLevel logLevel, string category, string message, ILogSate state)
        {
            var _log = new Starlog()
            {
                Level = logLevel,
                Sender = category,
                State = state,
                Message = message
            };

            logQueue.Enqueue(_log);
        }

        public static void ClearLog()
        {
            if (!cleaningEnabled || handler == null)
                return;

            try
            {
                if (DateTime.Now.Day != clearLogTime.Day)
                {
                    if (DateTime.Now.Hour != clearLogTime.Hour)
                    {
                        if (DateTime.Now.Minute != clearLogTime.Minute)
                        {
                            handler.Clean(clearLogTime);
                            clearLogTime = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void CreateHandler(LogLevel level)
        {
            handler = new LogHandler(jsonOptions, level);
        }

        public static void SetLevel(int logLevel)
        {
            _logLevel = logLevel;
        }

        public static void Start()
        {
            threadLive = true;
            _logLevel = 2;
            logger.Start();
        }

        public static void Start(int logLevel)
        {
            CreateHandler(LogLevel.Info);
            SetLevel(logLevel);
            if (!threadLive)
            {
                threadLive = true;
                logger.Start();
            }
        }

        public static void Stop()
        {
            logger.Join();
            threadLive = false;
        }

        private static void logging()
        {
            while (threadLive)
            {
                try
                {
                    Clock = DateTime.Now;
                    Thread.Sleep(1000);
                    int count = logQueue.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Starlog log;
                        if (logQueue.TryDequeue(out log))
                        {
                            if (handler != null)
                            {
                                handler.Write(log);
                            }
                        }
                    }

                    if (cleaningEnabled)
                        ClearLog();
                }
                catch (Exception ex) { }
            }
        }

        private static JsonSerializerOptions JsonOptionsBuilder()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new LogExceptionConverter());
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = System.Text.Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull;
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            return options;
        }
    }
}
