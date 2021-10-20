using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.Data.Schema
{
    /// <summary>
    /// 字段属性
    /// </summary>
    public struct ColumnProperty
    {
        public ColumnProperty(PropertyInfo property)
        {
            this.Property = property;
            ColumnAttribute? column = property.GetAttribute<ColumnAttribute>();
            DatabaseGeneratedAttribute? generate = property.GetAttribute<DatabaseGeneratedAttribute>();
            KeyAttribute? key = property.GetAttribute<KeyAttribute>();

            this.Name = column?.Name;
            this.Identity = generate == null ? false : generate.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
            this.IsKey = key != null;
        }

        /// <summary>
        /// 本身的字段属性
        /// </summary>
        public PropertyInfo Property;

        /// <summary>
        /// 数据库的字段名
        /// </summary>
        public string? Name;

        /// <summary>
        /// 是否自动编号
        /// </summary>
        public bool Identity;

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsKey;

        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="column"></param>
        public static implicit operator bool(ColumnProperty column)
        {
            return !string.IsNullOrEmpty(column.Name);
        }
    }
}
