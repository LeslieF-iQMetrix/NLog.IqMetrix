using System;
using System.Linq;
using System.Text;
using Android.App;
using Android.Provider;
using Android.Widget;
using Android.OS;
using Java.IO;
using NLog.Core;
using NLog.Core.Config;
using NLog.Core.Layouts;
using NLog.Core.Targets;
using Org.Json;
using PubNubMessaging.Core;
using Environment = Android.OS.Environment;

namespace NLog.Android.Test
{
    [Activity(Label = "NLog.Android.Test", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        int _traceCount = 0;
        int _debugCount = 0;
        int _infoCount = 0;
        int _warnCount = 0;
        int _errorCount = 0;
        int _fatalCount = 0;

        private Logger _logger;
        private Pubnub _pubnub;
        private static string _androidId;

        protected override void OnCreate(Bundle bundle)
        {
            // NLog stuff.
            _androidId = Settings.Secure.GetString(BaseContext.ContentResolver,
                                                   Settings.Secure.AndroidId);
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("LumberMill", typeof(LumberMillTarget));
            InitializeNLog();

            base.OnCreate(bundle);
            _logger.Debug("Base Android activity created");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _logger.Debug("Content view has been set to 'Main'");

            // Pubnub stuff
            _logger.Debug("Attempting to subscribe to Pubnub channel...");
            _pubnub = new Pubnub("pub-c-c872944b-417d-41ca-ab55-d86bf3678b0e"
                , "sub-c-4b54ec72-3848-11e3-a73c-02ee2ddab7fe"
                , "sec-c-ZTA3NjdjNWEtN2M4Ni00MDI5LTkyNTMtNjFmNDQ2YzBiNjFj"
                , ""
                , false);
            _pubnub.Subscribe<string>("LogLevel", ChangeLogLevel, ConnectCallback, ErrorCallback);

            // Get our button from the layout resource,
            // and attach an event to it
            var traceButton = FindViewById<Button>(Resource.Id.TraceButton);
            var debugButton = FindViewById<Button>(Resource.Id.DebugButton);
            var infoButton = FindViewById<Button>(Resource.Id.InfoButton);
            var warnButton = FindViewById<Button>(Resource.Id.WarnButton);
            var errorButton = FindViewById<Button>(Resource.Id.ErrorButton);
            var fatalButton = FindViewById<Button>(Resource.Id.FatalButton);

            traceButton.Click += delegate {
                traceButton.Text = string.Format("TRACE: {0} clicks!", ++_traceCount);
                _logger.Trace(string.Format("TRACE: Count is at {0}", _traceCount));
            };

            debugButton.Click += delegate
            {
                debugButton.Text = string.Format("DEBUG: {0} clicks!", ++_debugCount);
                _logger.Debug(string.Format("DEBUG: Count is at {0}", _debugCount));
            };

            infoButton.Click += delegate
            {
                infoButton.Text = string.Format("INFO: {0} clicks!", ++_infoCount);
                _logger.Info(string.Format("INFO: Count is at {0}", _infoCount));
            };

            warnButton.Click += delegate
            {
                warnButton.Text = string.Format("WARN: {0} clicks!", ++_warnCount);
                _logger.Warn(string.Format("WARN: Count is at {0}", _warnCount));
            };

            errorButton.Click += delegate
            {
                errorButton.Text = string.Format("ERROR: {0} clicks!", ++_errorCount);
                _logger.Error(string.Format("ERROR: Count is at {0}", _errorCount));
            };

            fatalButton.Click += delegate
            {
                fatalButton.Text = string.Format("FATAL: {0} clicks!", ++_fatalCount);
                _logger.Fatal(string.Format("FATAL: Count is at {0}", _fatalCount));
            };

            _logger.Info("Application initialization complete. Start pressing buttons now!");

        }

        private void InitializeNLog()
        {
            var sdCard = Environment.ExternalStorageDirectory;
            var logDirectory = new File(sdCard.AbsolutePath + "/log");
            logDirectory.Mkdirs();
            var config = new LoggingConfiguration();
            var csvFileTarget = new FileTarget
                {
                    FileName = logDirectory.AbsolutePath + "/${shortdate}.csv"
                    ,
                    Layout = new CsvLayout
                        {
                            Columns =
                                {
                                    new CsvColumn("Time", "${longdate}"),
                                    new CsvColumn("Level", "${level}"),
                                    new CsvColumn("Lessage", "${message}"),
                                    new CsvColumn("Logger", "${logger}")
                                },
                        }
                };
            var lumberMillTarget = new LumberMillTarget(_androidId);

            config.AddTarget("CSV Logfile", csvFileTarget);
            config.AddTarget("Lumbermill Log", lumberMillTarget);

            var rule1 = new LoggingRule("*", LogLevel.Trace, csvFileTarget);
            config.LoggingRules.Add(rule1);
            var rule2 = new LoggingRule("*", LogLevel.Trace, lumberMillTarget);
            config.LoggingRules.Add(rule2);

            LogManager.Configuration = config;

            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("NLog successfully initialized.");
            _logger.Debug("Application starting up...");
        }

        private void ConnectCallback(string result)
        {
            var message = string.Format("Pubnub: Connect Callback - {0}", result);
//            _logger.Info(message);
        }

        private void ErrorCallback(string result)
        {
            var message = string.Format("Pubnub: Something went wrong - {0}", result);
//            _logger.Error(message);
        }

        private void ChangeLogLevel(string result)
        {
            var jsonArray = new JSONArray(result);
            var jsonObject = jsonArray.GetJSONObject(0);

            var logLevelString = jsonObject.Get("level").ToString();
            var logLevel = LogLevel.FromString(logLevelString);
            var toEnable = (bool)jsonObject.Get("enable");
            SetLogLevel(logLevel, toEnable);
            var message = string.Format("Set logging level to {0}", logLevelString);
//            _logger.Info(message);
        }

        private static void SetLogLevel(LogLevel logLevel, bool toEnable)
        {
            var rules = LogManager.Configuration.LoggingRules;

            if (toEnable)
            {
                foreach (var rule in rules.Where(rule => !rule.IsLoggingEnabledForLevel(logLevel)))
                {
                    rule.EnableLoggingForLevel(logLevel);
                }
            }
            else
            {
                foreach (var rule in rules.Where(rule => rule.IsLoggingEnabledForLevel(logLevel)))
                {
                    rule.DisableLoggingForLevel(logLevel);
                }
            }
            LogManager.ReconfigExistingLoggers();
        }
    }
}

