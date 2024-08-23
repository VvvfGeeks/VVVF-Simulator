using System;
using System.IO;
using System.Reflection;

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
            if (Index >= BlockCount) Index = (int)BlockCount - 1;
            else if(Index < 0) Index = 0;

            X %= MyMath.M_2PI;
            int Orthant = (int)(X / MyMath.M_PI_2);
            double Angle = X % MyMath.M_PI_2;

            if ((Orthant & 0x01) == 1)
                Angle = MyMath.M_PI_2 - Angle;

            int Pwm = 0;
            bool Inverted = Polarity[Index];

            for (int i = 0; i < SwitchCount; i++)
            {
                (double SwitchAngle, int Output) = SwitchAngleTable[Index * SwitchCount + i];
                if (SwitchAngle <= Angle) Pwm = Output;
                else break;
            }

            if (Orthant > 1)
                Pwm = MaxPwmLevel - Pwm;

            if(Inverted)
                Pwm = MaxPwmLevel - Pwm;

            return Pwm;
        }

        /// <summary>
        /// Custom Pwm Instances with Preset
        /// </summary>
        public static readonly CustomPwm L2Chm3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm3Default.bin"));
        public static readonly CustomPwm L2Chm3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm3Alt1.bin"));

        public static readonly CustomPwm L2Chm5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Default.bin"));
        public static readonly CustomPwm L2Chm5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt1.bin"));
        public static readonly CustomPwm L2Chm5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt2.bin"));
        public static readonly CustomPwm L2Chm5Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm5Alt3.bin"));

        public static readonly CustomPwm L2Chm7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Default.bin"));
        public static readonly CustomPwm L2Chm7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt1.bin"));
        public static readonly CustomPwm L2Chm7Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt2.bin"));
        public static readonly CustomPwm L2Chm7Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt3.bin"));
        public static readonly CustomPwm L2Chm7Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt4.bin"));
        public static readonly CustomPwm L2Chm7Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm7Alt5.bin"));

        public static readonly CustomPwm L2Chm9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Default.bin"));
        public static readonly CustomPwm L2Chm9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt1.bin"));
        public static readonly CustomPwm L2Chm9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt2.bin"));
        public static readonly CustomPwm L2Chm9Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt3.bin"));
        public static readonly CustomPwm L2Chm9Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt4.bin"));
        public static readonly CustomPwm L2Chm9Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt5.bin"));
        public static readonly CustomPwm L2Chm9Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt6.bin"));
        public static readonly CustomPwm L2Chm9Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt7.bin"));
        public static readonly CustomPwm L2Chm9Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm9Alt8.bin"));

        public static readonly CustomPwm L2Chm11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Default.bin"));
        public static readonly CustomPwm L2Chm11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt1.bin"));
        public static readonly CustomPwm L2Chm11Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt2.bin"));
        public static readonly CustomPwm L2Chm11Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt3.bin"));
        public static readonly CustomPwm L2Chm11Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt4.bin"));
        public static readonly CustomPwm L2Chm11Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt5.bin"));
        public static readonly CustomPwm L2Chm11Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt6.bin"));
        public static readonly CustomPwm L2Chm11Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt7.bin"));
        public static readonly CustomPwm L2Chm11Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt8.bin"));
        public static readonly CustomPwm L2Chm11Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt9.bin"));
        public static readonly CustomPwm L2Chm11Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt10.bin"));
        public static readonly CustomPwm L2Chm11Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm11Alt11.bin"));

        public static readonly CustomPwm L2Chm13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Default.bin"));
        public static readonly CustomPwm L2Chm13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt1.bin"));
        public static readonly CustomPwm L2Chm13Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt2.bin"));
        public static readonly CustomPwm L2Chm13Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt3.bin"));
        public static readonly CustomPwm L2Chm13Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt4.bin"));
        public static readonly CustomPwm L2Chm13Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt5.bin"));
        public static readonly CustomPwm L2Chm13Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt6.bin"));
        public static readonly CustomPwm L2Chm13Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt7.bin"));
        public static readonly CustomPwm L2Chm13Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt8.bin"));
        public static readonly CustomPwm L2Chm13Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt9.bin"));
        public static readonly CustomPwm L2Chm13Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt10.bin"));
        public static readonly CustomPwm L2Chm13Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt11.bin"));
        public static readonly CustomPwm L2Chm13Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt12.bin"));
        public static readonly CustomPwm L2Chm13Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm13Alt13.bin"));

        public static readonly CustomPwm L2Chm15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Default.bin"));
        public static readonly CustomPwm L2Chm15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt1.bin"));
        public static readonly CustomPwm L2Chm15Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt2.bin"));
        public static readonly CustomPwm L2Chm15Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt3.bin"));
        public static readonly CustomPwm L2Chm15Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt4.bin"));
        public static readonly CustomPwm L2Chm15Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt5.bin"));
        public static readonly CustomPwm L2Chm15Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt6.bin"));
        public static readonly CustomPwm L2Chm15Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt7.bin"));
        public static readonly CustomPwm L2Chm15Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt8.bin"));
        public static readonly CustomPwm L2Chm15Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt9.bin"));
        public static readonly CustomPwm L2Chm15Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt10.bin"));
        public static readonly CustomPwm L2Chm15Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt11.bin"));
        public static readonly CustomPwm L2Chm15Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt12.bin"));
        public static readonly CustomPwm L2Chm15Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt13.bin"));
        public static readonly CustomPwm L2Chm15Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt14.bin"));
        public static readonly CustomPwm L2Chm15Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt15.bin"));
        public static readonly CustomPwm L2Chm15Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt16.bin"));
        public static readonly CustomPwm L2Chm15Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt17.bin"));
        public static readonly CustomPwm L2Chm15Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt18.bin"));
        public static readonly CustomPwm L2Chm15Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt19.bin"));
        public static readonly CustomPwm L2Chm15Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt20.bin"));
        public static readonly CustomPwm L2Chm15Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt21.bin"));
        public static readonly CustomPwm L2Chm15Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt22.bin"));
        public static readonly CustomPwm L2Chm15Alt23 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm15Alt23.bin"));

        public static readonly CustomPwm L2Chm17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Default.bin"));
        public static readonly CustomPwm L2Chm17Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt1.bin"));
        public static readonly CustomPwm L2Chm17Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt2.bin"));
        public static readonly CustomPwm L2Chm17Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt3.bin"));
        public static readonly CustomPwm L2Chm17Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt4.bin"));
        public static readonly CustomPwm L2Chm17Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt5.bin"));
        public static readonly CustomPwm L2Chm17Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt6.bin"));
        public static readonly CustomPwm L2Chm17Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt7.bin"));
        public static readonly CustomPwm L2Chm17Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt8.bin"));
        public static readonly CustomPwm L2Chm17Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt9.bin"));
        public static readonly CustomPwm L2Chm17Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt10.bin"));
        public static readonly CustomPwm L2Chm17Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm17Alt11.bin"));

        public static readonly CustomPwm L2Chm19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Default.bin"));
        public static readonly CustomPwm L2Chm19Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt1.bin"));
        public static readonly CustomPwm L2Chm19Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt2.bin"));
        public static readonly CustomPwm L2Chm19Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt3.bin"));
        public static readonly CustomPwm L2Chm19Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt4.bin"));
        public static readonly CustomPwm L2Chm19Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt5.bin"));
        public static readonly CustomPwm L2Chm19Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt6.bin"));
        public static readonly CustomPwm L2Chm19Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt7.bin"));
        public static readonly CustomPwm L2Chm19Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt8.bin"));
        public static readonly CustomPwm L2Chm19Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt9.bin"));
        public static readonly CustomPwm L2Chm19Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt10.bin"));
        public static readonly CustomPwm L2Chm19Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm19Alt11.bin"));

        public static readonly CustomPwm L2Chm21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Default.bin"));
        public static readonly CustomPwm L2Chm21Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt1.bin"));
        public static readonly CustomPwm L2Chm21Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt2.bin"));
        public static readonly CustomPwm L2Chm21Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt3.bin"));
        public static readonly CustomPwm L2Chm21Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt4.bin"));
        public static readonly CustomPwm L2Chm21Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt5.bin"));
        public static readonly CustomPwm L2Chm21Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt6.bin"));
        public static readonly CustomPwm L2Chm21Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt7.bin"));
        public static readonly CustomPwm L2Chm21Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt8.bin"));
        public static readonly CustomPwm L2Chm21Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt9.bin"));
        public static readonly CustomPwm L2Chm21Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt10.bin"));
        public static readonly CustomPwm L2Chm21Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt11.bin"));
        public static readonly CustomPwm L2Chm21Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt12.bin"));
        public static readonly CustomPwm L2Chm21Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm21Alt13.bin"));

        public static readonly CustomPwm L2Chm23Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Default.bin"));
        public static readonly CustomPwm L2Chm23Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt1.bin"));
        public static readonly CustomPwm L2Chm23Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt2.bin"));
        public static readonly CustomPwm L2Chm23Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt3.bin"));
        public static readonly CustomPwm L2Chm23Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt4.bin"));
        public static readonly CustomPwm L2Chm23Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt5.bin"));
        public static readonly CustomPwm L2Chm23Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt6.bin"));
        public static readonly CustomPwm L2Chm23Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt7.bin"));
        public static readonly CustomPwm L2Chm23Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt8.bin"));
        public static readonly CustomPwm L2Chm23Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt9.bin"));
        public static readonly CustomPwm L2Chm23Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt10.bin"));
        public static readonly CustomPwm L2Chm23Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt11.bin"));
        public static readonly CustomPwm L2Chm23Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt12.bin"));
        public static readonly CustomPwm L2Chm23Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt13.bin"));
        public static readonly CustomPwm L2Chm23Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm23Alt14.bin"));

        public static readonly CustomPwm L2Chm25Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Default.bin"));
        public static readonly CustomPwm L2Chm25Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt1.bin"));
        public static readonly CustomPwm L2Chm25Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt2.bin"));
        public static readonly CustomPwm L2Chm25Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt3.bin"));
        public static readonly CustomPwm L2Chm25Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt4.bin"));
        public static readonly CustomPwm L2Chm25Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt5.bin"));
        public static readonly CustomPwm L2Chm25Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt6.bin"));
        public static readonly CustomPwm L2Chm25Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt7.bin"));
        public static readonly CustomPwm L2Chm25Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt8.bin"));
        public static readonly CustomPwm L2Chm25Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt9.bin"));
        public static readonly CustomPwm L2Chm25Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt10.bin"));
        public static readonly CustomPwm L2Chm25Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt11.bin"));
        public static readonly CustomPwm L2Chm25Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt12.bin"));
        public static readonly CustomPwm L2Chm25Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt13.bin"));
        public static readonly CustomPwm L2Chm25Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt14.bin"));
        public static readonly CustomPwm L2Chm25Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt15.bin"));
        public static readonly CustomPwm L2Chm25Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt16.bin"));
        public static readonly CustomPwm L2Chm25Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt17.bin"));
        public static readonly CustomPwm L2Chm25Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt18.bin"));
        public static readonly CustomPwm L2Chm25Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt19.bin"));
        public static readonly CustomPwm L2Chm25Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2Chm25Alt20.bin"));

        public static readonly CustomPwm L3Chm1Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm1Default.bin"));

        public static readonly CustomPwm L3Chm3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Default.bin"));
        public static readonly CustomPwm L3Chm3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Alt1.bin"));
        public static readonly CustomPwm L3Chm3Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm3Alt2.bin"));

        public static readonly CustomPwm L3Chm5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Default.bin"));
        public static readonly CustomPwm L3Chm5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt1.bin"));
        public static readonly CustomPwm L3Chm5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt2.bin"));
        public static readonly CustomPwm L3Chm5Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt3.bin"));
        public static readonly CustomPwm L3Chm5Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm5Alt4.bin"));

        public static readonly CustomPwm L3Chm7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Default.bin"));
        public static readonly CustomPwm L3Chm7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt1.bin"));
        public static readonly CustomPwm L3Chm7Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt2.bin"));
        public static readonly CustomPwm L3Chm7Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt3.bin"));
        public static readonly CustomPwm L3Chm7Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt4.bin"));
        public static readonly CustomPwm L3Chm7Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt5.bin"));
        public static readonly CustomPwm L3Chm7Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm7Alt6.bin"));

        public static readonly CustomPwm L3Chm9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Default.bin"));
        public static readonly CustomPwm L3Chm9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt1.bin"));
        public static readonly CustomPwm L3Chm9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt2.bin"));
        public static readonly CustomPwm L3Chm9Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt3.bin"));
        public static readonly CustomPwm L3Chm9Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt4.bin"));
        public static readonly CustomPwm L3Chm9Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt5.bin"));
        public static readonly CustomPwm L3Chm9Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt6.bin"));
        public static readonly CustomPwm L3Chm9Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm9Alt7.bin"));

        public static readonly CustomPwm L3Chm11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Default.bin"));
        public static readonly CustomPwm L3Chm11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt1.bin"));
        public static readonly CustomPwm L3Chm11Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt2.bin"));
        public static readonly CustomPwm L3Chm11Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt3.bin"));
        public static readonly CustomPwm L3Chm11Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt4.bin"));
        public static readonly CustomPwm L3Chm11Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt5.bin"));
        public static readonly CustomPwm L3Chm11Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt6.bin"));
        public static readonly CustomPwm L3Chm11Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt7.bin"));
        public static readonly CustomPwm L3Chm11Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt8.bin"));
        public static readonly CustomPwm L3Chm11Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt9.bin"));
        public static readonly CustomPwm L3Chm11Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm11Alt10.bin"));

        public static readonly CustomPwm L3Chm13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Default.bin"));
        public static readonly CustomPwm L3Chm13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt1.bin"));
        public static readonly CustomPwm L3Chm13Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt2.bin"));
        public static readonly CustomPwm L3Chm13Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt3.bin"));
        public static readonly CustomPwm L3Chm13Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt4.bin"));
        public static readonly CustomPwm L3Chm13Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt5.bin"));
        public static readonly CustomPwm L3Chm13Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt6.bin"));
        public static readonly CustomPwm L3Chm13Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt7.bin"));
        public static readonly CustomPwm L3Chm13Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt8.bin"));
        public static readonly CustomPwm L3Chm13Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt9.bin"));
        public static readonly CustomPwm L3Chm13Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt10.bin"));
        public static readonly CustomPwm L3Chm13Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt11.bin"));
        public static readonly CustomPwm L3Chm13Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt12.bin"));
        public static readonly CustomPwm L3Chm13Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt13.bin"));
        public static readonly CustomPwm L3Chm13Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm13Alt14.bin"));

        public static readonly CustomPwm L3Chm15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Default.bin"));
        public static readonly CustomPwm L3Chm15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt1.bin"));
        public static readonly CustomPwm L3Chm15Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt2.bin"));
        public static readonly CustomPwm L3Chm15Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt3.bin"));
        public static readonly CustomPwm L3Chm15Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt4.bin"));
        public static readonly CustomPwm L3Chm15Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt5.bin"));
        public static readonly CustomPwm L3Chm15Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt6.bin"));
        public static readonly CustomPwm L3Chm15Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt7.bin"));
        public static readonly CustomPwm L3Chm15Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt8.bin"));
        public static readonly CustomPwm L3Chm15Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt9.bin"));
        public static readonly CustomPwm L3Chm15Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt10.bin"));
        public static readonly CustomPwm L3Chm15Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt11.bin"));
        public static readonly CustomPwm L3Chm15Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt12.bin"));
        public static readonly CustomPwm L3Chm15Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt13.bin"));
        public static readonly CustomPwm L3Chm15Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt14.bin"));
        public static readonly CustomPwm L3Chm15Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt15.bin"));
        public static readonly CustomPwm L3Chm15Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt16.bin"));
        public static readonly CustomPwm L3Chm15Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm15Alt17.bin"));

        public static readonly CustomPwm L3Chm17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Default.bin"));
        public static readonly CustomPwm L3Chm17Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt1.bin"));
        public static readonly CustomPwm L3Chm17Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt2.bin"));
        public static readonly CustomPwm L3Chm17Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt3.bin"));
        public static readonly CustomPwm L3Chm17Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt4.bin"));
        public static readonly CustomPwm L3Chm17Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt5.bin"));
        public static readonly CustomPwm L3Chm17Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt6.bin"));
        public static readonly CustomPwm L3Chm17Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt7.bin"));
        public static readonly CustomPwm L3Chm17Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt8.bin"));
        public static readonly CustomPwm L3Chm17Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt9.bin"));
        public static readonly CustomPwm L3Chm17Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt10.bin"));
        public static readonly CustomPwm L3Chm17Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt11.bin"));
        public static readonly CustomPwm L3Chm17Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt12.bin"));
        public static readonly CustomPwm L3Chm17Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt13.bin"));
        public static readonly CustomPwm L3Chm17Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt14.bin"));
        public static readonly CustomPwm L3Chm17Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt15.bin"));
        public static readonly CustomPwm L3Chm17Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt16.bin"));
        public static readonly CustomPwm L3Chm17Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt17.bin"));
        public static readonly CustomPwm L3Chm17Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt18.bin"));
        public static readonly CustomPwm L3Chm17Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm17Alt19.bin"));

        public static readonly CustomPwm L3Chm19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Default.bin"));
        public static readonly CustomPwm L3Chm19Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt1.bin"));
        public static readonly CustomPwm L3Chm19Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt2.bin"));
        public static readonly CustomPwm L3Chm19Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt3.bin"));
        public static readonly CustomPwm L3Chm19Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt4.bin"));
        public static readonly CustomPwm L3Chm19Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt5.bin"));
        public static readonly CustomPwm L3Chm19Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt6.bin"));
        public static readonly CustomPwm L3Chm19Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt7.bin"));
        public static readonly CustomPwm L3Chm19Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt8.bin"));
        public static readonly CustomPwm L3Chm19Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt9.bin"));
        public static readonly CustomPwm L3Chm19Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt10.bin"));
        public static readonly CustomPwm L3Chm19Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt11.bin"));
        public static readonly CustomPwm L3Chm19Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt12.bin"));
        public static readonly CustomPwm L3Chm19Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt13.bin"));
        public static readonly CustomPwm L3Chm19Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt14.bin"));
        public static readonly CustomPwm L3Chm19Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt15.bin"));
        public static readonly CustomPwm L3Chm19Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt16.bin"));
        public static readonly CustomPwm L3Chm19Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt17.bin"));
        public static readonly CustomPwm L3Chm19Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt18.bin"));
        public static readonly CustomPwm L3Chm19Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt19.bin"));
        public static readonly CustomPwm L3Chm19Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt20.bin"));
        public static readonly CustomPwm L3Chm19Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt21.bin"));
        public static readonly CustomPwm L3Chm19Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt22.bin"));
        public static readonly CustomPwm L3Chm19Alt23 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt23.bin"));
        public static readonly CustomPwm L3Chm19Alt24 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt24.bin"));
        public static readonly CustomPwm L3Chm19Alt25 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm19Alt25.bin"));

        public static readonly CustomPwm L3Chm21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Default.bin"));
        public static readonly CustomPwm L3Chm21Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt1.bin"));
        public static readonly CustomPwm L3Chm21Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt2.bin"));
        public static readonly CustomPwm L3Chm21Alt3 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt3.bin"));
        public static readonly CustomPwm L3Chm21Alt4 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt4.bin"));
        public static readonly CustomPwm L3Chm21Alt5 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt5.bin"));
        public static readonly CustomPwm L3Chm21Alt6 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt6.bin"));
        public static readonly CustomPwm L3Chm21Alt7 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt7.bin"));
        public static readonly CustomPwm L3Chm21Alt8 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt8.bin"));
        public static readonly CustomPwm L3Chm21Alt9 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt9.bin"));
        public static readonly CustomPwm L3Chm21Alt10 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt10.bin"));
        public static readonly CustomPwm L3Chm21Alt11 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt11.bin"));
        public static readonly CustomPwm L3Chm21Alt12 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt12.bin"));
        public static readonly CustomPwm L3Chm21Alt13 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt13.bin"));
        public static readonly CustomPwm L3Chm21Alt14 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt14.bin"));
        public static readonly CustomPwm L3Chm21Alt15 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt15.bin"));
        public static readonly CustomPwm L3Chm21Alt16 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt16.bin"));
        public static readonly CustomPwm L3Chm21Alt17 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt17.bin"));
        public static readonly CustomPwm L3Chm21Alt18 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt18.bin"));
        public static readonly CustomPwm L3Chm21Alt19 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt19.bin"));
        public static readonly CustomPwm L3Chm21Alt20 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt20.bin"));
        public static readonly CustomPwm L3Chm21Alt21 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt21.bin"));
        public static readonly CustomPwm L3Chm21Alt22 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3Chm21Alt22.bin"));

        public static readonly CustomPwm L2She3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She3Default.bin"));
        public static readonly CustomPwm L2She3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She3Alt1.bin"));
        public static readonly CustomPwm L2She9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Default.bin"));
        public static readonly CustomPwm L2She9Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Alt1.bin"));
        public static readonly CustomPwm L2She9Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She9Alt2.bin"));
        public static readonly CustomPwm L2She7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She7Default.bin"));
        public static readonly CustomPwm L2She7Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She7Alt1.bin"));
        public static readonly CustomPwm L2She5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Default.bin"));
        public static readonly CustomPwm L2She5Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Alt1.bin"));
        public static readonly CustomPwm L2She5Alt2 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She5Alt2.bin"));
        public static readonly CustomPwm L2She11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She11Default.bin"));
        public static readonly CustomPwm L2She11Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She11Alt1.bin"));
        public static readonly CustomPwm L2She13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She13Default.bin"));
        public static readonly CustomPwm L2She13Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She13Alt1.bin"));
        public static readonly CustomPwm L2She15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She15Default.bin"));
        public static readonly CustomPwm L2She15Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L2She15Alt1.bin"));

        public static readonly CustomPwm L3She1Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She1Default.bin"));
        public static readonly CustomPwm L3She3Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She3Default.bin"));
        public static readonly CustomPwm L3She3Alt1 = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She3Alt1.bin"));
        public static readonly CustomPwm L3She5Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She5Default.bin"));
        public static readonly CustomPwm L3She7Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She7Default.bin"));
        public static readonly CustomPwm L3She9Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She9Default.bin"));
        public static readonly CustomPwm L3She11Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She11Default.bin"));
        public static readonly CustomPwm L3She13Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She13Default.bin"));
        public static readonly CustomPwm L3She15Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She15Default.bin"));
        public static readonly CustomPwm L3She17Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She17Default.bin"));
        public static readonly CustomPwm L3She19Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She19Default.bin"));
        public static readonly CustomPwm L3She21Default = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Vvvf.SwitchAngle.L3She21Default.bin"));
    }
}
