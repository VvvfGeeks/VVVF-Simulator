using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace VvvfSimulator.Vvvf
{
    public class CustomPwm
    {
        private const int MaxPwmLevel = 2;

        public readonly int SwitchCount = 5;
        public readonly double ModulationIndexDivision = 0.01;
        public readonly double MinimumModulationIndex = 0.0;
        public readonly uint BlockCount = 0;
        public readonly (double SwitchAngle, int Output)[] SwitchAngleTable = [];
        public readonly bool[] Polarity = [];

        public CustomPwm(Stream? St)
        {
            if (St == null) throw new Exception();

            St.Seek(0, SeekOrigin.Begin);
            byte SwitchCount = (byte)St.ReadByte();

            byte[] DivisionRaw = new byte[8];
            St.Read(DivisionRaw, 0, 8);
            double Division = BitConverter.ToDouble(DivisionRaw, 0);

            byte[] StartModulationRaw = new byte[8];
            St.Read(StartModulationRaw, 0, 8);
            double StartModulation = BitConverter.ToDouble(StartModulationRaw, 0);

            byte[] LengthRaw = new byte[4];
            St.Read(LengthRaw, 0, 4);
            uint Length = BitConverter.ToUInt32(LengthRaw, 0);

            this.SwitchCount = SwitchCount;
            this.ModulationIndexDivision = Division;
            this.MinimumModulationIndex = StartModulation;
            this.BlockCount = Length;

            this.SwitchAngleTable = new (double, int)[this.BlockCount * this.SwitchCount];
            this.Polarity = new bool[this.BlockCount];

            for (int i = 0; i < this.BlockCount; i++)
            {
                byte[] PolarityRaw = new byte[1];
                St.Read(PolarityRaw, 0, 1);
                bool Polarity = PolarityRaw[0] == 1;
                this.Polarity[i] = Polarity;

                for (int j = 0; j < this.SwitchCount; j++)
                {
                    byte[] LevelRaw = new byte[1];
                    byte[] SwitchAngleRaw = new byte[8];
                    St.Read(LevelRaw, 0, 1);
                    St.Read(SwitchAngleRaw, 0, 8);
                    int Output = LevelRaw[0];
                    double SwitchAngle = BitConverter.ToDouble(SwitchAngleRaw, 0);
                    this.SwitchAngleTable[i * this.SwitchCount + j] = new(SwitchAngle, Output);
                }
            }

            St.Close();
        }

        public int GetPwm(double M, double X)
        {
            int Index = (int)((M - MinimumModulationIndex) / ModulationIndexDivision);
            Index = Math.Clamp(Index, 0, (int)BlockCount - 1);

            (double SwitchAngle, int Output)[] Alpha = new (double SwitchAngle, int Output)[SwitchCount];
            bool Inverted = Polarity[Index];

            for (int i = 0; i < SwitchCount; i++)
            {
                Alpha[i] = SwitchAngleTable[Index * SwitchCount + i];
            }

            return CustomPwm.GetPwm(ref Alpha, X, Inverted);
        }
    
        public static int GetPwm(ref (double SwitchAngle, int Output)[] Alpha, double X, bool Inverted)
        {
            X %= MyMath.M_2PI;
            int Orthant = (int)(X / MyMath.M_PI_2);
            double Angle = X % MyMath.M_PI_2;

            if ((Orthant & 0x01) == 1)
                Angle = MyMath.M_PI_2 - Angle;
            int Pwm = 0;
            for (int i = 0; i < Alpha.Length; i++)
            {
                (double SwitchAngle, int Output) = Alpha[i];
                if (SwitchAngle <= Angle) Pwm = Output;
                else break;
            }

            if ((Orthant > 1 || Inverted) && !(Orthant > 1 && Inverted))
                Pwm = MaxPwmLevel - Pwm;

            return Pwm;
        }
    }
    public static class CustomPwmPresets
    {
        public static void Load()
        {
            if (Loaded || Loading) return;
            Loading = true;
            Task t = Task.Run(() =>
            {
                _L2Chm3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm3Default.bin"));
                _L2Chm3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm3Alt1.bin"));

                _L2Chm5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Default.bin"));
                _L2Chm5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt1.bin"));
                _L2Chm5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt2.bin"));
                _L2Chm5Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt3.bin"));

                _L2Chm7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Default.bin"));
                _L2Chm7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt1.bin"));
                _L2Chm7Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt2.bin"));
                _L2Chm7Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt3.bin"));
                _L2Chm7Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt4.bin"));
                _L2Chm7Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt5.bin"));

                _L2Chm9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Default.bin"));
                _L2Chm9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt1.bin"));
                _L2Chm9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt2.bin"));
                _L2Chm9Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt3.bin"));
                _L2Chm9Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt4.bin"));
                _L2Chm9Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt5.bin"));
                _L2Chm9Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt6.bin"));
                _L2Chm9Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt7.bin"));
                _L2Chm9Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt8.bin"));

                _L2Chm11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Default.bin"));
                _L2Chm11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt1.bin"));
                _L2Chm11Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt2.bin"));
                _L2Chm11Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt3.bin"));
                _L2Chm11Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt4.bin"));
                _L2Chm11Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt5.bin"));
                _L2Chm11Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt6.bin"));
                _L2Chm11Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt7.bin"));
                _L2Chm11Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt8.bin"));
                _L2Chm11Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt9.bin"));
                _L2Chm11Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt10.bin"));
                _L2Chm11Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt11.bin"));
                _L2Chm11Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt12.bin"));

                _L2Chm13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Default.bin"));
                _L2Chm13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt1.bin"));
                _L2Chm13Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt2.bin"));
                _L2Chm13Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt3.bin"));
                _L2Chm13Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt4.bin"));
                _L2Chm13Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt5.bin"));
                _L2Chm13Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt6.bin"));
                _L2Chm13Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt7.bin"));
                _L2Chm13Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt8.bin"));
                _L2Chm13Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt9.bin"));
                _L2Chm13Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt10.bin"));
                _L2Chm13Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt11.bin"));
                _L2Chm13Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt12.bin"));
                _L2Chm13Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt13.bin"));

                _L2Chm15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Default.bin"));
                _L2Chm15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt1.bin"));
                _L2Chm15Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt2.bin"));
                _L2Chm15Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt3.bin"));
                _L2Chm15Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt4.bin"));
                _L2Chm15Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt5.bin"));
                _L2Chm15Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt6.bin"));
                _L2Chm15Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt7.bin"));
                _L2Chm15Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt8.bin"));
                _L2Chm15Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt9.bin"));
                _L2Chm15Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt10.bin"));
                _L2Chm15Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt11.bin"));
                _L2Chm15Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt12.bin"));
                _L2Chm15Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt13.bin"));
                _L2Chm15Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt14.bin"));
                _L2Chm15Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt15.bin"));
                _L2Chm15Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt16.bin"));
                _L2Chm15Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt17.bin"));
                _L2Chm15Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt18.bin"));
                _L2Chm15Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt19.bin"));
                _L2Chm15Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt20.bin"));
                _L2Chm15Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt21.bin"));
                _L2Chm15Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt22.bin"));
                _L2Chm15Alt23 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt23.bin"));

                _L2Chm17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Default.bin"));
                _L2Chm17Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt1.bin"));
                _L2Chm17Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt2.bin"));
                _L2Chm17Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt3.bin"));
                _L2Chm17Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt4.bin"));
                _L2Chm17Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt5.bin"));
                _L2Chm17Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt6.bin"));
                _L2Chm17Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt7.bin"));
                _L2Chm17Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt8.bin"));
                _L2Chm17Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt9.bin"));
                _L2Chm17Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt10.bin"));
                _L2Chm17Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt11.bin"));

                _L2Chm19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Default.bin"));
                _L2Chm19Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt1.bin"));
                _L2Chm19Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt2.bin"));
                _L2Chm19Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt3.bin"));
                _L2Chm19Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt4.bin"));
                _L2Chm19Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt5.bin"));
                _L2Chm19Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt6.bin"));
                _L2Chm19Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt7.bin"));
                _L2Chm19Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt8.bin"));
                _L2Chm19Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt9.bin"));
                _L2Chm19Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt10.bin"));
                _L2Chm19Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt11.bin"));

                _L2Chm21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Default.bin"));
                _L2Chm21Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt1.bin"));
                _L2Chm21Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt2.bin"));
                _L2Chm21Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt3.bin"));
                _L2Chm21Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt4.bin"));
                _L2Chm21Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt5.bin"));
                _L2Chm21Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt6.bin"));
                _L2Chm21Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt7.bin"));
                _L2Chm21Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt8.bin"));
                _L2Chm21Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt9.bin"));
                _L2Chm21Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt10.bin"));
                _L2Chm21Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt11.bin"));
                _L2Chm21Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt12.bin"));
                _L2Chm21Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt13.bin"));

                _L2Chm23Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Default.bin"));
                _L2Chm23Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt1.bin"));
                _L2Chm23Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt2.bin"));
                _L2Chm23Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt3.bin"));
                _L2Chm23Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt4.bin"));
                _L2Chm23Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt5.bin"));
                _L2Chm23Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt6.bin"));
                _L2Chm23Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt7.bin"));
                _L2Chm23Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt8.bin"));
                _L2Chm23Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt9.bin"));
                _L2Chm23Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt10.bin"));
                _L2Chm23Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt11.bin"));
                _L2Chm23Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt12.bin"));
                _L2Chm23Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt13.bin"));
                _L2Chm23Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt14.bin"));

                _L2Chm25Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Default.bin"));
                _L2Chm25Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt1.bin"));
                _L2Chm25Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt2.bin"));
                _L2Chm25Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt3.bin"));
                _L2Chm25Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt4.bin"));
                _L2Chm25Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt5.bin"));
                _L2Chm25Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt6.bin"));
                _L2Chm25Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt7.bin"));
                _L2Chm25Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt8.bin"));
                _L2Chm25Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt9.bin"));
                _L2Chm25Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt10.bin"));
                _L2Chm25Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt11.bin"));
                _L2Chm25Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt12.bin"));
                _L2Chm25Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt13.bin"));
                _L2Chm25Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt14.bin"));
                _L2Chm25Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt15.bin"));
                _L2Chm25Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt16.bin"));
                _L2Chm25Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt17.bin"));
                _L2Chm25Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt18.bin"));
                _L2Chm25Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt19.bin"));
                _L2Chm25Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt20.bin"));

                _L3Chm1Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm1Default.bin"));

                _L3Chm3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Default.bin"));
                _L3Chm3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Alt1.bin"));
                _L3Chm3Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Alt2.bin"));

                _L3Chm5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Default.bin"));
                _L3Chm5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt1.bin"));
                _L3Chm5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt2.bin"));
                _L3Chm5Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt3.bin"));
                _L3Chm5Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt4.bin"));

                _L3Chm7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Default.bin"));
                _L3Chm7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt1.bin"));
                _L3Chm7Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt2.bin"));
                _L3Chm7Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt3.bin"));
                _L3Chm7Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt4.bin"));
                _L3Chm7Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt5.bin"));
                _L3Chm7Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt6.bin"));

                _L3Chm9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Default.bin"));
                _L3Chm9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt1.bin"));
                _L3Chm9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt2.bin"));
                _L3Chm9Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt3.bin"));
                _L3Chm9Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt4.bin"));
                _L3Chm9Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt5.bin"));
                _L3Chm9Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt6.bin"));
                _L3Chm9Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt7.bin"));

                _L3Chm11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Default.bin"));
                _L3Chm11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt1.bin"));
                _L3Chm11Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt2.bin"));
                _L3Chm11Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt3.bin"));
                _L3Chm11Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt4.bin"));
                _L3Chm11Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt5.bin"));
                _L3Chm11Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt6.bin"));
                _L3Chm11Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt7.bin"));
                _L3Chm11Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt8.bin"));
                _L3Chm11Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt9.bin"));
                _L3Chm11Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt10.bin"));

                _L3Chm13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Default.bin"));
                _L3Chm13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt1.bin"));
                _L3Chm13Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt2.bin"));
                _L3Chm13Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt3.bin"));
                _L3Chm13Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt4.bin"));
                _L3Chm13Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt5.bin"));
                _L3Chm13Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt6.bin"));
                _L3Chm13Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt7.bin"));
                _L3Chm13Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt8.bin"));
                _L3Chm13Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt9.bin"));
                _L3Chm13Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt10.bin"));
                _L3Chm13Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt11.bin"));
                _L3Chm13Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt12.bin"));
                _L3Chm13Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt13.bin"));
                _L3Chm13Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt14.bin"));

                _L3Chm15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Default.bin"));
                _L3Chm15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt1.bin"));
                _L3Chm15Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt2.bin"));
                _L3Chm15Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt3.bin"));
                _L3Chm15Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt4.bin"));
                _L3Chm15Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt5.bin"));
                _L3Chm15Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt6.bin"));
                _L3Chm15Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt7.bin"));
                _L3Chm15Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt8.bin"));
                _L3Chm15Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt9.bin"));
                _L3Chm15Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt10.bin"));
                _L3Chm15Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt11.bin"));
                _L3Chm15Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt12.bin"));
                _L3Chm15Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt13.bin"));
                _L3Chm15Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt14.bin"));
                _L3Chm15Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt15.bin"));
                _L3Chm15Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt16.bin"));
                _L3Chm15Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt17.bin"));

                _L3Chm17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Default.bin"));
                _L3Chm17Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt1.bin"));
                _L3Chm17Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt2.bin"));
                _L3Chm17Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt3.bin"));
                _L3Chm17Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt4.bin"));
                _L3Chm17Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt5.bin"));
                _L3Chm17Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt6.bin"));
                _L3Chm17Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt7.bin"));
                _L3Chm17Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt8.bin"));
                _L3Chm17Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt9.bin"));
                _L3Chm17Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt10.bin"));
                _L3Chm17Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt11.bin"));
                _L3Chm17Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt12.bin"));
                _L3Chm17Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt13.bin"));
                _L3Chm17Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt14.bin"));
                _L3Chm17Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt15.bin"));
                _L3Chm17Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt16.bin"));
                _L3Chm17Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt17.bin"));
                _L3Chm17Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt18.bin"));
                _L3Chm17Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt19.bin"));

                _L3Chm19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Default.bin"));
                _L3Chm19Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt1.bin"));
                _L3Chm19Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt2.bin"));
                _L3Chm19Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt3.bin"));
                _L3Chm19Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt4.bin"));
                _L3Chm19Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt5.bin"));
                _L3Chm19Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt6.bin"));
                _L3Chm19Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt7.bin"));
                _L3Chm19Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt8.bin"));
                _L3Chm19Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt9.bin"));
                _L3Chm19Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt10.bin"));
                _L3Chm19Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt11.bin"));
                _L3Chm19Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt12.bin"));
                _L3Chm19Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt13.bin"));
                _L3Chm19Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt14.bin"));
                _L3Chm19Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt15.bin"));
                _L3Chm19Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt16.bin"));
                _L3Chm19Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt17.bin"));
                _L3Chm19Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt18.bin"));
                _L3Chm19Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt19.bin"));
                _L3Chm19Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt20.bin"));
                _L3Chm19Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt21.bin"));
                _L3Chm19Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt22.bin"));
                _L3Chm19Alt23 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt23.bin"));
                _L3Chm19Alt24 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt24.bin"));
                _L3Chm19Alt25 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt25.bin"));

                _L3Chm21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Default.bin"));
                _L3Chm21Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt1.bin"));
                _L3Chm21Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt2.bin"));
                _L3Chm21Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt3.bin"));
                _L3Chm21Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt4.bin"));
                _L3Chm21Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt5.bin"));
                _L3Chm21Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt6.bin"));
                _L3Chm21Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt7.bin"));
                _L3Chm21Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt8.bin"));
                _L3Chm21Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt9.bin"));
                _L3Chm21Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt10.bin"));
                _L3Chm21Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt11.bin"));
                _L3Chm21Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt12.bin"));
                _L3Chm21Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt13.bin"));
                _L3Chm21Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt14.bin"));
                _L3Chm21Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt15.bin"));
                _L3Chm21Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt16.bin"));
                _L3Chm21Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt17.bin"));
                _L3Chm21Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt18.bin"));
                _L3Chm21Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt19.bin"));
                _L3Chm21Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt20.bin"));
                _L3Chm21Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt21.bin"));
                _L3Chm21Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt22.bin"));

                _L2She3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She3Default.bin"));
                _L2She3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She3Alt1.bin"));
                _L2She9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Default.bin"));
                _L2She9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Alt1.bin"));
                _L2She9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Alt2.bin"));
                _L2She7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She7Default.bin"));
                _L2She7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She7Alt1.bin"));
                _L2She5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Default.bin"));
                _L2She5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Alt1.bin"));
                _L2She5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Alt2.bin"));
                _L2She11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She11Default.bin"));
                _L2She11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She11Alt1.bin"));
                _L2She13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She13Default.bin"));
                _L2She13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She13Alt1.bin"));
                _L2She15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She15Default.bin"));
                _L2She15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She15Alt1.bin"));
                _L2She17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Default.bin"));
                _L2She17Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Alt1.bin"));

                _L3She1Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She1Default.bin"));
                _L3She3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She3Default.bin"));
                _L3She3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She3Alt1.bin"));
                _L3She5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She5Default.bin"));
                _L3She7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She7Default.bin"));
                _L3She9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She9Default.bin"));
                _L3She11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She11Default.bin"));
                _L3She13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She13Default.bin"));
                _L3She15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She15Default.bin"));
                _L3She17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She17Default.bin"));
                _L3She19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She19Default.bin"));
                _L3She21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She21Default.bin"));
                Loaded = true;
            });
            t.Wait();
            Loading = false;
        }
        public static bool Loaded = false;
        private static bool Loading = false;

        private static CustomPwm? _L2Chm3Default;
        private static CustomPwm? _L2Chm3Alt1;

        private static CustomPwm? _L2Chm5Default;
        private static CustomPwm? _L2Chm5Alt1;
        private static CustomPwm? _L2Chm5Alt2;
        private static CustomPwm? _L2Chm5Alt3;

        private static CustomPwm? _L2Chm7Default;
        private static CustomPwm? _L2Chm7Alt1;
        private static CustomPwm? _L2Chm7Alt2;
        private static CustomPwm? _L2Chm7Alt3;
        private static CustomPwm? _L2Chm7Alt4;
        private static CustomPwm? _L2Chm7Alt5;

        private static CustomPwm? _L2Chm9Default;
        private static CustomPwm? _L2Chm9Alt1;
        private static CustomPwm? _L2Chm9Alt2;
        private static CustomPwm? _L2Chm9Alt3;
        private static CustomPwm? _L2Chm9Alt4;
        private static CustomPwm? _L2Chm9Alt5;
        private static CustomPwm? _L2Chm9Alt6;
        private static CustomPwm? _L2Chm9Alt7;
        private static CustomPwm? _L2Chm9Alt8;

        private static CustomPwm? _L2Chm11Default;
        private static CustomPwm? _L2Chm11Alt1;
        private static CustomPwm? _L2Chm11Alt2;
        private static CustomPwm? _L2Chm11Alt3;
        private static CustomPwm? _L2Chm11Alt4;
        private static CustomPwm? _L2Chm11Alt5;
        private static CustomPwm? _L2Chm11Alt6;
        private static CustomPwm? _L2Chm11Alt7;
        private static CustomPwm? _L2Chm11Alt8;
        private static CustomPwm? _L2Chm11Alt9;
        private static CustomPwm? _L2Chm11Alt10;
        private static CustomPwm? _L2Chm11Alt11;
        private static CustomPwm? _L2Chm11Alt12;

        private static CustomPwm? _L2Chm13Default;
        private static CustomPwm? _L2Chm13Alt1;
        private static CustomPwm? _L2Chm13Alt2;
        private static CustomPwm? _L2Chm13Alt3;
        private static CustomPwm? _L2Chm13Alt4;
        private static CustomPwm? _L2Chm13Alt5;
        private static CustomPwm? _L2Chm13Alt6;
        private static CustomPwm? _L2Chm13Alt7;
        private static CustomPwm? _L2Chm13Alt8;
        private static CustomPwm? _L2Chm13Alt9;
        private static CustomPwm? _L2Chm13Alt10;
        private static CustomPwm? _L2Chm13Alt11;
        private static CustomPwm? _L2Chm13Alt12;
        private static CustomPwm? _L2Chm13Alt13;

        private static CustomPwm? _L2Chm15Default;
        private static CustomPwm? _L2Chm15Alt1;
        private static CustomPwm? _L2Chm15Alt2;
        private static CustomPwm? _L2Chm15Alt3;
        private static CustomPwm? _L2Chm15Alt4;
        private static CustomPwm? _L2Chm15Alt5;
        private static CustomPwm? _L2Chm15Alt6;
        private static CustomPwm? _L2Chm15Alt7;
        private static CustomPwm? _L2Chm15Alt8;
        private static CustomPwm? _L2Chm15Alt9;
        private static CustomPwm? _L2Chm15Alt10;
        private static CustomPwm? _L2Chm15Alt11;
        private static CustomPwm? _L2Chm15Alt12;
        private static CustomPwm? _L2Chm15Alt13;
        private static CustomPwm? _L2Chm15Alt14;
        private static CustomPwm? _L2Chm15Alt15;
        private static CustomPwm? _L2Chm15Alt16;
        private static CustomPwm? _L2Chm15Alt17;
        private static CustomPwm? _L2Chm15Alt18;
        private static CustomPwm? _L2Chm15Alt19;
        private static CustomPwm? _L2Chm15Alt20;
        private static CustomPwm? _L2Chm15Alt21;
        private static CustomPwm? _L2Chm15Alt22;
        private static CustomPwm? _L2Chm15Alt23;

        private static CustomPwm? _L2Chm17Default;
        private static CustomPwm? _L2Chm17Alt1;
        private static CustomPwm? _L2Chm17Alt2;
        private static CustomPwm? _L2Chm17Alt3;
        private static CustomPwm? _L2Chm17Alt4;
        private static CustomPwm? _L2Chm17Alt5;
        private static CustomPwm? _L2Chm17Alt6;
        private static CustomPwm? _L2Chm17Alt7;
        private static CustomPwm? _L2Chm17Alt8;
        private static CustomPwm? _L2Chm17Alt9;
        private static CustomPwm? _L2Chm17Alt10;
        private static CustomPwm? _L2Chm17Alt11;

        private static CustomPwm? _L2Chm19Default;
        private static CustomPwm? _L2Chm19Alt1;
        private static CustomPwm? _L2Chm19Alt2;
        private static CustomPwm? _L2Chm19Alt3;
        private static CustomPwm? _L2Chm19Alt4;
        private static CustomPwm? _L2Chm19Alt5;
        private static CustomPwm? _L2Chm19Alt6;
        private static CustomPwm? _L2Chm19Alt7;
        private static CustomPwm? _L2Chm19Alt8;
        private static CustomPwm? _L2Chm19Alt9;
        private static CustomPwm? _L2Chm19Alt10;
        private static CustomPwm? _L2Chm19Alt11;

        private static CustomPwm? _L2Chm21Default;
        private static CustomPwm? _L2Chm21Alt1;
        private static CustomPwm? _L2Chm21Alt2;
        private static CustomPwm? _L2Chm21Alt3;
        private static CustomPwm? _L2Chm21Alt4;
        private static CustomPwm? _L2Chm21Alt5;
        private static CustomPwm? _L2Chm21Alt6;
        private static CustomPwm? _L2Chm21Alt7;
        private static CustomPwm? _L2Chm21Alt8;
        private static CustomPwm? _L2Chm21Alt9;
        private static CustomPwm? _L2Chm21Alt10;
        private static CustomPwm? _L2Chm21Alt11;
        private static CustomPwm? _L2Chm21Alt12;
        private static CustomPwm? _L2Chm21Alt13;

        private static CustomPwm? _L2Chm23Default;
        private static CustomPwm? _L2Chm23Alt1;
        private static CustomPwm? _L2Chm23Alt2;
        private static CustomPwm? _L2Chm23Alt3;
        private static CustomPwm? _L2Chm23Alt4;
        private static CustomPwm? _L2Chm23Alt5;
        private static CustomPwm? _L2Chm23Alt6;
        private static CustomPwm? _L2Chm23Alt7;
        private static CustomPwm? _L2Chm23Alt8;
        private static CustomPwm? _L2Chm23Alt9;
        private static CustomPwm? _L2Chm23Alt10;
        private static CustomPwm? _L2Chm23Alt11;
        private static CustomPwm? _L2Chm23Alt12;
        private static CustomPwm? _L2Chm23Alt13;
        private static CustomPwm? _L2Chm23Alt14;

        private static CustomPwm? _L2Chm25Default;
        private static CustomPwm? _L2Chm25Alt1;
        private static CustomPwm? _L2Chm25Alt2;
        private static CustomPwm? _L2Chm25Alt3;
        private static CustomPwm? _L2Chm25Alt4;
        private static CustomPwm? _L2Chm25Alt5;
        private static CustomPwm? _L2Chm25Alt6;
        private static CustomPwm? _L2Chm25Alt7;
        private static CustomPwm? _L2Chm25Alt8;
        private static CustomPwm? _L2Chm25Alt9;
        private static CustomPwm? _L2Chm25Alt10;
        private static CustomPwm? _L2Chm25Alt11;
        private static CustomPwm? _L2Chm25Alt12;
        private static CustomPwm? _L2Chm25Alt13;
        private static CustomPwm? _L2Chm25Alt14;
        private static CustomPwm? _L2Chm25Alt15;
        private static CustomPwm? _L2Chm25Alt16;
        private static CustomPwm? _L2Chm25Alt17;
        private static CustomPwm? _L2Chm25Alt18;
        private static CustomPwm? _L2Chm25Alt19;
        private static CustomPwm? _L2Chm25Alt20;

        private static CustomPwm? _L3Chm1Default;

        private static CustomPwm? _L3Chm3Default;
        private static CustomPwm? _L3Chm3Alt1;
        private static CustomPwm? _L3Chm3Alt2;

        private static CustomPwm? _L3Chm5Default;
        private static CustomPwm? _L3Chm5Alt1;
        private static CustomPwm? _L3Chm5Alt2;
        private static CustomPwm? _L3Chm5Alt3;
        private static CustomPwm? _L3Chm5Alt4;

        private static CustomPwm? _L3Chm7Default;
        private static CustomPwm? _L3Chm7Alt1;
        private static CustomPwm? _L3Chm7Alt2;
        private static CustomPwm? _L3Chm7Alt3;
        private static CustomPwm? _L3Chm7Alt4;
        private static CustomPwm? _L3Chm7Alt5;
        private static CustomPwm? _L3Chm7Alt6;

        private static CustomPwm? _L3Chm9Default;
        private static CustomPwm? _L3Chm9Alt1;
        private static CustomPwm? _L3Chm9Alt2;
        private static CustomPwm? _L3Chm9Alt3;
        private static CustomPwm? _L3Chm9Alt4;
        private static CustomPwm? _L3Chm9Alt5;
        private static CustomPwm? _L3Chm9Alt6;
        private static CustomPwm? _L3Chm9Alt7;

        private static CustomPwm? _L3Chm11Default;
        private static CustomPwm? _L3Chm11Alt1;
        private static CustomPwm? _L3Chm11Alt2;
        private static CustomPwm? _L3Chm11Alt3;
        private static CustomPwm? _L3Chm11Alt4;
        private static CustomPwm? _L3Chm11Alt5;
        private static CustomPwm? _L3Chm11Alt6;
        private static CustomPwm? _L3Chm11Alt7;
        private static CustomPwm? _L3Chm11Alt8;
        private static CustomPwm? _L3Chm11Alt9;
        private static CustomPwm? _L3Chm11Alt10;

        private static CustomPwm? _L3Chm13Default;
        private static CustomPwm? _L3Chm13Alt1;
        private static CustomPwm? _L3Chm13Alt2;
        private static CustomPwm? _L3Chm13Alt3;
        private static CustomPwm? _L3Chm13Alt4;
        private static CustomPwm? _L3Chm13Alt5;
        private static CustomPwm? _L3Chm13Alt6;
        private static CustomPwm? _L3Chm13Alt7;
        private static CustomPwm? _L3Chm13Alt8;
        private static CustomPwm? _L3Chm13Alt9;
        private static CustomPwm? _L3Chm13Alt10;
        private static CustomPwm? _L3Chm13Alt11;
        private static CustomPwm? _L3Chm13Alt12;
        private static CustomPwm? _L3Chm13Alt13;
        private static CustomPwm? _L3Chm13Alt14;

        private static CustomPwm? _L3Chm15Default;
        private static CustomPwm? _L3Chm15Alt1;
        private static CustomPwm? _L3Chm15Alt2;
        private static CustomPwm? _L3Chm15Alt3;
        private static CustomPwm? _L3Chm15Alt4;
        private static CustomPwm? _L3Chm15Alt5;
        private static CustomPwm? _L3Chm15Alt6;
        private static CustomPwm? _L3Chm15Alt7;
        private static CustomPwm? _L3Chm15Alt8;
        private static CustomPwm? _L3Chm15Alt9;
        private static CustomPwm? _L3Chm15Alt10;
        private static CustomPwm? _L3Chm15Alt11;
        private static CustomPwm? _L3Chm15Alt12;
        private static CustomPwm? _L3Chm15Alt13;
        private static CustomPwm? _L3Chm15Alt14;
        private static CustomPwm? _L3Chm15Alt15;
        private static CustomPwm? _L3Chm15Alt16;
        private static CustomPwm? _L3Chm15Alt17;

        private static CustomPwm? _L3Chm17Default;
        private static CustomPwm? _L3Chm17Alt1;
        private static CustomPwm? _L3Chm17Alt2;
        private static CustomPwm? _L3Chm17Alt3;
        private static CustomPwm? _L3Chm17Alt4;
        private static CustomPwm? _L3Chm17Alt5;
        private static CustomPwm? _L3Chm17Alt6;
        private static CustomPwm? _L3Chm17Alt7;
        private static CustomPwm? _L3Chm17Alt8;
        private static CustomPwm? _L3Chm17Alt9;
        private static CustomPwm? _L3Chm17Alt10;
        private static CustomPwm? _L3Chm17Alt11;
        private static CustomPwm? _L3Chm17Alt12;
        private static CustomPwm? _L3Chm17Alt13;
        private static CustomPwm? _L3Chm17Alt14;
        private static CustomPwm? _L3Chm17Alt15;
        private static CustomPwm? _L3Chm17Alt16;
        private static CustomPwm? _L3Chm17Alt17;
        private static CustomPwm? _L3Chm17Alt18;
        private static CustomPwm? _L3Chm17Alt19;

        private static CustomPwm? _L3Chm19Default;
        private static CustomPwm? _L3Chm19Alt1;
        private static CustomPwm? _L3Chm19Alt2;
        private static CustomPwm? _L3Chm19Alt3;
        private static CustomPwm? _L3Chm19Alt4;
        private static CustomPwm? _L3Chm19Alt5;
        private static CustomPwm? _L3Chm19Alt6;
        private static CustomPwm? _L3Chm19Alt7;
        private static CustomPwm? _L3Chm19Alt8;
        private static CustomPwm? _L3Chm19Alt9;
        private static CustomPwm? _L3Chm19Alt10;
        private static CustomPwm? _L3Chm19Alt11;
        private static CustomPwm? _L3Chm19Alt12;
        private static CustomPwm? _L3Chm19Alt13;
        private static CustomPwm? _L3Chm19Alt14;
        private static CustomPwm? _L3Chm19Alt15;
        private static CustomPwm? _L3Chm19Alt16;
        private static CustomPwm? _L3Chm19Alt17;
        private static CustomPwm? _L3Chm19Alt18;
        private static CustomPwm? _L3Chm19Alt19;
        private static CustomPwm? _L3Chm19Alt20;
        private static CustomPwm? _L3Chm19Alt21;
        private static CustomPwm? _L3Chm19Alt22;
        private static CustomPwm? _L3Chm19Alt23;
        private static CustomPwm? _L3Chm19Alt24;
        private static CustomPwm? _L3Chm19Alt25;

        private static CustomPwm? _L3Chm21Default;
        private static CustomPwm? _L3Chm21Alt1;
        private static CustomPwm? _L3Chm21Alt2;
        private static CustomPwm? _L3Chm21Alt3;
        private static CustomPwm? _L3Chm21Alt4;
        private static CustomPwm? _L3Chm21Alt5;
        private static CustomPwm? _L3Chm21Alt6;
        private static CustomPwm? _L3Chm21Alt7;
        private static CustomPwm? _L3Chm21Alt8;
        private static CustomPwm? _L3Chm21Alt9;
        private static CustomPwm? _L3Chm21Alt10;
        private static CustomPwm? _L3Chm21Alt11;
        private static CustomPwm? _L3Chm21Alt12;
        private static CustomPwm? _L3Chm21Alt13;
        private static CustomPwm? _L3Chm21Alt14;
        private static CustomPwm? _L3Chm21Alt15;
        private static CustomPwm? _L3Chm21Alt16;
        private static CustomPwm? _L3Chm21Alt17;
        private static CustomPwm? _L3Chm21Alt18;
        private static CustomPwm? _L3Chm21Alt19;
        private static CustomPwm? _L3Chm21Alt20;
        private static CustomPwm? _L3Chm21Alt21;
        private static CustomPwm? _L3Chm21Alt22;

        private static CustomPwm? _L2She3Default;
        private static CustomPwm? _L2She3Alt1;
        private static CustomPwm? _L2She9Default;
        private static CustomPwm? _L2She9Alt1;
        private static CustomPwm? _L2She9Alt2;
        private static CustomPwm? _L2She7Default;
        private static CustomPwm? _L2She7Alt1;
        private static CustomPwm? _L2She5Default;
        private static CustomPwm? _L2She5Alt1;
        private static CustomPwm? _L2She5Alt2;
        private static CustomPwm? _L2She11Default;
        private static CustomPwm? _L2She11Alt1;
        private static CustomPwm? _L2She13Default;
        private static CustomPwm? _L2She13Alt1;
        private static CustomPwm? _L2She15Default;
        private static CustomPwm? _L2She15Alt1;
        private static CustomPwm? _L2She17Default;
        private static CustomPwm? _L2She17Alt1;

        private static CustomPwm? _L3She1Default;
        private static CustomPwm? _L3She3Default;
        private static CustomPwm? _L3She3Alt1;
        private static CustomPwm? _L3She5Default;
        private static CustomPwm? _L3She7Default;
        private static CustomPwm? _L3She9Default;
        private static CustomPwm? _L3She11Default;
        private static CustomPwm? _L3She13Default;
        private static CustomPwm? _L3She15Default;
        private static CustomPwm? _L3She17Default;
        private static CustomPwm? _L3She19Default;
        private static CustomPwm? _L3She21Default;

        public static CustomPwm L2Chm3Default { get => _L2Chm3Default ?? new(null); }
        public static CustomPwm L2Chm3Alt1 { get => _L2Chm3Alt1 ?? new(null); }

        public static CustomPwm L2Chm5Default { get => _L2Chm5Default ?? new(null); }
        public static CustomPwm L2Chm5Alt1 { get => _L2Chm5Alt1 ?? new(null); }
        public static CustomPwm L2Chm5Alt2 { get => _L2Chm5Alt2 ?? new(null); }
        public static CustomPwm L2Chm5Alt3 { get => _L2Chm5Alt3 ?? new(null); }

        public static CustomPwm L2Chm7Default { get => _L2Chm7Default ?? new(null); }
        public static CustomPwm L2Chm7Alt1 { get => _L2Chm7Alt1 ?? new(null); }
        public static CustomPwm L2Chm7Alt2 { get => _L2Chm7Alt2 ?? new(null); }
        public static CustomPwm L2Chm7Alt3 { get => _L2Chm7Alt3 ?? new(null); }
        public static CustomPwm L2Chm7Alt4 { get => _L2Chm7Alt4 ?? new(null); }
        public static CustomPwm L2Chm7Alt5 { get => _L2Chm7Alt5 ?? new(null); }

        public static CustomPwm L2Chm9Default { get => _L2Chm9Default ?? new(null); }
        public static CustomPwm L2Chm9Alt1 { get => _L2Chm9Alt1 ?? new(null); }
        public static CustomPwm L2Chm9Alt2 { get => _L2Chm9Alt2 ?? new(null); }
        public static CustomPwm L2Chm9Alt3 { get => _L2Chm9Alt3 ?? new(null); }
        public static CustomPwm L2Chm9Alt4 { get => _L2Chm9Alt4 ?? new(null); }
        public static CustomPwm L2Chm9Alt5 { get => _L2Chm9Alt5 ?? new(null); }
        public static CustomPwm L2Chm9Alt6 { get => _L2Chm9Alt6 ?? new(null); }
        public static CustomPwm L2Chm9Alt7 { get => _L2Chm9Alt7 ?? new(null); }
        public static CustomPwm L2Chm9Alt8 { get => _L2Chm9Alt8 ?? new(null); }

        public static CustomPwm L2Chm11Default { get => _L2Chm11Default ?? new(null); }
        public static CustomPwm L2Chm11Alt1 { get => _L2Chm11Alt1 ?? new(null); }
        public static CustomPwm L2Chm11Alt2 { get => _L2Chm11Alt2 ?? new(null); }
        public static CustomPwm L2Chm11Alt3 { get => _L2Chm11Alt3 ?? new(null); }
        public static CustomPwm L2Chm11Alt4 { get => _L2Chm11Alt4 ?? new(null); }
        public static CustomPwm L2Chm11Alt5 { get => _L2Chm11Alt5 ?? new(null); }
        public static CustomPwm L2Chm11Alt6 { get => _L2Chm11Alt6 ?? new(null); }
        public static CustomPwm L2Chm11Alt7 { get => _L2Chm11Alt7 ?? new(null); }
        public static CustomPwm L2Chm11Alt8 { get => _L2Chm11Alt8 ?? new(null); }
        public static CustomPwm L2Chm11Alt9 { get => _L2Chm11Alt9 ?? new(null); }
        public static CustomPwm L2Chm11Alt10 { get => _L2Chm11Alt10 ?? new(null); }
        public static CustomPwm L2Chm11Alt11 { get => _L2Chm11Alt11 ?? new(null); }
        public static CustomPwm L2Chm11Alt12 { get => _L2Chm11Alt12 ?? new(null); }

        public static CustomPwm L2Chm13Default { get => _L2Chm13Default ?? new(null); }
        public static CustomPwm L2Chm13Alt1 { get => _L2Chm13Alt1 ?? new(null); }
        public static CustomPwm L2Chm13Alt2 { get => _L2Chm13Alt2 ?? new(null); }
        public static CustomPwm L2Chm13Alt3 { get => _L2Chm13Alt3 ?? new(null); }
        public static CustomPwm L2Chm13Alt4 { get => _L2Chm13Alt4 ?? new(null); }
        public static CustomPwm L2Chm13Alt5 { get => _L2Chm13Alt5 ?? new(null); }
        public static CustomPwm L2Chm13Alt6 { get => _L2Chm13Alt6 ?? new(null); }
        public static CustomPwm L2Chm13Alt7 { get => _L2Chm13Alt7 ?? new(null); }
        public static CustomPwm L2Chm13Alt8 { get => _L2Chm13Alt8 ?? new(null); }
        public static CustomPwm L2Chm13Alt9 { get => _L2Chm13Alt9 ?? new(null); }
        public static CustomPwm L2Chm13Alt10 { get => _L2Chm13Alt10 ?? new(null); }
        public static CustomPwm L2Chm13Alt11 { get => _L2Chm13Alt11 ?? new(null); }
        public static CustomPwm L2Chm13Alt12 { get => _L2Chm13Alt12 ?? new(null); }
        public static CustomPwm L2Chm13Alt13 { get => _L2Chm13Alt13 ?? new(null); }

        public static CustomPwm L2Chm15Default { get => _L2Chm15Default ?? new(null); }
        public static CustomPwm L2Chm15Alt1 { get => _L2Chm15Alt1 ?? new(null); }
        public static CustomPwm L2Chm15Alt2 { get => _L2Chm15Alt2 ?? new(null); }
        public static CustomPwm L2Chm15Alt3 { get => _L2Chm15Alt3 ?? new(null); }
        public static CustomPwm L2Chm15Alt4 { get => _L2Chm15Alt4 ?? new(null); }
        public static CustomPwm L2Chm15Alt5 { get => _L2Chm15Alt5 ?? new(null); }
        public static CustomPwm L2Chm15Alt6 { get => _L2Chm15Alt6 ?? new(null); }
        public static CustomPwm L2Chm15Alt7 { get => _L2Chm15Alt7 ?? new(null); }
        public static CustomPwm L2Chm15Alt8 { get => _L2Chm15Alt8 ?? new(null); }
        public static CustomPwm L2Chm15Alt9 { get => _L2Chm15Alt9 ?? new(null); }
        public static CustomPwm L2Chm15Alt10 { get => _L2Chm15Alt10 ?? new(null); }
        public static CustomPwm L2Chm15Alt11 { get => _L2Chm15Alt11 ?? new(null); }
        public static CustomPwm L2Chm15Alt12 { get => _L2Chm15Alt12 ?? new(null); }
        public static CustomPwm L2Chm15Alt13 { get => _L2Chm15Alt13 ?? new(null); }
        public static CustomPwm L2Chm15Alt14 { get => _L2Chm15Alt14 ?? new(null); }
        public static CustomPwm L2Chm15Alt15 { get => _L2Chm15Alt15 ?? new(null); }
        public static CustomPwm L2Chm15Alt16 { get => _L2Chm15Alt16 ?? new(null); }
        public static CustomPwm L2Chm15Alt17 { get => _L2Chm15Alt17 ?? new(null); }
        public static CustomPwm L2Chm15Alt18 { get => _L2Chm15Alt18 ?? new(null); }
        public static CustomPwm L2Chm15Alt19 { get => _L2Chm15Alt19 ?? new(null); }
        public static CustomPwm L2Chm15Alt20 { get => _L2Chm15Alt20 ?? new(null); }
        public static CustomPwm L2Chm15Alt21 { get => _L2Chm15Alt21 ?? new(null); }
        public static CustomPwm L2Chm15Alt22 { get => _L2Chm15Alt22 ?? new(null); }
        public static CustomPwm L2Chm15Alt23 { get => _L2Chm15Alt23 ?? new(null); }

        public static CustomPwm L2Chm17Default { get => _L2Chm17Default ?? new(null); }
        public static CustomPwm L2Chm17Alt1 { get => _L2Chm17Alt1 ?? new(null); }
        public static CustomPwm L2Chm17Alt2 { get => _L2Chm17Alt2 ?? new(null); }
        public static CustomPwm L2Chm17Alt3 { get => _L2Chm17Alt3 ?? new(null); }
        public static CustomPwm L2Chm17Alt4 { get => _L2Chm17Alt4 ?? new(null); }
        public static CustomPwm L2Chm17Alt5 { get => _L2Chm17Alt5 ?? new(null); }
        public static CustomPwm L2Chm17Alt6 { get => _L2Chm17Alt6 ?? new(null); }
        public static CustomPwm L2Chm17Alt7 { get => _L2Chm17Alt7 ?? new(null); }
        public static CustomPwm L2Chm17Alt8 { get => _L2Chm17Alt8 ?? new(null); }
        public static CustomPwm L2Chm17Alt9 { get => _L2Chm17Alt9 ?? new(null); }
        public static CustomPwm L2Chm17Alt10 { get => _L2Chm17Alt10 ?? new(null); }
        public static CustomPwm L2Chm17Alt11 { get => _L2Chm17Alt11 ?? new(null); }

        public static CustomPwm L2Chm19Default { get => _L2Chm19Default ?? new(null); }
        public static CustomPwm L2Chm19Alt1 { get => _L2Chm19Alt1 ?? new(null); }
        public static CustomPwm L2Chm19Alt2 { get => _L2Chm19Alt2 ?? new(null); }
        public static CustomPwm L2Chm19Alt3 { get => _L2Chm19Alt3 ?? new(null); }
        public static CustomPwm L2Chm19Alt4 { get => _L2Chm19Alt4 ?? new(null); }
        public static CustomPwm L2Chm19Alt5 { get => _L2Chm19Alt5 ?? new(null); }
        public static CustomPwm L2Chm19Alt6 { get => _L2Chm19Alt6 ?? new(null); }
        public static CustomPwm L2Chm19Alt7 { get => _L2Chm19Alt7 ?? new(null); }
        public static CustomPwm L2Chm19Alt8 { get => _L2Chm19Alt8 ?? new(null); }
        public static CustomPwm L2Chm19Alt9 { get => _L2Chm19Alt9 ?? new(null); }
        public static CustomPwm L2Chm19Alt10 { get => _L2Chm19Alt10 ?? new(null); }
        public static CustomPwm L2Chm19Alt11 { get => _L2Chm19Alt11 ?? new(null); }

        public static CustomPwm L2Chm21Default { get => _L2Chm21Default ?? new(null); }
        public static CustomPwm L2Chm21Alt1 { get => _L2Chm21Alt1 ?? new(null); }
        public static CustomPwm L2Chm21Alt2 { get => _L2Chm21Alt2 ?? new(null); }
        public static CustomPwm L2Chm21Alt3 { get => _L2Chm21Alt3 ?? new(null); }
        public static CustomPwm L2Chm21Alt4 { get => _L2Chm21Alt4 ?? new(null); }
        public static CustomPwm L2Chm21Alt5 { get => _L2Chm21Alt5 ?? new(null); }
        public static CustomPwm L2Chm21Alt6 { get => _L2Chm21Alt6 ?? new(null); }
        public static CustomPwm L2Chm21Alt7 { get => _L2Chm21Alt7 ?? new(null); }
        public static CustomPwm L2Chm21Alt8 { get => _L2Chm21Alt8 ?? new(null); }
        public static CustomPwm L2Chm21Alt9 { get => _L2Chm21Alt9 ?? new(null); }
        public static CustomPwm L2Chm21Alt10 { get => _L2Chm21Alt10 ?? new(null); }
        public static CustomPwm L2Chm21Alt11 { get => _L2Chm21Alt11 ?? new(null); }
        public static CustomPwm L2Chm21Alt12 { get => _L2Chm21Alt12 ?? new(null); }
        public static CustomPwm L2Chm21Alt13 { get => _L2Chm21Alt13 ?? new(null); }

        public static CustomPwm L2Chm23Default { get => _L2Chm23Default ?? new(null); }
        public static CustomPwm L2Chm23Alt1 { get => _L2Chm23Alt1 ?? new(null); }
        public static CustomPwm L2Chm23Alt2 { get => _L2Chm23Alt2 ?? new(null); }
        public static CustomPwm L2Chm23Alt3 { get => _L2Chm23Alt3 ?? new(null); }
        public static CustomPwm L2Chm23Alt4 { get => _L2Chm23Alt4 ?? new(null); }
        public static CustomPwm L2Chm23Alt5 { get => _L2Chm23Alt5 ?? new(null); }
        public static CustomPwm L2Chm23Alt6 { get => _L2Chm23Alt6 ?? new(null); }
        public static CustomPwm L2Chm23Alt7 { get => _L2Chm23Alt7 ?? new(null); }
        public static CustomPwm L2Chm23Alt8 { get => _L2Chm23Alt8 ?? new(null); }
        public static CustomPwm L2Chm23Alt9 { get => _L2Chm23Alt9 ?? new(null); }
        public static CustomPwm L2Chm23Alt10 { get => _L2Chm23Alt10 ?? new(null); }
        public static CustomPwm L2Chm23Alt11 { get => _L2Chm23Alt11 ?? new(null); }
        public static CustomPwm L2Chm23Alt12 { get => _L2Chm23Alt12 ?? new(null); }
        public static CustomPwm L2Chm23Alt13 { get => _L2Chm23Alt13 ?? new(null); }
        public static CustomPwm L2Chm23Alt14 { get => _L2Chm23Alt14 ?? new(null); }

        public static CustomPwm L2Chm25Default { get => _L2Chm25Default ?? new(null); }
        public static CustomPwm L2Chm25Alt1 { get => _L2Chm25Alt1 ?? new(null); }
        public static CustomPwm L2Chm25Alt2 { get => _L2Chm25Alt2 ?? new(null); }
        public static CustomPwm L2Chm25Alt3 { get => _L2Chm25Alt3 ?? new(null); }
        public static CustomPwm L2Chm25Alt4 { get => _L2Chm25Alt4 ?? new(null); }
        public static CustomPwm L2Chm25Alt5 { get => _L2Chm25Alt5 ?? new(null); }
        public static CustomPwm L2Chm25Alt6 { get => _L2Chm25Alt6 ?? new(null); }
        public static CustomPwm L2Chm25Alt7 { get => _L2Chm25Alt7 ?? new(null); }
        public static CustomPwm L2Chm25Alt8 { get => _L2Chm25Alt8 ?? new(null); }
        public static CustomPwm L2Chm25Alt9 { get => _L2Chm25Alt9 ?? new(null); }
        public static CustomPwm L2Chm25Alt10 { get => _L2Chm25Alt10 ?? new(null); }
        public static CustomPwm L2Chm25Alt11 { get => _L2Chm25Alt11 ?? new(null); }
        public static CustomPwm L2Chm25Alt12 { get => _L2Chm25Alt12 ?? new(null); }
        public static CustomPwm L2Chm25Alt13 { get => _L2Chm25Alt13 ?? new(null); }
        public static CustomPwm L2Chm25Alt14 { get => _L2Chm25Alt14 ?? new(null); }
        public static CustomPwm L2Chm25Alt15 { get => _L2Chm25Alt15 ?? new(null); }
        public static CustomPwm L2Chm25Alt16 { get => _L2Chm25Alt16 ?? new(null); }
        public static CustomPwm L2Chm25Alt17 { get => _L2Chm25Alt17 ?? new(null); }
        public static CustomPwm L2Chm25Alt18 { get => _L2Chm25Alt18 ?? new(null); }
        public static CustomPwm L2Chm25Alt19 { get => _L2Chm25Alt19 ?? new(null); }
        public static CustomPwm L2Chm25Alt20 { get => _L2Chm25Alt20 ?? new(null); }

        public static CustomPwm L3Chm1Default { get => _L3Chm1Default ?? new(null); }

        public static CustomPwm L3Chm3Default { get => _L3Chm3Default ?? new(null); }
        public static CustomPwm L3Chm3Alt1 { get => _L3Chm3Alt1 ?? new(null); }
        public static CustomPwm L3Chm3Alt2 { get => _L3Chm3Alt2 ?? new(null); }

        public static CustomPwm L3Chm5Default { get => _L3Chm5Default ?? new(null); }
        public static CustomPwm L3Chm5Alt1 { get => _L3Chm5Alt1 ?? new(null); }
        public static CustomPwm L3Chm5Alt2 { get => _L3Chm5Alt2 ?? new(null); }
        public static CustomPwm L3Chm5Alt3 { get => _L3Chm5Alt3 ?? new(null); }
        public static CustomPwm L3Chm5Alt4 { get => _L3Chm5Alt4 ?? new(null); }

        public static CustomPwm L3Chm7Default { get => _L3Chm7Default ?? new(null); }
        public static CustomPwm L3Chm7Alt1 { get => _L3Chm7Alt1 ?? new(null); }
        public static CustomPwm L3Chm7Alt2 { get => _L3Chm7Alt2 ?? new(null); }
        public static CustomPwm L3Chm7Alt3 { get => _L3Chm7Alt3 ?? new(null); }
        public static CustomPwm L3Chm7Alt4 { get => _L3Chm7Alt4 ?? new(null); }
        public static CustomPwm L3Chm7Alt5 { get => _L3Chm7Alt5 ?? new(null); }
        public static CustomPwm L3Chm7Alt6 { get => _L3Chm7Alt6 ?? new(null); }

        public static CustomPwm L3Chm9Default { get => _L3Chm9Default ?? new(null); }
        public static CustomPwm L3Chm9Alt1 { get => _L3Chm9Alt1 ?? new(null); }
        public static CustomPwm L3Chm9Alt2 { get => _L3Chm9Alt2 ?? new(null); }
        public static CustomPwm L3Chm9Alt3 { get => _L3Chm9Alt3 ?? new(null); }
        public static CustomPwm L3Chm9Alt4 { get => _L3Chm9Alt4 ?? new(null); }
        public static CustomPwm L3Chm9Alt5 { get => _L3Chm9Alt5 ?? new(null); }
        public static CustomPwm L3Chm9Alt6 { get => _L3Chm9Alt6 ?? new(null); }
        public static CustomPwm L3Chm9Alt7 { get => _L3Chm9Alt7 ?? new(null); }

        public static CustomPwm L3Chm11Default { get => _L3Chm11Default ?? new(null); }
        public static CustomPwm L3Chm11Alt1 { get => _L3Chm11Alt1 ?? new(null); }
        public static CustomPwm L3Chm11Alt2 { get => _L3Chm11Alt2 ?? new(null); }
        public static CustomPwm L3Chm11Alt3 { get => _L3Chm11Alt3 ?? new(null); }
        public static CustomPwm L3Chm11Alt4 { get => _L3Chm11Alt4 ?? new(null); }
        public static CustomPwm L3Chm11Alt5 { get => _L3Chm11Alt5 ?? new(null); }
        public static CustomPwm L3Chm11Alt6 { get => _L3Chm11Alt6 ?? new(null); }
        public static CustomPwm L3Chm11Alt7 { get => _L3Chm11Alt7 ?? new(null); }
        public static CustomPwm L3Chm11Alt8 { get => _L3Chm11Alt8 ?? new(null); }
        public static CustomPwm L3Chm11Alt9 { get => _L3Chm11Alt9 ?? new(null); }
        public static CustomPwm L3Chm11Alt10 { get => _L3Chm11Alt10 ?? new(null); }

        public static CustomPwm L3Chm13Default { get => _L3Chm13Default ?? new(null); }
        public static CustomPwm L3Chm13Alt1 { get => _L3Chm13Alt1 ?? new(null); }
        public static CustomPwm L3Chm13Alt2 { get => _L3Chm13Alt2 ?? new(null); }
        public static CustomPwm L3Chm13Alt3 { get => _L3Chm13Alt3 ?? new(null); }
        public static CustomPwm L3Chm13Alt4 { get => _L3Chm13Alt4 ?? new(null); }
        public static CustomPwm L3Chm13Alt5 { get => _L3Chm13Alt5 ?? new(null); }
        public static CustomPwm L3Chm13Alt6 { get => _L3Chm13Alt6 ?? new(null); }
        public static CustomPwm L3Chm13Alt7 { get => _L3Chm13Alt7 ?? new(null); }
        public static CustomPwm L3Chm13Alt8 { get => _L3Chm13Alt8 ?? new(null); }
        public static CustomPwm L3Chm13Alt9 { get => _L3Chm13Alt9 ?? new(null); }
        public static CustomPwm L3Chm13Alt10 { get => _L3Chm13Alt10 ?? new(null); }
        public static CustomPwm L3Chm13Alt11 { get => _L3Chm13Alt11 ?? new(null); }
        public static CustomPwm L3Chm13Alt12 { get => _L3Chm13Alt12 ?? new(null); }
        public static CustomPwm L3Chm13Alt13 { get => _L3Chm13Alt13 ?? new(null); }
        public static CustomPwm L3Chm13Alt14 { get => _L3Chm13Alt14 ?? new(null); }

        public static CustomPwm L3Chm15Default { get => _L3Chm15Default ?? new(null); }
        public static CustomPwm L3Chm15Alt1 { get => _L3Chm15Alt1 ?? new(null); }
        public static CustomPwm L3Chm15Alt2 { get => _L3Chm15Alt2 ?? new(null); }
        public static CustomPwm L3Chm15Alt3 { get => _L3Chm15Alt3 ?? new(null); }
        public static CustomPwm L3Chm15Alt4 { get => _L3Chm15Alt4 ?? new(null); }
        public static CustomPwm L3Chm15Alt5 { get => _L3Chm15Alt5 ?? new(null); }
        public static CustomPwm L3Chm15Alt6 { get => _L3Chm15Alt6 ?? new(null); }
        public static CustomPwm L3Chm15Alt7 { get => _L3Chm15Alt7 ?? new(null); }
        public static CustomPwm L3Chm15Alt8 { get => _L3Chm15Alt8 ?? new(null); }
        public static CustomPwm L3Chm15Alt9 { get => _L3Chm15Alt9 ?? new(null); }
        public static CustomPwm L3Chm15Alt10 { get => _L3Chm15Alt10 ?? new(null); }
        public static CustomPwm L3Chm15Alt11 { get => _L3Chm15Alt11 ?? new(null); }
        public static CustomPwm L3Chm15Alt12 { get => _L3Chm15Alt12 ?? new(null); }
        public static CustomPwm L3Chm15Alt13 { get => _L3Chm15Alt13 ?? new(null); }
        public static CustomPwm L3Chm15Alt14 { get => _L3Chm15Alt14 ?? new(null); }
        public static CustomPwm L3Chm15Alt15 { get => _L3Chm15Alt15 ?? new(null); }
        public static CustomPwm L3Chm15Alt16 { get => _L3Chm15Alt16 ?? new(null); }
        public static CustomPwm L3Chm15Alt17 { get => _L3Chm15Alt17 ?? new(null); }

        public static CustomPwm L3Chm17Default { get => _L3Chm17Default ?? new(null); }
        public static CustomPwm L3Chm17Alt1 { get => _L3Chm17Alt1 ?? new(null); }
        public static CustomPwm L3Chm17Alt2 { get => _L3Chm17Alt2 ?? new(null); }
        public static CustomPwm L3Chm17Alt3 { get => _L3Chm17Alt3 ?? new(null); }
        public static CustomPwm L3Chm17Alt4 { get => _L3Chm17Alt4 ?? new(null); }
        public static CustomPwm L3Chm17Alt5 { get => _L3Chm17Alt5 ?? new(null); }
        public static CustomPwm L3Chm17Alt6 { get => _L3Chm17Alt6 ?? new(null); }
        public static CustomPwm L3Chm17Alt7 { get => _L3Chm17Alt7 ?? new(null); }
        public static CustomPwm L3Chm17Alt8 { get => _L3Chm17Alt8 ?? new(null); }
        public static CustomPwm L3Chm17Alt9 { get => _L3Chm17Alt9 ?? new(null); }
        public static CustomPwm L3Chm17Alt10 { get => _L3Chm17Alt10 ?? new(null); }
        public static CustomPwm L3Chm17Alt11 { get => _L3Chm17Alt11 ?? new(null); }
        public static CustomPwm L3Chm17Alt12 { get => _L3Chm17Alt12 ?? new(null); }
        public static CustomPwm L3Chm17Alt13 { get => _L3Chm17Alt13 ?? new(null); }
        public static CustomPwm L3Chm17Alt14 { get => _L3Chm17Alt14 ?? new(null); }
        public static CustomPwm L3Chm17Alt15 { get => _L3Chm17Alt15 ?? new(null); }
        public static CustomPwm L3Chm17Alt16 { get => _L3Chm17Alt16 ?? new(null); }
        public static CustomPwm L3Chm17Alt17 { get => _L3Chm17Alt17 ?? new(null); }
        public static CustomPwm L3Chm17Alt18 { get => _L3Chm17Alt18 ?? new(null); }
        public static CustomPwm L3Chm17Alt19 { get => _L3Chm17Alt19 ?? new(null); }

        public static CustomPwm L3Chm19Default { get => _L3Chm19Default ?? new(null); }
        public static CustomPwm L3Chm19Alt1 { get => _L3Chm19Alt1 ?? new(null); }
        public static CustomPwm L3Chm19Alt2 { get => _L3Chm19Alt2 ?? new(null); }
        public static CustomPwm L3Chm19Alt3 { get => _L3Chm19Alt3 ?? new(null); }
        public static CustomPwm L3Chm19Alt4 { get => _L3Chm19Alt4 ?? new(null); }
        public static CustomPwm L3Chm19Alt5 { get => _L3Chm19Alt5 ?? new(null); }
        public static CustomPwm L3Chm19Alt6 { get => _L3Chm19Alt6 ?? new(null); }
        public static CustomPwm L3Chm19Alt7 { get => _L3Chm19Alt7 ?? new(null); }
        public static CustomPwm L3Chm19Alt8 { get => _L3Chm19Alt8 ?? new(null); }
        public static CustomPwm L3Chm19Alt9 { get => _L3Chm19Alt9 ?? new(null); }
        public static CustomPwm L3Chm19Alt10 { get => _L3Chm19Alt10 ?? new(null); }
        public static CustomPwm L3Chm19Alt11 { get => _L3Chm19Alt11 ?? new(null); }
        public static CustomPwm L3Chm19Alt12 { get => _L3Chm19Alt12 ?? new(null); }
        public static CustomPwm L3Chm19Alt13 { get => _L3Chm19Alt13 ?? new(null); }
        public static CustomPwm L3Chm19Alt14 { get => _L3Chm19Alt14 ?? new(null); }
        public static CustomPwm L3Chm19Alt15 { get => _L3Chm19Alt15 ?? new(null); }
        public static CustomPwm L3Chm19Alt16 { get => _L3Chm19Alt16 ?? new(null); }
        public static CustomPwm L3Chm19Alt17 { get => _L3Chm19Alt17 ?? new(null); }
        public static CustomPwm L3Chm19Alt18 { get => _L3Chm19Alt18 ?? new(null); }
        public static CustomPwm L3Chm19Alt19 { get => _L3Chm19Alt19 ?? new(null); }
        public static CustomPwm L3Chm19Alt20 { get => _L3Chm19Alt20 ?? new(null); }
        public static CustomPwm L3Chm19Alt21 { get => _L3Chm19Alt21 ?? new(null); }
        public static CustomPwm L3Chm19Alt22 { get => _L3Chm19Alt22 ?? new(null); }
        public static CustomPwm L3Chm19Alt23 { get => _L3Chm19Alt23 ?? new(null); }
        public static CustomPwm L3Chm19Alt24 { get => _L3Chm19Alt24 ?? new(null); }
        public static CustomPwm L3Chm19Alt25 { get => _L3Chm19Alt25 ?? new(null); }

        public static CustomPwm L3Chm21Default { get => _L3Chm21Default ?? new(null); }
        public static CustomPwm L3Chm21Alt1 { get => _L3Chm21Alt1 ?? new(null); }
        public static CustomPwm L3Chm21Alt2 { get => _L3Chm21Alt2 ?? new(null); }
        public static CustomPwm L3Chm21Alt3 { get => _L3Chm21Alt3 ?? new(null); }
        public static CustomPwm L3Chm21Alt4 { get => _L3Chm21Alt4 ?? new(null); }
        public static CustomPwm L3Chm21Alt5 { get => _L3Chm21Alt5 ?? new(null); }
        public static CustomPwm L3Chm21Alt6 { get => _L3Chm21Alt6 ?? new(null); }
        public static CustomPwm L3Chm21Alt7 { get => _L3Chm21Alt7 ?? new(null); }
        public static CustomPwm L3Chm21Alt8 { get => _L3Chm21Alt8 ?? new(null); }
        public static CustomPwm L3Chm21Alt9 { get => _L3Chm21Alt9 ?? new(null); }
        public static CustomPwm L3Chm21Alt10 { get => _L3Chm21Alt10 ?? new(null); }
        public static CustomPwm L3Chm21Alt11 { get => _L3Chm21Alt11 ?? new(null); }
        public static CustomPwm L3Chm21Alt12 { get => _L3Chm21Alt12 ?? new(null); }
        public static CustomPwm L3Chm21Alt13 { get => _L3Chm21Alt13 ?? new(null); }
        public static CustomPwm L3Chm21Alt14 { get => _L3Chm21Alt14 ?? new(null); }
        public static CustomPwm L3Chm21Alt15 { get => _L3Chm21Alt15 ?? new(null); }
        public static CustomPwm L3Chm21Alt16 { get => _L3Chm21Alt16 ?? new(null); }
        public static CustomPwm L3Chm21Alt17 { get => _L3Chm21Alt17 ?? new(null); }
        public static CustomPwm L3Chm21Alt18 { get => _L3Chm21Alt18 ?? new(null); }
        public static CustomPwm L3Chm21Alt19 { get => _L3Chm21Alt19 ?? new(null); }
        public static CustomPwm L3Chm21Alt20 { get => _L3Chm21Alt20 ?? new(null); }
        public static CustomPwm L3Chm21Alt21 { get => _L3Chm21Alt21 ?? new(null); }
        public static CustomPwm L3Chm21Alt22 { get => _L3Chm21Alt22 ?? new(null); }

        public static CustomPwm L2She3Default { get => _L2She3Default ?? new(null); }
        public static CustomPwm L2She3Alt1 { get => _L2She3Alt1 ?? new(null); }
        public static CustomPwm L2She9Default { get => _L2She9Default ?? new(null); }
        public static CustomPwm L2She9Alt1 { get => _L2She9Alt1 ?? new(null); }
        public static CustomPwm L2She9Alt2 { get => _L2She9Alt2 ?? new(null); }
        public static CustomPwm L2She7Default { get => _L2She7Default ?? new(null); }
        public static CustomPwm L2She7Alt1 { get => _L2She7Alt1 ?? new(null); }
        public static CustomPwm L2She5Default { get => _L2She5Default ?? new(null); }
        public static CustomPwm L2She5Alt1 { get => _L2She5Alt1 ?? new(null); }
        public static CustomPwm L2She5Alt2 { get => _L2She5Alt2 ?? new(null); }
        public static CustomPwm L2She11Default { get => _L2She11Default ?? new(null); }
        public static CustomPwm L2She11Alt1 { get => _L2She11Alt1 ?? new(null); }
        public static CustomPwm L2She13Default { get => _L2She13Default ?? new(null); }
        public static CustomPwm L2She13Alt1 { get => _L2She13Alt1 ?? new(null); }
        public static CustomPwm L2She15Default { get => _L2She15Default ?? new(null); }
        public static CustomPwm L2She15Alt1 { get => _L2She15Alt1 ?? new(null); }
        public static CustomPwm L2She17Default { get => _L2She17Default ?? new(null); }
        public static CustomPwm L2She17Alt1 { get => _L2She17Alt1 ?? new(null); }

        public static CustomPwm L3She1Default { get => _L3She1Default ?? new(null); }
        public static CustomPwm L3She3Default { get => _L3She3Default ?? new(null); }
        public static CustomPwm L3She3Alt1 { get => _L3She3Alt1 ?? new(null); }
        public static CustomPwm L3She5Default { get => _L3She5Default ?? new(null); }
        public static CustomPwm L3She7Default { get => _L3She7Default ?? new(null); }
        public static CustomPwm L3She9Default { get => _L3She9Default ?? new(null); }
        public static CustomPwm L3She11Default { get => _L3She11Default ?? new(null); }
        public static CustomPwm L3She13Default { get => _L3She13Default ?? new(null); }
        public static CustomPwm L3She15Default { get => _L3She15Default ?? new(null); }
        public static CustomPwm L3She17Default { get => _L3She17Default ?? new(null); }
        public static CustomPwm L3She19Default { get => _L3She19Default ?? new(null); }
        public static CustomPwm L3She21Default { get => _L3She21Default ?? new(null); }
    }
}
