using Microsoft.Extensions.Logging;
using SP.StudioCore.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Jobs
{
    /// <summary>
    /// 要执行任务的基类
    /// </summary>
    public abstract class IJobBase
    {
        protected ILogger Logger => IocCollection.GetService<ILogger>();

        /// <summary>
        /// 时间执行间隔（毫秒）
        /// </summary>
        public abstract int Interval { get; }

        /// <summary>
        /// 是否允许并发执行
        /// </summary>
        public virtual bool IsTheard => false;

        /// <summary>
        /// 执行任务
        /// </summary>
        public abstract object Execute();
    }
}
