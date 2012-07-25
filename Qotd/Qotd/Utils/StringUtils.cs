using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qotd.Utils
{
    public static class StringUtils
    {
        public static string Crop(this string str, int length)
        {
            if (str.Length > length && length > 3)
                return str.Substring(0, length - 3) + "...";
            return str;
        }
    }
}
