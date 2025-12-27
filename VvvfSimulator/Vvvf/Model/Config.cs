using System;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;

namespace VvvfSimulator.Vvvf.Model
{
    public static class Config
    {
        #region PulseType
        public static PulseTypeName[] GetAvailablePulseType(int Level)
        {
            return Level switch
            {
                2 => [PulseTypeName.ASYNC, PulseTypeName.SYNC, PulseTypeName.SHE, PulseTypeName.CHM, PulseTypeName.HO, PulseTypeName.ΔΣ],
                3 => [PulseTypeName.ASYNC, PulseTypeName.SYNC, PulseTypeName.SHE, PulseTypeName.CHM],
                _ => []
            };
        }
        #endregion
        #region PulseCount
        public static int[] GetAvailablePulseCount(PulseTypeName PulseType, int Level)
        {
            if (Level == 2)
            {
                return PulseType switch
                {
                    PulseTypeName.SYNC => [-1],
                    PulseTypeName.HO => [5, 7, 9, 11, 13, 15, 17],
                    PulseTypeName.SHE => [3, 5, 7, 9, 11, 13, 15, 17, 19, 21],
                    PulseTypeName.CHM => [3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25],
                    _ => [],
                };
            }

            if (Level == 3)
            {
                return PulseType switch
                {
                    PulseTypeName.SYNC => [-1],
                    PulseTypeName.SHE => [1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21],
                    PulseTypeName.CHM => [1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21],
                    _ => [],
                };
            }

            return [];
        }
        #endregion
        #region AltMode
        public static PulseAlternative[] GetPulseAlternatives(Pulse PulseMode, int Level)
        {
            return GetPulseAlternatives(PulseMode.PulseType, PulseMode.PulseCount, Level);
        }
        public static PulseAlternative[] GetPulseAlternatives(PulseTypeName PulseType, int PulseCount, int Level)
        {
            static PulseAlternative[] AlternativesDefaultToX(int X, PulseAlternative[] Custom)
            {
                PulseAlternative[] Alternatives = new PulseAlternative[Custom.Length + X + 1];
                Alternatives[0] = PulseAlternative.Default;
                for (int i = 0; i < Custom.Length; i++)
                    Alternatives[i + 1] = Custom[i];
                for (int i = 0; i < X; i++)
                {
                    Alternatives[i + Custom.Length + 1] = (PulseAlternative)((int)PulseAlternative.Alt1 + i);
                }
                return Alternatives;
            }

            if (Level == 3)
            {
                if (PulseType == PulseTypeName.SYNC)
                {
                    return PulseCount switch
                    {
                        1 => [PulseAlternative.Default, PulseAlternative.Alt1],
                        5 => [PulseAlternative.Default, PulseAlternative.Alt1],
                        _ => [PulseAlternative.Default],
                    };
                }

                if (PulseType == PulseTypeName.ASYNC)
                {
                    return [PulseAlternative.Default];
                }

                if (PulseType == PulseTypeName.CHM)
                {
                    return PulseCount switch
                    {
                        3 => AlternativesDefaultToX(2, []),
                        5 => AlternativesDefaultToX(4, []),
                        7 => AlternativesDefaultToX(6, []),
                        9 => AlternativesDefaultToX(7, []),
                        11 => AlternativesDefaultToX(10, []),
                        13 => AlternativesDefaultToX(14, []),
                        15 => AlternativesDefaultToX(17, []),
                        17 => AlternativesDefaultToX(19, []),
                        19 => AlternativesDefaultToX(25, []),
                        21 => AlternativesDefaultToX(22, []),
                        _ => [PulseAlternative.Default],
                    };
                }

                if (PulseType == PulseTypeName.SHE)
                {
                    return PulseCount switch
                    {
                        3 => AlternativesDefaultToX(1, []),
                        _ => [PulseAlternative.Default],
                    };
                }

                return [PulseAlternative.Default];
            }

            if (Level == 2)
            {
                if (PulseType == PulseTypeName.SYNC)
                {
                    return PulseCount switch
                    {
                        1 => AlternativesDefaultToX(2, []),
                        3 => AlternativesDefaultToX(1, [PulseAlternative.CP, PulseAlternative.Square,]),
                        5 => AlternativesDefaultToX(1, [PulseAlternative.CP, PulseAlternative.Square,]),
                        6 => AlternativesDefaultToX(1, [PulseAlternative.CP, PulseAlternative.Square,]),
                        8 => AlternativesDefaultToX(1, [PulseAlternative.CP, PulseAlternative.Square,]),
                        9 => AlternativesDefaultToX(1, [PulseAlternative.CP, PulseAlternative.Square,]),
                        11 => AlternativesDefaultToX(1, [PulseAlternative.CP, PulseAlternative.Square,]),
                        13 => AlternativesDefaultToX(1, [PulseAlternative.CP, PulseAlternative.Square,]),
                        17 => AlternativesDefaultToX(1, [PulseAlternative.CP, PulseAlternative.Square,]),
                        _ => [PulseAlternative.Default, PulseAlternative.CP, PulseAlternative.Square,],
                    };
                }

                if (PulseType == PulseTypeName.ASYNC)
                {
                    return [PulseAlternative.Default];
                }

                if (PulseType == PulseTypeName.CHM)
                {
                    return PulseCount switch
                    {
                        3 => AlternativesDefaultToX(2, []),
                        5 => AlternativesDefaultToX(3, []),
                        7 => AlternativesDefaultToX(5, []),
                        9 => AlternativesDefaultToX(8, []),
                        11 => AlternativesDefaultToX(11, []),
                        13 => AlternativesDefaultToX(13, []),
                        15 => AlternativesDefaultToX(23, []),
                        17 => AlternativesDefaultToX(11, []),
                        19 => AlternativesDefaultToX(11, []),
                        21 => AlternativesDefaultToX(13, []),
                        23 => AlternativesDefaultToX(14, []),
                        25 => AlternativesDefaultToX(20, []),

                        _ => [PulseAlternative.Default],
                    };
                }

                if (PulseType == PulseTypeName.SHE)
                {
                    return PulseCount switch
                    {
                        3 => AlternativesDefaultToX(1, []),
                        5 => AlternativesDefaultToX(2, []),
                        7 => AlternativesDefaultToX(1, []),
                        9 => AlternativesDefaultToX(3, []),
                        11 => AlternativesDefaultToX(3, []),
                        13 => AlternativesDefaultToX(3, []),
                        15 => AlternativesDefaultToX(3, []),
                        17 => AlternativesDefaultToX(6, []),
                        19 => AlternativesDefaultToX(6, []),
                        21 => AlternativesDefaultToX(6, []),
                        _ => [PulseAlternative.Default],
                    };
                }

                if (PulseType == PulseTypeName.HO)
                {
                    return PulseCount switch
                    {
                        5 => AlternativesDefaultToX(7, []),
                        7 => AlternativesDefaultToX(9, []),
                        9 => AlternativesDefaultToX(6, []),
                        11 => AlternativesDefaultToX(5, []),
                        13 => AlternativesDefaultToX(3, []),
                        15 => AlternativesDefaultToX(2, []),
                        _ => [PulseAlternative.Default],
                    };
                }

                return [PulseAlternative.Default];
            }

            return [PulseAlternative.Default];
        }
        #endregion
        #region BaseWave
        public static bool IsBaseWaveHarmonicAvailable(Pulse PulseMode, int Level)
        {
            if (Level == 2)
            {
                if (PulseMode.PulseType == PulseTypeName.ΔΣ) return true;
                if (PulseMode.BaseWave >= BaseWaveType.SV) return false;
                if (PulseMode.Alternative == PulseAlternative.CP) return true;
                if (PulseMode.Alternative > PulseAlternative.Default) return false;
                if (PulseMode.PulseType == PulseTypeName.SYNC)
                {
                    if (PulseMode.PulseCount == 1) return false;
                    return true;
                }
                if (PulseMode.PulseType == PulseTypeName.ASYNC) return true;
                return false;
            }

            if (Level == 3)
            {
                if (PulseMode.BaseWave >= BaseWaveType.SV) return false;
                if (PulseMode.Alternative > PulseAlternative.Default) return false;
                if (PulseMode.PulseType == PulseTypeName.SYNC) return true;
                if (PulseMode.PulseType == PulseTypeName.ASYNC) return true;
                return false;
            }
            return false;
        }
        public static bool IsBaseWaveChangeable(Pulse PulseMode, int Level)
        {
            if (Level == 2)
            {
                if (PulseMode.PulseType == PulseTypeName.ΔΣ) return true;
                if (PulseMode.Alternative == PulseAlternative.CP) return true;
                if (PulseMode.Alternative > PulseAlternative.Default) return false;
                if (PulseMode.PulseType == PulseTypeName.SYNC)
                {
                    if (PulseMode.PulseCount == 1) return false;
                    return true;
                }
                if (PulseMode.PulseType == PulseTypeName.ASYNC) return true;
                return false;
            }

            if (Level == 3)
            {
                if (PulseMode.Alternative > PulseAlternative.Default) return false;
                if (PulseMode.PulseType == PulseTypeName.SYNC) return true;
                if (PulseMode.PulseType == PulseTypeName.ASYNC) return true;
                return false;
            }
            return false;
        }
        public static bool IsDiscreteTimeAvailable(Pulse PulseMode, int Level)
        {
            if (Level == 2)
            {
                if ((PulseMode.PulseCount == 3 || PulseMode.PulseCount == 6 || PulseMode.PulseCount == 8) && PulseMode.Alternative == PulseAlternative.Alt1) return false;
                return true;
            }

            if (Level == 3)
            {
                return true;
            }
            return false;
        }
        #endregion
        #region CarrierWave
        public static bool IsCarrierWaveChangeable(Pulse PulseMode, int Level)
        {
            if (Level == 2)
            {
                if (PulseMode.PulseType == PulseTypeName.SHE) return false;
                if (PulseMode.PulseType == PulseTypeName.CHM) return false;
                if (PulseMode.PulseType == PulseTypeName.HO) return false;
                if (PulseMode.PulseType == PulseTypeName.ΔΣ) return false;
                if (PulseMode.PulseType == PulseTypeName.SYNC)
                {
                    if (PulseMode.PulseCount == 1 && (PulseMode.Alternative == PulseAlternative.Alt1 || PulseMode.Alternative == PulseAlternative.Alt2)) return false;
                    if (PulseMode.PulseCount == 3 && PulseMode.Alternative == PulseAlternative.Alt1) return false;
                    if ((PulseMode.PulseCount == 5 || PulseMode.PulseCount == 9 || PulseMode.PulseCount == 13 || PulseMode.PulseCount == 17) && PulseMode.Alternative == PulseAlternative.Alt1) return false;
                    if (PulseMode.PulseCount == 11 && PulseMode.Alternative == PulseAlternative.Alt1) return false;
                    if ((PulseMode.PulseCount == 6 || PulseMode.PulseCount == 8) && PulseMode.Alternative == PulseAlternative.Alt1) return false;
                    if (PulseMode.Alternative == PulseAlternative.CP) return false;
                    if (PulseMode.Alternative == PulseAlternative.Square) return false;
                }
                return true;
            }

            if (Level == 3)
            {
                if (PulseMode.PulseType == PulseTypeName.SHE) return false;
                if (PulseMode.PulseType == PulseTypeName.CHM) return false;
                if (PulseMode.PulseType == PulseTypeName.SYNC)
                {
                    if (PulseMode.PulseCount == 1 && PulseMode.Alternative == PulseAlternative.Alt1) return false;
                    if (PulseMode.PulseCount == 5 && PulseMode.Alternative == PulseAlternative.Alt1) return false;

                }
                return true;
            }
            return false;
        }
        public static CarrierWaveConfiguration.CarrierWaveOption[] GetAvailableCarrierWaveOptions(Pulse PulseMode, int Level)
        {
            return Enum.GetValues<CarrierWaveConfiguration.CarrierWaveOption>();
        }
        #endregion
        #region PulseDataKey
        public static PulseDataKey[] GetAvailablePulseDataKey(Pulse PulseMode, int Level)
        {

            if (Level == 2)
            {
                return PulseMode.PulseType switch
                {
                    PulseTypeName.SYNC => PulseMode.PulseCount switch
                    {
                        3 => PulseMode.Alternative switch
                        {
                            PulseAlternative.Alt1 => [PulseDataKey.Phase],
                            _ => [],
                        },
                        6 => PulseMode.Alternative switch
                        {
                            PulseAlternative.Alt1 => [PulseDataKey.PulseWidth],
                            _ => [],
                        },
                        8 => PulseMode.Alternative switch
                        {
                            PulseAlternative.Alt1 => [PulseDataKey.PulseWidth],
                            _ => [],
                        },
                        _ => []
                    },
                    PulseTypeName.ΔΣ => [PulseDataKey.UpdateFrequency],
                    _ => []
                };
            }

            if (Level == 3)
            {
                return PulseMode.PulseType switch
                {
                    PulseTypeName.SYNC => PulseMode.PulseCount switch
                    {
                        1 => [],
                        5 => PulseMode.Alternative switch
                        {
                            PulseAlternative.Alt1 => [PulseDataKey.PulseWidth],
                            _ => [PulseDataKey.Dipolar],
                        },
                        _ => [PulseDataKey.Dipolar]
                    },
                    PulseTypeName.ASYNC => [PulseDataKey.Dipolar],
                    _ => []
                };
            }

            return [];
        }
        public static double GetPulseDataKeyDefaultConstant(PulseDataKey Key)
        {
            return Key switch
            {
                PulseDataKey.Dipolar => -1,
                PulseDataKey.Phase => 0,
                PulseDataKey.PulseWidth => 0.2,
                PulseDataKey.UpdateFrequency => 440,
                _ => 0,
            };
        }
        #endregion
    }
}
