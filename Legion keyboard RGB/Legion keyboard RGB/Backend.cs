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

        private const string APP_VERSION = "0.1";

        private const string L_ARG_FPS_OVERRIDE = "fps";
        private const string L_ARG_LOG_WARNINGS = "logwarns";
        private const string L_ARG_COL_CORR_LIGHTNESS = "light";
        private const string L_ARG_COL_CORR_SATURATION = "sat";

        private bool logWarnings = false;

        private string dataFolderPath = Environment.GetEnvironmentVariable("appdata") + "/GhostMiner/Ambient KB/";
        private string outputLogFileNane = "output.log";

        private Bitmap? screenBitmap;
        private Bitmap? scaledBitmap;
        private Graphics? screenGraphics;
        private Graphics? scaledGraphics;
        private Rectangle screenBounds;

        private System.Drawing.Size downscaleSize = new System.Drawing.Size(64, 30);
        private int bottomMargin  = 40; // Taskbar height
        private int topMargin = 30; // Titlebar height

        private float desiredFrametime = 50f;
        private float currentExecTime  = 0f;

        private float loopFps = 0f;
        private float execTimeFPS = 0f;
        private float remainingTime = 0f;

        private float colCorr_saturatiomMultiplier = 1f;
        private float colCorr_lightnessMultiplier = 1f;
        private bool appQuitting = false;

        public void Initialize(string[] args)
        {
            //args = ["sat=2", "light=1,25", "logwarns", "fps=60"];

            CheckLaunchArguments(args);
            InitApp();
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

                Color[] averageColours = FastAverager.GetSliceAverages(scaledBitmap);
                Color[] correctedColours = new Color[averageColours.Length];

                for (int i = 0; i < averageColours.Length; i++)
                {
                    correctedColours[i] = ColourCorrection.AdjustSaturationAndLightness(averageColours[i], colCorr_saturatiomMultiplier, colCorr_lightnessMultiplier);
                }

                BacklightDriver.SetBacklightColour(correctedColours[0], correctedColours[1], correctedColours[2], correctedColours[3]);

#if DEBUG
                // Debug stuff
                CreateAvgColBitmap (averageColours);
                CreateCorrColBitmap(correctedColours);
#endif

                execTimeStopwatch.Stop();
                currentExecTime = execTimeStopwatch.ElapsedMilliseconds;

                execTimeFPS = 1000 / currentExecTime;
                remainingTime = desiredFrametime - currentExecTime;
                loopFps = 1000 / (remainingTime + currentExecTime);
                float currentMs = 1000f / execTimeFPS;
                if (remainingTime < 0f)
                {
                    loopFps = 1000 / (remainingTime * -1 + currentExecTime);
                }

                MainWindow.Instance.UpdateStatus(GetCurrentSystemTime(), execTimeFPS, currentMs, averageColours, remainingTime, correctedColours, colCorr_saturatiomMultiplier, colCorr_lightnessMultiplier, topMargin, bottomMargin);



                if (remainingTime < 0f && logWarnings == true)
                {
                    MessageLogger.LogMessage($"[{GetCurrentSystemTime()}] Cannot keep up! Running {(remainingTime * -1).ToString("00.0")}ms behind! " +
                                             $"({loopFps.ToString("0.0")} FPS)");
                }

                await Task.Delay((int)Math.Clamp(desiredFrametime - remainingTime, 10, desiredFrametime));
            } // loop end

        } // Main() end

        [Obsolete]
        private void UpdateStatus(string currentFps, Color[] avgCols, float remainingTime)
        {
            throw new Exception("Obsolete method. Also not compatible with WPF");

            if (appQuitting == true)
            {
                return;
            }

            Console.SetWindowSize(85, 12);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(0, 0);

            float currentMs = 1000f / float.Parse(currentFps);

            Console.Write($"\n--- Ambient keyboard v. {APP_VERSION} ------------------------------------------\n" +
                          $" System time: {GetCurrentSystemTime()}                          \n" +
                          $" Current FPS: {currentFps} ({currentMs.ToString("0.0")}ms)      \n" +
                          $" Colours:     {GetZoneColoursString(avgCols)}                   \n" +
                          $" Correction:  Saturation {colCorr_saturatiomMultiplier * 100f}% \n" +
                          $"              Lightness  {colCorr_lightnessMultiplier * 100f}%  \n" +
                          $"-----------------------------------------------------------------------" +
                          $"\n Made by GhostMiner - https://Ghost-Miner.github.io/");

            if (remainingTime <= -1f && remainingTime > -10f)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (remainingTime <= -10f)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
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

        #region Mostly debug stuff - Log zone colours and save as images
        private string GetZoneColoursString(Color[] _avgColours)
        {
            string zoneColours = $"1: {_avgColours[0].R} {_avgColours[0].G} {_avgColours[0].B} | " +
                                 $"2: {_avgColours[1].R} {_avgColours[1].G} {_avgColours[1].B} | " +
                                 $"3: {_avgColours[2].R} {_avgColours[2].G} {_avgColours[2].B} | " +
                                 $"4: {_avgColours[3].R} {_avgColours[3].G} {_avgColours[3].B}";

            return zoneColours;
        }

        Bitmap zone1Bitmap = new Bitmap(10, 10);
        Bitmap zone2Bitmap = new Bitmap(10, 10);
        Bitmap zone3Bitmap = new Bitmap(10, 10);
        Bitmap zone4Bitmap = new Bitmap(10, 10);

#if DEBUG
        private void CreateAvgColBitmap(Color[] avgColor)
        {
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            using (Graphics gfx = Graphics.FromImage(zone1Bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(avgColor[0].R, avgColor[0].G, avgColor[0].B)))
            {
                gfx.FillRectangle(brush, 0, 0, 10, 10);
            }

            using (Graphics gfx = Graphics.FromImage(zone2Bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(avgColor[1].R, avgColor[1].G, avgColor[1].B)))
            {
                gfx.FillRectangle(brush, 0, 0, 10, 10);
            }

            using (Graphics gfx = Graphics.FromImage(zone3Bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(avgColor[2].R, avgColor[2].G, avgColor[2].B)))
            {
                gfx.FillRectangle(brush, 0, 0, 10, 10);
            }
            using (Graphics gfx = Graphics.FromImage(zone4Bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(avgColor[3].R, avgColor[3].G, avgColor[3].B)))
            {
                gfx.FillRectangle(brush, 0, 0, 10, 10);
            }

            try
            {
                zone1Bitmap.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\avgCol1.jpg", ImageFormat.Png);
                zone2Bitmap.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\avgCol2.jpg", ImageFormat.Png);
                zone3Bitmap.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\avgCol3.jpg", ImageFormat.Png);
                zone4Bitmap.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\avgCol4.jpg", ImageFormat.Png);

                scaledBitmap?.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\scaled.jpg", ImageFormat.Png);
            }
            catch (Exception ex)
            {
                MessageLogger.LogMessage($"[{GetCurrentSystemTime()}] [ERROR] {ex.Message}");
            }
        }
#endif

#if DEBUG
        private void CreateCorrColBitmap(Color[] corrColour)
        {
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            using (Graphics gfx = Graphics.FromImage(zone1Bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(corrColour[0].R, corrColour[0].G, corrColour[0].B)))
            {
                gfx.FillRectangle(brush, 0, 0, 10, 10);
            }

            using (Graphics gfx = Graphics.FromImage(zone2Bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(corrColour[1].R, corrColour[1].G, corrColour[1].B)))
            {
                gfx.FillRectangle(brush, 0, 0, 10, 10);
            }

            using (Graphics gfx = Graphics.FromImage(zone3Bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(corrColour[2].R, corrColour[2].G, corrColour[2].B)))
            {
                gfx.FillRectangle(brush, 0, 0, 10, 10);
            }
            using (Graphics gfx = Graphics.FromImage(zone4Bitmap))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(corrColour[3].R, corrColour[3].G, corrColour[3].B)))
            {
                gfx.FillRectangle(brush, 0, 0, 10, 10);
            }

            try
            {
                zone1Bitmap.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\corrCol1.jpg", ImageFormat.Png);
                zone2Bitmap.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\corrCol2.jpg", ImageFormat.Png);
                zone3Bitmap.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\corrCol3.jpg", ImageFormat.Png);
                zone4Bitmap.Save("D:\\Uzivatel\\Admin\\Dokumenty\\GitHub\\Ghost-Miner.github.io\\kbTest\\corrCol4.jpg", ImageFormat.Png);
            }
            catch (Exception ex)
            {
                MessageLogger.LogMessage($"[{GetCurrentSystemTime()}] [ERROR] {ex.Message}");
            }
        }
