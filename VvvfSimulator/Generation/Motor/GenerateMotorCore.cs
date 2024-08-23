using System;
using static VvvfSimulator.Vvvf.Struct;



namespace VvvfSimulator.Generation.Motor
{
    public class GenerateMotorCore
    {
        public class MotorData
        {
            readonly public static double M_2PI = 6.283185307179586476925286766559;
            readonly public static double M_PI = 3.1415926535897932384626433832795;
            readonly public static double M_PI_2 = 1.5707963267948966192313216916398;
            readonly public static double M_2_PI = 0.63661977236758134307553505349006;
            readonly public static double M_1_PI = 0.31830988618379067153776752674503;
            readonly public static double M_1_2PI = 0.15915494309189533576888376337251;
            readonly public static double M_PI_180 = 0.01745329251994329576923690768489;
            readonly public static double M_PI_4 = 0.78539816339744830961566084581988;

            public double SIM_SAMPLE_FREQ = 192000.0;
            public MotorSpecification motor_Specification = new();
            public MotorParam motor_Param = new();

            public class MotorSpecification
            {
                public double R_s { get; set; } = 1.898; /*stator resistance(ohm)*/
                public double R_r { get; set; } = 1.45;  /*Rotor resistance(ohm)*/
                public double L_s { get; set; } = 0.196; /*Stator inductance(H)*/
                public double L_r { get; set; } = 0.196; /*Rotor inductance(H)*/
                public double L_m { get; set; } = 0.187; /*Mutual inductance(H)*/
                public double NP { get; set; } = 2;/* Polar logarithm*/
                public double DAMPING { get; set; } = 500.0;/* damping */
                public double INERTIA { get; set; } = 0.05; /*Rotational inertia mass(kg.m^2)*/
                public double STATICF { get; set; } = 0.005879; /*Static friction(N.m.s)*/

                public MotorSpecification Clone()
                {
                    return (MotorSpecification)MemberwiseClone();
                }
            }
            public class MotorParam
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
                public double TePre { get; set; } = 0;
            }


            public WaveValues Get_LineVoltage(WaveValues v) {
                int UV = v.U - v.V;
                int VW = v.V - v.W;
                int WU = v.W - v.U;

                return new(UV, VW, WU);
            }

            public void AynMotorControler(WaveValues v)
            {
                motor_Param.Uabc[0] = 220 * v.U / 2.0;//Phase to ground 220 Phase to phase 380 
                motor_Param.Uabc[1] = 220 * v.V / 2.0;//Phase to ground 220 Phase to phase 380 
                motor_Param.Uabc[2] = 220 * v.W / 2.0;//Phase to ground 220 Phase to phase 380 
            }

