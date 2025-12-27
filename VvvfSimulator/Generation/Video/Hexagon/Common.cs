using System.Collections.Generic;
using static VvvfSimulator.Vvvf.MyMath;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Video.Hexagon
{
    public class Common
    {
        public static PointD ToVectorXY(PhaseState State)
        {
            if (State.U == State.V && State.V == State.W) return new(0, 0);
            return new(
                -0.5 * State.W - 0.5 * State.V + State.U,
                -0.866025403784438646763 * State.W + 0.866025403784438646763 * State.V
            );
        }
        public static void GetPoints(ref PhaseState[] UVW, out PointD[] LinePoints, out PointD[] ZeroPoints)
        {
            int TotalWaveLength = UVW.Length;
            PhaseState PreVectorUVW = UVW[0];
            int PreIndex = 0;

            PointD CurrentPoint = new(0, 0);
            PointD MaxValue = new(double.MinValue, double.MinValue);
            PointD MinValue = new(double.MaxValue, double.MaxValue);
            List<PointD> _LinePoints = [CurrentPoint];
            List<PointD> _ZeroPoints = [];

            void UpdatePoints(int Length, PointD VectorXY)
            {
                CurrentPoint += Length * VectorXY;

                if (VectorXY.IsZero()) _ZeroPoints.Add(CurrentPoint);
                else _LinePoints.Add(CurrentPoint);

                MaxValue = PointD.Max(CurrentPoint, MaxValue);
                MinValue = PointD.Min(CurrentPoint, MinValue);
            }

            for (int Index = 1; Index < TotalWaveLength; Index++)
            {
                PhaseState VectorUVW = UVW[Index];
                if (VectorUVW.Equals(PreVectorUVW)) continue;
                UpdatePoints(Index - PreIndex, ToVectorXY(PreVectorUVW));
                PreIndex = Index;
                PreVectorUVW = VectorUVW;
            }

            UpdatePoints(TotalWaveLength - 1 - PreIndex, ToVectorXY(PreVectorUVW));

            PointD DifferenceCenter = -0.5 * (MaxValue + MinValue);
            LinePoints = [.. _LinePoints.ConvertAll((Point) => 3.0 / (2 * TotalWaveLength) * (Point + DifferenceCenter))];
            ZeroPoints = [.. _ZeroPoints.ConvertAll((Point) => 3.0 / (2 * TotalWaveLength) * (Point + DifferenceCenter))];
        }
        
    }
}
