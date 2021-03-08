using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SP.StudioCore.Services
{
    /// <summary>
    /// 服务的基类
    /// </summary>
    public abstract class IAction
    {
        protected readonly Stopwatch sw;

        /// <summary>
        /// 循環的休眠時間（毫秒）
        /// </summary>
        protected virtual int SleepTime => 1000;

        /// <summary>
        /// 执行次数
        /// </summary>
        protected int TimeCount = 0;

        protected readonly string[] args;

        protected IAction(string[] args)
        {
            sw = new Stopwatch();
            this.args = args;
            while (true)
            {
                try
                {
                    _ = this.Run();
                }
                catch (Exception ex)
                {
                    this.Error(ex);
                }
                finally
                {
                    this.TimeCount++;
                    Thread.Sleep(this.SleepTime);
                }
            }
        }

        protected abstract int Run();

        /// <summary>
        /// 處理錯誤信息
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void Error(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        /// <summary>
        /// 对外显示
        /// </summary>
        protected virtual ConsoleModel WriteLine(string type, int count, int time, string message)
        {
            ConsoleModel model = new ConsoleModel(type, count, time, message);
            this.SaveConsoleModel(model);
            Console.WriteLine(model.ToString());
            return model;
        }

        protected virtual void SaveConsoleModel(ConsoleModel model)
        {

        }
    }
}
