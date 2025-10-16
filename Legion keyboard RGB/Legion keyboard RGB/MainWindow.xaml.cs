using DrawingColor = System.Drawing.Color;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace Legion_keyboard_RGB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow Instance;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<OnInputfieldsChangedEventArgs> OnInputfieldsChanged;

        public class OnInputfieldsChangedEventArgs : EventArgs
        {
            public required string saturationfieldValue;
            public required string lightnessFieldValue;
            public required string topMarginFieldValue;
            public required string bottomMarginFieldValue;
            public required string freamerateFieldValue;
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Instance = this;

            Backend.Instance.Initialize();
            
            StartBackendLoop(this, EventArgs.Empty);
        }


        private string saturationfieldValue;
        public string SaturationfieldValue
        {
            get { return saturationfieldValue; }
            set
            {
                saturationfieldValue = value; 
                OnPropertychanged();
            }
        }

        private string lightnessFieldValue;
        public string LightnessFieldValue
        {
            get { return lightnessFieldValue; }
            set
            {
                lightnessFieldValue = value;
                OnPropertychanged();
            }
        }

        private string topMarginFieldValue;
        public string TopMarginFieldValue
        {
            get { return topMarginFieldValue; }
            set
            {
                topMarginFieldValue = value;
                OnPropertychanged();
            }
        }

        private string bottomMarginFieldValue;
        public string BottomMarginFieldValue
        {
            get { return bottomMarginFieldValue; }
            set
            {
                bottomMarginFieldValue = value;
                OnPropertychanged();
            }
        }
        private string fpsFieldValue;
        public string FPSFieldValue
        {
            get { return fpsFieldValue; }
            set
            {
                fpsFieldValue = value;
                OnPropertychanged();
            }
        }




        private void OnPropertychanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnInputfieldsChanged?.Invoke(this, new OnInputfieldsChangedEventArgs()
            {
                bottomMarginFieldValue = bottomMarginFieldValue,
                lightnessFieldValue    = lightnessFieldValue,
                topMarginFieldValue    = topMarginFieldValue,
                saturationfieldValue   = saturationfieldValue,
                freamerateFieldValue   = fpsFieldValue,
            });
        }


        private async void StartBackendLoop(object sender, EventArgs e)
        {
            await Backend.Instance.MainFunction();
        }

        public void UpdateUI(UIValues UiValues)
        {
            systemTimeText .Text = $"Time: {UiValues.currentTime}";
            refreshRateText.Text = $"FPS: {UiValues.currentFramerate:0.0}/{UiValues.desiredFramerate:0.0} ({UiValues.currentFrameTime:0.00}ms) ";

            statusTextDisplay.Text = $"Saturation: {UiValues.saturationMultiplier * 100}%\t" +
                                     $"Lightness: {UiValues.lightnessMultiplier * 100}%\n" +
                                     $"Top margin: {UiValues.topMargin}\t" +
                                     $"Bottom margin: {UiValues.bottomMargin}";

            // Rectangles with the raw colors
            System.Windows.Shapes.Rectangle[] averageColoursRectangles = [avgColour_1, avgColour_2, avgColour_3, avgColour_4];

            for (int i = 0; i < averageColoursRectangles.Length && i < UiValues.averageColours.Length; i++)
            {
                DrawingColor col = UiValues.averageColours[i];
                averageColoursRectangles[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B));
            }

            // Rectangles with colour correction applied
            System.Windows.Shapes.Rectangle[] correctedColourRectamgles = [correctedColour_1, correctedColour_2, correctedColour_3, correctedColour_4];

            for (int i = 0; i < correctedColourRectamgles.Length && i < UiValues.correctedColours.Length; i++)
            {
                DrawingColor col = UiValues.correctedColours[i];
                correctedColourRectamgles[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B));
            }
        }

        public class UIValues
        {
            public string currentTime;

            public float currentFramerate;
            public float currentFrameTime;
            public float desiredFramerate;

            public DrawingColor[] averageColours;
            public DrawingColor[] correctedColours;

            public float saturationMultiplier;
            public float lightnessMultiplier;

            public float topMargin;
            public float bottomMargin;

            public int frameRate;
        }

        private void SaveSettingsbutton_Click(object sender, RoutedEventArgs e)
        {
            Backend.Instance.SaveSettings();
        }

        private void LoadSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Backend.Instance.LoadSavedSettings();
        }
    } // class end
} // namespace end