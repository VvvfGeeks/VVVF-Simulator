using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.Data.TrainAudio;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Gear
{
    /// <summary>
    /// TrainAudio_Gear_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class GearSetting : Page
    {
        private readonly Struct Configuration;
        private readonly Window Owner;
        public GearSetting(Window parent, Struct Configuration)
        {
            Owner = parent;
            this.Configuration = Configuration;
            InitializeComponent();

            Update_ListView();
        }


        private void Update_ListView()
        {
            Gear_Sound_List.ItemsSource = Configuration.GearSound;
            Gear_Sound_List.Items.Refresh();

            var item = (Struct.HarmonicData)Gear_Sound_List.SelectedItem;
            if (item == null) return;
            Gear_Edit_Frame.Navigate(new HarmonicSetting(item, Gear_Sound_List));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object tag = mi.Tag;

            if (tag.Equals("Add"))
            {
                Configuration.GearSound.Add(new Struct.HarmonicData());
                Update_ListView();
            }
            else if (tag.Equals("Remove"))
            {
                if (Gear_Sound_List.SelectedIndex < 0) return;
                Gear_Edit_Frame.Navigate(null);
                Configuration.GearSound.RemoveAt(Gear_Sound_List.SelectedIndex);
                Update_ListView();
            }
            else if (tag.Equals("Copy"))
            {
                if (Gear_Sound_List.SelectedIndex < 0) return;
                Struct.HarmonicData harmonic_Data = (Struct.HarmonicData)Gear_Sound_List.SelectedItem;
                Configuration.GearSound.Add(harmonic_Data.Clone());
                Update_ListView();
            }
            else if (tag.Equals("Calculate"))
            {
                GearCalculate taggw = new(Owner, 16,101);
                this.Opacity = 0.8;
                taggw.ShowDialog();
                this.Opacity = 1.0;
                Configuration.SetCalculatedGearHarmonic(taggw.Gear1, taggw.Gear2);
                Update_ListView();
            }
        }

        private void Gear_Sound_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (Struct.HarmonicData)Gear_Sound_List.SelectedItem;
            if (item == null) return;
            Gear_Edit_Frame.Navigate(new HarmonicSetting(item, Gear_Sound_List));
        }
    }
}
