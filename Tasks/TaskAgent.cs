using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;


namespace SP.StudioCore.Tasks
{
    public static class TaskAgent
    {
        /// <summary>
        /// 仿JS的setTimeout方法
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval"></param>
        public static void setTimeout(Action action, double interval)
        {
            Timer timer = new Timer(interval);
            timer.Elapsed += (sender, e) =>
            {
                action.Invoke();
                timer.Enabled = false;
                timer.Dispose();
            };
            timer.Start();
        }
    }
}
