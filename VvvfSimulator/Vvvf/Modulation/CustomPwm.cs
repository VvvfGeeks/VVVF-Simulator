using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;

namespace VvvfSimulator.Vvvf.Modulation
{
    public class CustomPwm
    {
        private const int MaxPwmLevel = 2;

        public byte SwitchCount = 0;
        public double ModulationIndexDivision = 0.0;
        public double MinimumModulationIndex = 0.0;
        public uint BlockCount = 0;
        public (double SwitchAngle, byte Output)[] SwitchAngleTable = [];
        public byte[] StartLevelTable = [];

        public CustomPwm()
        {
        }

        public CustomPwm(Stream? St)
        {
            if (St == null) throw new Exception();

            St.Seek(0, SeekOrigin.Begin);
            byte SwitchCount = (byte)St.ReadByte();

            byte[] DivisionRaw = new byte[8];
            St.ReadExactly(DivisionRaw, 0, 8);
            double Division = BitConverter.ToDouble(DivisionRaw, 0);

            byte[] StartModulationRaw = new byte[8];
            St.ReadExactly(StartModulationRaw, 0, 8);
            double StartModulation = BitConverter.ToDouble(StartModulationRaw, 0);

            byte[] LengthRaw = new byte[4];
            St.ReadExactly(LengthRaw, 0, 4);
            uint Length = BitConverter.ToUInt32(LengthRaw, 0);

            this.SwitchCount = SwitchCount;
            this.ModulationIndexDivision = Division;
            this.MinimumModulationIndex = StartModulation;
            this.BlockCount = Length;

            this.SwitchAngleTable = new (double, byte)[this.BlockCount * this.SwitchCount];
            this.StartLevelTable = new byte[this.BlockCount];

            for (int i = 0; i < this.BlockCount; i++)
            {
                this.StartLevelTable[i] = (byte)St.ReadByte();

                for (int j = 0; j < this.SwitchCount; j++)
                {
                    byte[] LevelRaw = new byte[1];
                    byte[] SwitchAngleRaw = new byte[8];
                    St.ReadExactly(LevelRaw, 0, 1);
                    St.ReadExactly(SwitchAngleRaw, 0, 8);
                    byte Output = LevelRaw[0];
                    double SwitchAngle = BitConverter.ToDouble(SwitchAngleRaw, 0);
                    this.SwitchAngleTable[i * this.SwitchCount + j] = new(SwitchAngle, Output);
                }
            }

            St.Close();
        }

        public delegate int GetPwmDelegate(double M, double X);

        public int GetPwm(double M, double X)
        {
            int Index = (int)((M - MinimumModulationIndex) / ModulationIndexDivision);
            Index = Math.Clamp(Index, 0, (int)BlockCount - 1);

            (double SwitchAngle, byte Output)[] Alpha = new (double SwitchAngle, byte Output)[SwitchCount];
            byte StartLevel = StartLevelTable[Index];

            for (int i = 0; i < SwitchCount; i++)
            {
                Alpha[i] = SwitchAngleTable[Index * SwitchCount + i];
            }

            return CustomPwm.GetPwm(ref Alpha, X, StartLevel);
        }

