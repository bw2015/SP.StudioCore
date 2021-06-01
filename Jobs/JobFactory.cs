using SP.StudioCore.Ioc;
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

        public static void Run(Assembly assembly)
        {
            IEnumerable<IJobBase> jobs = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(IJobBase)))
                .Select(t => (IJobBase)Activator.CreateInstance(t));
            string service = assembly.GetName().Name;

            Parallel.ForEach(jobs, job =>
            {
                string jobName = job.GetType().Name;
                string lockKey = $"{service}:{jobName}";
                Console.WriteLine($"Job任务：{jobName}开始执行");
                Stopwatch sw = new();
                while (true)
                {
                    sw.Restart();
                    try
                    {
                        if (job.IsTheard || JobDelegate.LockJob(lockKey))
                        {
                            job.Execute();
                            Console.WriteLine($"Job:{jobName}执行完毕，耗时：{sw.ElapsedMilliseconds}ms");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {                        
                        if (!job.IsTheard)
                        {
                            JobDelegate.UnlockJob(lockKey);
                        }
                    }
                    Thread.Sleep(job.Interval);
                }
            });

        }
    }
}
