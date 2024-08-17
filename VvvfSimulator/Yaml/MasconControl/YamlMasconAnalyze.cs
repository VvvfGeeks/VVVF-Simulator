using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze.YamlMasconData;

namespace VvvfSimulator.Yaml.MasconControl
{
    public class YamlMasconAnalyze
    {
        public class YamlMasconData
        {

            public List<YamlMasconDataPoint> points = new();

            public YamlMasconData Clone()
            {
                YamlMasconData ymd = (YamlMasconData)MemberwiseClone();

                List<YamlMasconDataPoint> clone_points = new();
                for(int i = 0; i < points.Count; i++)
                {
                    clone_points.Add(points[i].Clone());
                }

                ymd.points = clone_points;
                return ymd;

            }

            public double GetEstimatedSteps(double sampleTime)
            {
                double totalDuration = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    YamlMasconDataPoint point = points[i];
                    totalDuration += point.duration > 0 ? point.duration : 0;
                }

                return totalDuration / sampleTime;
            }

            public YamlMasconDataCompiled GetCompiled()
            {
                return new YamlMasconDataCompiled(this);
            }

            public class YamlMasconDataPoint {

                public int order { get; set; } = 0;
                public double rate { get; set; } = 0; //Hz / s
                public double duration { get; set; } = 0;// S
                public bool brake { get; set; } = false;
                public bool mascon_on { get; set; } = true;


                public YamlMasconDataPoint() { }
                public YamlMasconDataPoint(int Order)
                {
                    order = Order;
                }

                public YamlMasconDataPoint Clone()
                {
                    return (YamlMasconDataPoint)MemberwiseClone();
                }
            }

        }

        public class YamlMasconDataCompiled
        {
            public List<YamlMasconDataCompiledPoint> Points { get; set; } = new List<YamlMasconDataCompiledPoint>();

            public YamlMasconDataCompiled(YamlMasconData ymd)
            {
                YamlMasconData _ymd = ymd.Clone();
                _ymd.points.Sort((a, b) => a.order - b.order);

                double currentTime = 0;
                double currentFrequency = 0;
                for(int i = 0; i < _ymd.points.Count; i++)
                {
                    YamlMasconDataPoint yaml_Mascon_Data_Point = _ymd.points[i];
                    if(yaml_Mascon_Data_Point.duration == -1)
                    {
                        currentFrequency = yaml_Mascon_Data_Point.rate;
                        continue;
                    }

                    double deltaTime = yaml_Mascon_Data_Point.duration;
                    double deltaFrequency = deltaTime * yaml_Mascon_Data_Point.rate * (yaml_Mascon_Data_Point.brake ? -1 : 1);

                    if (deltaTime == 0) continue;

                    YamlMasconDataCompiledPoint yaml_Mascon_Data_Compiled_Point = new()
                    {
                        StartTime = currentTime,
                        EndTime = currentTime + deltaTime,
                        StartFrequency = currentFrequency,
                        EndFrequency = currentFrequency + deltaFrequency,
                        IsMasconOn = yaml_Mascon_Data_Point.mascon_on,
                        IsAccel = !yaml_Mascon_Data_Point.brake
                    };
                    Points.Add(yaml_Mascon_Data_Compiled_Point);

                    currentTime += deltaTime;
                    currentFrequency += deltaFrequency;
                }
            }

            public double GetEstimatedSteps(double sampleTime)
            {
                double totalTime = this.Points.Last().EndTime;
                return totalTime / sampleTime;
            }

            public class YamlMasconDataCompiledPoint
            {
                public double StartTime { get; set; } = 0;
                public double EndTime { get; set; } = 0;
                public double StartFrequency { get; set; } = 0;
                public double EndFrequency { get; set; } = 0;
                public bool IsMasconOn { get; set; } = true;
                public bool IsAccel { get; set; } = true;

                public YamlMasconDataCompiledPoint Clone()
                {
                    return (YamlMasconDataCompiledPoint)MemberwiseClone();
                }
            }
        }

        public class YamlMasconManage
        {

            public static YamlMasconData DefaultData = new()
            {
                points = new()
                {
                    new YamlMasconDataPoint()
                    {
                        rate = 5,
                        duration = 20,
                        brake = false,
                        mascon_on = true,
                        order = 0,
                    },
                    new YamlMasconDataPoint()
                    {
                        rate = 0,
                        duration = 4,
                        brake = false,
                        mascon_on = false,
                        order = 1,
                    },
                    new YamlMasconDataPoint()
                    {
                        rate = 5,
                        duration = 20,
                        brake = true,
                        mascon_on = true,
                        order = 2,
                    },
                }
            };
            public static YamlMasconData CurrentData = DefaultData.Clone();

            public static YamlMasconData Sort()
            {
                CurrentData.points.Sort((a, b) => Math.Sign(a.order - b.order));
                return CurrentData;
            }
            public static bool SaveYaml(String path)
            {
                try
                {
                    using TextWriter writer = File.CreateText(path);
                    var serializer = new Serializer();
                    serializer.Serialize(writer, CurrentData);
                    writer.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static bool LoadYaml(String path)
            {
                try
                {
                    var input = new StreamReader(path, Encoding.UTF8);
                    var deserializer = new Deserializer();
                    YamlMasconData deserializeObject = deserializer.Deserialize<YamlMasconData>(input);
                    CurrentData = deserializeObject;
                    input.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static YamlMasconData DeepClone(YamlMasconData src)
            {
                YamlMasconData deserializeObject = new Deserializer().Deserialize<YamlMasconData>(new Serializer().Serialize(src));
                return deserializeObject;
            }

        }
        
    }
}
