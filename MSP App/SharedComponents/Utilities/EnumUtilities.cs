
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

        public static string AddSpacesToCapital(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var newText = new System.Text.StringBuilder(text.Length * 2);
            newText.Append(text[0]);

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                {
                    newText.Append(' ');
                }
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
