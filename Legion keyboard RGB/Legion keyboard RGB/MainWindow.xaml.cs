using DrawingColor = System.Drawing.Color;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using System.Data;
using System.Drawing;
using System.Security.Policy;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Legion_keyboard_RGB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Instance = this;

            Backend.Instance.Initialize(["sat=2", "light=1,75", "logwarns", "fps=30"]);
            
            StartBackendLoop(this, EventArgs.Empty);
        }

        private string boundText;

        public string BoundText
        {
            get { return boundText; }
            set
            {
                boundText = value;
                OnPropertychanged();
            }
        }

        private void OnPropertychanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private async void StartBackendLoop(object sender, EventArgs e)
        {
            await Backend.Instance.MainFunction();
        }

        //internal void UpdateUI(string _currentTime, int _currentFps, float _currentMs, DrawingColor[] _avgCols, float _remainingTime, DrawingColor[] _correctedCols, 
        //                       float _saturationValue, float _lightnessValue, float _topMargin, float _bottomMargin, int _desiredFps)
        //{
        //    systemTimeText.Text  = $"Time: {_currentTime}";
        //    refreshRateText.Text = $"FPS: {_currentFps}/{_desiredFps} ({_currentMs:0.00}ms) ";

        //    lightnessValueField .Text = _lightnessValue.ToString();
        //    saturationValueField.Text = _saturationValue.ToString();

        //    topMarginvalueField   .Text = _topMargin.ToString();
        //    bottomMarginvalueField.Text = _bottomMargin.ToString();

        //    statusTextDisplay.Text = $"Saturation: {_saturationValue}\n" +
        //                             $"Lightness: {_lightnessValue}\n" +
        //                             $"Top margin: {_topMargin}\n" +
        //                             $"Bottom margin: {_bottomMargin}";

        //    // Rectangles
        //    System.Windows.Shapes.Rectangle[] averageColoursRectangles = [avgColour_1, avgColour_2, avgColour_3, avgColour_4];

        //    for (int i = 0; i < averageColoursRectangles.Length && i < _avgCols.Length; i++)
        //    {
        //        DrawingColor col = _avgCols[i];
        //        averageColoursRectangles[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B));
        //    }

        //    System.Windows.Shapes.Rectangle[] correctedColourRectamgles = [correctedColour_1, correctedColour_2, correctedColour_3, correctedColour_4];

        //    for (int i = 0; i < correctedColourRectamgles.Length && i < _correctedCols.Length; i++)
        //    {
        //        DrawingColor col = _correctedCols[i];
        //        correctedColourRectamgles[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B));
        //    }
        //}

        public void UpdateUI(UIValues UiValues)
        {
            systemTimeText .Text = $"Time: {UiValues.currentTime}";
            refreshRateText.Text = $"FPS: {UiValues.currentFramerate:0.0}/{UiValues.desiredFramerate:0.0} ({UiValues.currentFrameTime:0.00}ms) ";

            statusTextDisplay.Text = $"Saturation: {UiValues.saturationMultiplier}\n" +
                                     $"Lightness: {UiValues.lightnessMultiplier}\n" +
                                     $"Top margin: {UiValues.topMargin}\n" +
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
        }
    } // class end
} // namespace end