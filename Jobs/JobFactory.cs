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
        private static IJobDelegate JobDelegate = IocCollection.GetService<IJobDelegate>();

        public static void Run(Assembly assembly, ParallelOptions options = null)
        {
            if (assembly == null) return;
            IEnumerable<IJobBase?> jobs = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(IJobBase)))
                .Select(t => (IJobBase?)Activator.CreateInstance(t));

            string? service = assembly?.GetName().Name;

            if (options == null)
            {
                options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount * 4, jobs.Count())
                };
            }
            List<string> logs = new List<string>();
            while (true)
            {
                int interval = 0;
                logs.Clear();
                Parallel.ForEach(jobs, options,
                job =>
                {
                    string jobName = job.GetType().Name;
                    string lockKey = $"{service}:{jobName}";
                    Stopwatch sw = new();
                    sw.Restart();
                    try
                    {
                        if (job.IsTheard || JobDelegate.LockJob(lockKey))
                        {
                            if (job.Execute().Result)
                            {
                                Console.WriteLine($"Job:{jobName}执行完毕，耗时：{sw.ElapsedMilliseconds}ms");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteLine(ex.Message, ConsoleColor.Red);
                    }
                    finally
                    {
                        if (!job.IsTheard)
                        {
                            JobDelegate.UnlockJob(lockKey);
                        }
                    }
                    if (interval < job.Interval) interval = job.Interval;
                });
                Thread.Sleep(interval);
            }

        }
    }
}
