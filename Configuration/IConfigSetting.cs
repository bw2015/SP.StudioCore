using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Configuration
{
    /// <summary>
    /// appsetting配置
    /// </summary>
    public interface IConfigSetting
    {
        /// <summary>
        /// 获取配置内容
        /// </summary>
        public string GetConfigContent(string content);
    }
}
