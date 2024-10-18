using System;
using System.Collections.Generic;
using System.Linq;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Calculate;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsync.CarrierFrequency.YamlAsyncParameterCarrierFreqTable;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;

namespace VvvfSimulator.Generation.Pi3Generator
{
    public class Pi3Generate
    {

		public class Pi3Compiler
		{
			public int Indent { get; set; } = 0;
			public string Code { get; set; } = "";

			public void AddIndent() { Indent++; }
			public void DecrementIndent() {  Indent--; }

			public void WriteCode(string code) { this.Code += code; }
            public void WriteLineCode(string code) {
                WriteIndent();
                this.Code += code + "\r\n"; 
            }
			public void WriteIndent()
			{
				for(int i = 0; i < Indent; i++) { this.Code += "	"; }
			}

            public string GetCode() { return this.Code;}
		}
       
        private static void WriteWaveStatChange(
            Pi3Compiler compiler,
            YamlVvvfSoundData reference,
            YamlMasconData.YamlMasconDataPattern yaml,
            double min_freq
        )
        {
            compiler.WriteLineCode("pwm->min_freq = " + min_freq + ";");
            compiler.WriteLineCode("double _wave_stat = status->wave_stat;");
            compiler.WriteLineCode("_wave_stat = _wave_stat < pwm->min_freq ? pwm->min_freq : _wave_stat;");

            compiler.WriteLineCode("if (status->mascon_off)");
            compiler.WriteLineCode("{");
            compiler.AddIndent();

            compiler.WriteLineCode("status->free_freq_change = " + yaml.Off.FrequencyChangeRate + ";");
            compiler.WriteLineCode("if (status->wave_stat > " + yaml.Off.MaxControlFrequency + ")");
            compiler.AddIndent();
            compiler.WriteLineCode("status->wave_stat = " + yaml.Off.MaxControlFrequency + ";");
            compiler.DecrementIndent();

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");
            compiler.WriteLineCode("if (status->free_run && !status->mascon_off)");
            compiler.WriteLineCode("{");
            compiler.AddIndent();

            compiler.WriteLineCode("status->free_freq_change = " + yaml.On.FrequencyChangeRate + ";");
            compiler.WriteLineCode("if (status->wave_stat >= " + yaml.On.MaxControlFrequency + ")");
            compiler.AddIndent();
            compiler.WriteLineCode("status->wave_stat = status->sin_angle_freq * M_1_2PI;");
            compiler.DecrementIndent();

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");
        }

