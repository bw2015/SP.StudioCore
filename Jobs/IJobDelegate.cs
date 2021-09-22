﻿using System;
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
    }
}
