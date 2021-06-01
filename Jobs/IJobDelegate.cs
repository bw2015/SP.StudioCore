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
        /// <returns></returns>
        public bool LockJob(string key);

        /// <summary>
        /// 解锁任务
        /// </summary>
        /// <param name="key"></param>
        public void UnlockJob(string key);
    }
}
