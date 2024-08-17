﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.Generation.Pi3Generator;
using VvvfSimulator.GUI.Create.Waveform;
using VvvfSimulator.GUI.Mascon;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Simulator.RealTime;
using VvvfSimulator.GUI.Simulator.RealTime.Setting;
using VvvfSimulator.GUI.Simulator.RealTime.UniqueWindow;
using VvvfSimulator.GUI.TaskViewer;
using VvvfSimulator.GUI.TrainAudio;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Yaml.VvvfSound;
using YamlDotNet.Core;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfUtil;

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

        public MainWindow()
        {
            Instance = this;
            DataContext = BindingData;
            InitializeComponent();
        }

        private void SettingButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name;
            if (name.Equals("settings_level"))
                SettingContentViewer.Navigate(new Uri("GUI/Create/Settings/Level.xaml", UriKind.Relative));
            else if (name.Equals("settings_minimum"))
                SettingContentViewer.Navigate(new Uri("GUI/Create/Settings/MinimumFrequency.xaml", UriKind.Relative));
            else if (name.Equals("settings_mascon"))
                SettingContentViewer.Navigate(new Uri("GUI/Create/Settings/Jerk.xaml", UriKind.Relative));
        }

        private void SettingEditClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split("_");

            var list_view = command[0].Equals("accelerate") ? accelerate_settings : brake_settings;
            var settings = command[0].Equals("accelerate") ? YamlVvvfManage.CurrentData.AcceleratePattern : YamlVvvfManage.CurrentData.BrakingPattern;

            if (command[1].Equals("add"))
                settings.Add(new YamlControlData());

            list_view.Items.Refresh();
        }
        private void SettingsLoad(object sender, RoutedEventArgs e)
        {
            ListView btn = (ListView)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
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
            ListView btn = (ListView)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;


            if (tag.Equals("accelerate"))
                AccelerateSelectedShow();
            else
                BrakeSelectedShow();

        }

        private void AccelerateSelectedShow()
        {
            int selected = accelerate_settings.SelectedIndex;
            if (selected < 0)
            {
                SettingContentViewer.Navigate(null);
            }
            else
            {
                YamlVvvfSoundData ysd = YamlVvvfManage.CurrentData;
                var selected_data = ysd.AcceleratePattern[selected];
                SettingContentViewer.Navigate(new Top(selected_data, ysd.Level));
            }
        }
        private void BrakeSelectedShow()
        {
            int selected = brake_settings.SelectedIndex;
            if (selected < 0)
            {
                SettingContentViewer.Navigate(null);
            }
            else
            {
                YamlVvvfSoundData ysd = YamlVvvfManage.CurrentData;
                var selected_data = ysd.BrakingPattern[selected];
                SettingContentViewer.Navigate(new Top(selected_data, ysd.Level));
            }
        }

        public void UpdateControlList()
        {
            accelerate_settings.ItemsSource = YamlVvvfManage.CurrentData.AcceleratePattern;
            brake_settings.ItemsSource = YamlVvvfManage.CurrentData.BrakingPattern;
            accelerate_settings.Items.Refresh();
            brake_settings.Items.Refresh();
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

        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object? tag = mi.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split(".");
            if (command[0].Equals("brake"))
            {
                if (command[1].Equals("sort"))
                {
                    YamlVvvfManage.CurrentData.SortBrakingPattern(false);
                    BrakeSelectedShow();
                }
                else if (command[1].Equals("copy"))
                {
                    int selected = brake_settings.SelectedIndex;
                    if (selected < 0) return;

                    YamlVvvfSoundData ysd = YamlVvvfManage.CurrentData;
                    var selected_data = ysd.BrakingPattern[selected];
                    YamlVvvfManage.CurrentData.BrakingPattern.Add(selected_data.Clone());
                    BrakeSelectedShow();
                }
                else if (command[1].Equals("delete"))
                {
                    YamlVvvfManage.CurrentData.BrakingPattern.RemoveAt(brake_settings.SelectedIndex);
                }
                UpdateControlList();
            }
            else if (command[0].Equals("accelerate"))
            {
                if (command[1].Equals("sort"))
                {
                    YamlVvvfManage.CurrentData.SortAcceleratePattern(false);
                    AccelerateSelectedShow();
                }
                else if (command[1].Equals("copy"))
                {
                    int selected = accelerate_settings.SelectedIndex;
                    if (selected < 0) return;

                    YamlVvvfSoundData ysd = YamlVvvfManage.CurrentData;
                    YamlControlData selected_data = ysd.AcceleratePattern[selected];
                    YamlVvvfManage.CurrentData.AcceleratePattern.Add(selected_data.Clone());
                    BrakeSelectedShow();
                }
                else if (command[1].Equals("delete"))
                {
                    YamlVvvfManage.CurrentData.AcceleratePattern.RemoveAt(accelerate_settings.SelectedIndex);
                }
                UpdateControlList();
            }
        }

        private String load_path = "";
        public String GetLoadedYamlName()
        {
            return Path.GetFileNameWithoutExtension(load_path);
        }
        private void File_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            if (tag.Equals("Load"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (dialog.ShowDialog() == false) return;

                try
                {
                    YamlVvvfManage.Load(dialog.FileName);
                    MessageBox.Show(LanguageManager.GetString("MainWindow.Message.File.Load.Ok.Message"), LanguageManager.GetString("MainWindow.Message.File.Load.Ok.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (YamlException ex)
                {
                    String error_message = LanguageManager.GetString("MainWindow.Message.File.Load.Error.Message");
                    error_message += "\r\n";
                    error_message += "\r\n" + ex.End.ToString() + "\r\n";
                    MessageBox.Show(error_message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }


                load_path = dialog.FileName;
                UpdateControlList();
                //update_Control_Showing();
                SettingContentViewer.Navigate(null);

            }
            else if (tag.Equals("Save_As"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = GetLoadedYamlName()
                };

                // ダイアログを表示する
                if (dialog.ShowDialog() == false) return;
                load_path = dialog.FileName;
                if (YamlVvvfManage.Save(dialog.FileName))
                    MessageBox.Show(LanguageManager.GetString("MainWindow.Message.File.SaveAs.Ok.Message"), LanguageManager.GetString("MainWindow.Message.File.SaveAs.Ok.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(LanguageManager.GetString("MainWindow.Message.File.SaveAs.Error.Message"), LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (tag.Equals("Save"))
            {
                String save_path = load_path;
                if (save_path.Length == 0)
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Yaml (*.yaml)|*.yaml",
                        FileName = "VVVF"
                    };

                    // ダイアログを表示する
                    if (dialog.ShowDialog() == false) return;
                    load_path = dialog.FileName;
                    save_path = load_path;
                }
                if (YamlVvvfManage.Save(save_path))
                    MessageBox.Show(LanguageManager.GetString("MainWindow.Message.File.Save.Ok.Message"), LanguageManager.GetString("MainWindow.Message.File.Save.Ok.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(LanguageManager.GetString("MainWindow.Message.File.Save.Error.Message"), LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private static GenerationBasicParameter GetGenerationBasicParameter()
        {
            GenerationBasicParameter generationBasicParameter = new(
                YamlMasconManage.CurrentData.GetCompiled(),
                YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData),
                new ProgressData()
            );

            return generationBasicParameter;
        }
        private Boolean SolveCommand(String[] command)
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

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Audio.VvvfSound.Audio.ExportWavFile(generationBasicParameter, sample_freq, raw, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Audio.Vvvf") + GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }

                else if (command[1].Equals("RealTime"))
                {
                    RealTimeParameter parameter = new()
                    {
                        Quit = false
                    };

                    MainWindow.SetInteractive(false);

                    System.IO.Ports.SerialPort? serial = null;
                    if (command.Length == 3)
                    {
                        try
                        {
                            ComPortSelector com = new(this);
                            com.ShowDialog();
                            serial = new()
                            {
                                PortName = com.GetComPortName(),
                            };
                        }
                        catch
                        {
                            MessageBox.Show(LanguageManager.GetString("MainWindow.Menu.RealTime.VVVF.USB.Error.Message"), LanguageManager.GetString("MainWindow.Menu.RealTime.VVVF.USB.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                            return true;
                        }
                    }

                    MasconWindow mascon = new(parameter);
                    mascon.Show();
                    mascon.StartTask();

                    if (Properties.Settings.Default.RealTime_VVVF_WaveForm_Show)
                    {
                        RealtimeDisplay.WaveForm window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_Control_Show)
                    {
                        RealtimeDisplay.ControlStatus window = new(
                            parameter,
                            (RealtimeDisplay.ControlStatus.RealTimeControlStatStyle)Properties.Settings.Default.RealTime_VVVF_Control_Style,
                            Properties.Settings.Default.RealTime_VVVF_Control_Precise
                        );
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_Hexagon_Show)
                    {
                        RealtimeDisplay.Hexagon window = new(
                            parameter,
                            (RealtimeDisplay.Hexagon.RealTimeHexagonStyle)Properties.Settings.Default.RealTime_VVVF_Hexagon_Style,
                            Properties.Settings.Default.RealTime_VVVF_Hexagon_ZeroVector
                        );
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_FFT_Show)
                    {
                        RealtimeDisplay.Fft window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_FS_Show)
                    {
                        Fs window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    bool do_clone = !Properties.Settings.Default.RealTime_VVVF_EditAllow;
                    YamlVvvfSoundData data = do_clone ? YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData) : YamlVvvfManage.CurrentData;

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Audio.VvvfSound.RealTime.Calculate(data, parameter, serial);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        MainWindow.SetInteractive(true);
                        SystemSounds.Beep.Play();
                    });

                    return Properties.Settings.Default.RealTime_VVVF_EditAllow;
                }
                else if (command[1].Equals("Setting"))
                {
                    Basic setting = new(this, Basic.RealTimeBasicSettingMode.VVVF);
                    setting.ShowDialog();
                }
            }
            else if (command[0].Equals("Train"))
            {
                if (command[1].Equals("WAV"))
                {

                    var dialog = new SaveFileDialog { Filter = "wav|*.wav|raw|*.wav" };
                    if (dialog.ShowDialog() == false) return true;

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            bool raw = dialog.FilterIndex == 2;
                            YamlTrainSoundData trainSound_Data_clone = YamlTrainSoundDataManage.CurrentData.Clone();
                            Generation.Audio.TrainSound.Audio.ExportWavFile(generationBasicParameter, trainSound_Data_clone, 192000, raw, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Audio.Train") + GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
                else if (command[1].Equals("RealTime"))
                {
                    RealTimeParameter parameter = new()
                    {
                        Quit = false
                    };

                    MasconWindow mascon = new(parameter);
                    mascon.Show();
                    mascon.StartTask();

                    if (Properties.Settings.Default.RealTime_Train_WaveForm_Show)
                    {
                        RealtimeDisplay.WaveForm window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_Train_Control_Show)
                    {
                        RealtimeDisplay.ControlStatus window = new(
                            parameter,
                            (RealtimeDisplay.ControlStatus.RealTimeControlStatStyle)Properties.Settings.Default.RealTime_Train_Control_Style,
                            Properties.Settings.Default.RealTime_Train_Control_Precise
                        );
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_Train_Hexagon_Show)
                    {
                        RealtimeDisplay.Hexagon window = new(
                            parameter,
                            (RealtimeDisplay.Hexagon.RealTimeHexagonStyle)Properties.Settings.Default.RealTime_Train_Hexagon_Style,
                            Properties.Settings.Default.RealTime_Train_Hexagon_ZeroVector
                        );
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_Train_FFT_Show)
                    {
                        RealtimeDisplay.Fft window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_Train_FS_Show)
                    {
                        Fs window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }


                    MainWindow.SetInteractive(false);
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            bool do_clone = !Properties.Settings.Default.RealTime_Train_EditAllow;
                            YamlVvvfSoundData data;
                            if (do_clone)
                                data = YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData);
                            else
                                data = YamlVvvfManage.CurrentData;
                            Generation.Audio.TrainSound.RealTime.Generate(data, parameter);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        MainWindow.SetInteractive(true);
                        SystemSounds.Beep.Play();
                    });
                    return Properties.Settings.Default.RealTime_Train_EditAllow;
                }
                else if (command[1].Equals("Setting"))
                {
                    Basic setting = new(this, Basic.RealTimeBasicSettingMode.Train);
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

                if (command[1].Equals("Original"))
                {
                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Video.ControlInfo.GenerateControlOriginal generate = new();
                            generate.ExportVideo(generationBasicParameter, dialog.FileName, fps);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.Control.Original") + GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }

                else if (command[1].Equals("Original2"))
                {
                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            Generation.Video.ControlInfo.GenerateControlOriginal2 generation = new();
                            generation.ExportVideo(generationBasicParameter, dialog.FileName, fps);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.Control.Original2") + GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }


            }
            else if (command[0].Equals("WaveForm"))
            {
                var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                if (dialog.ShowDialog() == false) return true;

                GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                Task task = Task.Run(() =>
                {
                    try
                    {
                        if (command[1].Equals("Original"))
                            new Generation.Video.WaveForm.GenerateWaveFormUV().ExportVideo2(generationBasicParameter, dialog.FileName);
                        else if (command[1].Equals("Spaced"))
                            new Generation.Video.WaveForm.GenerateWaveFormUV().ExportVideo1(generationBasicParameter, dialog.FileName);
                        else if (command[1].Equals("UVW"))
                            new Generation.Video.WaveForm.GenerateWaveFormUVW().ExportVideo(generationBasicParameter, dialog.FileName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    SystemSounds.Beep.Play();
                });

                string description = command[1] switch
                {
                    "Original" => LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.WaveForm.Original"),
                    "Spaced" => LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.WaveForm.Spaced"),
                    _ => LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.WaveForm.UVW")
                };
                TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, description + GetLoadedYamlName());
                TaskViewer.TaskList.Add(taskProgressData);
            }
            else if (command[0].Equals("Hexagon"))
            {
                if (command[1].Equals("Original"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;
                    MessageBoxResult result = MessageBox.Show(LanguageManager.GetString("MainWindow.Message.Generate.Movie.Hexagon.Original.EnableZeroVector"), LanguageManager.GetString("Generic.Title.Ask"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                    bool circle = result == MessageBoxResult.Yes;
                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            new Generation.Video.Hexagon.GenerateHexagonOriginal().ExportVideo(generationBasicParameter, dialog.FileName, circle);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.Hexagon.Original") + GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
                else if (command[1].Equals("Explain"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;
                    MessageBoxResult result = MessageBox.Show(LanguageManager.GetString("MainWindow.Message.Generate.Movie.Hexagon.Explain.EnableZeroVector"), LanguageManager.GetString("Generic.Title.Ask"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                    bool circle = result == MessageBoxResult.Yes;
                    double frequency = new DoubleNumberInput(this, LanguageManager.GetString("MainWindow.Message.Generate.Movie.Hexagon.Explain.EnterFrequency")).GetEnteredValue();
                    if(double.IsNaN(frequency)) return true;

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            new Generation.Video.Hexagon.GenerateHexagonExplain().generate_wave_hexagon_explain(generationBasicParameter, dialog.FileName, circle, frequency);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.Hexagon.Explain") + GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
                else if (command[1].Equals("OriginalImage"))
                {
                    var dialog = new SaveFileDialog { Filter = "png (*.png)|*.png" };
                    if (dialog.ShowDialog() == false) return true;
                    MessageBoxResult result = MessageBox.Show(LanguageManager.GetString("MainWindow.Message.Generate.Image.Hexagon.Original.EnableZeroVector"), LanguageManager.GetString("Generic.Title.Ask"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                    bool circle = result == MessageBoxResult.Yes;
                    double frequency = new DoubleNumberInput(this, LanguageManager.GetString("MainWindow.Message.Generate.Image.Hexagon.Original.EnterFrequency")).GetEnteredValue();
                    if (double.IsNaN(frequency)) return true;

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            YamlVvvfSoundData clone = YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData);
                            new Generation.Video.Hexagon.GenerateHexagonOriginal().ExportImage(dialog.FileName, clone, circle, frequency);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }

            }
            else if (command[0].Equals("FFT"))
            {
                if (command[1].Equals("Video"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            new Generation.Video.FFT.GenerateFFT().ExportVideo(generationBasicParameter, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, LanguageManager.GetString("MainWindow.TaskDescription.Generate.Movie.FFT.Original") + GetLoadedYamlName());
                    TaskViewer.TaskList.Add(taskProgressData);
                }
                else if (command[1].Equals("Image"))
                {
                    var dialog = new SaveFileDialog { Filter = "png (*.png)|*.png" };
                    if (dialog.ShowDialog() == false) return true;
                    double frequency = new DoubleNumberInput(this, LanguageManager.GetString("MainWindow.Message.Generate.Image.FFT.Original.EnterFrequency")).GetEnteredValue();
                    if (double.IsNaN(frequency)) return true;

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            YamlVvvfSoundData clone = YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData);
                            new Generation.Video.FFT.GenerateFFT().ExportImage(dialog.FileName, clone, frequency);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }

            }
            else if (command[0].Equals("SourceCode"))
            {
                if (command[1].Equals("RPi3"))
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "C (*.c)|*.c",
                        FileName = GetLoadedYamlName()
                    };

                    // ダイアログを表示する
                    if (dialog.ShowDialog() == false) return false;

                    try
                    {
                        using (StreamWriter outputFile = new(dialog.FileName))
                        {
                            outputFile.Write(Pi3Generate.GenerateC(YamlVvvfManage.CurrentData, Path.GetFileNameWithoutExtension(dialog.FileName)));
                        }
                        MessageBox.Show(LanguageManager.GetString("MainWindow.Message.Generate.SourceCode.RPi3.Ok.Message"), LanguageManager.GetString("MainWindow.Message.Generate.SourceCode.RPi3.Ok.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                

            }
            return true;
        }
        private void Generation_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split("_");


            MainWindow.SetInteractive(false);
            bool unblock = SolveCommand(command);
            if (!unblock) return;
            MainWindow.SetInteractive(true);
            SystemSounds.Beep.Play();

        }
        private void Window_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("LCalc"))
            {
                LinearCalculator lc = new();
                lc.Show();
            }
            else if (tag_str.Equals("TaskProgressView"))
            {
                TaskViewer tvm = new();
                tvm.Show();
            } 
            else if (tag_str.Equals("AccelPattern"))
            {
                MainWindow.SetInteractive(false);
                ControlEditor gmcw = new();
                gmcw.ShowDialog();
                MainWindow.SetInteractive(true);
            }
            else if (tag_str.Equals("TrainSoundSetting"))
            {
                MainWindow.SetInteractive(false);
                YamlTrainSoundData _TrainSound_Data = YamlTrainSoundDataManage.CurrentData;
                SettingsWindow tahw = new(_TrainSound_Data);
                tahw.ShowDialog();
                MainWindow.SetInteractive(true);
            }
        }
        private void Tools_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("MIDI"))
            {
                GUI.MIDIConvert.Main converter = new();
                converter.Show();
            }
            else if (tag_str.Equals("AutoVoltage"))
            {
                SetInteractive(false);
                YamlVvvfManage.CurrentData.SortAcceleratePattern(false);
                YamlVvvfManage.CurrentData.SortBrakingPattern(false);
                double DefaultAccelerateMaxFrequency = YamlVvvfManage.CurrentData.AcceleratePattern.Count > 0 ? YamlVvvfManage.CurrentData.AcceleratePattern[0].ControlFrequencyFrom : 60.0;
                double DefaultBrakeMaxFrequency = YamlVvvfManage.CurrentData.BrakingPattern.Count > 0 ? YamlVvvfManage.CurrentData.BrakingPattern[0].ControlFrequencyFrom : 60.0;

                static void Quit()
                {
                    SetInteractive(true);
                    SystemSounds.Beep.Play();
                }

                List<TextBoxListWindow.InputContext> inputs =
                [
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.AcceleratePatternMaxFrequency"), DefaultAccelerateMaxFrequency, typeof(double)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.AcceleratePatternMaxVoltage"), 100.0, typeof(double)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.BrakePatternMaxFrequency"), DefaultBrakeMaxFrequency, typeof(double)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.BrakePatternMaxVoltage"), 100.0, typeof(double)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.MaxEffort"), 50, typeof(int)),
                    new (LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.Precision"), 0.5, typeof(double)),
                ];

                TextBoxListWindow InputWindow = new(this, LanguageManager.GetString("MainWindow.Dialog.Tools.AutoVoltage.Title"), inputs);
                if (InputWindow.Contexts == null)
                {
                    Quit();
                    return;
                }

                AutoModulationIndexConfiguration Configuration = new()
                {
                    Data = YamlVvvfManage.CurrentData,
                    AccelEndFrequency = (double)InputWindow.Contexts[0].Value,
                    AccelMaxVoltage = (double)InputWindow.Contexts[1].Value,
                    BrakeEndFrequency = (double)InputWindow.Contexts[2].Value,
                    BrakeMaxVoltage = (double)InputWindow.Contexts[3].Value,
                    MaxEffort = (int)InputWindow.Contexts[4].Value,
                    Precision = (double)InputWindow.Contexts[5].Value,
                };

                Task.Run(() =>
                {
                    
                    bool result = YamlVvvfUtil.AutoModulationIndex(Configuration);
                    if (!result)
                        MessageBox.Show(LanguageManager.GetStringWithNewLine("MainWindow.Message.Tools.AutoVoltage.Error"), LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);

                    SetInteractive(true);
                    SystemSounds.Beep.Play();
                });

            }
            else if (tag_str.Equals("FreeRunAmpZero"))
            {
                SetInteractive(false);
                Task.Run(() =>
                {
                    bool result = YamlVvvfUtil.SetFreeRunModulationIndexToZero(YamlVvvfManage.CurrentData);
                    if (!result)
                        MessageBox.Show(LanguageManager.GetString("Generic.Message.Error"), LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);

                    SetInteractive(true);
                    SystemSounds.Beep.Play();
                });
            }
            else if (tag_str.Equals("EndAmplitudeContinuous"))
            {
                SetInteractive(false);
                Task.Run(() =>
                {
                    bool result = YamlVvvfUtil.SetFreeRunEndAmplitudeContinuous(YamlVvvfManage.CurrentData);
                    if (!result)
                        MessageBox.Show(LanguageManager.GetString("Generic.Message.Error"), LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);

                    SetInteractive(true);
                    SystemSounds.Beep.Play();
                });
            }

        }
        private void Edit_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("Reset"))
            {
                YamlVvvfManage.CurrentData = new();
                SettingContentViewer.Navigate(null);
                UpdateControlList();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if(tag == null) return;

            if (tag.Equals("Close"))
                Close();
            else if (tag.Equals("Maximize"))
            {
                if(WindowState.Equals(WindowState.Maximized))
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else if (tag.Equals("Minimize"))
                WindowState = WindowState.Minimized;
        }
    }
}
