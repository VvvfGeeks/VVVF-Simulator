using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using VvvfSimulator.GUI.Resource.Language;

namespace VvvfSimulator.GUI.Util
{
    /// <summary>
    /// BitmapViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class BitmapViewer : Window
    {
        private readonly ViewModel BindingData = new();
        private class ViewModel : INotifyPropertyChanged
        {
            private BitmapFrame? _Image;
            public BitmapFrame? Image { get { return _Image; } set { _Image = value; RaisePropertyChanged(nameof(Image)); } }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        };
        public BitmapViewer()
        {
            DataContext = BindingData;
            InitializeComponent();
        }

        public void SetImage(Bitmap image)
        {
            using Stream st = new MemoryStream();
            image.Save(st, ImageFormat.Bmp);
            st.Seek(0, SeekOrigin.Begin);
            var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BindingData.Image = data;
        }
        public void SetWindowSize(int width, int height)
        {
            Dispatcher.Invoke(() =>
            {
                double ControlRatio = (double)width / height;

                if (ControlRatio > 1)
                {
                    Width = SystemParameters.WorkArea.Width * 2 / 4;
                    Height = Width / ControlRatio;
                }
                else
                {
                    Height = SystemParameters.WorkArea.Height * 2 / 4;
                    Width = Height * ControlRatio;
                }

            });
        }

        public void SetTitle(string title)
        {
            Dispatcher.Invoke(() =>
            {
                Title = title;
            });
        }
    }

    public class BitmapViewerManager
    {
        public readonly BitmapViewer Viewer = new();
        public void Show()
        {
            Viewer.Dispatcher.Invoke(() =>
            {
                Viewer.Show();
            });
        }
        public void Close()
        {
            Viewer.Dispatcher.Invoke(() =>
            {
                Viewer.Close();
            });
        }

        private bool require_resize = true;
        public void SetImage(Bitmap image, string title)
        {
            if (require_resize)
            {
                Viewer.SetWindowSize(image.Width, image.Height);
                Viewer.SetTitle(title);
                require_resize = false;
            }

            Viewer.SetImage(image);
        }

        public void SetImage(Bitmap image)
        {
            if (require_resize)
            {
                Viewer.SetWindowSize(image.Width, image.Height);
                Viewer.SetTitle(LanguageManager.GetString("BitmapViewer.Title.Default"));
                require_resize = false;
            }

            Viewer.SetImage(image);
        }
    }
}
