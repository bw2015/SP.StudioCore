using Microsoft.Data.SqlClient;
using SP.StudioCore.Ioc;
using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP.StudioCore.Jobs
{
    /// <summary>
    /// Job工厂
    /// </summary>
    public static class JobFactory
    {
        private static IJobDelegate? JobDelegate = IocCollection.GetService<IJobDelegate>();

        public static async Task Run(Assembly? assembly, string[] args, params string[] runjob)
        {
            await Run<IJobBase>(assembly, args, runjob);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runjob">需要被执行的Job</param>
        public static async Task Run<TJob>(Assembly? assembly, string[] args, params string[] runjob) where TJob : IJobBase
        {
            if (assembly == null) return;
            IEnumerable<TJob?> jobs = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(TJob)) && (!runjob.Any() || runjob.Contains(t.Name)))
                .Select(t => (TJob?)Activator.CreateInstance(t, new[] { args }))
                .Where(t => t != null);

            string? service = assembly?.GetName().Name;
            int index = 0;
            List<TJob> joblist = new List<TJob>();
            foreach (TJob? job in jobs)
            {
                if (job == null) continue;
                for (int i = 0; i < job.ThreadCount; i++)
                {
                    index++;
                    joblist.Add(job);
                    ConsoleHelper.WriteLine($"{index}.{job.GetType().Name}", ConsoleColor.DarkGreen);
                }
            }

            Parallel.ForEach(joblist, async job =>
             {
                 if (job == null) return;

                 while (true)
                 {
                     string jobName = $"{service}:{job.GetType().Name}";
                     bool isRun = false;
                     Stopwatch sw = Stopwatch.StartNew();
                     JobResult? result = null;
                     try
                     {
                         if (job.IsTheard || (JobDelegate != null && JobDelegate.LockJob(jobName, job.Interval)))
                         {
                             result = await Task.Run(job.Execute);
                             JobDelegate?.ServiceLog(result.JobName, result, sw.ElapsedMilliseconds);
                             isRun = true;
                         }
                     }
                     catch (Exception ex)
                     {
                         ConsoleHelper.Error($" {jobName} {ex.Message}");
                         JobDelegate?.Exception(ex);
                     }
                     finally
                     {
                         if (isRun)
                         {
                             ConsoleHelper.WriteLine($"{result?.ToString()} => 执行完毕\t{sw.ElapsedMilliseconds}ms",
                                 (result ?? false) ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                         }
                         else
                         {
                             ConsoleHelper.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {jobName} => 跳过执行\t{sw.ElapsedMilliseconds}ms", ConsoleColor.DarkYellow);
                         }
                         //if (!job.IsTheard)
                         //{
                         //    JobDelegate.UnlockJob(jobName);
                         //}
                     }

                     await Task.Delay(job.Interval);
                 }
             });
        }

    }
}

