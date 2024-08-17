using System.Windows;
using System.Windows.Controls;

namespace VvvfSimulator.GUI.Resource.Class
{
    public class ParseTextBox
    {

        public static double ParseDouble(TextBox Element, double Minimum=double.MinValue, double ErrorValue=0)
        {
            try
            {
                VisualStateManager.GoToState(Element, "Success", false);
                double val = double.Parse(Element.Text);
                if (val < Minimum) throw new System.Exception();
                return val;
            }
            catch
            {
                VisualStateManager.GoToState(Element, "Error", false);
                return ErrorValue;
            }
        }

        public static int ParseInt(TextBox Element, int Minimum=int.MinValue, int ErrorValue=0)
        {
            try
            {
                VisualStateManager.GoToState(Element, "Success", false);
                int val = int.Parse(Element.Text);
                if (val < Minimum) throw new System.Exception();
                return val;
            }
            catch
            {
                VisualStateManager.GoToState(Element, "Error", false);
                return ErrorValue;
            }
        }
    }
}