            private void Asyn_ModuleMt()
            {
                //The following motor models are from
                //《Research on some key technologies of high performance frequency converter for asynchronous motor》Wang siran, Zhejiang University
                double u_sm = motor_Param.Udq0[0];
                double u_st = motor_Param.Udq0[1];
                double T_e;
                double T_L = motor_Param.TL;
                double R_s = motor_Specification.R_s;
                double R_r = motor_Specification.R_r;
                double L_m = motor_Specification.L_m;
                double L_r = motor_Specification.L_r;
                double L_s = motor_Specification.L_s;
                double i_d = motor_Param.Idq0[0];
                double i_q = motor_Param.Idq0[1];
                double w_r = motor_Param.w_r;
                double FLUX = motor_Param.r_Flux;
                double NP = motor_Specification.NP;
                double wsl = motor_Param.wsl;
                double rsita = motor_Param.sita_r;
                double wmr;
                double sitamr = motor_Param.sitamr;
                // Other
                double i_m1 = motor_Param.i_m1;
                //Rotor electrical constant
                double T_r = L_r / R_r;
                double temp2 = L_m * L_m;
                double temp1;
                double eta = 1 - temp2 / (L_s * L_r);
                double temp;

                temp1 = eta * L_s;
                temp = eta * L_s * L_r * T_r;
                i_d = ((-R_s / temp1 - temp2 / temp) * i_d + L_m / temp * FLUX + u_sm / temp1) / SIM_SAMPLE_FREQ + i_d; ///励磁电流方程
                i_q = ((-R_s / temp1 - temp2 / temp) * i_q - FLUX * (L_m * w_r / (temp1 * L_r)) + u_st / temp1) / SIM_SAMPLE_FREQ + i_q;  ///转矩电流方程
                                                                                                                                           ///转子磁链方程
                                                                                                                                           ///FLUX = i_mL_m/(T_rs +1);///一阶惯性环节 双线性变换推导出
                FLUX = L_m / (temp + 1) * (i_d + i_m1) - (1 - temp) * FLUX / (temp + 1); /*Rotor flux linkage is the first - order inertia of excitation current*/
                i_m1 = i_d;
                T_e = NP * i_q * FLUX * L_m / L_r; /*Moment equation*/
                if (FLUX != 0)
                    wsl = L_m * i_q / (T_r * FLUX); /*The slip equation may be divided by 0 here*/
                if ((Math.Abs(T_e - T_L) < motor_Specification.STATICF) && (w_r == 0)) /*Simulating static friction*/
                    w_r = 0;
                else /*Simulated running equation of motion*/
                    w_r = NP * ((T_e - T_L - (motor_Specification.DAMPING * w_r / NP)) / motor_Specification.INERTIA) / SIM_SAMPLE_FREQ + w_r;

                rsita += w_r / SIM_SAMPLE_FREQ;
                wmr = (wsl + w_r) / SIM_SAMPLE_FREQ;
                /*Input rotor position*/
                sitamr += wmr;
                sitamr %= M_2PI;
                if (sitamr < 0)
                    sitamr += M_2PI;
                /*Rotor position obtained by integration*/
                rsita %= M_2PI;
                if (rsita < 0)
                    rsita += M_2PI;

                motor_Param.w_mr = wmr;
                motor_Param.sitamr = sitamr;
                motor_Param.sita_r = rsita;
                motor_Param.Idq0[0] = i_d;
                motor_Param.Idq0[1] = i_q;
                motor_Param.w_r = w_r;
                motor_Param.r_Flux = FLUX;
                motor_Param.wsl = wsl;
                motor_Param.TePre = motor_Param.Te;
                motor_Param.Te = T_e;
                motor_Param.i_m1 = i_m1;
            }

            public void Asyn_Moduleabc()
            {
                var p = motor_Param;
                p.Udq0[0] = Math.Cos(p.sitamr) * p.Uabc[0] + Math.Cos(p.sitamr - M_2PI / 3) * p.Uabc[1] + Math.Cos(p.sitamr + M_2PI / 3) * p.Uabc[2];
                p.Udq0[1] = -Math.Sin(p.sitamr) * p.Uabc[0] + -Math.Sin(p.sitamr - M_2PI / 3) * p.Uabc[1] + -Math.Sin(p.sitamr + M_2PI / 3) * p.Uabc[2];

                Asyn_ModuleMt();

                double al = p.Idq0[0] * Math.Cos(p.sitamr) - p.Idq0[1] * Math.Sin(p.sitamr);
                double be = p.Idq0[1] * Math.Cos(p.sitamr) + p.Idq0[0] * Math.Sin(p.sitamr);

                p.Iabc[0] = Math.Sqrt(3 / 2.0) * (al * 1 + be * 0);
                p.Iabc[1] = Math.Sqrt(3 / 2.0) * (al * -1 / 2.0 + be * Math.Sqrt(3) / 2);
                p.Iabc[2] = Math.Sqrt(3 / 2.0) * (al * -1 / 2.0 + be * -Math.Sqrt(3) / 2);
            }

        }

    }

  
}
