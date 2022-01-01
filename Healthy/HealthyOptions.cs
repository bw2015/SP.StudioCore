using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Healthy
{
    public class HealthyOptions
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string? ServiceName { get; set; }
        /// <summary>
        /// 机器名称
        /// </summary>
        public string? HostName { get; set; }

        public string? Host { get; set; }

        /// <summary>
        /// 服务端口 没有指定端口为非Web应用程序
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// 服务健康检查地址
        /// </summary>
        public string? HealthCheck { get; set; }

        public string Register { get; set; } = "/healthy/register";

        /// <summary>
        /// 标识
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 服务端地址
        /// </summary>
        public string? Address { get; set; }

        public string AddressCheck { get; set; } = "/healthy/check";

        /// <summary>
        /// 检查间隔时间
        /// </summary>
        public TimeSpan? Interval { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan? Timeout { get; set; }

    }
}
