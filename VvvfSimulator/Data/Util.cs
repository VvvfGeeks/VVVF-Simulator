using System.Collections;
using System.Reflection;

namespace VvvfSimulator.Data
{
    public static class Util
    {
        public static string GetPropertyValues(object Class)
        {
            string Result = string.Empty;
            PropertyInfo[] Properties = Class.GetType().GetProperties();
            for(int i = 0; i < Properties.Length; i++)
            {
                PropertyInfo Property = Properties[i];
                Result += string.Format("{0} : {1}", Property.Name, GetPropertyValue(Property.GetValue(Class))) + (i + 1 == Properties.Length ? "" : ", ");
            }
            return Result;
        }
        private static string GetPropertyValue(object? Obj)
        {
            if (Obj is IList List)
            {
                string Result = "[";
                for (int i = 0; i < List.Count; i++)
                {
                    Result += GetPropertyValue(List[i]) + (i + 1 == List.Count ? "" : ", ");
                }
                Result += "]";
                return Result;
            }
            return Obj?.ToString() ?? "null";
        }
    }
}
