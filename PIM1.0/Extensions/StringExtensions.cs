namespace IngameScript
{
    public static class StringExtensions
    {
        public static bool TryParseDouble(this string str, out double result, double defaultValue = 0)
        {
            if (double.TryParse(str, out result))
            {
                return true;
            }

            result = defaultValue;
            return false;
        }

        public static bool TryParseInt(this string str, out int result, int defaultValue = 0)
        {
            if (int.TryParse(str, out result))
            {
                return true;
            }
            result = defaultValue;
            return false;
        }

        public static bool TryParseLong(this string str, out long result, long defaultValue = 0)
        {
            if (long.TryParse(str, out result))
            {
                return true;
            }
            result = defaultValue;
            return false;
        }
    }
}
