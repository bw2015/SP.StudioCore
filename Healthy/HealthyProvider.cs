using SP.StudioCore.Array;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP.StudioCore.Healthy
{
    internal static class HealthyProvider
    {
        public static void Run(HealthyOptions options)
        {
            //定义客户端ID
            string clientId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(options.ServiceName)) return;
            Task.Run(() =>
            {
                try
                {
                    var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(c => c.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
                    //注册服务
                    string reuslt = NetAgent.UploadData($"{options.Address}{options.Register}", new Dictionary<string, object> {
                        { "ClientID", clientId },
                        { "ServiceName", options.ServiceName },
                        { "Host", options.Host==null?(ip==null?"127.0.0.1":ip.ToString()):options.Host },
                        { "Port", options.Port?? 0},
                        { "HostName",Dns.GetHostName() },
                        { "Tags",options.Tags??string.Empty },
                        { "HealthCheck", options.HealthCheck??string.Empty }
                     }.ToQueryString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            if (!options.Port.HasValue)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            NetAgent.UploadData($"{options.Address}/healthy/check", new Dictionary<string, object> {
                                        { "ClientID", clientId },
                               }.ToQueryString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            Thread.Sleep(options.Interval.HasValue ? options.Interval.Value.Milliseconds : 1000 * 5);
                        }
                    }
                });
            }
        }
    }
}
