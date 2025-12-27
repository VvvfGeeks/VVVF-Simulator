namespace VvvfSimulator.Vvvf.Modulation
{
    public class DeltaSigma
    {
        private double integrator = 0.0;
        private double lastProcessTime = 0.0;
        private double lastUpdateTime = 0.0;
        private int lastOutBit = 0;

        public double FeedbackInterval { get; set; } = 1e-4;

        public DeltaSigma Clone()
        {
            DeltaSigma Copy = (DeltaSigma)MemberwiseClone();
            return Copy;
        }

        public int Process(double input, double nowTime)
        {
            double dt = nowTime - lastProcessTime;
            lastProcessTime = nowTime;

            double quantized = (lastOutBit == 1) ? 1.0 : -1.0;
            integrator += (input - quantized) * dt;

            if (nowTime - lastUpdateTime >= FeedbackInterval)
            {
                lastOutBit = (integrator >= 0.0) ? 1 : 0;
                lastUpdateTime = nowTime;
            }

            return lastOutBit;
        }

        public void Reset(double nowTime = 0.0)
        {
            integrator = 0.0;
            lastOutBit = 0;
            lastUpdateTime = nowTime;
            lastProcessTime = nowTime;
        }
        public void ResetIfLastTime(double lastTime)
        {
            if (lastTime != lastProcessTime) Reset(lastTime);
        }
    }

}
