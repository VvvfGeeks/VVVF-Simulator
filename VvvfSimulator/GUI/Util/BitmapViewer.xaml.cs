using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        public event RoutedEventHandler? SettingMenuClicked;
        private readonly bool EnableSettingMenu;
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
        public BitmapViewer(bool settingMenu)
        {
            DataContext = BindingData;
            InitializeComponent();

            EnableSettingMenu = settingMenu;
            Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/GUI/Resource/Dictionary/ContextMenu.xaml", UriKind.Relative)
            });
            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            ContextMenu contextMenu = new()
            {
                Style = (Style)FindResource("SlimContextMenu")
            };

            if (EnableSettingMenu)
            {
                MenuItem settingItem = new()
                {
                    Header = LanguageManager.GetString("BitmapViewer.ContextMenu.Setting")
                };

                settingItem.Click += (s, e) =>
                {
                    SettingMenuClicked?.Invoke(s, e);
                };

                contextMenu.Items.Add(settingItem);
                contextMenu.Items.Add(new Separator());
            }

            MenuItem minimizeItem = new()
            {
                Header = LanguageManager.GetString("BitmapViewer.ContextMenu.Minimize")
            };

            minimizeItem.Click += (_, _) =>
            {
                WindowState = WindowState.Minimized;
            };

            MenuItem maximizeItem = new()
            {
                Header = LanguageManager.GetString("BitmapViewer.ContextMenu.Maximize")
            };

            maximizeItem.Click += (_, _) =>
            {
                WindowState = WindowState.Maximized;
            };

            MenuItem closeItem = new()
            {
                Header = LanguageManager.GetString("BitmapViewer.ContextMenu.Close")
            };

            closeItem.Click += (_, _) => Close();

            contextMenu.Items.Add(minimizeItem);
            contextMenu.Items.Add(maximizeItem);
            contextMenu.Items.Add(closeItem);

            ContextMenu = contextMenu;
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

    public class BitmapViewerManager(bool SettingMenu = false)
    {
        public readonly BitmapViewer Viewer = new(SettingMenu);
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