        private static void WriteWavePatterns(
            Pi3Compiler compiler,
            YamlVvvfSoundData reference,
            List<YamlControlData> list,
            YamlMasconData.YamlMasconDataPattern freqInfo
        )
        {
            List<YamlControlData> control_list = new(list);
            control_list.Sort((a, b) => b.ControlFrequencyFrom.CompareTo(a.ControlFrequencyFrom));

            for (int i = 0; control_list.Count > i;i++)
            {
                YamlControlData data = control_list[i];
                List<string> _if = [];

                if(!data.EnableNormal)
                    _if.Add("status->free_run");
                if (!data.EnableFreeRunOff)
                    _if.Add("!(status->free_run && status->mascon_off)");
                if (!data.EnableFreeRunOn)
                    _if.Add("!(status->free_run && !status->mascon_off)");

                {
                    string _condition = "(" + data.ControlFrequencyFrom + " <= _wave_stat)";

                    if (data.StuckFreeRunOn && data.StuckFreeRunOff)
                        _condition += " || " + "(status->free_run && status->sin_angle_freq > " + data.ControlFrequencyFrom + " * M_2PI)";
                    else
                    {
                        if (data.StuckFreeRunOn) _condition += " || (!status->mascon_off && status->free_run && status->sin_angle_freq > " + data.ControlFrequencyFrom + " * M_2PI)";
                        if (data.StuckFreeRunOff) _condition += " || (status->mascon_off && status->free_run && status->sin_angle_freq > " + data.ControlFrequencyFrom + " * M_2PI)";
                    }

                    _if.Add(_condition);
                }
                

                if (!data.EnableFreeRunOn && !data.EnableFreeRunOff)
                    _if.Add("!status->free_run");
                else {
                    if (!data.EnableFreeRunOn) _if.Add("!(status->free_run && !status->mascon_off)");
                    if (!data.EnableFreeRunOff) _if.Add("!(status->free_run && status->mascon_off)");
                }
                if (data.RotateFrequencyBelow != -1) _if.Add("status->sin_angle_freq <" + data.RotateFrequencyBelow + " * M_2PI");
                if (data.RotateFrequencyFrom != -1) _if.Add("status->sin_angle_freq > " + data.RotateFrequencyFrom + " * M_2PI");

                string _s = (i == 0 ? "if" : "else if") + "(";
                for(int x = 0; x < _if.Count; x++)
                {
                    _s += (x == 0 ? "" : " && ") + "(" + _if[x] + ")";
                }
                _s += ")";
                compiler.WriteLineCode(_s);
                compiler.WriteLineCode("{");
                compiler.AddIndent();

                compiler.WriteLineCode("pwm->pulse_mode.pulse_name = " + data.PulseMode.PulseType.ToString() + ";");
                compiler.WriteLineCode("pwm->pulse_mode.alt_mode = " + data.PulseMode.Alternative.ToString() + ";");

                {
                    YamlControlData.YamlAmplitude amplitude = data.Amplitude;
                    static void _WriteAmplitudeControl(
                        Pi3Compiler compiler,
                        YamlControlData.YamlAmplitude.YamlControlDataAmplitude? _default,
                        YamlControlData.YamlAmplitude.YamlControlDataAmplitude _target,
                        bool refer_freq_sin,
                        double max_freq
                    )
                    {
                        
                        if (_default == null) // This section should have _target as _default
                        {
                            compiler.WriteLineCode("double _amp = 0;");
                            compiler.WriteLineCode("double _c = " + (!refer_freq_sin ? "_wave_stat" : "status->sin_angle_freq * M_1_2PI") + ";");

                            compiler.WriteLineCode("{"); compiler.AddIndent();

                            YamlControlData.YamlAmplitude.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter _t = _target.Parameter;

                            if(_t.EndAmplitude == _t.StartAmplitude) compiler.WriteLineCode("_amp = " + _t.StartAmplitude + ";");
                            else if (_target.Mode == AmplitudeMode.Linear)
                            {
                                _WriteRangeLimitCheck(compiler, null, _t, true, true); ;
                                double _a = (_t.EndAmplitude - _t.StartAmplitude) / (_t.EndFrequency - _t.StartFrequency);
                                compiler.WriteLineCode("_amp = " + _a + " * _c + " + (-_a * _t.StartFrequency + _t.StartAmplitude) + ";");
                            }
                            else if (_target.Mode == AmplitudeMode.Wide_3_Pulse)
                            {
                                _WriteRangeLimitCheck(compiler, null, _t, true, true);
                                double _a = (_t.EndAmplitude - _t.StartAmplitude) / (_t.EndFrequency - _t.StartFrequency);
                                double _b = -_a * _t.StartFrequency + _t.StartAmplitude;
                                compiler.WriteLineCode("_amp = " + (0.2 * _a) + " * _c + " + (0.2 * _b + 0.8) + ";");
                            }
                            else if (_target.Mode == AmplitudeMode.Inv_Proportional)
                            {
                                _WriteRangeLimitCheck(compiler, null, _t, true, true);
                                double _a = (1.0 / _t.EndAmplitude - 1.0 / _t.StartAmplitude) / (_t.EndFrequency - _t.StartFrequency);
                                double _b = -_a * _t.StartFrequency + (1.0 / _t.StartAmplitude);
                                compiler.WriteLineCode("double _x = " + _a + " * _c + " + _b + ";");

                                double c = -_t.CurveChangeRate;
                                double k = _t.EndAmplitude;
                                double l = _t.StartAmplitude;
                                double a = 1 / ((1 / l) - (1 / k)) * (1 / (l - c) - 1 / (k - c));
                                double b = 1 / (1 - 1 / l * k) * (1 / (l - c) - 1 / l * k / (k - c));
                                compiler.WriteLineCode("_amp = 1 / (" + a + " * _x + " + b + ") +" + c + " ;");
                            }
                            else if (_target.Mode == AmplitudeMode.Sine)
                            {
                                _WriteRangeLimitCheck(compiler, null, _t, false, true);
                                compiler.WriteLineCode("double _x = _c * " + (Math.PI / (2.0 * _t.EndFrequency)));
                                compiler.WriteLineCode("_amp = _x * " + _t.EndAmplitude);
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231019180255");
                            }

                            if (_t.CutOffAmplitude >= 0)
                                compiler.WriteLineCode("if (" + _t.CutOffAmplitude + " > _amp) _amp = 0;");
                            if (_t.MaxAmplitude != -1)
                                compiler.WriteLineCode("if (" + _t.MaxAmplitude + " < _amp) _amp = " + _t.MaxAmplitude + ";");

                            compiler.DecrementIndent(); compiler.WriteLineCode("}");
                        }
                        else
                        {
                            YamlControlData.YamlAmplitude.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter _t = _target.Parameter;
                            YamlControlData.YamlAmplitude.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter _d = _default.Parameter;

                            if (_t.EndAmplitude == -1 || _t.StartAmplitude == -1) _WriteAmplitudeControl(compiler, null, _default, true, 0);
                            else compiler.WriteLineCode("double _c, _amp;");

                            compiler.WriteLineCode("{"); compiler.AddIndent();

                            compiler.WriteLineCode("_c = _wave_stat;");

                            if(_t.EndFrequency == -1)
                            {
                                if (_d.DisableRangeLimit) compiler.WriteLineCode("double _f_end = status->sin_angle_freq * M_1_2PI;");
                                else
                                {
                                    compiler.WriteLineCode("double _f_end = status->sin_angle_freq * M_1_2PI > " + max_freq + " ? " + max_freq + " : status->sin_angle_freq * M_1_2PI;");
                                    compiler.WriteLineCode("_f_end = _f_end > " + _d.EndFrequency + " ? " + _d.EndFrequency + " : _f_end;");
                                }
                            }

                            if (_target.Mode == AmplitudeMode.Linear)
                            {
                                _WriteRangeLimitCheck(compiler, _d, _t, true, true);
                                string _a = "double _a = (" + (_t.EndAmplitude == -1 ? "_amp" : _t.EndAmplitude.ToString()) + " - " + (_t.StartAmplitude == -1 ? "_amp" : _t.StartAmplitude.ToString()) + ") / (" + (_t.EndFrequency == -1 ? "_f_end" : _t.EndFrequency.ToString()) + " - " + (_t.StartFrequency == -1 ? "0" : _t.StartFrequency.ToString()) + ");";
                                string _b = "double _b = -_a * " + (_t.StartFrequency == -1 ? "0" : _t.StartFrequency.ToString()) + " + " + (_t.StartAmplitude == -1 ? "_amp" : _t.StartAmplitude.ToString()) + ";";
                                compiler.WriteLineCode(_a);
                                compiler.WriteLineCode(_b);
                                compiler.WriteLineCode("_amp = _a * _c + _b;");
                            }
                            else if (_target.Mode == AmplitudeMode.Wide_3_Pulse)
                            {
                                _WriteRangeLimitCheck(compiler, _d, _t, true, true);
                                string _a = "double _a = (" + (_t.EndAmplitude == -1 ? "_amp" : _t.EndAmplitude.ToString()) + " - " + (_t.StartAmplitude == -1 ? "_amp" : _t.StartAmplitude.ToString()) + ") / (" + (_t.EndFrequency == -1 ? "_f_end" : _t.EndFrequency.ToString()) + " - " + (_t.StartFrequency == -1 ? "0" : _t.StartFrequency.ToString()) + ");";
                                string _b = "double _b = -_a * " + (_t.StartFrequency == -1 ? "0" : _t.StartFrequency.ToString()) + " + " + (_t.StartAmplitude == -1 ? "_amp" : _t.StartAmplitude.ToString()) + ";";
                                compiler.WriteLineCode(_a);
                                compiler.WriteLineCode(_b);
                                compiler.WriteLineCode("_amp = 0.2 * (_a * _c + _b) + 0.8;");
                            }
                            else if (_target.Mode == AmplitudeMode.Inv_Proportional)
                            {
                                _WriteRangeLimitCheck(compiler, _d, _t, true, true);
                                string _a = "double _a = (1.0 / " + (_t.EndAmplitude == -1 ? "_amp" : _t.EndAmplitude.ToString()) + " - 1.0 / " + (_t.StartAmplitude == -1 ? "1" : _t.StartAmplitude.ToString()) + ") / (" + (_t.EndFrequency == -1 ? "_f_end" : _t.EndFrequency.ToString()) + " - " + (_t.StartFrequency == -1 ? "0" : _t.StartFrequency.ToString()) + ");";                                
                                string _b = "double _b = -_a * " + (_t.StartFrequency == -1 ? "0" : _t.StartFrequency.ToString()) + " + 1.0 / " + (_t.StartAmplitude == -1 ? "1" : _t.StartAmplitude.ToString()) + ";";
                                compiler.WriteLineCode(_a);
                                compiler.WriteLineCode(_b);
                                compiler.WriteLineCode("double _x = _a * _c + _b;");

                                compiler.WriteLineCode("double c = " + -(_t.CurveChangeRate == -1 ? _d.CurveChangeRate : _t.CurveChangeRate) + ";");
                                compiler.WriteLineCode("double k = " + (_t.EndAmplitude == -1 ? "_amp" : _t.EndAmplitude) + ";");
                                compiler.WriteLineCode("double l = " + (_t.StartAmplitude == -1 ? "1" : _t.StartAmplitude) + ";");
                                compiler.WriteLineCode("if(l == k)"); compiler.WriteLineCode("{"); compiler.AddIndent();
                                compiler.WriteLineCode("_amp = l;");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}"); 
                                compiler.WriteLineCode("else"); compiler.WriteLineCode("{"); compiler.AddIndent();
                                compiler.WriteLineCode("double a = 1 / ((1 / l) - (1 / k)) * (1 / (l - c) - 1 / (k - c));");
                                compiler.WriteLineCode("double b = 1 / (1 - (1 / l) * k) * (1 / (l - c) - (1 / l) * k / (k - c));");
                                compiler.WriteLineCode("_amp = 1.0 / (a * _x + b) + c;");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}"); 
                            }
                            else if (_target.Mode == AmplitudeMode.Sine)
                            {
                                _WriteRangeLimitCheck(compiler, _d, _t, false, true);
                                compiler.WriteLineCode("double _x = M_PI_2 * _c /" +  (_t.EndFrequency == -1 ? "_f_end" : _t.EndFrequency.ToString()) + ";");
                                compiler.WriteLineCode("_amp = sin(_x) * " + (_t.EndAmplitude == -1 ? "_amp" : _t.EndAmplitude.ToString()) + ";");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018213430");
                            }

                            if ((_t.CutOffAmplitude == -1 ? _d.CutOffAmplitude : _t.CutOffAmplitude) >= 0)
                                compiler.WriteLineCode("if (" + (_t.CutOffAmplitude == -1 ? _d.CutOffAmplitude : _t.CutOffAmplitude) + " > _amp) _amp = 0;");
                            if ((_t.MaxAmplitude == -1 ? _d.MaxAmplitude : _t.MaxAmplitude) != -1)
                                compiler.WriteLineCode("if (" + (_t.MaxAmplitude == -1 ? _d.MaxAmplitude : _t.MaxAmplitude) + " < _amp) _amp = " + (_t.MaxAmplitude == -1 ? _d.MaxAmplitude : _t.MaxAmplitude) + ";");

                            compiler.DecrementIndent(); compiler.WriteLineCode("}");

                        }

                        static void _WriteRangeLimitCheck(
                            Pi3Compiler compiler,
                            YamlControlData.YamlAmplitude.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter? _d,
                            YamlControlData.YamlAmplitude.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter _t,
                            bool min, bool max
                        )
                        {
                            if (_t.DisableRangeLimit) return;
                            if(_d == null)
                            {
                                if (min) compiler.WriteLineCode("if (_c < " + _t.StartFrequency + ") _c = " + _t.StartFrequency + ";");
                                if (max) compiler.WriteLineCode("if (_c > " + _t.EndFrequency + ") _c = " + _t.EndFrequency + ";");
                            }
                            else
                            {
                                if (min) compiler.WriteLineCode("if (_c < " + (_t.StartFrequency == -1 ? _d.StartFrequency : _t.StartFrequency) + ") _c = " + (_t.StartFrequency == -1 ? _d.StartFrequency : _t.StartFrequency) + ";");
                                if (max) compiler.WriteLineCode("if (_c > " + (_t.EndFrequency == -1 ? _d.EndFrequency : _t.EndFrequency) + ") _c = " + (_t.EndFrequency == -1 ? _d.EndFrequency : _t.EndFrequency) + ";");
                            }
                        }
                    }

                    compiler.WriteLineCode("if (!status->free_run) {"); compiler.AddIndent();
                    _WriteAmplitudeControl(compiler, null, amplitude.DefaultAmplitude, false, 0);
                    compiler.WriteLineCode("pwm->amplitude = _amp;");
                    compiler.DecrementIndent(); compiler.WriteLineCode("}");

                    compiler.WriteLineCode("if (status->free_run && !status->mascon_off) {"); compiler.AddIndent();
                    _WriteAmplitudeControl(compiler, amplitude.DefaultAmplitude, amplitude.FreeRunAmplitude.On, false, freqInfo.On.MaxControlFrequency);
                    compiler.WriteLineCode("pwm->amplitude = _amp;");
                    compiler.DecrementIndent(); compiler.WriteLineCode("}");

                    compiler.WriteLineCode("if (status->free_run && status->mascon_off) {"); compiler.AddIndent();
                    _WriteAmplitudeControl(compiler, amplitude.DefaultAmplitude, amplitude.FreeRunAmplitude.Off, false, freqInfo.Off.MaxControlFrequency);
                    compiler.WriteLineCode("pwm->amplitude = _amp;");
                    compiler.DecrementIndent(); compiler.WriteLineCode("}");


                }

