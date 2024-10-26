namespace VvvfSimulator.GUI.Simulator.RealTime.Controller
{
    public class Controller
    {
        public static IController GetWindow(ControllerStyle style, 
            Generation.Audio.GenerateRealTimeCommon.RealTimeParameter param, PropertyType type)
        {
            return style switch
            {
                ControllerStyle.Design1 => new Design1.Design1(param),
                _ => new Design2.Design2(param, type),
            };
        }
    }
}
