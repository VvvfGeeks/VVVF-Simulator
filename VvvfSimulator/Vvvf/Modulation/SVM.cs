namespace VvvfSimulator.Vvvf.Modulation
{
    public class SVM
    {
        public class FunctionTime
        {
            public double T0;
            public double T1;
            public double T2;

            public static FunctionTime operator *(FunctionTime a, double d) => new()
            {
                T0 = a.T0 * d,
                T1 = a.T1 * d,
                T2 = a.T2 * d
            };

            public static FunctionTime operator *(double d, FunctionTime a) => new()
            {
                T0 = a.T0 * d,
                T1 = a.T1 * d,
                T2 = a.T2 * d
            };
            public Vabc GetVabc(int sector)
            {
                Vabc Res = new();
                switch (sector)
                {
                    case 1:
                        {
                            Res.U = T1 + T2 + 0.5 * T0;
                            Res.V = T2 + 0.5 * T0;
                            Res.W = 0.5 * T0;
                        }
                        break;
                    case 2:
                        {
                            Res.U = T1 + 0.5 * T0;
                            Res.V = T1 + T2 + 0.5 * T0;
                            Res.W = 0.5 * T0;
                        }
                        break;
                    case 3:
                        {
                            Res.U = 0.5 * T0;
                            Res.V = T1 + T2 + 0.5 * T0;
                            Res.W = T2 + 0.5 * T0;
                        }
                        break;
                    case 4:
                        {
                            Res.U = 0.5 * T0;
                            Res.V = T1 + 0.5 * T0;
                            Res.W = T1 + T2 + 0.5 * T0;
                        }
                        break;
                    case 5:
                        {
                            Res.U = T2 + 0.5 * T0;
                            Res.V = 0.5 * T0;
                            Res.W = T1 + T2 + 0.5 * T0;
                        }
                        break;
                    case 6:
                        {
                            Res.U = T1 + T2 + 0.5 * T0;
                            Res.V = 0.5 * T0;
                            Res.W = T1 + 0.5 * T0;
                        }
                        break;
                }
                return Res;
            }
        };
        public class Vabc
        {
            public double U;
            public double V;
            public double W;
            public static Vabc operator +(Vabc a, double d) => new()
            {
                U = a.U + d,
                V = a.V + d,
                W = a.W + d
            };

            public static Vabc operator *(Vabc a, double d) => new()
            {
                U = a.U * d,
                V = a.V * d,
                W = a.W * d
            };

            public static Vabc operator -(Vabc a) => new()
            {
                U = -a.U,
                V = -a.V,
                W = -a.W,
            };

            public static Vabc operator -(Vabc a, Vabc b) => new()
            {
                U = a.U - b.U,
                V = a.V - b.V,
                W = a.W - b.W
            };

            public Valbe Clark()
            {
                return new()
                {
                    Alpha = (2 * U - V - W) / 3,
                    Beta = (V - W) / MyMath.M_SQRT3
                };
            }
        };
        public class Valbe
        {
            public double Alpha;
            public double Beta;
            public FunctionTime GetFunctionTime(int sector)
            {
                FunctionTime ft = new();
                switch (sector)
                {
                    case 1:
                        {
                            ft.T1 = MyMath.M_SQRT3_2 * Alpha - 0.5 * Beta;
                            ft.T2 = Beta;
                        }
                        break;
                    case 2:
                        {
                            ft.T1 = MyMath.M_SQRT3_2 * Alpha + 0.5 * Beta;
                            ft.T2 = 0.5 * Beta - MyMath.M_SQRT3_2 * Alpha;
                        }
                        break;
                    case 3:
                        {
                            ft.T1 = Beta;
                            ft.T2 = -(MyMath.M_SQRT3_2 * Alpha + 0.5 * Beta);
                        }
                        break;
                    case 4:
                        {
                            ft.T1 = 0.5 * Beta - MyMath.M_SQRT3_2 * Alpha;
                            ft.T2 = -Beta;
                        }
                        break;
                    case 5:
                        {
                            ft.T1 = -(MyMath.M_SQRT3_2 * Alpha + 0.5 * Beta);
                            ft.T2 = MyMath.M_SQRT3_2 * Alpha - 0.5 * Beta;
                        }
                        break;
                    case 6:
                        {
                            ft.T1 = -Beta;
                            ft.T2 = MyMath.M_SQRT3_2 * Alpha + 0.5 * Beta;
                        }
                        break;
                }
                ft.T0 = 1.0 - ft.T1 - ft.T2;
                return ft;
            }
            public int EstimateSector()
            {
                int A = Beta > 0.0 ? 0 : 1;
                int B = Beta - MyMath.M_SQRT3 * Alpha > 0.0 ? 0 : 1;
                int C = Beta + MyMath.M_SQRT3 * Alpha > 0.0 ? 0 : 1;
                return (4 * A + 2 * B + C) switch
                {
                    0 => 2,
                    1 => 3,
                    2 => 1,
                    3 => 0,
                    4 => 0,
                    5 => 4,
                    6 => 6,
                    7 => 5,
                    _ => 2,
                };
            }
        }
    }
}