                if (data.PulseMode.PulseType == PulseTypeName.ASYNC)
                {
                    {
                        YamlControlData.YamlAsync.CarrierFrequency async = data.AsyncModulationData.CarrierWaveData;
                        if (async.Mode == YamlControlData.YamlAsync.CarrierFrequency.CarrierFrequencyValueMode.Const)
                        {
                            compiler.WriteLineCode("pwm->carrier_freq.base_freq = " + async.Constant + ";");
                        }
                        else if (async.Mode == YamlControlData.YamlAsync.CarrierFrequency.CarrierFrequencyValueMode.Moving)
                        {
                            YamlControlData.YamlMovingValue moving = async.MovingValue;
                            if (moving.Type == YamlControlData.YamlMovingValue.MovingValueType.Proportional)
                            {
                                compiler.WriteLineCode("{"); compiler.AddIndent();
                                double _a = (moving.EndValue - moving.StartValue) / (moving.End - moving.Start);
                                double _b = -_a * moving.Start + moving.StartValue;
                                compiler.WriteLineCode("pwm->carrier_freq.base_freq = " + _a + " * _wave_stat + " + _b + ";");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}");
                            }
                            else if (moving.Type == YamlControlData.YamlMovingValue.MovingValueType.Sine)
                            {
                                compiler.WriteLineCode("{"); compiler.AddIndent();
                                double _start = Math.Asin(moving.StartValue / moving.EndValue);
                                double _a = (Math.PI / 2.0 - _start) / (moving.End - moving.Start);
                                double _b = -_a * moving.Start + _start;
                                compiler.WriteLineCode("pwm->carrier_freq.base_freq = sin(" + _a + " * _wave_stat + " + _b + ") * " + moving.EndValue + ";");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018205819");
                            }

                        }else if (async.Mode == YamlControlData.YamlAsync.CarrierFrequency.CarrierFrequencyValueMode.Table)
                        {
                            List<YamlAsyncParameterCarrierFreqTableValue> _list = new(async.CarrierFrequencyTable.CarrierFrequencyTableValues);
                            _list.Sort((a, b) => b.ControlFrequencyFrom.CompareTo(a.ControlFrequencyFrom));
                            for (int x = 0; x < _list.Count; x++)
                            {
                                YamlAsyncParameterCarrierFreqTableValue val = _list[x];
                                string _con = (x == 0 ? "if" : "else if") + "(";
                                _con += "_wave_stat >= " + val.ControlFrequencyFrom;
                                if(val.FreeRunStuckAtHere) _con += " || " + "(status->free_run && status->sin_angle_freq > " + val.ControlFrequencyFrom + " * M_2PI)";
                                _con += ")";
                                compiler.WriteLineCode(_con);
                                compiler.AddIndent(); compiler.WriteLineCode("pwm->carrier_freq.base_freq = " + val.CarrierFrequency + ";");
                                compiler.DecrementIndent();

                            }

                        }
                        else
                        {
                            compiler.WriteLineCode(" // @ 20231018205827");
                        }
                    }

