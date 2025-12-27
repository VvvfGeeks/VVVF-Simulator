using System.Collections.Generic;
using System.Linq;

namespace VvvfSimulator.Data.BaseFrequency
{
    public class StructCompiled
    {
        public List<Point> Points { get; set; } = [];

        public StructCompiled(Struct ymd)
        {
            Struct _ymd = ymd.Clone();
            _ymd.Points.Sort((a, b) => a.Order - b.Order);

            double currentTime = 0;
            double currentFrequency = 0;
            for (int i = 0; i < _ymd.Points.Count; i++)
            {
                Struct.Point OriginalPoint = _ymd.Points[i];
                if (OriginalPoint.Duration == -1)
                {
                    currentFrequency = OriginalPoint.Rate;
                    continue;
                }

                double deltaTime = OriginalPoint.Duration;
                double deltaFrequency = deltaTime * OriginalPoint.Rate * (OriginalPoint.Brake ? -1 : 1);

                if (deltaTime == 0) continue;

                Point Point = new()
                {
                    StartTime = currentTime,
                    EndTime = currentTime + deltaTime,
                    StartFrequency = currentFrequency,
                    EndFrequency = currentFrequency + deltaFrequency,
                    IsPowerOn = OriginalPoint.PowerOn,
                    IsAccel = !OriginalPoint.Brake
                };
                Points.Add(Point);

                currentTime += deltaTime;
                currentFrequency += deltaFrequency;
            }
        }

        public double GetEstimatedSteps(double sampleTime)
        {
            double totalTime = this.Points.Last().EndTime;
            return totalTime / sampleTime;
        }

        public class Point
        {
            public double StartTime { get; set; } = 0;
            public double EndTime { get; set; } = 0;
            public double StartFrequency { get; set; } = 0;
            public double EndFrequency { get; set; } = 0;
            public bool IsPowerOn { get; set; } = true;
            public bool IsAccel { get; set; } = true;

            public Point Clone()
            {
                return (Point)MemberwiseClone();
            }
        }
    }
}
