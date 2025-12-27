using NextMidi.Data;
using NextMidi.Data.Track;
using NextMidi.Filing.Midi;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.TaskViewer;
using VvvfSimulator.GUI.Util;
using static VvvfSimulator.Generation.GenerateCommon;
using Button = System.Windows.Controls.Button;
using DataFormats = System.Windows.DataFormats;

namespace VvvfSimulator.GUI.MIDIConvert
{
    /// <summary>
    /// MIDIConvert_Main.xaml の相互作用ロジック
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
        }

        public static bool Conversion(String midi_path, String output_path, MidiConvertConfig midi_Convert_Config)
        {
            //MIDIDataを変換
            MidiData midiData;
            try
            {
                midiData = MidiReader.ReadFrom(midi_path);
            }
            catch
            {
                DialogBox.Show(LanguageManager.GetString("MidiConvert.Main.Message.MidiConvertError"), LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                return false;
            }

            String file_path = output_path.Replace(Path.GetExtension(output_path) , "");

            TrackCollection tracks = midiData.Tracks;

            for(int i = 0; i < tracks.Count; i++)
            {
                int priority = 1;
                while (true)
                {
                    BaseFrequency.LoadMidi.MidiLoadData loadData = new()
                    {
                        track = i,
                        division = 1,
                        path = midi_path,
                        priority = priority,
                    };
                    Data.BaseFrequency.Struct? ymd = Data.BaseFrequency.MidiConverter.Convert(loadData);
                    if (ymd == null) return false;
                    if (ymd.Points.Count == 0) break;

                    GenerationParameter generationBasicParameter = new(
                        ymd.GetCompiled(),
                        Data.Vvvf.Manager.DeepClone(Data.Vvvf.Manager.Current), 
                        Data.TrainAudio.Manager.DeepClone(Data.TrainAudio.Manager.Current),
                        new GenerationParameter.ProgressData()
                    );

                    string task_description = string.Format(LanguageManager.GetString("MidiConvert.TaskDescription.Convert.Description"), Path.GetFileNameWithoutExtension(midi_path), i, priority);
                    String export_path = Path.GetFullPath(file_path + "_" + i.ToString() + "_" + priority.ToString() + Path.GetExtension(output_path));

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Audio.VvvfSound.Audio.ExportWavLine(generationBasicParameter, midi_Convert_Config.SampleFrequency, midi_Convert_Config.ExportRaw, export_path);
                            System.Media.SystemSounds.Beep.Play();
                        }
                        catch(Exception ex)
                        {
                            DialogBox.Show(task_description + "\r\n" + ex.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.Progress, task_description);
                    TaskViewer.TaskViewer.TaskList.Add(taskProgressData);

                    if(!midi_Convert_Config.MultiThread) task.Wait();

                    priority++;
                }
                
            }

            return true;
        }

        public MidiConvertConfig config = new();
        public class MidiConvertConfig
        {
            public bool MultiThread { get; set; } = true;
            public int SampleFrequency { get; set; } = 192000;
            public bool ExportRaw { get; set; } = false;
        }

        private string? midi_path;
        private bool midi_selected = false;
        private string? export_path;
        private bool export_selected = false;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = (Button)sender;
            if (btn == null) return;
            String? tag = btn.Tag.ToString();
            if(tag == null) return;

            if (tag.Equals("Browse"))
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Midi (*.mid)|*.mid|All (*.*)|*.*",
                };
                if(dialog.ShowDialog() == false) return;
                String path = dialog.FileName;
                if (path.Length == 0) return;

                midi_path = path;
                midi_selected = true;
            }
            else if (tag.Equals("Select"))
            {
                var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "wav (*.wav)|*.wav" };
                if (dialog.ShowDialog() == false) return;

                String path = dialog.FileName;
                if(path.Length == 0) return;

                export_path = path;
                export_selected = true;
                
            }
            else if (tag.Equals("Config"))
            {
                Config configWindow = new(this, config);
                configWindow.ShowDialog();
            }
            else if (tag.Equals("Convert"))
            {
                if(midi_path == null || export_path == null) return;

                Action action = new(() => Conversion((string)midi_path.Clone(), (string)export_path.Clone(), config));

                if (config.MultiThread) action();
                else Task.Run(action);

                System.Media.SystemSounds.Beep.Play();
                Close();
            }

            BtnConvert.IsEnabled = export_selected && midi_selected;
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
                Close();
            else if (tag.Equals("Maximize"))
            {
                if (WindowState.Equals(WindowState.Maximized))
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else if (tag.Equals("Minimize"))
                WindowState = WindowState.Minimized;
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string path = (((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0) ?? "").ToString() ?? "";
            if (path.ToLower().EndsWith(".yaml"))
            {
                midi_path = path;
                midi_selected = true;
            }
        }
    }
}
