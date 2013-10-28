using System;
using System.IO;
using MonoTouch.UIKit;
using System.Drawing;
using NLog.Core;
using NLog.Core.Config;
using NLog.Core.Layouts;
using NLog.Core.Targets;

namespace NLog.iOS.Test
{
    public class MyViewController : UIViewController
    {
        UIButton _button;
        int _numClicks = 0;
        private const float ButtonWidth = 200;
        private const float ButtonHeight = 50;

        private Logger _logger;

        public MyViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = UIColor.White;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            InitializeNLog();

            _logger = LogManager.GetCurrentClassLogger();
            _logger.Trace("Sample trace message");
            _logger.Debug("Sample debug message");
            _logger.Info("Sample informational message");
            _logger.Warn("Sample warning message");
            _logger.Error("Sample error message");
            _logger.Fatal("Sample fatal error message");
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "test.txt");
            File.WriteAllText(file, "THIS IS A TESTING THING INTO FILE!");
            _button = UIButton.FromType(UIButtonType.RoundedRect);

            _button.Frame = new RectangleF(
                View.Frame.Width / 2 - ButtonWidth / 2,
                View.Frame.Height / 2 - ButtonHeight / 2,
                ButtonWidth,
                ButtonHeight);

            _button.SetTitle("Click me", UIControlState.Normal);

            _button.TouchUpInside += (object sender, EventArgs e) =>
            {
                _button.SetTitle(String.Format("clicked {0} times", _numClicks++), UIControlState.Normal);
                _logger.Info(String.Format("clicked {0} times", _numClicks++));
            };

            _button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(_button);
        }


        private static void InitializeNLog()
        {
            var saveDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            SimpleConfigurator.ConfigureForTargetLogging(
                new FileTarget()
                {
                    FileName = Path.Combine(saveDir,"${shortdate}.csv")
                    ,
                    Layout = new CsvLayout()
                    {
                        Columns =
                                    {
                                        new CsvColumn("Time", "${longdate}"),
                                        new CsvColumn("Level", "${level}"),
                                        new CsvColumn("Lessage", "${message}"),
                                        new CsvColumn("Logger", "${logger}")
                                    },
                    }
                },
                    LogLevel.Info);
        }

    }
}

