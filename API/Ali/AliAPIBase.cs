using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.API.Ali
{
    public abstract class AliAPIBase<T> : ISetting where T : ISetting
    {
        /// <summary>
        /// 网关
        /// </summary>
        [Description("网关")]
        public virtual string? Gateway { get; set; }

        [Description("密钥")]
        public virtual string? AppCode { get; set; }

        protected AliAPIBase(string queryString) : base(queryString)
        {
        }
    }
}
