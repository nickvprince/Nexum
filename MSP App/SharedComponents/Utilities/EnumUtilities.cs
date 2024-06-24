using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Utilities
{
    public class EnumUtilities
    {
        public static string EnumToString<T>(T enumValue) where T : Enum
        {
            return enumValue.ToString();
        }

        public static T StringToEnum<T>(string enumString) where T : Enum
        {
            if (Enum.TryParse(typeof(T), enumString, out var result))
            {
                return (T)result;
            }
            else
            {
                throw new ArgumentException($"Invalid value '{enumString}' for enum '{typeof(T).Name}'");
            }
        }
    }
}
