using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Jobs
{
    /// <summary>
    /// 运行的返回
    /// </summary>
    public class JobResult
    {
        /// <summary>
        /// 运行日志
        /// </summary>
        public List<string> logs { get; private set; }

        /// <summary>
        /// Job名称
        /// </summary>
        public string JobName { get; private set; }

        public JobResult(IJobBase job)
        {
            this.JobName = job.GetType().Name;
            this.logs = new List<string>();
        }

        public void Add(string log)
        {
            this.logs.Add(log);
        }

        public int Length => this.logs?.Count ?? 0;

        public void Clear()
        {
            this.logs?.Clear();
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        public override string ToString()
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] - {this.JobName} => {this.logs.ToJson()}";
        }

        public static implicit operator bool(JobResult result)
        {
            return result != null && result.logs != null && result.logs.Any();
        }
    }
}
