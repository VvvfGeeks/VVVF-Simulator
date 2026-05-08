namespace VvvfSimulator.Vvvf.Modulation
{
    public class BaseWave
    {
        public double AngleFrequency { get; set; } = 0;
        public bool IsZeroFrequency() => AngleFrequency == 0;
        public double Frequency
        {
            get
            {
                return AngleFrequency / MyMath.M_2PI;
            }
            set
            {
                AngleFrequency = MyMath.M_2PI * value;
            }
        }
        public double Time { get; set; } = 0;
        public BaseWave Clone()
        {
            BaseWave Copy = (BaseWave)MemberwiseClone();
            return Copy;
        }
    }
}
