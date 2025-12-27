using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Simulator.RealTime;
using VvvfSimulator.GUI.TaskViewer;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Vvvf.Modulation;
using YamlDotNet.Core;
using static VvvfSimulator.Generation.Audio.RealTime;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationParameter;

namespace VvvfSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewData BindingData = new();
        public class ViewData : ViewModelBase
        {
            private bool _Blocked = false;
            public bool Blocked
            {
                get
                {
                    return _Blocked;
                }
                set
                {
                    _Blocked = value;
                    RaisePropertyChanged(nameof(Blocked));
                }
            }
        };

        private static MainWindow? Instance;
        public static MainWindow? GetInstance()
        {
            return Instance;
        }
        public static void Invoke(Action callBack)
        {
            Instance?.Dispatcher.Invoke(callBack);
        }
        public static void SetInteractive(bool val)
        {
            if (Instance == null) return;
            Instance.BindingData.Blocked = !val;
        }
        public static bool IsInteractable()
        {
            if (Instance == null) return false;
            return !Instance.BindingData.Blocked;
        }

        public MainWindow()
        {
            Instance = this;
            DataContext = BindingData;
            InitializeComponent();
            OnFirstLoad();
        }

        private void OnFirstLoad()
        {
            if (App.HasArgs) LoadYaml(App.StartupArgs[0]);
            CustomPwmPresets.Load();
            Generation.Video.Fonts.Manager.Load();
        }

        #region YamlHandle
        public void LoadYaml(string Path)
        {
            try
            {
                Data.Vvvf.Manager.LoadCurrent(Path);
                UpdateControlList();
                SettingContentViewer.Navigate(null);
                DialogBox.Show(this, LanguageManager.GetString("MainWindow.Message.File.Load.Ok.Message"), LanguageManager.GetString("MainWindow.Message.File.Load.Ok.Title"), [DialogBoxButton.Ok], DialogBoxIcon.Ok);
            }
            catch (YamlException ex)
            {
                string error_message = LanguageManager.GetString("MainWindow.Message.File.Load.Error.Message");
                error_message += "\r\n";
                error_message += "\r\n" + ex.End.ToString() + "\r\n";
                DialogBox.Show(this, error_message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string error_message = LanguageManager.GetString("MainWindow.Message.File.Load.Error.Message");
                error_message += "\r\n";
                error_message += "\r\n" + ex.Message + "\r\n";
                DialogBox.Show(this, error_message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
            }
        }
        public void SaveYaml(string Path, bool SaveAs)
        {
            string MessagePath = SaveAs ? "SaveAs" : "Save";
            if (Data.Vvvf.Manager.SaveCurrent(Path))
                DialogBox.Show(this,
                    LanguageManager.GetString("MainWindow.Message.File." + MessagePath + ".Ok.Message"),
                    LanguageManager.GetString("MainWindow.Message.File." + MessagePath + ".Ok.Title"),
                    [DialogBoxButton.Ok], DialogBoxIcon.Ok);
            else
                DialogBox.Show(this,
                    LanguageManager.GetString("MainWindow.Message.File." + MessagePath + ".Error.Message"),
                    LanguageManager.GetString("Generic.Title.Error"),
                    [DialogBoxButton.Ok], DialogBoxIcon.Error);
        }
        public bool SaveBefore(string MessagePath)
        {
            if (Data.Vvvf.Manager.IsCurrentEquivalent(
                Data.Vvvf.Manager.LoadPath.Length == 0 ? Data.Vvvf.Manager.Template : Data.Vvvf.Manager.LoadData))
                return true;

            DialogBoxButton? Result = DialogBox.Show(
                    this,
                    LanguageManager.GetString(MessagePath + ".Message"),
                    LanguageManager.GetString(MessagePath + ".Title"),
                    [DialogBoxButton.Yes, DialogBoxButton.No, DialogBoxButton.Cancel], DialogBoxIcon.Question);

            if (Result == DialogBoxButton.Yes)
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Data.Vvvf.Manager.GetLoadedYamlName()
                };
                if (dialog.ShowDialog() ?? false)
                {
                    SaveYaml(dialog.FileName, true);
                    return true;
                }
                else
                    return false;
            }
            else if (Result == DialogBoxButton.No)
                return true;
            else
                return false;
        }
        #endregion

        #region MenuClick
        private void File_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            object? tag = button.Tag;
            if (tag == null) return;
            if (tag.Equals("Load"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (SaveBefore("MainWindow.Message.File.SaveBefore.Load") && (dialog.ShowDialog() ?? false))
                    LoadYaml(dialog.FileName);
            }
            else if (tag.Equals("SaveAs"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Data.Vvvf.Manager.GetLoadedYamlName()
                };
                if (dialog.ShowDialog() ?? false)
                    SaveYaml(dialog.FileName, true);
            }
            else if (tag.Equals("Save"))
            {
                if (Data.Vvvf.Manager.LoadPath.Length == 0)
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Yaml (*.yaml)|*.yaml",
                        FileName = "VVVF"
                    };
                    if (dialog.ShowDialog() ?? false)
                        SaveYaml(dialog.FileName, true);
                }
                else
                    SaveYaml(Data.Vvvf.Manager.LoadPath, false);
            }
        }
        private static GenerationParameter GetGenerationBasicParameter()
        {
            return new(
                Data.BaseFrequency.Manager.Current.GetCompiled(),
                Data.Vvvf.Manager.DeepClone(Data.Vvvf.Manager.Current),
                Data.TrainAudio.Manager.DeepClone(Data.TrainAudio.Manager.Current),
                new ProgressData()
            );
        }
        private bool SolveCommand(string[] command)
        {
            if (command[0].Equals("VVVF"))
            {
                if (command[1].Equals("WAV"))
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "normal(192k)|*.wav|normal(5M)|*.wav|raw(192k)|*.wav|raw(5M)|*.wav",
                        FilterIndex = 1
                    };
                    if (dialog.ShowDialog() == false) return true;

                    int sample_freq = new int[] { 192000, 5000000, 192000, 5000000 }[dialog.FilterIndex - 1];
                    bool raw = new bool[] { false, false, true, true }[dialog.FilterIndex - 1];

                    GenerationParameter parameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            if (command[2].Equals("Line"))
                                Generation.Audio.VvvfSound.Audio.ExportWavLine(parameter, sample_freq, raw, dialog.FileName);
                            else if (command[2].Equals("Phases"))
                                Generation.Audio.VvvfSound.Audio.ExportWavPhases(parameter, sample_freq, raw, dialog.FileName);
                            else if (command[2].Equals("PhaseCurrent"))
                                Generation.Audio.VvvfSound.Audio.ExportWavPhaseCurrent(parameter, sample_freq, raw, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, parameter.Progress,
                        LanguageManager.GetString("MainWindow.TaskDescription.Generate.Audio.Vvvf." + command[2]) + Data.Vvvf.Manager.GetLoadedYamlName()
                    );
                    TaskViewer.TaskList.Add(taskProgressData);
                }

                else if (command[1].Equals("RealTime"))
                {
                    VvvfSoundParameter Param = new(
                        Properties.Settings.Default.RealTime_VVVF_EditAllow ? Data.Vvvf.Manager.Current : Data.Vvvf.Manager.DeepClone(Data.Vvvf.Manager.Current),
                        Data.TrainAudio.Manager.DeepClone(Data.TrainAudio.Manager.Current)
                    );

                    MainWindow.SetInteractive(false);

                    if (command.Length == 3)
                    {
                        try
                        {
                            GUI.Simulator.RealTime.Setting.ComPortSelector com = new(this);
                            com.ShowDialog();
                            Param.Port = new()
                            {
                                PortName = com.GetComPortName(),
                            };
                        }
                        catch
                        {
                            DialogBox.Show(this, LanguageManager.GetString("MainWindow.Menu.RealTime.VVVF.USB.Error.Message"), LanguageManager.GetString("MainWindow.Menu.RealTime.VVVF.USB.Error.Title"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                            return true;
                        }
                    }

                    GUI.Simulator.RealTime.Controller.IController Controller = GUI.Simulator.RealTime.Controller.Controller.GetWindow(
                        (GUI.Simulator.RealTime.Controller.ControllerStyle)Properties.Settings.Default.RealTime_VVVF_Controller_Style,
                        Param);
                    Controller.GetInstance().Show();
                    Controller.StartTask();

                    if (Properties.Settings.Default.RealTime_VVVF_WaveForm_Line_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.WaveFormLine(Param);
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_WaveForm_Phase_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.WaveFormPhase(Param);
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_Control_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.ControlStatus(
                            Param,
                            (RealtimeDisplay.ControlStatus.RealTimeControlStatStyle)Properties.Settings.Default.RealTime_VVVF_Control_Style,
                            Properties.Settings.Default.RealTime_VVVF_Control_Precise
                        );
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_Hexagon_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.Hexagon(
                            Param,
                            (RealtimeDisplay.Hexagon.RealTimeHexagonStyle)Properties.Settings.Default.RealTime_VVVF_Hexagon_Style,
                            Properties.Settings.Default.RealTime_VVVF_Hexagon_ZeroVector
                        );
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_FFT_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.Fft(Param);
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_FS_Show)
                    {
                        IRealtimeDisplay Display = new GUI.Simulator.RealTime.UniqueWindow.Fs(Param);
                        Display.Show();
                        Display.Start();
                    }

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Audio.VvvfSound.RealTime.Calculate(Param);
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }

                        MainWindow.SetInteractive(true);
                        SystemSounds.Beep.Play();
                    });

                    return Properties.Settings.Default.RealTime_VVVF_EditAllow;
                }
                else if (command[1].Equals("Setting"))
                {
                    GUI.Simulator.RealTime.Setting.Basic setting = new(this, GUI.Simulator.RealTime.Setting.Basic.RealTimeBasicSettingMode.VVVF);
                    setting.ShowDialog();
                }
            }
            else if (command[0].Equals("Train"))
            {
                if (command[1].Equals("WAV"))
                {

                    var dialog = new SaveFileDialog { Filter = "wav|*.wav|raw|*.wav" };
                    if (dialog.ShowDialog() == false) return true;

                    GenerationParameter parameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            bool raw = dialog.FilterIndex == 2;
                            Generation.Audio.TrainSound.Audio.ExportWavFile(parameter, 192000, raw, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, parameter.Progress, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Audio.Train") + Data.Vvvf.Manager.GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
                else if (command[1].Equals("RealTime"))
                {
                    TrainSoundParameter Param = new(
                        Properties.Settings.Default.RealTime_VVVF_EditAllow ? Data.Vvvf.Manager.Current : Data.Vvvf.Manager.DeepClone(Data.Vvvf.Manager.Current),
                        Data.TrainAudio.Manager.DeepClone(Data.TrainAudio.Manager.Current)
                    );

                    GUI.Simulator.RealTime.Controller.IController Controller = GUI.Simulator.RealTime.Controller.Controller.GetWindow(
                        (GUI.Simulator.RealTime.Controller.ControllerStyle)Properties.Settings.Default.RealTime_Train_Controller_Style,
                        Param);
                    Controller.GetInstance().Show();
                    Controller.StartTask();

                    if (Properties.Settings.Default.RealTime_Train_WaveForm_Line_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.WaveFormLine(Param);
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_Train_WaveForm_Phase_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.WaveFormPhase(Param);
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_Train_Control_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.ControlStatus(
                            Param,
                            (RealtimeDisplay.ControlStatus.RealTimeControlStatStyle)Properties.Settings.Default.RealTime_Train_Control_Style,
                            Properties.Settings.Default.RealTime_Train_Control_Precise
                        );
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_Train_Hexagon_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.Hexagon(
                            Param,
                            (RealtimeDisplay.Hexagon.RealTimeHexagonStyle)Properties.Settings.Default.RealTime_Train_Hexagon_Style,
                            Properties.Settings.Default.RealTime_Train_Hexagon_ZeroVector
                        );
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_Train_FFT_Show)
                    {
                        IRealtimeDisplay Display = new RealtimeDisplay.Fft(Param);
                        Display.Show();
                        Display.Start();
                    }

                    if (Properties.Settings.Default.RealTime_Train_FS_Show)
                    {
                        IRealtimeDisplay Display = new GUI.Simulator.RealTime.UniqueWindow.Fs(Param);
                        Display.Show();
                        Display.Start();
                    }

                    MainWindow.SetInteractive(false);
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Audio.TrainSound.RealTime.Generate(Param);
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        MainWindow.SetInteractive(true);
                        SystemSounds.Beep.Play();
                    });
                    return Properties.Settings.Default.RealTime_Train_EditAllow;
                }
                else if (command[1].Equals("Setting"))
                {
                    GUI.Simulator.RealTime.Setting.Basic setting = new(this, GUI.Simulator.RealTime.Setting.Basic.RealTimeBasicSettingMode.Train);
                    setting.ShowDialog();
                }
            }
            else if (command[0].Equals("Control"))
            {
                int[] valid_fps = [120, 60, 30, 10, 5];
                string filter = "";
                for (int i = 0; i < valid_fps.Length; i++)
                {
                    filter += valid_fps[i] + "fps|*.mp4" + (i + 1 == valid_fps.Length ? "" : "|");
                }
                var dialog = new SaveFileDialog { Filter = filter, FilterIndex = 2 };
                if (dialog.ShowDialog() == false) return true;
                int fps = valid_fps[dialog.FilterIndex - 1];

                if (command[1].Equals("Design1"))
                {
                    GenerationParameter parameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Video.ControlInfo.Design1 generate = new();
                            generate.ExportVideo(parameter, dialog.FileName, fps);
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, parameter.Progress, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.Control.Original") + Data.Vvvf.Manager.GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }

                else if (command[1].Equals("Design2"))
                {
                    GenerationParameter parameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Video.ControlInfo.Design2 generation = new();
                            generation.ExportVideo(parameter, dialog.FileName, fps);
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, parameter.Progress, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.Control.Original2") + Data.Vvvf.Manager.GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
            }
            else if (command[0].Equals("WaveForm"))
            {
                var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                if (dialog.ShowDialog() == false) return true;

                GenerationParameter parameter = GetGenerationBasicParameter();
                Task task = Task.Run(() =>
                {
                    try
                    {
                        if (command[1].Equals("Original"))
                            new Generation.Video.WaveForm.GenerateWaveFormUV().ExportVideo2(parameter, dialog.FileName);
                        else if (command[1].Equals("Spaced"))
                            new Generation.Video.WaveForm.GenerateWaveFormUV().ExportVideo1(parameter, dialog.FileName);
                        else if (command[1].Equals("UVW"))
                            new Generation.Video.WaveForm.GenerateWaveFormUVW().ExportVideo(parameter, dialog.FileName);
                    }
                    catch (Exception e)
                    {
                        DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                    }
                    SystemSounds.Beep.Play();
                });

                string description = command[1] switch
                {
                    "Original" => LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.WaveForm.Original"),
                    "Spaced" => LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.WaveForm.Spaced"),
                    _ => LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.WaveForm.UVW")
                };
                TaskProgressData taskProgressData = new(task, parameter.Progress, description + Data.Vvvf.Manager.GetLoadedYamlName());
                TaskViewer.TaskList.Add(taskProgressData);
            }
            else if (command[0].Equals("Hexagon"))
            {
                if (command[1].Equals("Original"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;
                    DialogBoxButton? Result = DialogBox.Show(this, LanguageManager.GetString("MainWindow.Message.Generate.Movie.Hexagon.Original.EnableZeroVector"), LanguageManager.GetString("Generic.Title.Ask"), [DialogBoxButton.Yes, DialogBoxButton.No], DialogBoxIcon.Question);
                    bool DrawCircle = Result == DialogBoxButton.Yes;
                    GenerationParameter parameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            new Generation.Video.Hexagon.Design1().ExportVideo(parameter, dialog.FileName, DrawCircle);
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, parameter.Progress, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.Hexagon.Original") + Data.Vvvf.Manager.GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
                else if (command[1].Equals("Explain"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;
                    DialogBoxButton? Result = DialogBox.Show(this, LanguageManager.GetString("MainWindow.Message.Generate.Movie.Hexagon.Explain.EnableZeroVector"), LanguageManager.GetString("Generic.Title.Ask"), [DialogBoxButton.Yes, DialogBoxButton.No], DialogBoxIcon.Question);
                    bool DrawCircle = Result == DialogBoxButton.Yes;
                    DoubleNumberInput Input = new(this, LanguageManager.GetString("MainWindow.Message.Generate.Movie.Hexagon.Explain.EnterFrequency"));
                    if (!Input.IsEnteredValueValid()) return true;

                    GenerationParameter parameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            new Generation.Video.Hexagon.Explain().ExportVideo(parameter, dialog.FileName, DrawCircle, Input.GetEnteredValue());
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, parameter.Progress, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.Hexagon.Explain") + Data.Vvvf.Manager.GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
                else if (command[1].Equals("OriginalImage"))
                {
                    var dialog = new SaveFileDialog { Filter = "png (*.png)|*.png" };
                    if (dialog.ShowDialog() == false) return true;
                    DialogBoxButton? Result = DialogBox.Show(this, LanguageManager.GetString("MainWindow.Message.Generate.Image.Hexagon.Original.EnableZeroVector"), LanguageManager.GetString("Generic.Title.Ask"), [DialogBoxButton.Yes, DialogBoxButton.No], DialogBoxIcon.Question);
                    bool DrawCircle = Result == DialogBoxButton.Yes;
                    DoubleNumberInput Input = new(this, LanguageManager.GetString("MainWindow.Message.Generate.Image.Hexagon.Original.EnterFrequency"));
                    if (!Input.IsEnteredValueValid()) return true;
                    GenerationParameter parameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            new Generation.Video.Hexagon.Design1().ExportImage(parameter, dialog.FileName, DrawCircle, Input.GetEnteredValue());
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, parameter.Progress, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Image.Hexagon.Original") + Data.Vvvf.Manager.GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }

            }
            else if (command[0].Equals("FFT"))
            {
                if (command[1].Equals("Video"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;

                    GenerationParameter parameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            new Generation.Video.FFT.GenerateFFT().ExportVideo(parameter, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, parameter.Progress, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.FFT.Original") + Data.Vvvf.Manager.GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
                else if (command[1].Equals("Image"))
                {
                    var dialog = new SaveFileDialog { Filter = "png (*.png)|*.png" };
                    if (dialog.ShowDialog() == false) return true;
                    DoubleNumberInput Input = new(this, LanguageManager.GetString("MainWindow.Message.Generate.Image.FFT.Original.EnterFrequency"));
                    if (!Input.IsEnteredValueValid()) return true;
                    GenerationParameter parameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            new Generation.Video.FFT.GenerateFFT().ExportImage(parameter, dialog.FileName, Input.GetEnteredValue());
                        }
                        catch (Exception e)
                        {
                            DialogBox.Show(this, e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                    TaskProgressData taskProgressData = new(task, parameter.Progress, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Image.FFT.Original") + Data.Vvvf.Manager.GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }

            }
            return true;
        }
        private void Generation_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            object? tag = button.Tag;
            if (tag == null) return;
            string? tag_str = tag.ToString();
            if (tag_str == null) return;
            string[] command = tag_str.Split("_");


            MainWindow.SetInteractive(false);
            bool unblock = SolveCommand(command);
            if (!unblock) return;
            MainWindow.SetInteractive(true);
            SystemSounds.Beep.Play();

        }
        private void Window_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            object? tag = button.Tag;
            if (tag == null) return;
            string? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("LCalc"))
                new LinearCalculator().Show();
            else if (tag_str.Equals("TaskProgressView"))
                new TaskViewer().Show();
            else if (tag_str.Equals("AccelPattern"))
            {
                SetInteractive(false);
                new GUI.BaseFrequency.SettingWindow().ShowDialog();
                SetInteractive(true);
            }
            else if (tag_str.Equals("TrainSoundSetting"))
            {
                SetInteractive(false);
                new GUI.TrainAudio.SettingsWindow().ShowDialog();
                SetInteractive(true);
            }
            else if (tag_str.Equals("Preference"))
            {
                SetInteractive(false);
                new Preference(this).ShowDialog();
                SetInteractive(true);
            }
        }
        private void Tools_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            object? tag = button.Tag;
            if (tag == null) return;
            string? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("MIDI"))
            {
                GUI.MIDIConvert.Main converter = new();
                converter.Show();
            }
            else if (tag_str.Equals("AutoVoltage"))
            {
                SetInteractive(false);
                Data.Vvvf.Manager.Current.SortAcceleratePattern(false);
                Data.Vvvf.Manager.Current.SortBrakingPattern(false);
                double DefaultAccelerateMaxFrequency = Data.Vvvf.Manager.Current.AcceleratePattern.Count > 0 ? Data.Vvvf.Manager.Current.AcceleratePattern[0].ControlFrequencyFrom : 60.0;
                double DefaultBrakeMaxFrequency = Data.Vvvf.Manager.Current.BrakingPattern.Count > 0 ? Data.Vvvf.Manager.Current.BrakingPattern[0].ControlFrequencyFrom : 60.0;

                static void Quit()
                {
                    SetInteractive(true);
                    SystemSounds.Beep.Play();
                }

                List<DialogInputWindow.InputContext> AutoModulationConfigWindowInputs =
                [
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.AcceleratePatternMaxFrequency"), DialogInputWindow.InputContextMode.TextBox, DefaultAccelerateMaxFrequency, typeof(double)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.AcceleratePatternMaxVoltage"), DialogInputWindow.InputContextMode.TextBox, 100.0, typeof(double)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.BrakePatternMaxFrequency"), DialogInputWindow.InputContextMode.TextBox, DefaultBrakeMaxFrequency, typeof(double)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.BrakePatternMaxVoltage"), DialogInputWindow.InputContextMode.TextBox, 100.0, typeof(double)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.CalculationType"), DialogInputWindow.InputContextMode.ComboBox, FriendlyNameConverter.GetEquationSolverTypeNames(), typeof(MyMath.EquationSolver.EquationSolverType)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.MaxEffort"), DialogInputWindow.InputContextMode.TextBox, 50, typeof(int)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.Precision"), DialogInputWindow.InputContextMode.TextBox, 0.5, typeof(double)),
                ];
                DialogInputWindow AutoModulationConfigWindow = new(
                    this,
                    LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.Title"),
                    AutoModulationConfigWindowInputs
                );
                AutoModulationConfigWindow.ShowDialog();
                if (AutoModulationConfigWindow.Contexts == null)
                {
                    Quit();
                    return;
                }

                List<DialogInputWindow.InputContext> AutoModulationTableConfigWindowInputs =
                [
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.Table.Division"), DialogInputWindow.InputContextMode.TextBox, 1, typeof(int)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.Table.IsDivisionPerHz"), DialogInputWindow.InputContextMode.CheckBox, true, typeof(bool)),
                ];
                DialogInputWindow AutoModulationTableConfigWindow = new(
                    this,
                    LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.Table.Title"),
                    AutoModulationTableConfigWindowInputs
                );
                if (Data.Vvvf.Manager.Current.AcceleratePattern.Find(
                    (a) => a.Amplitude.Default.Mode.Equals(Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.Table)) != null ||
                    Data.Vvvf.Manager.Current.BrakingPattern.Find(
                    (a) => a.Amplitude.Default.Mode.Equals(Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.Table)) != null)
                {
                    AutoModulationTableConfigWindow.ShowDialog();
                    if (AutoModulationTableConfigWindow.Contexts == null)
                    {
                        Quit();
                        return;
                    }
                }

                Data.Tool.AutoModulationIndexSolver.SolveConfiguration Configuration = new(
                    Data.Vvvf.Manager.Current,
                    Data.TrainAudio.Manager.Current,
                    AutoModulationConfigWindow.GetValue<double>(0),
                    AutoModulationConfigWindow.GetValue<double>(1),
                    AutoModulationConfigWindow.GetValue<double>(2),
                    AutoModulationConfigWindow.GetValue<double>(3),
                    AutoModulationConfigWindow.GetValue<MyMath.EquationSolver.EquationSolverType>(4),
                    AutoModulationConfigWindow.GetValue<int>(5),
                    AutoModulationConfigWindow.GetValue<double>(6),
                    AutoModulationTableConfigWindow.GetValue<int>(0),
                    AutoModulationTableConfigWindow.GetValue<bool>(1)
                );

                Task.Run(() =>
                {

                    bool result = Data.Tool.AutoModulationIndexSolver.Run(Configuration);
                    if (!result)
                        DialogBox.Show(this, LanguageManager.GetStringWithNewLine("MainWindow.Message.Tools.AutoVoltage.Error"), LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);

                    SetInteractive(true);
                    SystemSounds.Beep.Play();
                });

            }
            else if (tag_str.Equals("FreeRunAmpZero"))
            {
                SetInteractive(false);
                Task.Run(() =>
                {
                    bool result = Data.Vvvf.Util.SetFreeRunModulationIndexToZero(Data.Vvvf.Manager.Current);
                    if (!result)
                        DialogBox.Show(this, LanguageManager.GetString("Generic.Message.Error"), LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);

                    SetInteractive(true);
                    SystemSounds.Beep.Play();
                });
            }
            else if (tag_str.Equals("EndAmplitudeContinuous"))
            {
                SetInteractive(false);
                Task.Run(() =>
                {
                    bool result = Data.Vvvf.Util.SetFreeRunEndAmplitudeContinuous(Data.Vvvf.Manager.Current);
                    if (!result)
                        DialogBox.Show(this, LanguageManager.GetString("Generic.Message.Error"), LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);

                    SetInteractive(true);
                    SystemSounds.Beep.Play();
                });
            }

        }
        private void Edit_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            object? tag = button.Tag;
            if (tag == null) return;
            string? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("Reset"))
            {
                DialogBoxButton? Result = DialogBox.Show(
                    this, 
                    LanguageManager.GetString("MainWindow.Message.Edit.Reset.Confirm.Message"),
                    LanguageManager.GetString("MainWindow.Message.Edit.Reset.Confirm.Title"),
                    [DialogBoxButton.Yes, DialogBoxButton.No], DialogBoxIcon.Question);
                if (Result != DialogBoxButton.Yes)
                    return;
                Data.Vvvf.Manager.ResetCurrent();
                SettingContentViewer.Navigate(null);
                UpdateControlList();
            }
        }
        private void Help_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            object? tag = button.Tag;
            if (tag == null) return;
            string? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("About"))
            {
                SetInteractive(false);
                new GUI.Simulator.About(this).ShowDialog();
                SetInteractive(true);
            }
        }
        #endregion

        private void AccelerateSelectedShow()
        {
            int Index = AcceleratePatternList.SelectedIndex;
            if (Index < 0)
                SettingContentViewer.Navigate(null);
            else
                SettingContentViewer.Navigate(
                    new GUI.Create.Waveform.WaveformEditor(
                        Data.Vvvf.Manager.Current.AcceleratePattern[Index],
                        Data.Vvvf.Manager.Current.Level));
        }
        private void BrakeSelectedShow()
        {
            int Index = BrakePatternList.SelectedIndex;
            if (Index < 0)
                SettingContentViewer.Navigate(null);
            else
                SettingContentViewer.Navigate(
                    new GUI.Create.Waveform.WaveformEditor(
                        Data.Vvvf.Manager.Current.BrakingPattern[Index],
                        Data.Vvvf.Manager.Current.Level));
        }
        public void UpdateControlList()
        {
            AcceleratePatternList.ItemsSource = Data.Vvvf.Manager.Current.AcceleratePattern;
            BrakePatternList.ItemsSource = Data.Vvvf.Manager.Current.BrakingPattern;
            AcceleratePatternList.Items.Refresh();
            BrakePatternList.Items.Refresh();
        }
        public void UpdateContentSelected()
        {
            if (setting_tabs.SelectedIndex == 1)
            {
                AccelerateSelectedShow();
            }
            else if (setting_tabs.SelectedIndex == 2)
            {
                BrakeSelectedShow();
            }
        }
        
        private void SettingButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name;
            if (name.Equals("settings_level"))
                SettingContentViewer.Navigate(new Uri("GUI/Create/Settings/Level.xaml", UriKind.Relative));
            else if (name.Equals("settings_minimum"))
                SettingContentViewer.Navigate(new Uri("GUI/Create/Settings/MinimumFrequency.xaml", UriKind.Relative));
            else if (name.Equals("settings_jerk"))
                SettingContentViewer.Navigate(new Uri("GUI/Create/Settings/Jerk.xaml", UriKind.Relative));
        }
        private void SettingEditClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            string? tag_str = tag.ToString();
            if (tag_str == null) return;
            string[] command = tag_str.Split("_");

            var list_view = command[0].Equals("accelerate") ? AcceleratePatternList : BrakePatternList;
            var settings = command[0].Equals("accelerate") ? Data.Vvvf.Manager.Current.AcceleratePattern : Data.Vvvf.Manager.Current.BrakingPattern;

            if (command[1].Equals("add"))
                settings.Add(new Data.Vvvf.Struct.PulseControl());

            list_view.Items.Refresh();
        }
        private void SettingsLoad(object sender, RoutedEventArgs e)
        {
            ListView btn = (ListView)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            string? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag.Equals("accelerate"))
            {
                UpdateControlList();
                AccelerateSelectedShow();
            }
            else
            {
                UpdateControlList();
                BrakeSelectedShow();
            }
        }
        private void SettingsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = (ListView)sender;
            string? tag = list.Tag?.ToString();
            if (tag == null) return;

            if (tag.Equals("accelerate"))
                AccelerateSelectedShow();
            else
                BrakeSelectedShow();
        }
        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            object? tag = mi.Tag;
            if (tag == null) return;
            string? tag_str = tag.ToString();
            if (tag_str == null) return;
            string[] command = tag_str.Split(".");
            if (command[0].Equals("brake"))
            {
                if (command[1].Equals("sort"))
                {
                    Data.Vvvf.Manager.Current.SortBrakingPattern(false);
                    BrakeSelectedShow();
                }
                else if (command[1].Equals("copy"))
                {
                    int selected = BrakePatternList.SelectedIndex;
                    if (selected < 0) return;

                    Data.Vvvf.Struct ysd = Data.Vvvf.Manager.Current;
                    var selected_data = ysd.BrakingPattern[selected];
                    Data.Vvvf.Manager.Current.BrakingPattern.Add(selected_data.Clone());
                    BrakeSelectedShow();
                }
                else if (command[1].Equals("delete"))
                {
                    Data.Vvvf.Manager.Current.BrakingPattern.RemoveAt(BrakePatternList.SelectedIndex);
                }
                UpdateControlList();
            }
            else if (command[0].Equals("accelerate"))
            {
                if (command[1].Equals("sort"))
                {
                    Data.Vvvf.Manager.Current.SortAcceleratePattern(false);
                    AccelerateSelectedShow();
                }
                else if (command[1].Equals("copy"))
                {
                    int selected = AcceleratePatternList.SelectedIndex;
                    if (selected < 0) return;

                    Data.Vvvf.Struct ysd = Data.Vvvf.Manager.Current;
                    Data.Vvvf.Struct.PulseControl selected_data = ysd.AcceleratePattern[selected];
                    Data.Vvvf.Manager.Current.AcceleratePattern.Add(selected_data.Clone());
                    BrakeSelectedShow();
                }
                else if (command[1].Equals("delete"))
                {
                    Data.Vvvf.Manager.Current.AcceleratePattern.RemoveAt(AcceleratePatternList.SelectedIndex);
                }
                UpdateControlList();
            }
        }

        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!SaveBefore("MainWindow.Message.File.SaveBefore.Close"))
            {
                e.Cancel = true;
                return;
            }
            
            Application.Current.Shutdown();
            Generation.Video.Fonts.Manager.Dispose();
        }
        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
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
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && IsInteractable())
            {
                string Path = (((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0) ?? "").ToString() ?? "";
                if (Path.ToLower().EndsWith(".yaml") && SaveBefore("MainWindow.Message.File.SaveBefore.Load")) LoadYaml(Path);
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;
            if (e.Key.Equals(Key.S) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (Data.Vvvf.Manager.LoadPath.Length == 0)
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Yaml (*.yaml)|*.yaml",
                        FileName = "VVVF"
                    };
                    if (dialog.ShowDialog() ?? false)
                        SaveYaml(dialog.FileName, true);
                }
                else
                    SaveYaml(Data.Vvvf.Manager.LoadPath, false);
            }
        }
    }
}
