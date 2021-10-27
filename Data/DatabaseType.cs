using SP.StudioCore.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SP.StudioCore.Data
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    [EnumConfig(ignore:false)]
    public enum DatabaseType : byte
    {
        /// <summary>
        /// SQL SERVER 2005 and high
        /// </summary>
        [Description("System.Data.SqlClient")]
        SqlServer,
        [Description("System.Data.Oledb")]
        Access,
        [Description("MySql.Data.MySqlClient")]
        MySql,
        [Description("System.Data.Sqlite")]
        SQLite,
        Oracle
    }
}
