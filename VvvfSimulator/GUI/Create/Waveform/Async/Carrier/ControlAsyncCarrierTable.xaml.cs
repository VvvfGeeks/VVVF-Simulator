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
using static VvvfSimulator.Data.Vvvf.Struct;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.TableValue;

namespace VvvfSimulator.GUI.Create.Waveform.Async
{
    /// <summary>
    /// Control_Async_Table.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlAsyncCarrierTable : UserControl
    {
        ViewData Data = new ViewData();
        public class ViewData {
            public List<Parameter> Async_Table_Data { get; set; } = new List<Parameter>();
        }

        PulseControl target;
        public ControlAsyncCarrierTable(PulseControl data)
        {
            InitializeComponent();

            Data.Async_Table_Data = data.AsyncModulationData.CarrierWaveData.CarrierFrequencyTable.Table;
            DataContext = Data;
            target = data;

        }

        private void DataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            target.AsyncModulationData.CarrierWaveData.CarrierFrequencyTable.Table = Data.Async_Table_Data;
        }
    }
}
