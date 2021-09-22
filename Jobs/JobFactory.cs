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

        public static void Run(Assembly assembly)
        {
            if (assembly == null) return;
            IEnumerable<IJobBase?> jobs = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(IJobBase)))
                .Select(t => (IJobBase?)Activator.CreateInstance(t));

            string? service = assembly?.GetName().Name;

            Parallel.ForEach(jobs, job =>
             {
                 if (job == null) return;

                 while (true)
                 {
                     string jobName = $"{service}:{job.GetType().Name}";
                     bool isRun = false;
                     try
                     {
                         if (job.IsTheard || JobDelegate.LockJob(jobName, job.Interval))
                         {
                             job.Execute().Wait();
                             isRun = true;
                         }
                     }
                     catch (Exception ex)
                     {
                         ConsoleHelper.WriteLine(ex.Message, ConsoleColor.Red);
                         JobDelegate.Exception(ex);
                     }
                     finally
                     {
                         if (isRun)
                         {
                             ConsoleHelper.WriteLine($"[{DateTime.Now}]\tJob:{jobName} => 执行完毕", ConsoleColor.Green);
                         }
                         else
                         {
                             ConsoleHelper.WriteLine($"[{DateTime.Now}]\tJob:{jobName} => 跳过执行", ConsoleColor.DarkYellow);
                         }
                         //if (!job.IsTheard)
                         //{
                         //    JobDelegate.UnlockJob(jobName);
                         //}
                     }
                     Thread.Sleep(job.Interval);
                 }
             });
        }

    }
}

