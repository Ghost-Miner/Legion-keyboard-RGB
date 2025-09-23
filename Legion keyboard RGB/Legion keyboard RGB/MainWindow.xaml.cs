using DrawingColor = System.Drawing.Color;
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
    public partial class MainWindow : Window
    {
        public static MainWindow Instance = null;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            Backend.Instance.Initialize(["sat=2", "light=2", "logwarns", "fps=20"]);
            
            BackendLoop(this, EventArgs.Empty);
        }

        private async void BackendLoop(object sender, EventArgs e)
        {
            await Backend.Instance.MainFunction();
        }

        public void UpdateStatus(string _currentTime, float _currentFps, float _currentMs, DrawingColor[] _avgCols, float _remainingTime, DrawingColor[] _correctedCols, float _saturationValue, float _lightnessValue, float _topMargin, float _bottomMargin)
        {
            systemTimeText.Text = $"Time: {_currentTime}";
            refreshRateText.Text = $"FPS: {_currentFps:0.0} ({_currentMs:0.00}ms)";

            lightnessValueField .Text = _lightnessValue.ToString();
            saturationValueField.Text = _saturationValue.ToString();

            topMarginvalueField   .Text = _topMargin.ToString();
            bottomMarginvalueField.Text = _bottomMargin.ToString();

            System.Windows.Shapes.Rectangle[] averageColoursRectangles = [avgColour_1, avgColour_2, avgColour_3, avgColour_4];

            for (int i = 0; i < averageColoursRectangles.Length && i < _avgCols.Length; i++)
            {
                DrawingColor col = _avgCols[i];
                averageColoursRectangles[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B));
            }

            System.Windows.Shapes.Rectangle[] correctedColourRectamgles = [correctedColour_1, correctedColour_2, correctedColour_3, correctedColour_4];

            for (int i = 0; i < correctedColourRectamgles.Length && i < _correctedCols.Length; i++)
            {
                DrawingColor col = _correctedCols[i];
                correctedColourRectamgles[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B));
            }
        }
    }
}