using System;
using static VvvfSimulator.Vvvf.MyMath;
using static VvvfSimulator.Vvvf.Model.Struct;


namespace VvvfSimulator.Vvvf.Model
{
    public class Motor(Data.TrainAudio.Struct.MotorSpecification Specification)
    {
        public readonly Data.TrainAudio.Struct.MotorSpecification Specification = Specification;
        public Status Parameter = new();
        public Motor Clone()
        {
            Motor Copy = new(Specification.Clone()) { Parameter = Parameter.Clone() };
            return Copy;
        }
        public void Process(double dt, double ωe, PhaseState Voltage)
        {
            Parameter.Ωe = ωe;
            Parameter.Vdq0[0] = (Math.Cos(Parameter.Θmr) * Voltage.U + Math.Cos(Parameter.Θmr - M_2PI / 3) * Voltage.V + Math.Cos(Parameter.Θmr + M_2PI / 3) * Voltage.W) * Specification.V / 2.0;
            Parameter.Vdq0[1] = (-Math.Sin(Parameter.Θmr) * Voltage.U + -Math.Sin(Parameter.Θmr - M_2PI / 3) * Voltage.V + -Math.Sin(Parameter.Θmr + M_2PI / 3) * Voltage.W) * Specification.V / 2.0;

            double ws = Parameter.Ωe;    // Electrical angular frequency [rad/s]
            double wr0 = Parameter.Ωr;   // Mechanical angular speed [rad/s] (initial value)

            // --- Read current states ---
            double ids0 = Parameter.Idq0[0];
            double iqs0 = Parameter.Idq0[1];
            double ird0 = Parameter.Ir[0];
            double irq0 = Parameter.Ir[1];
            double vds = Parameter.Vdq0[0];
            double vqs = Parameter.Vdq0[1];

            // --- Motor parameters ---
            double Rs = Specification.Rs;
            double Rr = Specification.Rr;
            double Ls = Specification.Ls;
            double Lr = Specification.Lr;
            double Lm = Specification.Lm;
            double p = Specification.Np;
            double TL = Parameter.TL;
            double J = Specification.Inertia;

            // --- Friction parameters ---
            double D_viscous = Specification.Fd;          // Viscous friction coefficient
            double Fc = Specification.Fc;                 // Coulomb (sliding) friction torque
            double Fs = Specification.Fs;                 // Static friction torque
            double omega_s = Specification.StribeckOmega; // Stribeck speed constant [rad/s]
            double k_smooth = Specification.FricSmoothK;  // tanh smoothing gain

            // --- Helper: compute flux linkages from stator & rotor dq currents ---
            double[] computeFlux(double ids, double iqs, double ird, double irq)
            {
                double psi_sd = Ls * ids + Lm * ird;
                double psi_sq = Ls * iqs + Lm * irq;
                double psi_rd = Lm * ids + Lr * ird;
                double psi_rq = Lm * iqs + Lr * irq;
                return [psi_sd, psi_sq, psi_rd, psi_rq];
            }

            // --- Helper: solve 2x2 linear system A·x = b ---
            double[] solve2x2(double a11, double a12, double a21, double a22, double b1, double b2)
            {
                double det = a11 * a22 - a12 * a21;
                if (Math.Abs(det) < 1e-12) det = Math.Sign(det) * 1e-12 + 1e-12;
                double x1 = (b1 * a22 - b2 * a12) / det;
                double x2 = (a11 * b2 - a21 * b1) / det;
                return [x1, x2];
            }

            // --- Friction model (includes static + Stribeck + tanh smoothing) ---
            double frictionTorque(double wr, double Te_minus_TL)
            {
                double absWr = Math.Abs(wr);
                const double eps_w = 1e-6;

                // --- Stribeck friction amplitude ---
                double stribeckAmp = Fs - (Fs - Fc) * Math.Exp(-Math.Pow(absWr / omega_s, 2.0));

                double smoothSign = Math.Tanh(k_smooth * wr);
                double T_visc = D_viscous * wr;
                double T_slide = T_visc + stribeckAmp * smoothSign;

                // --- Static friction region ---
                if (Math.Abs(wr) < eps_w)
                {
                    // If the difference between motor torque and load torque is smaller than the static friction → no motion
                    if (Math.Abs(Te_minus_TL) < Fs)
                    {
                        // Remain in a stationary state (friction completely cancels out the torque)
                        return Te_minus_TL;
                    }
                }

                // --- Dynamic friction (normal running state) ---
                return T_slide;
            }

            // --- Differential equations: returns [dids, diqs, dird, dirq, dwr] ---
            double[] derivatives(double ids, double iqs, double ird, double irq, double wr_current)
            {
                var psi = computeFlux(ids, iqs, ird, irq);
                double psi_sd = psi[0], psi_sq = psi[1], psi_rd = psi[2], psi_rq = psi[3];

                // d/dt (stator and rotor currents) from dq voltage equations
                double b1d = vds - Rs * ids + ws * psi_sq;
                double b2d = -Rr * ird + (ws - wr_current) * psi_rq;
                var solD = solve2x2(Ls, Lm, Lm, Lr, b1d, b2d);
                double d_ids = solD[0];
                double d_ird = solD[1];

                double b1q = vqs - Rs * iqs - ws * psi_sd;
                double b2q = -Rr * irq - (ws - wr_current) * psi_rd;
                var solQ = solve2x2(Ls, Lm, Lm, Lr, b1q, b2q);
                double d_iqs = solQ[0];
                double d_irq = solQ[1];

                // --- Electromagnetic torque ---
                double Te_inst = 1.5 * p * (psi_sd * iqs - psi_sq * ids);

                // --- Friction torque ---
                double T_fric = frictionTorque(wr_current, Te_inst - TL);

                // --- Mechanical acceleration ---
                double d_wr = p * (Te_inst - TL - T_fric) / J;

                return [d_ids, d_iqs, d_ird, d_irq, d_wr];
            }

            // --- Runge–Kutta 4th-order (RK4) integration step ---
            var k1 = derivatives(ids0, iqs0, ird0, irq0, wr0);

            var ids_k2 = ids0 + 0.5 * k1[0] * dt;
            var iqs_k2 = iqs0 + 0.5 * k1[1] * dt;
            var ird_k2 = ird0 + 0.5 * k1[2] * dt;
            var irq_k2 = irq0 + 0.5 * k1[3] * dt;
            var wr_k2 = wr0 + 0.5 * k1[4] * dt;
            var k2 = derivatives(ids_k2, iqs_k2, ird_k2, irq_k2, wr_k2);

            var ids_k3 = ids0 + 0.5 * k2[0] * dt;
            var iqs_k3 = iqs0 + 0.5 * k2[1] * dt;
            var ird_k3 = ird0 + 0.5 * k2[2] * dt;
            var irq_k3 = irq0 + 0.5 * k2[3] * dt;
            var wr_k3 = wr0 + 0.5 * k2[4] * dt;
            var k3 = derivatives(ids_k3, iqs_k3, ird_k3, irq_k3, wr_k3);

            var ids_k4 = ids0 + k3[0] * dt;
            var iqs_k4 = iqs0 + k3[1] * dt;
            var ird_k4 = ird0 + k3[2] * dt;
            var irq_k4 = irq0 + k3[3] * dt;
            var wr_k4 = wr0 + k3[4] * dt;
            var k4 = derivatives(ids_k4, iqs_k4, ird_k4, irq_k4, wr_k4);

            // --- RK4 synthesis step ---
            double ids_new = ids0 + (dt / 6.0) * (k1[0] + 2 * k2[0] + 2 * k3[0] + k4[0]);
            double iqs_new = iqs0 + (dt / 6.0) * (k1[1] + 2 * k2[1] + 2 * k3[1] + k4[1]);
            double ird_new = ird0 + (dt / 6.0) * (k1[2] + 2 * k2[2] + 2 * k3[2] + k4[2]);
            double irq_new = irq0 + (dt / 6.0) * (k1[3] + 2 * k2[3] + 2 * k3[3] + k4[3]);
            double wr_new = wr0 + (dt / 6.0) * (k1[4] + 2 * k2[4] + 2 * k3[4] + k4[4]);

            // --- Compute new flux linkages ---
            var psiNew = computeFlux(ids_new, iqs_new, ird_new, irq_new);
            double psi_sd_new = psiNew[0];
            double psi_sq_new = psiNew[1];
            double psi_rd_new = psiNew[2];
            double psi_rq_new = psiNew[3];

            // --- Update parameters ---
            Parameter.Flux_s[0] = psi_sd_new;
            Parameter.Flux_s[1] = psi_sq_new;
            Parameter.Flux_r[0] = psi_rd_new;
            Parameter.Flux_r[1] = psi_rq_new;
            Parameter.Φr = Math.Sqrt(psi_rd_new * psi_rd_new + psi_rq_new * psi_rq_new);

            // --- Update electromagnetic torque ---
            Parameter.Te = 1.5 * p * (psi_sd_new * iqs_new - psi_sq_new * ids_new);

            // --- Slip angular frequency ---
            Parameter.Ωsl = ws - wr_new;

            // --- Store new states ---
            Parameter.Idq0[0] = ids_new;
            Parameter.Idq0[1] = iqs_new;
            Parameter.Ir[0] = ird_new;
            Parameter.Ir[1] = irq_new;
            Parameter.Ωr = wr_new;

            // --- Update rotor and synchronous electrical angles ---
            Parameter.Θr += Parameter.Ωr * dt;
            Parameter.Θr %= 2.0 * Math.PI;
            if (Parameter.Θr < 0) Parameter.Θr += 2.0 * Math.PI;

            Parameter.Θmr += ws * dt;
            Parameter.Θmr %= 2.0 * Math.PI;
            if (Parameter.Θmr < 0) Parameter.Θmr += 2.0 * Math.PI;

            Parameter.DiffTe = Parameter.Te - Parameter.PreTe;
            Parameter.PreTe = Parameter.Te;

            for (int i = 0; i < 3; i++)
            {
                Parameter.DiffIdq0[i] = Parameter.Idq0[i] - Parameter.PreIdq0[i];
                Parameter.PreIdq0[i] = Parameter.Idq0[i];
            }
        }
        public void Reset()
        {
            Parameter = new();
        }
        public class Status
        {
            // Inherent Parameters
            // --- Electrical dynamic states ---
            public double[] Idq0 { get; set; } = new double[3];
            public double[] Vdq0 { get; set; } = new double[3];
            public double Ωsl { get; set; } = 0;     // Slip angular frequency
            public double Ωr { get; set; } = 0;     // Rotor electrical angular speed
            public double[] Ir { get; set; } = new double[2]; // Rotor dq currents
            public double Ωe { get; set; } = 0;     // Stator electrical angular speed

