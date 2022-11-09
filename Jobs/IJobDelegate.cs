using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Jobs
{
    public interface IJobDelegate
    {
        /// <summary>
        /// 锁定任务
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time">锁定的时间（毫秒）</param>
        /// <returns></returns>
        public bool LockJob(string key, int time);

        /// <summary>
        /// 解锁任务
        /// </summary>
        /// <param name="key"></param>
        public void UnlockJob(string key);

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="ex"></param>
        public void Exception(Exception ex);

        /// <summary>
        /// 保存服务日志
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="result"></param>
        /// <param name="time">运行耗时</param>
        public void ServiceLog(string jobName, JobResult result, long time);
    }
}