        public static int GetPwm(ref (double SwitchAngle, byte Output)[] Alpha, double X, byte StartLevel)
        {
            X %= MyMath.M_2PI;
            int Orthant = (int)(X / MyMath.M_PI_2);
            double Angle = X % MyMath.M_PI_2;

            if ((Orthant & 0x01) == 1)
                Angle = MyMath.M_PI_2 - Angle;
            int Pwm = StartLevel;
            for (int i = 0; i < Alpha.Length; i++)
            {
                (double SwitchAngle, byte Output) = Alpha[i];
                if (SwitchAngle <= Angle) Pwm = Output;
                else break;
            }

            if (Orthant > 1)
                Pwm = MaxPwmLevel - Pwm;

            return Pwm;
        }
    }
    public static class CustomPwmPresets
    {
        public static bool Loaded = false;
        private static bool Loading = false;
        public static void Load()
        {
            if (Loaded || Loading) return;
            Loading = true;
            Task t = Task.Run(() =>
            {
                L2Chm3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm3Default.bin"));
                L2Chm3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm3Alt1.bin"));
                L2Chm3Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm3Alt2.bin"));

                L2Chm5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Default.bin"));
                L2Chm5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt1.bin"));
                L2Chm5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt2.bin"));
                L2Chm5Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt3.bin"));

                L2Chm7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Default.bin"));
                L2Chm7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt1.bin"));
                L2Chm7Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt2.bin"));
                L2Chm7Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt3.bin"));
                L2Chm7Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt4.bin"));
                L2Chm7Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt5.bin"));

                L2Chm9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Default.bin"));
                L2Chm9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt1.bin"));
                L2Chm9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt2.bin"));
                L2Chm9Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt3.bin"));
                L2Chm9Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt4.bin"));
                L2Chm9Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt5.bin"));
                L2Chm9Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt6.bin"));
                L2Chm9Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt7.bin"));
                L2Chm9Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt8.bin"));

                L2Chm11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Default.bin"));
                L2Chm11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt1.bin"));
                L2Chm11Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt2.bin"));
                L2Chm11Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt3.bin"));
                L2Chm11Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt4.bin"));
                L2Chm11Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt5.bin"));
                L2Chm11Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt6.bin"));
                L2Chm11Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt7.bin"));
                L2Chm11Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt8.bin"));
                L2Chm11Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt9.bin"));
                L2Chm11Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt10.bin"));
                L2Chm11Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt11.bin"));

                L2Chm13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Default.bin"));
                L2Chm13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt1.bin"));
                L2Chm13Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt2.bin"));
                L2Chm13Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt3.bin"));
                L2Chm13Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt4.bin"));
                L2Chm13Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt5.bin"));
                L2Chm13Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt6.bin"));
                L2Chm13Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt7.bin"));
                L2Chm13Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt8.bin"));
                L2Chm13Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt9.bin"));
                L2Chm13Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt10.bin"));
                L2Chm13Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt11.bin"));
                L2Chm13Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt12.bin"));
                L2Chm13Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt13.bin"));

                L2Chm15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Default.bin"));
                L2Chm15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt1.bin"));
                L2Chm15Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt2.bin"));
                L2Chm15Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt3.bin"));
                L2Chm15Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt4.bin"));
                L2Chm15Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt5.bin"));
                L2Chm15Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt6.bin"));
                L2Chm15Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt7.bin"));
                L2Chm15Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt8.bin"));
                L2Chm15Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt9.bin"));
                L2Chm15Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt10.bin"));
                L2Chm15Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt11.bin"));
                L2Chm15Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt12.bin"));
                L2Chm15Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt13.bin"));
                L2Chm15Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt14.bin"));
                L2Chm15Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt15.bin"));
                L2Chm15Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt16.bin"));
                L2Chm15Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt17.bin"));
                L2Chm15Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt18.bin"));
                L2Chm15Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt19.bin"));
                L2Chm15Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt20.bin"));
                L2Chm15Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt21.bin"));
                L2Chm15Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt22.bin"));
                L2Chm15Alt23 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt23.bin"));

                L2Chm17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Default.bin"));
                L2Chm17Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt1.bin"));
                L2Chm17Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt2.bin"));
                L2Chm17Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt3.bin"));
                L2Chm17Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt4.bin"));
                L2Chm17Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt5.bin"));
                L2Chm17Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt6.bin"));
                L2Chm17Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt7.bin"));
                L2Chm17Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt8.bin"));
                L2Chm17Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt9.bin"));
                L2Chm17Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt10.bin"));
                L2Chm17Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt11.bin"));

                L2Chm19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Default.bin"));
                L2Chm19Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt1.bin"));
                L2Chm19Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt2.bin"));
                L2Chm19Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt3.bin"));
                L2Chm19Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt4.bin"));
                L2Chm19Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt5.bin"));
                L2Chm19Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt6.bin"));
                L2Chm19Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt7.bin"));
                L2Chm19Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt8.bin"));
                L2Chm19Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt9.bin"));
                L2Chm19Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt10.bin"));
                L2Chm19Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt11.bin"));

                L2Chm21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Default.bin"));
                L2Chm21Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt1.bin"));
                L2Chm21Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt2.bin"));
                L2Chm21Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt3.bin"));
                L2Chm21Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt4.bin"));
                L2Chm21Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt5.bin"));
                L2Chm21Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt6.bin"));
                L2Chm21Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt7.bin"));
                L2Chm21Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt8.bin"));
                L2Chm21Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt9.bin"));
                L2Chm21Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt10.bin"));
                L2Chm21Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt11.bin"));
                L2Chm21Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt12.bin"));
                L2Chm21Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt13.bin"));

                L2Chm23Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Default.bin"));
                L2Chm23Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt1.bin"));
                L2Chm23Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt2.bin"));
                L2Chm23Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt3.bin"));
                L2Chm23Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt4.bin"));
                L2Chm23Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt5.bin"));
                L2Chm23Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt6.bin"));
                L2Chm23Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt7.bin"));
                L2Chm23Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt8.bin"));
                L2Chm23Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt9.bin"));
                L2Chm23Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt10.bin"));
                L2Chm23Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt11.bin"));
                L2Chm23Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt12.bin"));
                L2Chm23Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt13.bin"));
                L2Chm23Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt14.bin"));

                L2Chm25Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Default.bin"));
                L2Chm25Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt1.bin"));
                L2Chm25Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt2.bin"));
                L2Chm25Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt3.bin"));
                L2Chm25Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt4.bin"));
                L2Chm25Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt5.bin"));
                L2Chm25Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt6.bin"));
                L2Chm25Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt7.bin"));
                L2Chm25Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt8.bin"));
                L2Chm25Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt9.bin"));
                L2Chm25Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt10.bin"));
                L2Chm25Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt11.bin"));
                L2Chm25Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt12.bin"));
                L2Chm25Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt13.bin"));
                L2Chm25Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt14.bin"));
                L2Chm25Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt15.bin"));
                L2Chm25Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt16.bin"));
                L2Chm25Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt17.bin"));
                L2Chm25Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt18.bin"));
                L2Chm25Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt19.bin"));
                L2Chm25Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt20.bin"));

                L3Chm1Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm1Default.bin"));

                L3Chm3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Default.bin"));
                L3Chm3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Alt1.bin"));
                L3Chm3Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Alt2.bin"));

                L3Chm5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Default.bin"));
                L3Chm5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt1.bin"));
                L3Chm5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt2.bin"));
                L3Chm5Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt3.bin"));
                L3Chm5Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt4.bin"));

                L3Chm7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Default.bin"));
                L3Chm7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt1.bin"));
                L3Chm7Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt2.bin"));
                L3Chm7Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt3.bin"));
                L3Chm7Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt4.bin"));
                L3Chm7Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt5.bin"));
                L3Chm7Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt6.bin"));

                L3Chm9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Default.bin"));
                L3Chm9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt1.bin"));
                L3Chm9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt2.bin"));
                L3Chm9Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt3.bin"));
                L3Chm9Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt4.bin"));
                L3Chm9Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt5.bin"));
                L3Chm9Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt6.bin"));
                L3Chm9Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt7.bin"));

                L3Chm11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Default.bin"));
                L3Chm11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt1.bin"));
                L3Chm11Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt2.bin"));
                L3Chm11Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt3.bin"));
                L3Chm11Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt4.bin"));
                L3Chm11Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt5.bin"));
                L3Chm11Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt6.bin"));
                L3Chm11Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt7.bin"));
                L3Chm11Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt8.bin"));
                L3Chm11Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt9.bin"));
                L3Chm11Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt10.bin"));

                L3Chm13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Default.bin"));
                L3Chm13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt1.bin"));
                L3Chm13Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt2.bin"));
                L3Chm13Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt3.bin"));
                L3Chm13Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt4.bin"));
                L3Chm13Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt5.bin"));
                L3Chm13Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt6.bin"));
                L3Chm13Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt7.bin"));
                L3Chm13Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt8.bin"));
                L3Chm13Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt9.bin"));
                L3Chm13Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt10.bin"));
                L3Chm13Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt11.bin"));
                L3Chm13Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt12.bin"));
                L3Chm13Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt13.bin"));
                L3Chm13Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt14.bin"));

                L3Chm15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Default.bin"));
                L3Chm15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt1.bin"));
                L3Chm15Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt2.bin"));
                L3Chm15Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt3.bin"));
                L3Chm15Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt4.bin"));
                L3Chm15Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt5.bin"));
                L3Chm15Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt6.bin"));
                L3Chm15Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt7.bin"));
                L3Chm15Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt8.bin"));
                L3Chm15Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt9.bin"));
                L3Chm15Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt10.bin"));
                L3Chm15Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt11.bin"));
                L3Chm15Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt12.bin"));
                L3Chm15Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt13.bin"));
                L3Chm15Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt14.bin"));
                L3Chm15Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt15.bin"));
                L3Chm15Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt16.bin"));
                L3Chm15Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt17.bin"));

                L3Chm17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Default.bin"));
                L3Chm17Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt1.bin"));
                L3Chm17Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt2.bin"));
                L3Chm17Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt3.bin"));
                L3Chm17Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt4.bin"));
                L3Chm17Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt5.bin"));
                L3Chm17Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt6.bin"));
                L3Chm17Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt7.bin"));
                L3Chm17Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt8.bin"));
                L3Chm17Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt9.bin"));
                L3Chm17Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt10.bin"));
                L3Chm17Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt11.bin"));
                L3Chm17Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt12.bin"));
                L3Chm17Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt13.bin"));
                L3Chm17Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt14.bin"));
                L3Chm17Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt15.bin"));
                L3Chm17Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt16.bin"));
                L3Chm17Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt17.bin"));
                L3Chm17Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt18.bin"));
                L3Chm17Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt19.bin"));

                L3Chm19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Default.bin"));
                L3Chm19Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt1.bin"));
                L3Chm19Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt2.bin"));
                L3Chm19Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt3.bin"));
                L3Chm19Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt4.bin"));
                L3Chm19Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt5.bin"));
                L3Chm19Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt6.bin"));
                L3Chm19Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt7.bin"));
                L3Chm19Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt8.bin"));
                L3Chm19Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt9.bin"));
                L3Chm19Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt10.bin"));
                L3Chm19Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt11.bin"));
                L3Chm19Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt12.bin"));
                L3Chm19Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt13.bin"));
                L3Chm19Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt14.bin"));
                L3Chm19Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt15.bin"));
                L3Chm19Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt16.bin"));
                L3Chm19Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt17.bin"));
                L3Chm19Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt18.bin"));
                L3Chm19Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt19.bin"));
                L3Chm19Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt20.bin"));
                L3Chm19Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt21.bin"));
                L3Chm19Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt22.bin"));
                L3Chm19Alt23 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt23.bin"));
                L3Chm19Alt24 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt24.bin"));
                L3Chm19Alt25 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt25.bin"));

                L3Chm21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Default.bin"));
                L3Chm21Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt1.bin"));
                L3Chm21Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt2.bin"));
                L3Chm21Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt3.bin"));
                L3Chm21Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt4.bin"));
                L3Chm21Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt5.bin"));
                L3Chm21Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt6.bin"));
                L3Chm21Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt7.bin"));
                L3Chm21Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt8.bin"));
                L3Chm21Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt9.bin"));
                L3Chm21Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt10.bin"));
                L3Chm21Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt11.bin"));
                L3Chm21Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt12.bin"));
                L3Chm21Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt13.bin"));
                L3Chm21Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt14.bin"));
                L3Chm21Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt15.bin"));
                L3Chm21Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt16.bin"));
                L3Chm21Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt17.bin"));
                L3Chm21Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt18.bin"));
                L3Chm21Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt19.bin"));
                L3Chm21Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt20.bin"));
                L3Chm21Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt21.bin"));
                L3Chm21Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt22.bin"));

                L2She3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She3Default.bin"));
                L2She3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She3Alt1.bin"));
                L2She5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Default.bin"));
                L2She5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Alt1.bin"));
                L2She5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Alt2.bin"));
                L2She7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She7Default.bin"));
                L2She7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She7Alt1.bin"));
                L2She9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Default.bin"));
                L2She9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Alt1.bin"));
                L2She9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Alt2.bin"));
                L2She9Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Alt3.bin"));
                L2She11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She11Default.bin"));
                L2She11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She11Alt1.bin"));
                L2She11Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She11Alt2.bin"));
                L2She11Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She11Alt3.bin"));
                L2She13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She13Default.bin"));
                L2She13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She13Alt1.bin"));
                L2She13Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She13Alt2.bin"));
                L2She13Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She13Alt3.bin"));
                L2She15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She15Default.bin"));
                L2She15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She15Alt1.bin"));
                L2She15Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She15Alt2.bin"));
                L2She15Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She15Alt3.bin"));
                L2She17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Default.bin"));
                L2She17Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Alt1.bin"));
                L2She17Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Alt2.bin"));
                L2She17Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Alt3.bin"));
                L2She17Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Alt4.bin"));
                L2She17Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Alt5.bin"));
                L2She17Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She17Alt6.bin"));
                L2She19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She19Default.bin"));
                L2She19Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She19Alt1.bin"));
                L2She19Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She19Alt2.bin"));
                L2She19Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She19Alt3.bin"));
                L2She19Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She19Alt4.bin"));
                L2She19Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She19Alt5.bin"));
                L2She19Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She19Alt6.bin"));
                L2She21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She21Default.bin"));
                L2She21Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She21Alt1.bin"));
                L2She21Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She21Alt2.bin"));
                L2She21Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She21Alt3.bin"));
                L2She21Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She21Alt4.bin"));
                L2She21Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She21Alt5.bin"));
                L2She21Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She21Alt6.bin"));

                L3She1Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She1Default.bin"));
                L3She3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She3Default.bin"));
                L3She3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She3Alt1.bin"));
                L3She5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She5Default.bin"));
                L3She7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She7Default.bin"));
                L3She9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She9Default.bin"));
                L3She11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She11Default.bin"));
                L3She13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She13Default.bin"));
                L3She15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She15Default.bin"));
                L3She17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She17Default.bin"));
                L3She19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She19Default.bin"));
                L3She21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She21Default.bin"));
                Loaded = true;
            });
            t.Wait();
            Loading = false;
        }

