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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runjob">需要被执行的Job</param>
        public static void Run(Assembly assembly, params string[] runjob)
        {
            if (assembly == null) return;
            IEnumerable<IJobBase?> jobs = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(IJobBase)) && (!runjob.Any() || runjob.Contains(t.Name)))
                .Select(t => (IJobBase?)Activator.CreateInstance(t));

            string? service = assembly?.GetName().Name;

            ConsoleHelper.WriteLine("JobName    |   FullName    ", ConsoleColor.Green);
            foreach (var job in jobs)
            {
                if (job == null) continue;
                ConsoleHelper.WriteLine($"{job.GetType().Name}    |   {job.GetType().FullName}    ", ConsoleColor.DarkGreen);
            }


            Parallel.ForEach(jobs, job =>
             {
                 if (job == null) return;

                 while (true)
                 {
                     string jobName = $"{service}:{job.GetType().Name}";
                     bool isRun = false;
                     Stopwatch sw = Stopwatch.StartNew();
                     try
                     {
                         if (job.IsTheard || (JobDelegate != null && JobDelegate.LockJob(jobName, job.Interval)))
                         {
                             object content = job.Execute();
                             JobDelegate?.ServiceLog(job.GetType().Name, content, sw.ElapsedMilliseconds);
                             isRun = true;
                         }
                     }
                     catch (SqlException ex)
                     {
                         if (ex.Number == 18456)
                         {
                             JobDelegate?.Exception(new Exception("database login failed"));
                         }
                     }
                     catch (Exception ex)
                     {
                         ConsoleHelper.WriteLine(ex.Message, ConsoleColor.Red);
                         JobDelegate?.Exception(ex);
                     }
                     finally
                     {
                         if (isRun)
                         {
                             ConsoleHelper.WriteLine($"[{DateTime.Now}]\tJob:{jobName} => 执行完毕\t{sw.ElapsedMilliseconds}ms", ConsoleColor.Green);
                         }
                         else
                         {
                             ConsoleHelper.WriteLine($"[{DateTime.Now}]\tJob:{jobName} => 跳过执行\t{sw.ElapsedMilliseconds}ms", ConsoleColor.DarkYellow);
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