#endif
        #endregion

        #region Init stuff
        private void CheckLaunchArguments(string[] _launchArgs)
        {
            if (_launchArgs.Length > 0)
            {
                foreach (string arg in _launchArgs)
                {
                    string[] argumentParts = arg.Split("=");
                    string argumentName = argumentParts[0];
                    string argumentValueStr = "";
                    float argumentValueFloat = 1f;

                    if (argumentParts.Length > 1)
                    {
                        argumentValueStr = argumentParts[1];
                    }

                    if (float.TryParse(argumentValueStr, out float result))
                    {
                        argumentValueFloat = result;
                    }

                    switch (argumentName)
                    {
                        case L_ARG_LOG_WARNINGS:
                            logWarnings = true;
                            break;

                        case L_ARG_COL_CORR_SATURATION:
                            colCorr_saturatiomMultiplier = Math.Clamp(argumentValueFloat, 0.1f, 10f);
                            break;

                        case L_ARG_COL_CORR_LIGHTNESS:
                            colCorr_lightnessMultiplier = Math.Clamp(argumentValueFloat, 0.1f, 10f);
                            break;

                        case L_ARG_FPS_OVERRIDE:
                            desiredFrametime = 1000 / Math.Clamp(argumentValueFloat, 1f, 100f);
                            break;
                    }
                }
            }
        }

        private void InitApp()
        {
            //AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;

            //Console.CursorVisible = false; 
            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine("Starting...");

            CheckIfDataFolderExists();
            InitializeGraphicsObjects();

            MessageLogger.Init(dataFolderPath + outputLogFileNane);

            BacklightDriver.InitializeKeybord(); // Initialize the driver and get the correct keyboard
            BacklightDriver.SetKeyboardEffect(); // Set the effect to  
            BacklightDriver.SetBacklightBrightness(2); // Set brightness to high
            BacklightDriver.SetBacklightColour(Color.White, Color.White, Color.White, Color.White);

            //Console.Clear();
            //Console.WriteLine("Ready.");
            //Console.WriteLine("Press any key to start.");
            //Console.Read();

            //Console.Clear();
        }

        private void InitializeGraphicsObjects()
        {
            //screenBounds = Screen.PrimaryScreen.Bounds;
            screenBounds = new Rectangle(0, 0, (int)SystemParameters.PrimaryScreenHeight, (int)SystemParameters.PrimaryScreenHeight);

            screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height - (bottomMargin + topMargin)); // Subtract titlebar and taskbar height px
            scaledBitmap = new Bitmap(downscaleSize.Width, downscaleSize.Height);

            screenGraphics = Graphics.FromImage(screenBitmap);
            scaledGraphics = Graphics.FromImage(scaledBitmap);

            scaledGraphics.PixelOffsetMode = PixelOffsetMode.Half;
            scaledGraphics.InterpolationMode = InterpolationMode.Bicubic;
            scaledGraphics.CompositingMode = CompositingMode.SourceCopy;
        }
        #endregion

        #region App shutdown stuff
        //private  void AppDomain_ProcessExit(object? sender, EventArgs e)
        //{
        //    PerformAppCleanup();
        //}

        //private  void PerformAppCleanup()
        //{
        //    appQuitting = true;
        //    Console.Clear();

        //    Console.SetCursorPosition(1, 1);
        //    Console.WriteLine("Cleaning up...");

        //    MessageLogger.AppQuit();
        //}
        #endregion
    }
}