        public static CustomPwm? GetCustomPwm(int Level, PulseTypeName PulseType, int PulseCount, PulseAlternative Alternative)
        {
            return Level switch
            {
                2 => PulseType switch
                {
                    PulseTypeName.SHE => PulseCount switch
                    {
                        21 => Alternative switch
                        {
                            PulseAlternative.Default => L2She21Default,
                            PulseAlternative.Alt1 => L2She21Alt1,
                            PulseAlternative.Alt2 => L2She21Alt2,
                            PulseAlternative.Alt3 => L2She21Alt3,
                            PulseAlternative.Alt4 => L2She21Alt4,
                            PulseAlternative.Alt5 => L2She21Alt5,
                            PulseAlternative.Alt6 => L2She21Alt6,
                            _ => null,
                        },
                        19 => Alternative switch
                        {
                            PulseAlternative.Default => L2She19Default,
                            PulseAlternative.Alt1 => L2She19Alt1,
                            PulseAlternative.Alt2 => L2She19Alt2,
                            PulseAlternative.Alt3 => L2She19Alt3,
                            PulseAlternative.Alt4 => L2She19Alt4,
                            PulseAlternative.Alt5 => L2She19Alt5,
                            PulseAlternative.Alt6 => L2She19Alt6,
                            _ => null,
                        },
                        17 => Alternative switch
                        {
                            PulseAlternative.Default => L2She17Default,
                            PulseAlternative.Alt1 => L2She17Alt1,
                            PulseAlternative.Alt2 => L2She17Alt2,
                            PulseAlternative.Alt3 => L2She17Alt3,
                            PulseAlternative.Alt4 => L2She17Alt4,
                            PulseAlternative.Alt5 => L2She17Alt5,
                            PulseAlternative.Alt6 => L2She17Alt6,
                            _ => null,
                        },
                        15 => Alternative switch
                        {
                            PulseAlternative.Default => L2She15Default,
                            PulseAlternative.Alt1 => L2She15Alt1,
                            PulseAlternative.Alt2 => L2She15Alt2,
                            PulseAlternative.Alt3 => L2She15Alt3,
                            _ => null,
                        },
                        13 => Alternative switch
                        {
                            PulseAlternative.Default => L2She13Default,
                            PulseAlternative.Alt1 => L2She13Alt1,
                            PulseAlternative.Alt2 => L2She13Alt2,
                            PulseAlternative.Alt3 => L2She13Alt3,
                            _ => null,
                        },
                        11 => Alternative switch
                        {
                            PulseAlternative.Default => L2She11Default,
                            PulseAlternative.Alt1 => L2She11Alt1,
                            PulseAlternative.Alt2 => L2She11Alt2,
                            PulseAlternative.Alt3 => L2She11Alt3,
                            _ => null,
                        },
                        9 => Alternative switch
                        {
                            PulseAlternative.Default => L2She9Default,
                            PulseAlternative.Alt1 => L2She9Alt1,
                            PulseAlternative.Alt2 => L2She9Alt2,
                            PulseAlternative.Alt3 => L2She9Alt3,
                            _ => null,
                        },
                        7 => Alternative switch
                        {
                            PulseAlternative.Default => L2She7Default,
                            PulseAlternative.Alt1 => L2She7Alt1,
                            _ => null,
                        },
                        5 => Alternative switch
                        {
                            PulseAlternative.Default => L2She5Default,
                            PulseAlternative.Alt1 => L2She5Alt1,
                            PulseAlternative.Alt2 => L2She5Alt2,
                            _ => null,
                        },
                        3 => Alternative switch
                        {
                            PulseAlternative.Default => L2She3Default,
                            PulseAlternative.Alt1 => L2She3Alt1,
                            _ => null,
                        },
                        _ => null,
                    },
                    PulseTypeName.CHM => PulseCount switch
                    {
                        25 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm25Default,
                            PulseAlternative.Alt1 => L2Chm25Alt1,
                            PulseAlternative.Alt2 => L2Chm25Alt2,
                            PulseAlternative.Alt3 => L2Chm25Alt3,
                            PulseAlternative.Alt4 => L2Chm25Alt4,
                            PulseAlternative.Alt5 => L2Chm25Alt5,
                            PulseAlternative.Alt6 => L2Chm25Alt6,
                            PulseAlternative.Alt7 => L2Chm25Alt7,
                            PulseAlternative.Alt8 => L2Chm25Alt8,
                            PulseAlternative.Alt9 => L2Chm25Alt9,
                            PulseAlternative.Alt10 => L2Chm25Alt10,
                            PulseAlternative.Alt11 => L2Chm25Alt11,
                            PulseAlternative.Alt12 => L2Chm25Alt12,
                            PulseAlternative.Alt13 => L2Chm25Alt13,
                            PulseAlternative.Alt14 => L2Chm25Alt14,
                            PulseAlternative.Alt15 => L2Chm25Alt15,
                            PulseAlternative.Alt16 => L2Chm25Alt16,
                            PulseAlternative.Alt17 => L2Chm25Alt17,
                            PulseAlternative.Alt18 => L2Chm25Alt18,
                            PulseAlternative.Alt19 => L2Chm25Alt19,
                            PulseAlternative.Alt20 => L2Chm25Alt20,
                            _ => null
                        },
                        23 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm23Default,
                            PulseAlternative.Alt1 => L2Chm23Alt1,
                            PulseAlternative.Alt2 => L2Chm23Alt2,
                            PulseAlternative.Alt3 => L2Chm23Alt3,
                            PulseAlternative.Alt4 => L2Chm23Alt4,
                            PulseAlternative.Alt5 => L2Chm23Alt5,
                            PulseAlternative.Alt6 => L2Chm23Alt6,
                            PulseAlternative.Alt7 => L2Chm23Alt7,
                            PulseAlternative.Alt8 => L2Chm23Alt8,
                            PulseAlternative.Alt9 => L2Chm23Alt9,
                            PulseAlternative.Alt10 => L2Chm23Alt10,
                            PulseAlternative.Alt11 => L2Chm23Alt11,
                            PulseAlternative.Alt12 => L2Chm23Alt12,
                            PulseAlternative.Alt13 => L2Chm23Alt13,
                            PulseAlternative.Alt14 => L2Chm23Alt14,
                            _ => null
                        },
                        21 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm21Default,
                            PulseAlternative.Alt1 => L2Chm21Alt1,
                            PulseAlternative.Alt2 => L2Chm21Alt2,
                            PulseAlternative.Alt3 => L2Chm21Alt3,
                            PulseAlternative.Alt4 => L2Chm21Alt4,
                            PulseAlternative.Alt5 => L2Chm21Alt5,
                            PulseAlternative.Alt6 => L2Chm21Alt6,
                            PulseAlternative.Alt7 => L2Chm21Alt7,
                            PulseAlternative.Alt8 => L2Chm21Alt8,
                            PulseAlternative.Alt9 => L2Chm21Alt9,
                            PulseAlternative.Alt10 => L2Chm21Alt10,
                            PulseAlternative.Alt11 => L2Chm21Alt11,
                            PulseAlternative.Alt12 => L2Chm21Alt12,
                            PulseAlternative.Alt13 => L2Chm21Alt13,
                            _ => null
                        },
                        19 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm19Default,
                            PulseAlternative.Alt1 => L2Chm19Alt1,
                            PulseAlternative.Alt2 => L2Chm19Alt2,
                            PulseAlternative.Alt3 => L2Chm19Alt3,
                            PulseAlternative.Alt4 => L2Chm19Alt4,
                            PulseAlternative.Alt5 => L2Chm19Alt5,
                            PulseAlternative.Alt6 => L2Chm19Alt6,
                            PulseAlternative.Alt7 => L2Chm19Alt7,
                            PulseAlternative.Alt8 => L2Chm19Alt8,
                            PulseAlternative.Alt9 => L2Chm19Alt9,
                            PulseAlternative.Alt10 => L2Chm19Alt10,
                            PulseAlternative.Alt11 => L2Chm19Alt11,
                            _ => null
                        },
                        17 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm17Default,
                            PulseAlternative.Alt1 => L2Chm17Alt1,
                            PulseAlternative.Alt2 => L2Chm17Alt2,
                            PulseAlternative.Alt3 => L2Chm17Alt3,
                            PulseAlternative.Alt4 => L2Chm17Alt4,
                            PulseAlternative.Alt5 => L2Chm17Alt5,
                            PulseAlternative.Alt6 => L2Chm17Alt6,
                            PulseAlternative.Alt7 => L2Chm17Alt7,
                            PulseAlternative.Alt8 => L2Chm17Alt8,
                            PulseAlternative.Alt9 => L2Chm17Alt9,
                            PulseAlternative.Alt10 => L2Chm17Alt10,
                            PulseAlternative.Alt11 => L2Chm17Alt11,
                            _ => null
                        },
                        15 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm15Default,
                            PulseAlternative.Alt1 => L2Chm15Alt1,
                            PulseAlternative.Alt2 => L2Chm15Alt2,
                            PulseAlternative.Alt3 => L2Chm15Alt3,
                            PulseAlternative.Alt4 => L2Chm15Alt4,
                            PulseAlternative.Alt5 => L2Chm15Alt5,
                            PulseAlternative.Alt6 => L2Chm15Alt6,
                            PulseAlternative.Alt7 => L2Chm15Alt7,
                            PulseAlternative.Alt8 => L2Chm15Alt8,
                            PulseAlternative.Alt9 => L2Chm15Alt9,
                            PulseAlternative.Alt10 => L2Chm15Alt10,
                            PulseAlternative.Alt11 => L2Chm15Alt11,
                            PulseAlternative.Alt12 => L2Chm15Alt12,
                            PulseAlternative.Alt13 => L2Chm15Alt13,
                            PulseAlternative.Alt14 => L2Chm15Alt14,
                            PulseAlternative.Alt15 => L2Chm15Alt15,
                            PulseAlternative.Alt16 => L2Chm15Alt16,
                            PulseAlternative.Alt17 => L2Chm15Alt17,
                            PulseAlternative.Alt18 => L2Chm15Alt18,
                            PulseAlternative.Alt19 => L2Chm15Alt19,
                            PulseAlternative.Alt20 => L2Chm15Alt20,
                            PulseAlternative.Alt21 => L2Chm15Alt21,
                            PulseAlternative.Alt22 => L2Chm15Alt22,
                            PulseAlternative.Alt23 => L2Chm15Alt23,
                            _ => null
                        },
                        13 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm13Default,
                            PulseAlternative.Alt1 => L2Chm13Alt1,
                            PulseAlternative.Alt2 => L2Chm13Alt2,
                            PulseAlternative.Alt3 => L2Chm13Alt3,
                            PulseAlternative.Alt4 => L2Chm13Alt4,
                            PulseAlternative.Alt5 => L2Chm13Alt5,
                            PulseAlternative.Alt6 => L2Chm13Alt6,
                            PulseAlternative.Alt7 => L2Chm13Alt7,
                            PulseAlternative.Alt8 => L2Chm13Alt8,
                            PulseAlternative.Alt9 => L2Chm13Alt9,
                            PulseAlternative.Alt10 => L2Chm13Alt10,
                            PulseAlternative.Alt11 => L2Chm13Alt11,
                            PulseAlternative.Alt12 => L2Chm13Alt12,
                            PulseAlternative.Alt13 => L2Chm13Alt13,
                            _ => null
                        },
                        11 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm11Default,
                            PulseAlternative.Alt1 => L2Chm11Alt1,
                            PulseAlternative.Alt2 => L2Chm11Alt2,
                            PulseAlternative.Alt3 => L2Chm11Alt3,
                            PulseAlternative.Alt4 => L2Chm11Alt4,
                            PulseAlternative.Alt5 => L2Chm11Alt5,
                            PulseAlternative.Alt6 => L2Chm11Alt6,
                            PulseAlternative.Alt7 => L2Chm11Alt7,
                            PulseAlternative.Alt8 => L2Chm11Alt8,
                            PulseAlternative.Alt9 => L2Chm11Alt9,
                            PulseAlternative.Alt10 => L2Chm11Alt10,
                            PulseAlternative.Alt11 => L2Chm11Alt11,
                            _ => null
                        },
                        9 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm9Default,
                            PulseAlternative.Alt1 => L2Chm9Alt1,
                            PulseAlternative.Alt2 => L2Chm9Alt2,
                            PulseAlternative.Alt3 => L2Chm9Alt3,
                            PulseAlternative.Alt4 => L2Chm9Alt4,
                            PulseAlternative.Alt5 => L2Chm9Alt5,
                            PulseAlternative.Alt6 => L2Chm9Alt6,
                            PulseAlternative.Alt7 => L2Chm9Alt7,
                            PulseAlternative.Alt8 => L2Chm9Alt8,
                            _ => null
                        },
                        7 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm7Default,
                            PulseAlternative.Alt1 => L2Chm7Alt1,
                            PulseAlternative.Alt2 => L2Chm7Alt2,
                            PulseAlternative.Alt3 => L2Chm7Alt3,
                            PulseAlternative.Alt4 => L2Chm7Alt4,
                            PulseAlternative.Alt5 => L2Chm7Alt5,
                            _ => null
                        },
                        5 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm5Default,
                            PulseAlternative.Alt1 => L2Chm5Alt1,
                            PulseAlternative.Alt2 => L2Chm5Alt2,
                            PulseAlternative.Alt3 => L2Chm5Alt3,
                            _ => null
                        },
                        3 => Alternative switch
                        {
                            PulseAlternative.Default => L2Chm3Default,
                            PulseAlternative.Alt1 => L2Chm3Alt1,
                            PulseAlternative.Alt2 => L2Chm3Alt2,
                            _ => null
                        },
                        _ => null,
                    },
                    _ => null,
                },
                3 => PulseType switch
                {
                    PulseTypeName.SHE => PulseCount switch
                    {
                        21 => Alternative switch
                        {
                            PulseAlternative.Default => L3She21Default,
                            _ => null,
                        },
                        19 => Alternative switch
                        {
                            PulseAlternative.Default => L3She19Default,
                            _ => null,
                        },
                        17 => Alternative switch
                        {
                            PulseAlternative.Default => L3She17Default,
                            _ => null,
                        },
                        15 => Alternative switch
                        {
                            PulseAlternative.Default => L3She15Default,
                            _ => null,
                        },
                        13 => Alternative switch
                        {
                            PulseAlternative.Default => L3She13Default,
                            _ => null,
                        },
                        11 => Alternative switch
                        {
                            PulseAlternative.Default => L3She11Default,
                            _ => null,
                        },
                        9 => Alternative switch
                        {
                            PulseAlternative.Default => L3She9Default,
                            _ => null,
                        },
                        7 => Alternative switch
                        {
                            PulseAlternative.Default => L3She7Default,
                            _ => null,
                        },
                        5 => Alternative switch
                        {
                            PulseAlternative.Default => L3She5Default,
                            _ => null,
                        },
                        3 => Alternative switch
                        {
                            PulseAlternative.Default => L3She3Default,
                            PulseAlternative.Alt1 => L3She3Alt1,
                            _ => null,
                        },
                        1 => Alternative switch
                        {
                            PulseAlternative.Default => L3She1Default,
                            _ => null,
                        },
                        _ => null,
                    },
                    PulseTypeName.CHM => PulseCount switch
                    {
                        21 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm21Default,
                            PulseAlternative.Alt1 => L3Chm21Alt1,
                            PulseAlternative.Alt2 => L3Chm21Alt2,
                            PulseAlternative.Alt3 => L3Chm21Alt3,
                            PulseAlternative.Alt4 => L3Chm21Alt4,
                            PulseAlternative.Alt5 => L3Chm21Alt5,
                            PulseAlternative.Alt6 => L3Chm21Alt6,
                            PulseAlternative.Alt7 => L3Chm21Alt7,
                            PulseAlternative.Alt8 => L3Chm21Alt8,
                            PulseAlternative.Alt9 => L3Chm21Alt9,
                            PulseAlternative.Alt10 => L3Chm21Alt10,
                            PulseAlternative.Alt11 => L3Chm21Alt11,
                            PulseAlternative.Alt12 => L3Chm21Alt12,
                            PulseAlternative.Alt13 => L3Chm21Alt13,
                            PulseAlternative.Alt14 => L3Chm21Alt14,
                            PulseAlternative.Alt15 => L3Chm21Alt15,
                            PulseAlternative.Alt16 => L3Chm21Alt16,
                            PulseAlternative.Alt17 => L3Chm21Alt17,
                            PulseAlternative.Alt18 => L3Chm21Alt18,
                            PulseAlternative.Alt19 => L3Chm21Alt19,
                            PulseAlternative.Alt20 => L3Chm21Alt20,
                            PulseAlternative.Alt21 => L3Chm21Alt21,
                            PulseAlternative.Alt22 => L3Chm21Alt22,
                            _ => null,
                        },
                        19 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm19Default,
                            PulseAlternative.Alt1 => L3Chm19Alt1,
                            PulseAlternative.Alt2 => L3Chm19Alt2,
                            PulseAlternative.Alt3 => L3Chm19Alt3,
                            PulseAlternative.Alt4 => L3Chm19Alt4,
                            PulseAlternative.Alt5 => L3Chm19Alt5,
                            PulseAlternative.Alt6 => L3Chm19Alt6,
                            PulseAlternative.Alt7 => L3Chm19Alt7,
                            PulseAlternative.Alt8 => L3Chm19Alt8,
                            PulseAlternative.Alt9 => L3Chm19Alt9,
                            PulseAlternative.Alt10 => L3Chm19Alt10,
                            PulseAlternative.Alt11 => L3Chm19Alt11,
                            PulseAlternative.Alt12 => L3Chm19Alt12,
                            PulseAlternative.Alt13 => L3Chm19Alt13,
                            PulseAlternative.Alt14 => L3Chm19Alt14,
                            PulseAlternative.Alt15 => L3Chm19Alt15,
                            PulseAlternative.Alt16 => L3Chm19Alt16,
                            PulseAlternative.Alt17 => L3Chm19Alt17,
                            PulseAlternative.Alt18 => L3Chm19Alt18,
                            PulseAlternative.Alt19 => L3Chm19Alt19,
                            PulseAlternative.Alt20 => L3Chm19Alt20,
                            PulseAlternative.Alt21 => L3Chm19Alt21,
                            PulseAlternative.Alt22 => L3Chm19Alt22,
                            PulseAlternative.Alt23 => L3Chm19Alt23,
                            PulseAlternative.Alt24 => L3Chm19Alt24,
                            PulseAlternative.Alt25 => L3Chm19Alt25,
                            _ => null,
                        },
                        17 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm17Default,
                            PulseAlternative.Alt1 => L3Chm17Alt1,
                            PulseAlternative.Alt2 => L3Chm17Alt2,
                            PulseAlternative.Alt3 => L3Chm17Alt3,
                            PulseAlternative.Alt4 => L3Chm17Alt4,
                            PulseAlternative.Alt5 => L3Chm17Alt5,
                            PulseAlternative.Alt6 => L3Chm17Alt6,
                            PulseAlternative.Alt7 => L3Chm17Alt7,
                            PulseAlternative.Alt8 => L3Chm17Alt8,
                            PulseAlternative.Alt9 => L3Chm17Alt9,
                            PulseAlternative.Alt10 => L3Chm17Alt10,
                            PulseAlternative.Alt11 => L3Chm17Alt11,
                            PulseAlternative.Alt12 => L3Chm17Alt12,
                            PulseAlternative.Alt13 => L3Chm17Alt13,
                            PulseAlternative.Alt14 => L3Chm17Alt14,
                            PulseAlternative.Alt15 => L3Chm17Alt15,
                            PulseAlternative.Alt16 => L3Chm17Alt16,
                            PulseAlternative.Alt17 => L3Chm17Alt17,
                            PulseAlternative.Alt18 => L3Chm17Alt18,
                            PulseAlternative.Alt19 => L3Chm17Alt19,
                            _ => null
                        },
                        15 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm15Default,
                            PulseAlternative.Alt1 => L3Chm15Alt1,
                            PulseAlternative.Alt2 => L3Chm15Alt2,
                            PulseAlternative.Alt3 => L3Chm15Alt3,
                            PulseAlternative.Alt4 => L3Chm15Alt4,
                            PulseAlternative.Alt5 => L3Chm15Alt5,
                            PulseAlternative.Alt6 => L3Chm15Alt6,
                            PulseAlternative.Alt7 => L3Chm15Alt7,
                            PulseAlternative.Alt8 => L3Chm15Alt8,
                            PulseAlternative.Alt9 => L3Chm15Alt9,
                            PulseAlternative.Alt10 => L3Chm15Alt10,
                            PulseAlternative.Alt11 => L3Chm15Alt11,
                            PulseAlternative.Alt12 => L3Chm15Alt12,
                            PulseAlternative.Alt13 => L3Chm15Alt13,
                            PulseAlternative.Alt14 => L3Chm15Alt14,
                            PulseAlternative.Alt15 => L3Chm15Alt15,
                            PulseAlternative.Alt16 => L3Chm15Alt16,
                            PulseAlternative.Alt17 => L3Chm15Alt17,
                            _ => null
                        },
                        13 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm13Default,
                            PulseAlternative.Alt1 => L3Chm13Alt1,
                            PulseAlternative.Alt2 => L3Chm13Alt2,
                            PulseAlternative.Alt3 => L3Chm13Alt3,
                            PulseAlternative.Alt4 => L3Chm13Alt4,
                            PulseAlternative.Alt5 => L3Chm13Alt5,
                            PulseAlternative.Alt6 => L3Chm13Alt6,
                            PulseAlternative.Alt7 => L3Chm13Alt7,
                            PulseAlternative.Alt8 => L3Chm13Alt8,
                            PulseAlternative.Alt9 => L3Chm13Alt9,
                            PulseAlternative.Alt10 => L3Chm13Alt10,
                            PulseAlternative.Alt11 => L3Chm13Alt11,
                            PulseAlternative.Alt12 => L3Chm13Alt12,
                            PulseAlternative.Alt13 => L3Chm13Alt13,
                            PulseAlternative.Alt14 => L3Chm13Alt14,
                            _ => null
                        },
                        11 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm11Default,
                            PulseAlternative.Alt1 => L3Chm11Alt1,
                            PulseAlternative.Alt2 => L3Chm11Alt2,
                            PulseAlternative.Alt3 => L3Chm11Alt3,
                            PulseAlternative.Alt4 => L3Chm11Alt4,
                            PulseAlternative.Alt5 => L3Chm11Alt5,
                            PulseAlternative.Alt6 => L3Chm11Alt6,
                            PulseAlternative.Alt7 => L3Chm11Alt7,
                            PulseAlternative.Alt8 => L3Chm11Alt8,
                            PulseAlternative.Alt9 => L3Chm11Alt9,
                            PulseAlternative.Alt10 => L3Chm11Alt10,
                            _ => null
                        },
                        9 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm9Default,
                            PulseAlternative.Alt1 => L3Chm9Alt1,
                            PulseAlternative.Alt2 => L3Chm9Alt2,
                            PulseAlternative.Alt3 => L3Chm9Alt3,
                            PulseAlternative.Alt4 => L3Chm9Alt4,
                            PulseAlternative.Alt5 => L3Chm9Alt5,
                            PulseAlternative.Alt6 => L3Chm9Alt6,
                            PulseAlternative.Alt7 => L3Chm9Alt7,
                            _ => null
                        },
                        7 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm7Default,
                            PulseAlternative.Alt1 => L3Chm7Alt1,
                            PulseAlternative.Alt2 => L3Chm7Alt2,
                            PulseAlternative.Alt3 => L3Chm7Alt3,
                            PulseAlternative.Alt4 => L3Chm7Alt4,
                            PulseAlternative.Alt5 => L3Chm7Alt5,
                            PulseAlternative.Alt6 => L3Chm7Alt6,
                            _ => null
                        },
                        5 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm5Default,
                            PulseAlternative.Alt1 => L3Chm5Alt1,
                            PulseAlternative.Alt2 => L3Chm5Alt2,
                            PulseAlternative.Alt3 => L3Chm5Alt3,
                            PulseAlternative.Alt4 => L3Chm5Alt4,
                            _ => null
                        },
                        3 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm3Default,
                            PulseAlternative.Alt1 => L3Chm3Alt1,
                            PulseAlternative.Alt2 => L3Chm3Alt2,
                            _ => null
                        },
                        1 => Alternative switch
                        {
                            PulseAlternative.Default => L3Chm1Default,
                            _ => null
                        },
                        _ => null,
                    },
                    _ => null,
                },
                _ => null,
            };
        }

        private static CustomPwm L2Chm3Default { get; set; } = new();
        private static CustomPwm L2Chm3Alt1 { get; set; } = new();
        private static CustomPwm L2Chm3Alt2 { get; set; } = new();

        private static CustomPwm L2Chm5Default { get; set; } = new();
        private static CustomPwm L2Chm5Alt1 { get; set; } = new();
        private static CustomPwm L2Chm5Alt2 { get; set; } = new();
        private static CustomPwm L2Chm5Alt3 { get; set; } = new();

        private static CustomPwm L2Chm7Default { get; set; } = new();
        private static CustomPwm L2Chm7Alt1 { get; set; } = new();
        private static CustomPwm L2Chm7Alt2 { get; set; } = new();
        private static CustomPwm L2Chm7Alt3 { get; set; } = new();
        private static CustomPwm L2Chm7Alt4 { get; set; } = new();
        private static CustomPwm L2Chm7Alt5 { get; set; } = new();

        private static CustomPwm L2Chm9Default { get; set; } = new();
        private static CustomPwm L2Chm9Alt1 { get; set; } = new();
        private static CustomPwm L2Chm9Alt2 { get; set; } = new();
        private static CustomPwm L2Chm9Alt3 { get; set; } = new();
        private static CustomPwm L2Chm9Alt4 { get; set; } = new();
        private static CustomPwm L2Chm9Alt5 { get; set; } = new();
        private static CustomPwm L2Chm9Alt6 { get; set; } = new();
        private static CustomPwm L2Chm9Alt7 { get; set; } = new();
        private static CustomPwm L2Chm9Alt8 { get; set; } = new();

        private static CustomPwm L2Chm11Default { get; set; } = new();
        private static CustomPwm L2Chm11Alt1 { get; set; } = new();
        private static CustomPwm L2Chm11Alt2 { get; set; } = new();
        private static CustomPwm L2Chm11Alt3 { get; set; } = new();
        private static CustomPwm L2Chm11Alt4 { get; set; } = new();
        private static CustomPwm L2Chm11Alt5 { get; set; } = new();
        private static CustomPwm L2Chm11Alt6 { get; set; } = new();
        private static CustomPwm L2Chm11Alt7 { get; set; } = new();
        private static CustomPwm L2Chm11Alt8 { get; set; } = new();
        private static CustomPwm L2Chm11Alt9 { get; set; } = new();
        private static CustomPwm L2Chm11Alt10 { get; set; } = new();
        private static CustomPwm L2Chm11Alt11 { get; set; } = new();

        private static CustomPwm L2Chm13Default { get; set; } = new();
        private static CustomPwm L2Chm13Alt1 { get; set; } = new();
        private static CustomPwm L2Chm13Alt2 { get; set; } = new();
        private static CustomPwm L2Chm13Alt3 { get; set; } = new();
        private static CustomPwm L2Chm13Alt4 { get; set; } = new();
        private static CustomPwm L2Chm13Alt5 { get; set; } = new();
        private static CustomPwm L2Chm13Alt6 { get; set; } = new();
        private static CustomPwm L2Chm13Alt7 { get; set; } = new();
        private static CustomPwm L2Chm13Alt8 { get; set; } = new();
        private static CustomPwm L2Chm13Alt9 { get; set; } = new();
        private static CustomPwm L2Chm13Alt10 { get; set; } = new();
        private static CustomPwm L2Chm13Alt11 { get; set; } = new();
        private static CustomPwm L2Chm13Alt12 { get; set; } = new();
        private static CustomPwm L2Chm13Alt13 { get; set; } = new();

        private static CustomPwm L2Chm15Default { get; set; } = new();
        private static CustomPwm L2Chm15Alt1 { get; set; } = new();
        private static CustomPwm L2Chm15Alt2 { get; set; } = new();
        private static CustomPwm L2Chm15Alt3 { get; set; } = new();
        private static CustomPwm L2Chm15Alt4 { get; set; } = new();
        private static CustomPwm L2Chm15Alt5 { get; set; } = new();
        private static CustomPwm L2Chm15Alt6 { get; set; } = new();
        private static CustomPwm L2Chm15Alt7 { get; set; } = new();
        private static CustomPwm L2Chm15Alt8 { get; set; } = new();
        private static CustomPwm L2Chm15Alt9 { get; set; } = new();
        private static CustomPwm L2Chm15Alt10 { get; set; } = new();
        private static CustomPwm L2Chm15Alt11 { get; set; } = new();
        private static CustomPwm L2Chm15Alt12 { get; set; } = new();
        private static CustomPwm L2Chm15Alt13 { get; set; } = new();
        private static CustomPwm L2Chm15Alt14 { get; set; } = new();
        private static CustomPwm L2Chm15Alt15 { get; set; } = new();
        private static CustomPwm L2Chm15Alt16 { get; set; } = new();
        private static CustomPwm L2Chm15Alt17 { get; set; } = new();
        private static CustomPwm L2Chm15Alt18 { get; set; } = new();
        private static CustomPwm L2Chm15Alt19 { get; set; } = new();
        private static CustomPwm L2Chm15Alt20 { get; set; } = new();
        private static CustomPwm L2Chm15Alt21 { get; set; } = new();
        private static CustomPwm L2Chm15Alt22 { get; set; } = new();
        private static CustomPwm L2Chm15Alt23 { get; set; } = new();

        private static CustomPwm L2Chm17Default { get; set; } = new();
        private static CustomPwm L2Chm17Alt1 { get; set; } = new();
        private static CustomPwm L2Chm17Alt2 { get; set; } = new();
        private static CustomPwm L2Chm17Alt3 { get; set; } = new();
        private static CustomPwm L2Chm17Alt4 { get; set; } = new();
        private static CustomPwm L2Chm17Alt5 { get; set; } = new();
        private static CustomPwm L2Chm17Alt6 { get; set; } = new();
        private static CustomPwm L2Chm17Alt7 { get; set; } = new();
        private static CustomPwm L2Chm17Alt8 { get; set; } = new();
        private static CustomPwm L2Chm17Alt9 { get; set; } = new();
        private static CustomPwm L2Chm17Alt10 { get; set; } = new();
        private static CustomPwm L2Chm17Alt11 { get; set; } = new();

        private static CustomPwm L2Chm19Default { get; set; } = new();
        private static CustomPwm L2Chm19Alt1 { get; set; } = new();
        private static CustomPwm L2Chm19Alt2 { get; set; } = new();
        private static CustomPwm L2Chm19Alt3 { get; set; } = new();
        private static CustomPwm L2Chm19Alt4 { get; set; } = new();
        private static CustomPwm L2Chm19Alt5 { get; set; } = new();
        private static CustomPwm L2Chm19Alt6 { get; set; } = new();
        private static CustomPwm L2Chm19Alt7 { get; set; } = new();
        private static CustomPwm L2Chm19Alt8 { get; set; } = new();
        private static CustomPwm L2Chm19Alt9 { get; set; } = new();
        private static CustomPwm L2Chm19Alt10 { get; set; } = new();
        private static CustomPwm L2Chm19Alt11 { get; set; } = new();

        private static CustomPwm L2Chm21Default { get; set; } = new();
        private static CustomPwm L2Chm21Alt1 { get; set; } = new();
        private static CustomPwm L2Chm21Alt2 { get; set; } = new();
        private static CustomPwm L2Chm21Alt3 { get; set; } = new();
        private static CustomPwm L2Chm21Alt4 { get; set; } = new();
        private static CustomPwm L2Chm21Alt5 { get; set; } = new();
        private static CustomPwm L2Chm21Alt6 { get; set; } = new();
        private static CustomPwm L2Chm21Alt7 { get; set; } = new();
        private static CustomPwm L2Chm21Alt8 { get; set; } = new();
        private static CustomPwm L2Chm21Alt9 { get; set; } = new();
        private static CustomPwm L2Chm21Alt10 { get; set; } = new();
        private static CustomPwm L2Chm21Alt11 { get; set; } = new();
        private static CustomPwm L2Chm21Alt12 { get; set; } = new();
        private static CustomPwm L2Chm21Alt13 { get; set; } = new();

        private static CustomPwm L2Chm23Default { get; set; } = new();
        private static CustomPwm L2Chm23Alt1 { get; set; } = new();
        private static CustomPwm L2Chm23Alt2 { get; set; } = new();
        private static CustomPwm L2Chm23Alt3 { get; set; } = new();
        private static CustomPwm L2Chm23Alt4 { get; set; } = new();
        private static CustomPwm L2Chm23Alt5 { get; set; } = new();
        private static CustomPwm L2Chm23Alt6 { get; set; } = new();
        private static CustomPwm L2Chm23Alt7 { get; set; } = new();
        private static CustomPwm L2Chm23Alt8 { get; set; } = new();
        private static CustomPwm L2Chm23Alt9 { get; set; } = new();
        private static CustomPwm L2Chm23Alt10 { get; set; } = new();
        private static CustomPwm L2Chm23Alt11 { get; set; } = new();
        private static CustomPwm L2Chm23Alt12 { get; set; } = new();
        private static CustomPwm L2Chm23Alt13 { get; set; } = new();
        private static CustomPwm L2Chm23Alt14 { get; set; } = new();

        private static CustomPwm L2Chm25Default { get; set; } = new();
        private static CustomPwm L2Chm25Alt1 { get; set; } = new();
        private static CustomPwm L2Chm25Alt2 { get; set; } = new();
        private static CustomPwm L2Chm25Alt3 { get; set; } = new();
        private static CustomPwm L2Chm25Alt4 { get; set; } = new();
        private static CustomPwm L2Chm25Alt5 { get; set; } = new();
        private static CustomPwm L2Chm25Alt6 { get; set; } = new();
        private static CustomPwm L2Chm25Alt7 { get; set; } = new();
        private static CustomPwm L2Chm25Alt8 { get; set; } = new();
        private static CustomPwm L2Chm25Alt9 { get; set; } = new();
        private static CustomPwm L2Chm25Alt10 { get; set; } = new();
        private static CustomPwm L2Chm25Alt11 { get; set; } = new();
        private static CustomPwm L2Chm25Alt12 { get; set; } = new();
        private static CustomPwm L2Chm25Alt13 { get; set; } = new();
        private static CustomPwm L2Chm25Alt14 { get; set; } = new();
        private static CustomPwm L2Chm25Alt15 { get; set; } = new();
        private static CustomPwm L2Chm25Alt16 { get; set; } = new();
        private static CustomPwm L2Chm25Alt17 { get; set; } = new();
        private static CustomPwm L2Chm25Alt18 { get; set; } = new();
        private static CustomPwm L2Chm25Alt19 { get; set; } = new();
        private static CustomPwm L2Chm25Alt20 { get; set; } = new();

        private static CustomPwm L3Chm1Default { get; set; } = new();

        private static CustomPwm L3Chm3Default { get; set; } = new();
        private static CustomPwm L3Chm3Alt1 { get; set; } = new();
        private static CustomPwm L3Chm3Alt2 { get; set; } = new();

        private static CustomPwm L3Chm5Default { get; set; } = new();
        private static CustomPwm L3Chm5Alt1 { get; set; } = new();
        private static CustomPwm L3Chm5Alt2 { get; set; } = new();
        private static CustomPwm L3Chm5Alt3 { get; set; } = new();
        private static CustomPwm L3Chm5Alt4 { get; set; } = new();

        private static CustomPwm L3Chm7Default { get; set; } = new();
        private static CustomPwm L3Chm7Alt1 { get; set; } = new();
        private static CustomPwm L3Chm7Alt2 { get; set; } = new();
        private static CustomPwm L3Chm7Alt3 { get; set; } = new();
        private static CustomPwm L3Chm7Alt4 { get; set; } = new();
        private static CustomPwm L3Chm7Alt5 { get; set; } = new();
        private static CustomPwm L3Chm7Alt6 { get; set; } = new();

        private static CustomPwm L3Chm9Default { get; set; } = new();
        private static CustomPwm L3Chm9Alt1 { get; set; } = new();
        private static CustomPwm L3Chm9Alt2 { get; set; } = new();
        private static CustomPwm L3Chm9Alt3 { get; set; } = new();
        private static CustomPwm L3Chm9Alt4 { get; set; } = new();
        private static CustomPwm L3Chm9Alt5 { get; set; } = new();
        private static CustomPwm L3Chm9Alt6 { get; set; } = new();
        private static CustomPwm L3Chm9Alt7 { get; set; } = new();

        private static CustomPwm L3Chm11Default { get; set; } = new();
        private static CustomPwm L3Chm11Alt1 { get; set; } = new();
        private static CustomPwm L3Chm11Alt2 { get; set; } = new();
        private static CustomPwm L3Chm11Alt3 { get; set; } = new();
        private static CustomPwm L3Chm11Alt4 { get; set; } = new();
        private static CustomPwm L3Chm11Alt5 { get; set; } = new();
        private static CustomPwm L3Chm11Alt6 { get; set; } = new();
        private static CustomPwm L3Chm11Alt7 { get; set; } = new();
        private static CustomPwm L3Chm11Alt8 { get; set; } = new();
        private static CustomPwm L3Chm11Alt9 { get; set; } = new();
        private static CustomPwm L3Chm11Alt10 { get; set; } = new();

        private static CustomPwm L3Chm13Default { get; set; } = new();
        private static CustomPwm L3Chm13Alt1 { get; set; } = new();
        private static CustomPwm L3Chm13Alt2 { get; set; } = new();
        private static CustomPwm L3Chm13Alt3 { get; set; } = new();
        private static CustomPwm L3Chm13Alt4 { get; set; } = new();
        private static CustomPwm L3Chm13Alt5 { get; set; } = new();
        private static CustomPwm L3Chm13Alt6 { get; set; } = new();
        private static CustomPwm L3Chm13Alt7 { get; set; } = new();
        private static CustomPwm L3Chm13Alt8 { get; set; } = new();
        private static CustomPwm L3Chm13Alt9 { get; set; } = new();
        private static CustomPwm L3Chm13Alt10 { get; set; } = new();
        private static CustomPwm L3Chm13Alt11 { get; set; } = new();
        private static CustomPwm L3Chm13Alt12 { get; set; } = new();
        private static CustomPwm L3Chm13Alt13 { get; set; } = new();
        private static CustomPwm L3Chm13Alt14 { get; set; } = new();

        private static CustomPwm L3Chm15Default { get; set; } = new();
        private static CustomPwm L3Chm15Alt1 { get; set; } = new();
        private static CustomPwm L3Chm15Alt2 { get; set; } = new();
        private static CustomPwm L3Chm15Alt3 { get; set; } = new();
        private static CustomPwm L3Chm15Alt4 { get; set; } = new();
        private static CustomPwm L3Chm15Alt5 { get; set; } = new();
        private static CustomPwm L3Chm15Alt6 { get; set; } = new();
        private static CustomPwm L3Chm15Alt7 { get; set; } = new();
        private static CustomPwm L3Chm15Alt8 { get; set; } = new();
        private static CustomPwm L3Chm15Alt9 { get; set; } = new();
        private static CustomPwm L3Chm15Alt10 { get; set; } = new();
        private static CustomPwm L3Chm15Alt11 { get; set; } = new();
        private static CustomPwm L3Chm15Alt12 { get; set; } = new();
        private static CustomPwm L3Chm15Alt13 { get; set; } = new();
        private static CustomPwm L3Chm15Alt14 { get; set; } = new();
        private static CustomPwm L3Chm15Alt15 { get; set; } = new();
        private static CustomPwm L3Chm15Alt16 { get; set; } = new();
        private static CustomPwm L3Chm15Alt17 { get; set; } = new();

        private static CustomPwm L3Chm17Default { get; set; } = new();
        private static CustomPwm L3Chm17Alt1 { get; set; } = new();
        private static CustomPwm L3Chm17Alt2 { get; set; } = new();
        private static CustomPwm L3Chm17Alt3 { get; set; } = new();
        private static CustomPwm L3Chm17Alt4 { get; set; } = new();
        private static CustomPwm L3Chm17Alt5 { get; set; } = new();
        private static CustomPwm L3Chm17Alt6 { get; set; } = new();
        private static CustomPwm L3Chm17Alt7 { get; set; } = new();
        private static CustomPwm L3Chm17Alt8 { get; set; } = new();
        private static CustomPwm L3Chm17Alt9 { get; set; } = new();
        private static CustomPwm L3Chm17Alt10 { get; set; } = new();
        private static CustomPwm L3Chm17Alt11 { get; set; } = new();
        private static CustomPwm L3Chm17Alt12 { get; set; } = new();
        private static CustomPwm L3Chm17Alt13 { get; set; } = new();
        private static CustomPwm L3Chm17Alt14 { get; set; } = new();
        private static CustomPwm L3Chm17Alt15 { get; set; } = new();
        private static CustomPwm L3Chm17Alt16 { get; set; } = new();
        private static CustomPwm L3Chm17Alt17 { get; set; } = new();
        private static CustomPwm L3Chm17Alt18 { get; set; } = new();
        private static CustomPwm L3Chm17Alt19 { get; set; } = new();

        private static CustomPwm L3Chm19Default { get; set; } = new();
        private static CustomPwm L3Chm19Alt1 { get; set; } = new();
        private static CustomPwm L3Chm19Alt2 { get; set; } = new();
        private static CustomPwm L3Chm19Alt3 { get; set; } = new();
        private static CustomPwm L3Chm19Alt4 { get; set; } = new();
        private static CustomPwm L3Chm19Alt5 { get; set; } = new();
        private static CustomPwm L3Chm19Alt6 { get; set; } = new();
        private static CustomPwm L3Chm19Alt7 { get; set; } = new();
        private static CustomPwm L3Chm19Alt8 { get; set; } = new();
        private static CustomPwm L3Chm19Alt9 { get; set; } = new();
        private static CustomPwm L3Chm19Alt10 { get; set; } = new();
        private static CustomPwm L3Chm19Alt11 { get; set; } = new();
        private static CustomPwm L3Chm19Alt12 { get; set; } = new();
        private static CustomPwm L3Chm19Alt13 { get; set; } = new();
        private static CustomPwm L3Chm19Alt14 { get; set; } = new();
        private static CustomPwm L3Chm19Alt15 { get; set; } = new();
        private static CustomPwm L3Chm19Alt16 { get; set; } = new();
        private static CustomPwm L3Chm19Alt17 { get; set; } = new();
        private static CustomPwm L3Chm19Alt18 { get; set; } = new();
        private static CustomPwm L3Chm19Alt19 { get; set; } = new();
        private static CustomPwm L3Chm19Alt20 { get; set; } = new();
        private static CustomPwm L3Chm19Alt21 { get; set; } = new();
        private static CustomPwm L3Chm19Alt22 { get; set; } = new();
        private static CustomPwm L3Chm19Alt23 { get; set; } = new();
        private static CustomPwm L3Chm19Alt24 { get; set; } = new();
        private static CustomPwm L3Chm19Alt25 { get; set; } = new();

        private static CustomPwm L3Chm21Default { get; set; } = new();
        private static CustomPwm L3Chm21Alt1 { get; set; } = new();
        private static CustomPwm L3Chm21Alt2 { get; set; } = new();
        private static CustomPwm L3Chm21Alt3 { get; set; } = new();
        private static CustomPwm L3Chm21Alt4 { get; set; } = new();
        private static CustomPwm L3Chm21Alt5 { get; set; } = new();
        private static CustomPwm L3Chm21Alt6 { get; set; } = new();
        private static CustomPwm L3Chm21Alt7 { get; set; } = new();
        private static CustomPwm L3Chm21Alt8 { get; set; } = new();
        private static CustomPwm L3Chm21Alt9 { get; set; } = new();
        private static CustomPwm L3Chm21Alt10 { get; set; } = new();
        private static CustomPwm L3Chm21Alt11 { get; set; } = new();
        private static CustomPwm L3Chm21Alt12 { get; set; } = new();
        private static CustomPwm L3Chm21Alt13 { get; set; } = new();
        private static CustomPwm L3Chm21Alt14 { get; set; } = new();
        private static CustomPwm L3Chm21Alt15 { get; set; } = new();
        private static CustomPwm L3Chm21Alt16 { get; set; } = new();
        private static CustomPwm L3Chm21Alt17 { get; set; } = new();
        private static CustomPwm L3Chm21Alt18 { get; set; } = new();
        private static CustomPwm L3Chm21Alt19 { get; set; } = new();
        private static CustomPwm L3Chm21Alt20 { get; set; } = new();
        private static CustomPwm L3Chm21Alt21 { get; set; } = new();
        private static CustomPwm L3Chm21Alt22 { get; set; } = new();

        private static CustomPwm L2She3Default { get; set; } = new();
        private static CustomPwm L2She3Alt1 { get; set; } = new();
        private static CustomPwm L2She5Default { get; set; } = new();
        private static CustomPwm L2She5Alt1 { get; set; } = new();
        private static CustomPwm L2She5Alt2 { get; set; } = new();
        private static CustomPwm L2She7Default { get; set; } = new();
        private static CustomPwm L2She7Alt1 { get; set; } = new();
        private static CustomPwm L2She9Default { get; set; } = new();
        private static CustomPwm L2She9Alt1 { get; set; } = new();
        private static CustomPwm L2She9Alt2 { get; set; } = new();
        private static CustomPwm L2She9Alt3 { get; set; } = new();
        private static CustomPwm L2She11Default { get; set; } = new();
        private static CustomPwm L2She11Alt1 { get; set; } = new();
        private static CustomPwm L2She11Alt2 { get; set; } = new();
        private static CustomPwm L2She11Alt3 { get; set; } = new();
        private static CustomPwm L2She13Default { get; set; } = new();
        private static CustomPwm L2She13Alt1 { get; set; } = new();
        private static CustomPwm L2She13Alt2 { get; set; } = new();
        private static CustomPwm L2She13Alt3 { get; set; } = new();
        private static CustomPwm L2She15Default { get; set; } = new();
        private static CustomPwm L2She15Alt1 { get; set; } = new();
        private static CustomPwm L2She15Alt2 { get; set; } = new();
        private static CustomPwm L2She15Alt3 { get; set; } = new();
        private static CustomPwm L2She17Default { get; set; } = new();
        private static CustomPwm L2She17Alt1 { get; set; } = new();
        private static CustomPwm L2She17Alt2 { get; set; } = new();
        private static CustomPwm L2She17Alt3 { get; set; } = new();
        private static CustomPwm L2She17Alt4 { get; set; } = new();
        private static CustomPwm L2She17Alt5 { get; set; } = new();
        private static CustomPwm L2She17Alt6 { get; set; } = new();
        private static CustomPwm L2She19Default { get; set; } = new();
        private static CustomPwm L2She19Alt1 { get; set; } = new();
        private static CustomPwm L2She19Alt2 { get; set; } = new();
        private static CustomPwm L2She19Alt3 { get; set; } = new();
        private static CustomPwm L2She19Alt4 { get; set; } = new();
        private static CustomPwm L2She19Alt5 { get; set; } = new();
        private static CustomPwm L2She19Alt6 { get; set; } = new();
        private static CustomPwm L2She21Default { get; set; } = new();
        private static CustomPwm L2She21Alt1 { get; set; } = new();
        private static CustomPwm L2She21Alt2 { get; set; } = new();
        private static CustomPwm L2She21Alt3 { get; set; } = new();
        private static CustomPwm L2She21Alt4 { get; set; } = new();
        private static CustomPwm L2She21Alt5 { get; set; } = new();
        private static CustomPwm L2She21Alt6 { get; set; } = new();

        private static CustomPwm L3She1Default { get; set; } = new();
        private static CustomPwm L3She3Default { get; set; } = new();
        private static CustomPwm L3She3Alt1 { get; set; } = new();
        private static CustomPwm L3She5Default { get; set; } = new();
        private static CustomPwm L3She7Default { get; set; } = new();
        private static CustomPwm L3She9Default { get; set; } = new();
        private static CustomPwm L3She11Default { get; set; } = new();
        private static CustomPwm L3She13Default { get; set; } = new();
        private static CustomPwm L3She15Default { get; set; } = new();
        private static CustomPwm L3She17Default { get; set; } = new();
        private static CustomPwm L3She19Default { get; set; } = new();
        private static CustomPwm L3She21Default { get; set; } = new();

    }
}