                    {
                        YamlControlData.YamlAsync.RandomModulation.YamlAsyncParameterRandomValue random_interval = data.AsyncModulationData.RandomData.Interval;
                        if (random_interval.Mode == YamlControlData.YamlAsync.RandomModulation.YamlAsyncParameterRandomValue.YamlAsyncParameterRandomValueMode.Const)
                        {
                            compiler.WriteLineCode("pwm->carrier_freq.interval = " + random_interval.Constant + ";");
                        }
                        else if (random_interval.Mode == YamlControlData.YamlAsync.RandomModulation.YamlAsyncParameterRandomValue.YamlAsyncParameterRandomValueMode.Moving)
                        {
                            YamlControlData.YamlMovingValue moving = random_interval.MovingValue;
                            if (moving.Type == YamlControlData.YamlMovingValue.MovingValueType.Proportional)
                            {
                                compiler.WriteLineCode("{"); compiler.AddIndent();
                                double _a = (moving.EndValue - moving.StartValue) / (moving.End - moving.Start);
                                double _b = -_a * moving.Start + moving.StartValue;
                                compiler.WriteLineCode("pwm->carrier_freq.interval = " + _a + " * _wave_stat + " + _b + ";");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018210032 ");
                            }

                        }
                        else
                        {
                            compiler.WriteLineCode(" // @ 20231018210021 ");
                        }
                    }

