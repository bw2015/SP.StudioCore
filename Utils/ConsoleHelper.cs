using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Utils
{
    public static class ConsoleHelper
    {
        private static object _lock = new object();

        public static string WriteLine(string message, ConsoleColor color)
        {
            lock (_lock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            return message;
        }

        public static string Write(string message, ConsoleColor color)
        {
            lock (_lock)
            {
                Console.ForegroundColor = color;
                Console.Write(message);
                Console.ResetColor();
            }
            return message;
        }

        private static readonly char TOKEN_INDENT_OPEN = '{';
        private static readonly char TOKEN_INDENT_CLOSE = '}';
        private static readonly char TOKEN_DIVIDER = ';';
        private static readonly char NEW_LINE = '\n';
        private static readonly char TAB = '\t';
        private static readonly char DOT = '.';
        private static readonly int HEX_BYTES_PER_LINE = 16;
        private static int MAX_DUMP_LENGTH = 1024;
        /// <summary>
        /// 二进制的调试信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bytesPerLine"></param>
        /// <returns></returns>
        public static string GetHexDump(this byte[] data, int bytesPerLine = 16)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Binary Size: " + data.Length + NEW_LINE);
            if (data == null || data.Length == 0) return stringBuilder.ToString();
            if (data.Length > MAX_DUMP_LENGTH)
            {
                stringBuilder.Append("** Data larger than max dump size of " + MAX_DUMP_LENGTH + ". Data not displayed");
                return stringBuilder.ToString();
            }

            StringBuilder stringBuilder2 = new StringBuilder();
            StringBuilder stringBuilder3 = new StringBuilder();
            int num = 0;
            int num2 = 0;
            do
            {
                byte b = data[num];
                string text = $"{b:x2}";
                if (text.Length == 1)
                {
                    text = "0" + text;
                }

                stringBuilder2.Append(text + " ");
                char value = (b < 33 || b > 126) ? DOT : Convert.ToChar(b);
                stringBuilder3.Append(value);
                if (++num2 == bytesPerLine)
                {
                    num2 = 0;
                    stringBuilder.Append(stringBuilder2.ToString() + TAB + stringBuilder3.ToString() + NEW_LINE);
                    stringBuilder2 = new StringBuilder();
                    stringBuilder3 = new StringBuilder();
                }
            }
            while (++num < data.Length);
            if (num2 != 0)
            {
                for (int num3 = bytesPerLine - num2; num3 > 0; num3--)
                {
                    stringBuilder2.Append("   ");
                    stringBuilder3.Append(" ");
                }

                stringBuilder.Append(stringBuilder2.ToString() + TAB + stringBuilder3.ToString() + NEW_LINE);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 在同一行复写
        /// </summary>
        public static void OverWrite(string context)
        {
            lock (_lock)
            {
                Console.WriteLine(context); 
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }

        }
    }
}
