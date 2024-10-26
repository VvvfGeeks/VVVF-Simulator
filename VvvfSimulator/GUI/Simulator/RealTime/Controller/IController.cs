using System.Windows;
using VvvfSimulator.Generation;
using VvvfSimulator.GUI.Simulator.RealTime.Setting;

namespace VvvfSimulator.GUI.Simulator.RealTime.Controller
{
    public interface IController
    {
        public void StartTask();

        public int GetPosition();

        public DeviceMode GetControllerMode();
        public void SetControllerMode(DeviceMode mode);

        public string GetComPort();
        public void SetComPort(string port);

        public void PrepareController();

        public Window GetInstance();

    }

    public enum PropertyType
    {
        VVVF, Train
    }
    public enum ControllerStyle
    {
        Design1, Design2
    }
}
