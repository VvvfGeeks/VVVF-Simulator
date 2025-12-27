using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VvvfSimulator.Generation;
using VvvfSimulator.GUI.Resource.Class;
using static VvvfSimulator.Generation.Audio.RealTime;
using static VvvfSimulator.Generation.Video.FS.GenerateFourierSeries;

namespace VvvfSimulator.GUI.Simulator.RealTime.UniqueWindow
{
    /// <summary>
    /// RealTime_FFT_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class Fs : Window, IRealtimeDisplay
    {
        private ViewModel BindingData = new();
        private readonly Parameter _Parameter;
        public class ViewModel : ViewModelBase
        {

            private BitmapFrame? _Image;
            public BitmapFrame? Image { get { return _Image; } set { _Image = value; RaisePropertyChanged(nameof(Image)); } }
        };
        public Fs(Parameter Parameter)
        {
            _Parameter = Parameter;

            InitializeComponent();
            DataContext = BindingData;
        }

        public void Start()
        {
            Task.Run(() => {
                while (!_Parameter.Quit)
                {
                    UpdateControl();
                }
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        Close();
                    });
                }
                catch { }
            });
        }

        private bool Resized = false;
        private int N = 100;
        private string StrCoefficients = "C = [0]";
        private void UpdateControl()
        {
            Vvvf.Model.Struct.Domain Domain = _Parameter.Control.Clone();
            Data.Vvvf.Struct ysd = _Parameter.VvvfSoundData;
            double[] Coefficients = GenerateBasic.Fourier.GetFourierCoefficients(Domain, 10000, N);
            StrCoefficients = GenerateBasic.Fourier.GetDesmosFourierCoefficientsArray(ref Coefficients);
            Bitmap image = GetImage(ref Coefficients);

            if (!Resized)
            {
                Dispatcher.Invoke(() =>
                {
                    Height = image.Height / 2;
                    Width = image.Width / 2;
                });
                Resized = true;
            }

            using (Stream st = new MemoryStream())
            {
                image.Save(st, ImageFormat.Bmp);
                st.Seek(0, SeekOrigin.Begin);
                var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                BindingData.Image = data;
            }

            image.Dispose();
        }

        private void Button_CopyCoefficients_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    Clipboard.SetText(StrCoefficients);
                }
                catch { }
            });
        }

        private void TextBox_N_TextChanged(object sender, TextChangedEventArgs e)
        {
            N = ParseTextBox.ParseInt(TextBox_N);
        }
    }
}
