using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.IO;

namespace Legion_keyboard_RGB
{
    internal class Backend
    {
        public static Backend Instance = new Backend();

        private bool   logWarnings = false;
        private string dataFolderPath = Environment.GetEnvironmentVariable("appdata") + "/GhostMiner/Ambient KB/";
        private string outputLogFileNane = "output.log";

        private Bitmap?   screenBitmap;
        private Bitmap?   scaledBitmap;
        private Graphics? screenGraphics;
        private Graphics? scaledGraphics;
        private Rectangle screenBounds;

        private Color[] averageColours;
        private Color[] correctedColours;

        private System.Drawing.Size downscaleSize = new System.Drawing.Size(64, 36); // FullHD gets scaled down 30x
        private int bottomMargin  = 40; // Taskbar height
        private int topMargin     = 30; // Titlebar height

        private float desiredFrametime = 50;
        private float currentExecTime  = 0f;

        private float currentFps = 0f;
        private float execTimeFPS = 0f;
        private float remainingTime = 0f;

        private float colCorr_saturatiomMultiplier = 1f;
        private float colCorr_lightnessMultiplier = 1f;
        private float currentMs = 00f;

        public void Initialize()
        {
            InitApp();

            MainWindow.Instance.PropertyChanged      += MainWindow_PropertyChanged;
            MainWindow.Instance.OnInputfieldsChanged += Instance_OnInputfieldsChanged;
        }

        private void Instance_OnInputfieldsChanged(object? sender, MainWindow.OnInputfieldsChangedEventArgs e)
        {
            float _satMultiplier   = colCorr_saturatiomMultiplier * 100;
            float _lightMultiplier = colCorr_lightnessMultiplier  * 100;
            int   _bottomMargin    = bottomMargin; 
            int   _topMargin       = topMargin;
            float _frameTime       = desiredFrametime;

            try { _satMultiplier   = float.Parse(e.saturationfieldValue);   }  catch (Exception) { _satMultiplier   = 100f; }
            try { _lightMultiplier = float.Parse(e.lightnessFieldValue);    }  catch (Exception) { _lightMultiplier = 100f; }
            try { _topMargin       = int  .Parse(e.topMarginFieldValue);    }  catch (Exception) { _topMargin       = 40;   }
            try { _bottomMargin    = int  .Parse(e.bottomMarginFieldValue); }  catch (Exception) { _bottomMargin    = 30;   }
            try { _frameTime       = float.Parse(e.freamerateFieldValue);   }  catch (Exception) { _frameTime       = 50f;  }

            UpdateOverrideValues(_satMultiplier / 100, _lightMultiplier / 100, _topMargin,_bottomMargin, _frameTime);
        }

        private void MainWindow_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateValuesForUI();
        }

