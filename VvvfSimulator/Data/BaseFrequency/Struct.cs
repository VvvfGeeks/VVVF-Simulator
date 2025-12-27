using System.Collections.Generic;
using static VvvfSimulator.Data.Util;

namespace VvvfSimulator.Data.BaseFrequency
{
    public class Struct
    {
        public List<Point> Points = [];
        public override string ToString()
        {
            return GetPropertyValues(this);
        }
        public Struct Clone()
        {
            Struct ymd = (Struct)MemberwiseClone();

            {
                List<Point> ClonePoints = [];
                for (int i = 0; i < Points.Count; i++)
                {
                    ClonePoints.Add(Points[i].Clone());
                }
                ymd.Points = ClonePoints;
            }

            return ymd;
        }
        public double GetEstimatedSteps(double sampleTime)
        {
            double totalDuration = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                Point point = Points[i];
                totalDuration += point.Duration > 0 ? point.Duration : 0;
            }

            return totalDuration / sampleTime;
        }
        public StructCompiled GetCompiled()
        {
            return new StructCompiled(this);
        }

        public class Point
        {
            public int Order { get; set; } = 0;
            public double Rate { get; set; } = 0; //Hz / s
            public double Duration { get; set; } = 0;// S
            public bool Brake { get; set; } = false;
            public bool PowerOn { get; set; } = true;
            public Point() { }
            public Point(int Order)
            {
                this.Order = Order;
            }
            public override string ToString()
            {
                return GetPropertyValues(this);
            }
            public Point Clone()
            {
                return (Point)MemberwiseClone();
            }
        }
    }
}
