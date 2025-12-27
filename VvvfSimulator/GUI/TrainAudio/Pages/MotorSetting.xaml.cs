using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.Data.TrainAudio;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Motor
{
    /// <summary>
    /// TrainAudio_MotorSound_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class MotorSetting : Page
    {
        readonly Struct TrainSoundData;
        readonly bool IgnoreUpdate = true;
        public MotorSetting(Struct Data)
        {
            InitializeComponent();
            TrainSoundData = Data;

            Struct.MotorSpecification Motor = Data.MotorSpec;
            V.Text = Motor.V.ToString();
            Rs.Text = Motor.Rs.ToString();
            Rr.Text = Motor.Rr.ToString();
            Ls.Text = Motor.Ls.ToString();
            Lr.Text = Motor.Lr.ToString();
            Lm.Text = Motor.Lm.ToString();
            Np.Text = Motor.Np.ToString();
            Damping.Text = Motor.Damping.ToString();
            Inertia.Text = Motor.Inertia.ToString();
            Fd.Text = Motor.Fd.ToString();
            Fc.Text = Motor.Fc.ToString();
            Fs.Text = Motor.Fs.ToString();
            StribeckOmega.Text = Motor.StribeckOmega.ToString();
            FricSmoothK.Text = Motor.FricSmoothK.ToString();

            MotorHarmonics.ItemsSource = Data.HarmonicSound;
            if (Data.HarmonicSound.Count > 0)
            {
                MotorHarmonics.SelectedIndex = 0;
                MotorHarmonicEditFrame.Navigate(new HarmonicSetting((Struct.HarmonicData)MotorHarmonics.SelectedItem,MotorHarmonics));
            }

            IgnoreUpdate = false;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            object Name = tb.Name;
            double Value = ParseTextBox.ParseDouble(tb);

            Struct.MotorSpecification Motor = TrainSoundData.MotorSpec;

            if (Name.Equals("V")) Motor.V = Value;
            else if (Name.Equals("Rs")) Motor.Rs = Value;
            else if (Name.Equals("Rr")) Motor.Rr = Value;
            else if (Name.Equals("Ls")) Motor.Ls = Value;
            else if (Name.Equals("Lr")) Motor.Lr = Value;
            else if (Name.Equals("Lm")) Motor.Lm = Value;
            else if (Name.Equals("Np")) Motor.Np = Value;
            else if (Name.Equals("Damping")) Motor.Damping = Value;
            else if (Name.Equals("Inertia")) Motor.Inertia = Value;
            else if (Name.Equals("Fd")) Motor.Fd = Value;
            else if (Name.Equals("Fc")) Motor.Fc = Value;
            else if (Name.Equals("Fs")) Motor.Fs = Value;
            else if (Name.Equals("StribeckOmega")) Motor.StribeckOmega = Value;
            else if (Name.Equals("FricSmoothK")) Motor.FricSmoothK = Value;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Item = (Struct.HarmonicData)MotorHarmonics.SelectedItem;
            if (Item == null) return;
            MotorHarmonicEditFrame.Navigate(new HarmonicSetting(Item, MotorHarmonics));
        }

        private void UpdateView()
        {
            MotorHarmonics.ItemsSource = TrainSoundData.HarmonicSound;
            MotorHarmonics.Items.Refresh();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            object Tag = ((MenuItem)sender).Tag;

            if (Tag.Equals("Add"))
            {
                TrainSoundData.HarmonicSound.Add(new Struct.HarmonicData());
                UpdateView();
            }
            else if (Tag.Equals("Remove"))
            {
                if (MotorHarmonics.SelectedIndex < 0) return;
                MotorHarmonicEditFrame.Navigate(null);
                TrainSoundData.HarmonicSound.RemoveAt(MotorHarmonics.SelectedIndex);
                UpdateView();  
            }
            else if (Tag.Equals("Clone"))
            {
                if (MotorHarmonics.SelectedIndex < 0) return;
                Struct.HarmonicData harmonic_Data = (Struct.HarmonicData)MotorHarmonics.SelectedItem;
                TrainSoundData.HarmonicSound.Add(harmonic_Data.Clone());
                UpdateView();
            }
        }
    }
}