            // --- Mechanical dynamic states ---
            public double Θr { get; set; } = 0;  // Electrical angle of rotor
            public double Θmr { get; set; } = 0;  // Mechanical rotor position (rad)
            public double TL { get; set; } = 0;      // Load torque (N·m)
            public double Te { get; set; } = 0;      // Electromagnetic torque (N·m)
            public double Φr { get; set; } = 0;  // Rotor flux linkage magnitude (Wb)

            // --- Optional additional dynamic states (recommended additions) ---
            public double[] Flux_s { get; set; } = new double[2]; // Stator flux dq components
            public double[] Flux_r { get; set; } = new double[2]; // Rotor flux dq components

            // --- Diagnostic / monitoring ---
            public double PreTe { get; set; } = 0;
            public double DiffTe { get; set; } = 0;
            public double[] PreIdq0 { get; set; } = new double[3];
            public double[] DiffIdq0 { get; set; } = new double[3];

            public Status Clone()
            {
                Status Copy = (Status)MemberwiseClone();
                Copy.Idq0 = (double[])Idq0.Clone();
                Copy.Vdq0 = (double[])Vdq0.Clone();
                Copy.Ir = (double[])Ir.Clone();
                Copy.Flux_s = (double[])Flux_s.Clone();
                Copy.Flux_r = (double[])Flux_r.Clone();
                Copy.PreIdq0 = (double[])PreIdq0.Clone();
                Copy.DiffIdq0 = (double[])DiffIdq0.Clone();
                return Copy;
            }

        }
    }
}
