using Microsoft.International.Converters.PinYinConverter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SP.StudioCore.Text
{
    /// <summary>
    /// 汉字与拼音的转换方法
    /// </summary>
    public static class PinyinAgent
    {
        /// <summary>
        /// 拼音的首字母输出（如果是字母则原样输出）
        /// </summary>
        /// <param name="chinese"></param>
        /// <returns></returns>
        public static string ToPinYinAbbr(this string chinese)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in chinese)
            {
                if (Regex.IsMatch(ch.ToString(), "[a-zA-Z]"))
                {
                    sb.Append(ch);
                }
                else if (ChineseChar.IsValidChar(ch))
                {
                    ChineseChar cc = new ChineseChar(ch);
                    var arr = cc.Pinyins;
                    if (arr == null || arr.Count == 0) continue;
                    sb.Append(arr[0][0]);
                }
            }
            return sb.ToString();
        }

     
    }
}
