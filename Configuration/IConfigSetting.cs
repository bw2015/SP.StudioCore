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
        public string Key { get; set; }
    }
}