                    {
                        YamlControlData.YamlAsync.RandomModulation.YamlAsyncParameterRandomValue random_range = data.AsyncModulationData.RandomData.Range;
                        if (random_range.Mode == YamlControlData.YamlAsync.RandomModulation.YamlAsyncParameterRandomValue.YamlAsyncParameterRandomValueMode.Const)
                        {
                            compiler.WriteLineCode("pwm->carrier_freq.range = " + random_range.Constant + ";");
                        }
                        else if (random_range.Mode == YamlControlData.YamlAsync.RandomModulation.YamlAsyncParameterRandomValue.YamlAsyncParameterRandomValueMode.Moving)
                        {
                            YamlControlData.YamlMovingValue moving = random_range.MovingValue;
                            if (moving.Type == YamlControlData.YamlMovingValue.MovingValueType.Proportional)
                            {
                                compiler.WriteLineCode("{"); compiler.AddIndent();
                                double _a = (moving.EndValue - moving.StartValue) / (moving.End - moving.Start);
                                double _b = -_a * moving.Start + moving.StartValue;
                                compiler.WriteLineCode("pwm->carrier_freq.range = " + _a + " * _wave_stat + " + _b + ";");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018210113 ");
                            }

                        }
                        else
                        {
                            compiler.WriteLineCode(" // @ 20231018210117 ");
                        }
                    }
                }

