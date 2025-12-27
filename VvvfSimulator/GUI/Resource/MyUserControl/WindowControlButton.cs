using System.Windows;
using System.Windows.Controls;

namespace VvvfSimulator.GUI.Resource.MyUserControl
{
    internal class WindowControlButton : Button
    {
        public WindowControlButton()
        {
        }

        public static readonly DependencyProperty BorderCornerProperty = DependencyProperty.Register(nameof(BorderCorner), typeof(CornerRadius), typeof(WindowControlButton), new UIPropertyMetadata(new CornerRadius(0)));
        public CornerRadius BorderCorner
        {
            get { return (CornerRadius)GetValue(BorderCornerProperty); }
            set { SetValue(BorderCornerProperty, value); }
        }
    }
}