        internal async Task MainFunction()
        {
            while (true)
            {
                Stopwatch execTimeStopwatch = new Stopwatch();
                execTimeStopwatch.Start();

                bool isUserDesktop = DesktopUtils.IsInteractiveDesktop();
                if (isUserDesktop)
                {
                    try
                    {
                        screenGraphics?.CopyFromScreen(screenBounds.X, (screenBounds.Y + topMargin), 0, 0, screenBounds.Size); // Remove titleBarHeight pixels from the top
                        scaledGraphics?.DrawImage(screenBitmap, 0, 0, downscaleSize.Width, downscaleSize.Height);
                    }
                    catch (Exception exception)
                    {
                        MessageLogger.LogMessage($"[{GetCurrentSystemTime()}] [ERROR] {exception.Message} {exception.StackTrace}");
                    }
                }

                averageColours = FastAverager.GetSliceAverages(scaledBitmap);
                correctedColours = new Color[averageColours.Length];

                for (int i = 0; i < averageColours.Length; i++)
                {
                    correctedColours[i] = ColourCorrection.AdjustSaturationAndLightness(averageColours[i], colCorr_saturatiomMultiplier, colCorr_lightnessMultiplier);
                }

                BacklightDriver.SetBacklightColour(correctedColours[0], correctedColours[1], correctedColours[2], correctedColours[3]);

                execTimeStopwatch.Stop();
                currentExecTime = execTimeStopwatch.ElapsedMilliseconds;

                execTimeFPS   = 1000 / currentExecTime;
                remainingTime = desiredFrametime - currentExecTime;
                currentFps    = 1000 / (remainingTime + currentExecTime);
                currentMs     = 1000f / execTimeFPS;

                if (remainingTime < 0f)
                {
                    currentFps = 1000 / (remainingTime * -1 + currentExecTime);
                }

                UpdateValuesForUI();

                if (remainingTime < 0f && logWarnings == true)
                {
                    MessageLogger.LogMessage($"[{GetCurrentSystemTime()}] Cannot keep up! Running {(remainingTime * -1).ToString("00.0")}ms behind! " +
                                             $"({currentFps.ToString("0.0")} FPS)");
                }

                /// Uncomment if you want to see the bitmaps update in real time.  A tool that can update the image at high rate is required.
                /// Might throw "General GDI+ exception" because this code is just writing to the same file too fast.
                //string bitmapSaveLocation = "D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\";
                //scaledBitmap?.Save(bitmapSaveLocation + "scaled.jpg",   ImageFormat.Jpeg);
                //screenBitmap?.Save(bitmapSaveLocation + "original.jpg", ImageFormat.Jpeg);

                await Task.Delay((int)Math.Clamp(desiredFrametime - remainingTime, 1, desiredFrametime));
            } // loop end

        } // Main() end


        private void UpdateValuesForUI()
        {
            MainWindow.Instance.UpdateUI(new MainWindow.UIValues
            {
                averageColours = averageColours,
                correctedColours = correctedColours,

                bottomMargin = bottomMargin,
                topMargin = topMargin,

                currentFramerate = currentFps,
                currentFrameTime = (1000 / currentFps), // 1 000 ms 
                currentTime = GetCurrentSystemTime(),
                desiredFramerate = (1000 / desiredFrametime), // 1 000 ms

                lightnessMultiplier = colCorr_lightnessMultiplier,
                saturationMultiplier = colCorr_saturatiomMultiplier,
            });
        }

        private void UpdateOverrideValues(float _satMmultiploer, float _lightMultiplier, int _topMargin, int _botttomMargin, float _fps)
        {
            if (_fps < 1f)
            {
                _fps = 1f;
            }
            colCorr_saturatiomMultiplier = _satMmultiploer;
            colCorr_lightnessMultiplier = _lightMultiplier;
            topMargin = _topMargin;
            bottomMargin = _botttomMargin;
            desiredFrametime = 1000 / _fps;
        }

        private void CheckIfDataFolderExists()
        {
            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }
        }

        public string GetCurrentSystemTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        #region Init stuff
        private void InitApp()
        {
            CheckIfDataFolderExists();
            InitializeGraphicsObjects();

            MessageLogger.Init(dataFolderPath + outputLogFileNane);

            BacklightDriver.InitializeKeybord(); // Initialize the driver and get the correct keyboard
            BacklightDriver.SetKeyboardEffect(); // Set the effect to  
            BacklightDriver.SetBacklightBrightness(2); // Set brightness to high
            BacklightDriver.SetBacklightColour(Color.White, Color.White, Color.White, Color.White);
        }

        private void InitializeGraphicsObjects()
        {
            screenBounds = new Rectangle(0, 0, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

            screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height - (bottomMargin + topMargin));
            scaledBitmap = new Bitmap(downscaleSize.Width, downscaleSize.Height);

            screenGraphics = Graphics.FromImage(screenBitmap);
            scaledGraphics = Graphics.FromImage(scaledBitmap);

            scaledGraphics.PixelOffsetMode = PixelOffsetMode.Half;
            scaledGraphics.InterpolationMode = InterpolationMode.Low;
            scaledGraphics.CompositingMode = CompositingMode.SourceCopy;
        }
        #endregion
    }
}