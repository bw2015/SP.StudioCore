using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP.StudioCore.Services
{
    /// <summary>
    /// 标记一个定时任务
    /// </summary>
    public abstract class BackgroundService
    {
        /// <summary>
        /// 间隔时间（默认1秒中执行一次）
        /// </summary>
        public virtual TimeSpan Time { get; set; } = TimeSpan.FromSeconds(1);
        /// <summary>
        /// 任务状态
        /// </summary>
        private bool Status = true;
        /// <summary>
        /// 开始任务
        /// </summary>
        public void Start()
        {
            while (Status)
            {
                try
                {
                    this.Execute();
                    Thread.Sleep(Time);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        /// <summary>
        /// 停止任务
        /// </summary>
        public void End()
        {
            this.Status = false;
        }
        /// <summary>
        /// 重启任务
        /// </summary>
        public void Reset()
        {
            this.Status = true;
        }
        public abstract void Execute();
    }
}
