using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VvvfSimulator;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqTable;

namespace VvvfSimulator.GUI.Create.Waveform.Async
{
    /// <summary>
    /// Control_Async_Table.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlAsyncCarrierTable : UserControl
    {
        View_Data vd = new View_Data();
        public class View_Data {
            public List<YamlAsyncParameterCarrierFreqTableValue> Async_Table_Data { get; set; } = new List<YamlAsyncParameterCarrierFreqTableValue>();
        }

        YamlControlData target;
        public ControlAsyncCarrierTable(YamlControlData data)
        {
            InitializeComponent();

            vd.Async_Table_Data = data.AsyncModulationData.CarrierWaveData.CarrierFrequencyTable.CarrierFrequencyTableValues;
            DataContext = vd;
            target = data;

        }

        private void DataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            target.AsyncModulationData.CarrierWaveData.CarrierFrequencyTable.CarrierFrequencyTableValues = vd.Async_Table_Data;
        }
    }
}
