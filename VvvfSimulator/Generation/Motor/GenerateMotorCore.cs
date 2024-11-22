using System;
using static VvvfSimulator.Vvvf.MyMath;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Generation.Motor
{
    public class GenerateMotorCore
    {
        // The following motor models are from
        //《Research on some key technologies of high performance frequency converter for asynchronous motor》Wang siran, Zhejiang University

        public class MotorSpecification
        {
            public double R_s { get; set; } = 1.898; /*stator resistance(ohm)*/
            public double R_r { get; set; } = 1.45;  /*Rotor resistance(ohm)*/
            public double L_s { get; set; } = 0.196; /*Stator inductance(H)*/
            public double L_r { get; set; } = 0.196; /*Rotor inductance(H)*/
            public double L_m { get; set; } = 0.187; /*Mutual inductance(H)*/
            public double NP { get; set; } = 2;/* Number of pole pair */
            public double DAMPING { get; set; } = 500.0;/* damping */
            public double INERTIA { get; set; } = 0.05; /*Rotational inertia mass(kg.m^2)*/
            public double STATICF { get; set; } = 0.005879; /*Static friction(N.m.s)*/

            public MotorSpecification Clone()
            {
                return (MotorSpecification)MemberwiseClone();
            }
        }
        public class MotorParameter
        {
            // Inherent Parameters
            public double RateCurent { get; set; } = 16.5;
            public double[] Iabc { get; set; } = new double[3];
            public double[] Idq0 { get; set; } = new double[3];
            public double[] Uabc { get; set; } = new double[3];
            public double[] Udq0 { get; set; } = new double[3];
            public double wsl { get; set; } = 0;
            public double w_r { get; set; } = 0;

            // Response mechanical parameters
            public double sita_r { get; set; } = 0;
            public double w_mr { get; set; } = 0;
            public double sitamr { get; set; } = 0;
            public double TL { get; set; } = 1;
            public double Te { get; set; } = 0;
            public double r_Flux { get; set; } = 0;

            // Others
            public double i_m1 { get; set; } = 0;
            public double[] PreIdq0 { get; set; } = new double[3];
            public double[] DiffIdq0 { get; set; } = new double[3];
        }
        public class Motor(double SamplingFrequency, MotorSpecification Specification, MotorParameter Parameter)
        {
            public readonly double SamplingFrequency = SamplingFrequency;
            public readonly MotorSpecification Specification = Specification;
            public readonly MotorParameter Parameter = Parameter;
            private void ParameterCalculation()
            {
                double u_sm = Parameter.Udq0[0];
                double u_st = Parameter.Udq0[1];
                double T_e;
                double T_L = Parameter.TL;
                double R_s = Specification.R_s;
                double R_r = Specification.R_r;
                double L_m = Specification.L_m;
                double L_r = Specification.L_r;
                double L_s = Specification.L_s;
                double i_d = Parameter.Idq0[0];
                double i_q = Parameter.Idq0[1];
                double w_r = Parameter.w_r;
                double FLUX = Parameter.r_Flux;
                double NP = Specification.NP;
                double wsl = Parameter.wsl;
                double rsita = Parameter.sita_r;
                double wmr;
                double sitamr = Parameter.sitamr;
                // Other
                double i_m1 = Parameter.i_m1;
                //Rotor electrical constant
                double T_r = L_r / R_r;
                double temp2 = L_m * L_m;
                double temp1;
                double eta = 1 - temp2 / (L_s * L_r);
                double temp;

                temp1 = eta * L_s;
                temp = eta * L_s * L_r * T_r;

                // Excitation current equation
                i_d = ((-R_s / temp1 - temp2 / temp) * i_d + L_m / temp * FLUX + u_sm / temp1) / SamplingFrequency + i_d; 
                
                // Torque-current equation
                i_q = ((-R_s / temp1 - temp2 / temp) * i_q - FLUX * (L_m * w_r / (temp1 * L_r)) + u_st / temp1) / SamplingFrequency + i_q;
                
                //转子磁链方程
                //FLUX = i_mL_m/(T_rs +1);///一阶惯性环节 双线性变换推导出
                
                //Rotor flux linkage is the first - order inertia of excitation current
                FLUX = L_m / (temp + 1) * (i_d + i_m1) - (1 - temp) * FLUX / (temp + 1); 
                
                i_m1 = i_d;
                T_e = NP * i_q * FLUX * L_m / L_r; /*Moment equation*/
                if (FLUX != 0)
                    wsl = L_m * i_q / (T_r * FLUX); /*The slip equation may be divided by 0 here*/
                if ((Math.Abs(T_e - T_L) < Specification.STATICF) && (w_r == 0)) /*Simulating static friction*/
                    w_r = 0;
                else /*Simulated running equation of motion*/
                    w_r = NP * ((T_e - T_L - (Specification.DAMPING * w_r / NP)) / Specification.INERTIA) / SamplingFrequency + w_r;

                rsita += w_r / SamplingFrequency;
                wmr = (wsl + w_r) / SamplingFrequency;
                /*Input rotor position*/
                sitamr += wmr;
                sitamr %= M_2PI;
                if (sitamr < 0)
                    sitamr += M_2PI;
                /*Rotor position obtained by integration*/
                rsita %= M_2PI;
                if (rsita < 0)
                    rsita += M_2PI;

                Parameter.w_mr = wmr;
                Parameter.sitamr = sitamr;
                Parameter.sita_r = rsita;
                Parameter.Idq0[0] = i_d;
                Parameter.Idq0[1] = i_q;
                Parameter.w_r = w_r;
                Parameter.r_Flux = FLUX;
                Parameter.wsl = wsl;
                Parameter.Te = T_e;
                Parameter.i_m1 = i_m1;
            }
            public void UpdateParameter(WaveValues Voltage, double Theta)
            {
                Parameter.sitamr = Theta;
                Parameter.Uabc[0] = 220 * Voltage.U / 2.0;
                Parameter.Uabc[1] = 220 * Voltage.V / 2.0;
                Parameter.Uabc[2] = 220 * Voltage.W / 2.0;

                Parameter.Udq0[0] = Math.Cos(Parameter.sitamr) * Parameter.Uabc[0] + Math.Cos(Parameter.sitamr - M_2PI / 3) * Parameter.Uabc[1] + Math.Cos(Parameter.sitamr + M_2PI / 3) * Parameter.Uabc[2];
                Parameter.Udq0[1] = -Math.Sin(Parameter.sitamr) * Parameter.Uabc[0] + -Math.Sin(Parameter.sitamr - M_2PI / 3) * Parameter.Uabc[1] + -Math.Sin(Parameter.sitamr + M_2PI / 3) * Parameter.Uabc[2];

                ParameterCalculation();

                double al = Parameter.Idq0[0] * Math.Cos(Parameter.sitamr) - Parameter.Idq0[1] * Math.Sin(Parameter.sitamr);
                double be = Parameter.Idq0[1] * Math.Cos(Parameter.sitamr) + Parameter.Idq0[0] * Math.Sin(Parameter.sitamr);

                Parameter.Iabc[0] = Math.Sqrt(3 / 2.0) * (al * 1 + be * 0);
                Parameter.Iabc[1] = Math.Sqrt(3 / 2.0) * (al * -1 / 2.0 + be * Math.Sqrt(3) / 2);
                Parameter.Iabc[2] = Math.Sqrt(3 / 2.0) * (al * -1 / 2.0 + be * -Math.Sqrt(3) / 2);

                for (int i = 0; i < 3; i++)
                {
                    Parameter.DiffIdq0[i] = Parameter.Idq0[i] - Parameter.PreIdq0[i];
                    Parameter.PreIdq0[i] = Parameter.Idq0[i];
                }
            }
        }
    }
}
