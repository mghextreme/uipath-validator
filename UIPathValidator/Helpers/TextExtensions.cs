using System.Text;

namespace UIPathValidator
{
    public static class TextExtensions
    {
        public static bool IsUppercaseLetter(this char letter)
        {
            return letter >= 65 && letter <= 90;
        }

        public static bool IsLowercaseLetter(this char letter)
        {
            return letter >= 97 && letter <= 122;
        }

        public static bool ContainsAccents(this string text)
        {
            var ascText = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(text));
            return !text.Equals(ascText);
        }
    }
}