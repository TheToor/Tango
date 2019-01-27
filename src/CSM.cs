using Harmony;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Reflection;

namespace CSM
{
    public class CSM : ICities.IUserMod
    {
        private readonly HarmonyInstance _harmony;

        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public CSM()
        {
            // Setup the correct logging configuration
            SetupLogging();

            try
            {
                _logger.Info("Attempting to patch Cities: Skylines using Harmony...");
                _harmony = HarmonyInstance.Create("net.gridentertainment.csm");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                _logger.Info("Successfully patched Cities: Skylines!");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Patching failed");
            }

            _logger.Info("Construction Complete!");
        }

        ~CSM()
        {
            _logger.Info("Unpatching Harmony...");
            _harmony.UnpatchAll();
            _logger.Info("Destruction complete!");
        }

        /// <summary>
        ///     This code sets up the different
        ///     logging levels for the mod.
        /// </summary>
        private void SetupLogging()
        {
            var config = new LoggingConfiguration();

            // The layout of the log
            var layout = "[${time}] [" + Assembly.GetAssembly(typeof(CSM)).GetName().Version.ToString() + "] [${level}] ${message} ${exception:format=tostring}";

            // Target for file logging
            var logfile = new FileTarget("logfile")
            {
                FileName = "multiplayer-logs/log-current.txt",
                ArchiveFileName = "multiplayer-logs/log-${shortdate}.txt",
                Layout = layout,
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 7,
                ConcurrentWrites = true,
            };

            // Target for console logging
            var logconsole = new ConsoleTarget("logconsole") { Layout = layout };

#if DEBUG
            // Log4View target
            var log4View = new NLogViewerTarget()
            {
                Address = "tcp://127.0.0.1:878",
                OnConnectionOverflow = NetworkTargetConnectionsOverflowAction.Block,
                NewLine = true,
                Layout = "${longdate}|${level}|${logger}|${message}"
            };
            // There may be alot of trace messages (which is kinda the point of trace) so we write the logs async so it doesn't block any threads
            var asyncWrapper = new AsyncTargetWrapper()
            {
                OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
                QueueLimit = 50000,
                WrappedTarget = log4View
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, asyncWrapper);
#endif


            // While in development set both levels to start at debug, later on we
            // want to set an option to do this.
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }

        public string Name => "Cities: Skylines Multiplayer";

        public string Description => "Multiplayer mod for Cities: Skylines.";

        /// <summary>
        ///     Log a message to the console.
        /// </summary>
        /// <param name="message"></param>
        [Obsolete("Use NLog instead.")]
        public static void Log(string message)
        {
            _logger.Info(message);
        }
    }
}