                if (PulseModeConfiguration.GetAvailablePulseDataKey(data.PulseMode, reference.Level).ToList().Contains(PulseDataKey.Dipolar)){
                    PulseDataValue? Value = data.PulseMode.PulseData.GetValueOrDefault(PulseDataKey.Dipolar);
                    if (Value == null) continue;

                    if (Value.Mode == PulseDataValue.PulseDataValueMode.Const)
                    {
                        compiler.WriteLineCode("pwm->dipolar = " + Value.Constant + ";");
                    }
                    else if (Value.Mode == PulseDataValue.PulseDataValueMode.Moving)
                    {
                        YamlControlData.YamlMovingValue moving = Value.MovingValue;
                        if (moving.Type == YamlControlData.YamlMovingValue.MovingValueType.Proportional)
                        {
                            compiler.WriteLineCode("{"); compiler.AddIndent();
                            double _a = (moving.EndValue - moving.StartValue) / (moving.End - moving.Start);
                            double _b = -_a * moving.Start + moving.StartValue;
                            compiler.WriteLineCode("pwm->dipolar = " + _a + " * _wave_stat + " + _b + ";");
                            compiler.DecrementIndent(); compiler.WriteLineCode("}");
                        }
                        else
                        {
                            compiler.WriteLineCode(" // @ 20231018210510 ");
                        }

                    }
                    else
                    {
                        compiler.WriteLineCode(" // @ 20231018210517 ");
                    }
                }

