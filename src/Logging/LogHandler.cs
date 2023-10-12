using NLog;
using NLog.Config;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Radical.Logging
{
    public class LogHandler : ILogHandler
    {
        private readonly LogFactory factory;
        private readonly ConcurrentDictionary<string, ILogger> loggerRegistry = new ConcurrentDictionary<string, ILogger>();

        private string sender;
        private ILogger logger;
        private LogLevel level;
        private LoggingConfiguration configuration;
        private JsonSerializerOptions jsonOptions;

        public LogHandler(JsonSerializerOptions options, LogLevel level)
        {
            var nlogEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new XmlLoggingConfiguration(nlogEnvironment == ""
                        ? "Properties/nlog.config"
                        : $"Properties/nlog.{nlogEnvironment}.config");
            configuration = config.Reload();
            factory = configuration.LogFactory;

            jsonOptions = options;
            this.level = level;
            sender = "Radical.Logging";
            logger = factory.GetCurrentClassLogger();
            loggerRegistry.TryAdd(sender, logger);
        }

        public ILogger GetLogger<TState>(TState state)
        {
            return factory.GetLogger(typeof(TState).FullName);
        }

        public bool Clean(DateTime olderThen)
        {

            return true;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return ((level.Ordinal - logLevel.Ordinal) < 1);
        }

        public void Write(Starlog log)
        {
            if (IsEnabled(log.Level))
            {
                loggerChooser(log).Log(log.Level, JsonSerializer.Serialize(log, jsonOptions));
            }
        }

        public void SetLevel(LogLevel level)
        {
            this.level = level;
        }

        private ILogger loggerChooser(Starlog log)
        {
            if (log.Sender != sender)
            {
                sender = log.Sender;
                logger = loggerRegistry.GetOrAdd(sender, l => factory.GetLogger(sender));
            }

            return logger;
        }

        private Starlog Optimize(Starlog log)
        {
            if (log.State?.DataObject != null &&
                log.State.DataObject.GetType().IsAssignableTo(typeof(IEnumerable)))
            {
                var dataenum = ((IEnumerable)log.State.DataObject).GetEnumerator();
                dataenum.MoveNext();
                log.State.DataObject = dataenum.Current;
            }
            return log;
        }
    }
}
