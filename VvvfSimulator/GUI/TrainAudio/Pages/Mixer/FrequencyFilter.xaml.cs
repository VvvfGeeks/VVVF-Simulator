using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.Data.TrainAudio;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Mixer
{
    /// <summary>
    /// TrainAudio_Filter_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class FrequencyFilter : Page
    {
        readonly Struct data;
        public FrequencyFilter(Struct train_Harmonic_Data)
        {
            data = train_Harmonic_Data;

            InitializeComponent();
            filterType_Selector.ItemsSource = FriendlyNameConverter.GetFrequencyFilterTypeNames();

            try
            {
                Filter_DataGrid.ItemsSource = data.Filters;
            }
            catch
            {

            }
        }
    }
}