                compiler.DecrementIndent();
                compiler.WriteLineCode("}");
                


            }

            compiler.WriteLineCode("else");
            compiler.WriteLineCode("{");
            compiler.AddIndent();
            compiler.WriteLineCode("pwm->none = true;");
            compiler.DecrementIndent();
            compiler.WriteLineCode("}");
        }

        public static string GenerateC(YamlVvvfSoundData vfsoundData, string functionName)
        {
            Pi3Compiler compiler = new();
            compiler.WriteLineCode("void calculate" + functionName + "(VvvfValues *status, PwmCalculateValues *pwm)");
            compiler.WriteLineCode("{");
            compiler.AddIndent();
            List<String> lines =
            [
                "CarrierFreq carrier_freq = {0, 0, 0};",
                "PulseMode pulse_mode = {P_1, Default};",
                "pwm->level = " + vfsoundData.Level.ToString() + ";",
                "pwm->dipolar = -1;",
                "pwm->min_freq = 0;",
                "pwm->amplitude = 0;",
                "pwm->none = false;",
                "pwm->pulse_mode = pulse_mode;",
                "pwm->carrier_freq = carrier_freq;"
            ];
            for(int i = 0; i < lines.Count; i++)
            {
                compiler.WriteLineCode(lines[i]);
            }

            compiler.WriteLineCode("if (status->brake)");
            compiler.WriteLineCode("{");
            compiler.AddIndent();

            WriteWaveStatChange(compiler, vfsoundData, vfsoundData.MasconData.Braking, vfsoundData.MinimumFrequency.Braking);
            WriteWavePatterns(compiler, vfsoundData, vfsoundData.BrakingPattern, vfsoundData.MasconData.Braking);

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");
            compiler.WriteLineCode("else");
            compiler.WriteLineCode("{");
            compiler.AddIndent();

            WriteWaveStatChange(compiler, vfsoundData, vfsoundData.MasconData.Accelerating, vfsoundData.MinimumFrequency.Accelerating);
            WriteWavePatterns(compiler, vfsoundData, vfsoundData.AcceleratePattern, vfsoundData.MasconData.Accelerating);

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");

            compiler.WriteLineCode("if (status->wave_stat == 0) pwm->none = true;");

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");

            return compiler.GetCode();
        }

    }
}
