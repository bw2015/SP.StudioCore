using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Log
{
    /// <summary>
    /// 本地帮助类
    /// </summary>
    public static class LocalizationHelper
    {
        private static readonly object _lock = new object();
        /// <summary>
        /// 本地日志打印
        /// </summary>
        public static void Log(string message)
        {
            string text = $"{DateTime.Now} {message}";
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
            string path = Directory.GetCurrentDirectory() + $"/Logs/{DateTime.Now.ToString("yyyyMMdd")}.log";
            //检查本地是否存在当天日志文件
            lock (_lock)
            {
                if (!File.Exists(path))
                {
                    //创建并写入
                    FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(text);
                    sw.Close();
                    fs.Close();
                }
                else
                {
                    //打开并写入
                    FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(text);
                    sw.Close();
                    fs.Close();
                }
            }

        }

    }
}
