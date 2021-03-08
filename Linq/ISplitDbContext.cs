using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Linq
{

    /// <summary>
    /// 可水平拆分的数据库链接上下文对象
    /// </summary>
    public interface ISplitDbContext
    {
        /// <summary>
        /// 设定链接字符串
        /// </summary>
        /// <param name="connection"></param>
        void SetDataConnection(string dbConnection);
    }
}
