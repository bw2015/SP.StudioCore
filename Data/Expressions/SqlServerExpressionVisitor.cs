using Dapper;
using SP.StudioCore.Enums;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Data.Expressions
{
    internal class SqlServerExpressionVisitor : ExpressionVisitor, IExpressionVisitor
    {
        public SqlServerExpressionVisitor()
        {

        }
        public SqlServerExpressionVisitor(Expression expression)
        {
            this.Visit(expression);
        }
        /// <summary>
        /// 字段
        /// </summary>
        protected readonly Stack<string> _field = new Stack<string>();
        /// <summary>
        /// 参数化
        /// </summary>
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        /// <summary>
        /// 各种方法拼接字段
        /// </summary>
        private readonly Dictionary<MethodType, List<string>> _method = new Dictionary<MethodType, List<string>>();
        /// <summary>
        /// 随机参数名
        /// </summary>
        private int paramIndex = 0;
        /// <summary>
        /// 表 实体类
        /// </summary>
        public Type Table { get; private set; }

        /// <summary>
        /// 当前正在执行的方法类型
        /// </summary>
        public MethodType MethodType { get; private set; }
        /// <summary>
        /// 获取sql解析字符串及参数化数据
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string GetCondition(out DynamicParameters parameters)
        {
            parameters = new DynamicParameters();
            string where = string.Concat(_field.ToArray());
            foreach (var item in _parameters)
            {
                parameters.Add(item.Key, item.Value);
            }
            _parameters.Clear();
            _field.Clear();
            return where;
        }
        /// <summary>
        /// 获取查询语句
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetSelect(out DynamicParameters parameters, out MethodType type)
        {
            parameters = new DynamicParameters();
            type = MethodType.None;
            string where = string.Empty;
            if (_field.Count > 0)
            {
                where = "WHERE " + string.Concat(_field.ToArray());
            }
            string field = string.Empty;
            if (this.Table == null) throw new NullReferenceException("泛型类型未指定");
            string table = this.Table.GetTableName();
            string orderby = string.Empty;
            string sql = "SELECT {0} FROM {1} {2} {3}";
            foreach (var item in _parameters)
            {
                parameters.Add(item.Key, item.Value);
            }
            foreach (var item in this._method)
            {
                switch (item.Key)
                {
                    case MethodType.OrderByDescending:
                        orderby = string.Format("ORDER BY {0} DESC", string.Join(",", item.Value.Select(c => $"[{c}]")));
                        break;
                    case MethodType.OrderBy:
                        orderby = string.Format("ORDER BY {0} ASE", string.Join(",", item.Value.Select(c => $"[{c}]")));
                        break;
                    case MethodType.Select:
                        field = string.Join(",", item.Value.Select(c => $"[{c}]"));
                        break;
                    case MethodType.Any:
                        type = item.Key;
                        field = "0";
                        sql = "SELECT 0 WHERE EXISTS(SELECT {0} FROM {1} {2} {3})";
                        break;
                    case MethodType.Count:
                        type = item.Key;
                        field = "0";
                        sql = "SELECT COUNT({0}) FROM {1} {2} {3}";
                        break;
                    case MethodType.FirstOrDefault:
                        type = item.Key;
                        sql = "SELECT TOP 1 {0} FROM {1} {2} {3}";
                        break;
                    default:
                        break;
                }
            }
            return string.Format(sql, field, table, where, orderby);
        }

        public string GetSelect(out DynamicParameters parameters, out Type type)
        {
            parameters = new DynamicParameters();
            type = this.Table;
            string where = string.Empty;
            if (_field.Count > 0)
            {
                where = "WHERE " + string.Concat(_field.ToArray());
            }
            string field = string.Empty;
            if (this.Table == null) throw new NullReferenceException("泛型类型未指定");
            string table = this.Table.GetTableName();
            string orderby = string.Empty;
            string sql = "SELECT {0} FROM {1} {2} {3}";
            foreach (var item in _parameters)
            {
                parameters.Add(item.Key, item.Value);
            }
            foreach (var item in this._method)
            {
                switch (item.Key)
                {
                    case MethodType.OrderByDescending:
                        orderby = string.Format("ORDER BY {0} DESC", string.Join(",", item.Value.Select(c => $"[{c}]")));
                        break;
                    case MethodType.OrderBy:
                        orderby = string.Format("ORDER BY {0} ASC", string.Join(",", item.Value.Select(c => $"[{c}]")));
                        break;
                    case MethodType.Select:
                        field = string.Join(",", item.Value.Select(c => $"[{c}]"));
                        break;
                    case MethodType.Take:
                        sql = "SELECT TOP " + string.Join(",", item.Value) + " {0} FROM {1} {2} {3}";
                        break;
                    default:
                        break;
                }
            }
            return string.Format(sql, field, table, where, orderby);
        }
        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }
        /// <summary>
        /// 常量表达式数
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node == null) throw new ArgumentNullException("ConstantExpression");
            object value = node.Value;
            Type type = value.GetType();
            TypeCode code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Boolean:
                    _field.Push((((bool)value) ? 1 : 0).ToString());
                    break;
                case TypeCode.Int64:
                    _field.Push(((long)value).ToString());
                    break;
                case TypeCode.DBNull:
                    break;
                case TypeCode.Int32:
                    if (this.MethodType == MethodType.Take || this.MethodType == MethodType.Skip)
                    {
                        _method[this.MethodType].Add($"{value}");
                    }
                    else
                    {
                        _field.Push(((int)value).ToString());
                    }
                    break;
                case TypeCode.Object:
                    //获取泛型类型
                    this.Table = type.GenericTypeArguments[0];
                    if (_method.ContainsKey(MethodType.Select)
                     || _method.ContainsKey(MethodType.Any)
                     || _method.ContainsKey(MethodType.Count))
                    {
                        // 此操作，不查询字段
                    }
                    else
                    {
                        foreach (var item in this.Table.GetProperties().Where(c => c.CanWrite && !c.HasAttribute<NotMappedAttribute>()))
                        {
                            string fieldname = this.GetColumnName(item);
                            this.AddColumnName(MethodType.Select, fieldname);
                        }
                    }
                    break;
                default:
                    this.AppendParameter(value);
                    break;
            }
            return node;
        }
        /// <summary>
        /// 解析匿名对象
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression node)
        {
            if (node == null) throw new ArgumentNullException("VisitNew");
            foreach (Expression expression in node.Arguments)
            {
                this.Visit(expression);
            }
            return node;
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null) throw new ArgumentNullException("MemberExpression");
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    this.AppendParameter(node, new Stack<MemberInfo>());
                    break;
                case ExpressionType.Constant:
                    ConstantExpression constant = (ConstantExpression)node.Expression;
                    switch (node.Member.MemberType)
                    {
                        case MemberTypes.Field:
                            this.AppendParameter(((FieldInfo)node.Member).GetValue(constant.Value));
                            break;
                        case MemberTypes.Property:
                            this.AppendParameter(((PropertyInfo)node.Member).GetValue(constant.Value));
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return node;
        }
        /// <summary>
        /// 方法表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }
        /// <summary>
        /// 方法表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node == null) throw new ArgumentNullException("MethodCallExpression");
            this.MethodType = node.Method.Name.ToEnum<MethodType>();
            switch (this.MethodType)
            {
                case MethodType.Any:
                case MethodType.Count:
                case MethodType.FirstOrDefault:
                case MethodType.Take:
                case MethodType.Skip:
                    this.AddColumnName(this.MethodType);
                    break;
                default:
                    break;
            }
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                this.Visit(node.Arguments[node.Arguments.Count - 1 - i]);
            }
            return node;
        }

        /// <summary>
        /// 二元表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node == null) throw new ArgumentNullException("BinaryExpression");
            this._field.Push(")");
            base.Visit(node.Right);//解析右边
            this._field.Push(node.NodeType.GetExpressionType());
            base.Visit(node.Left);//解析左边
            this._field.Push("(");
            return node;
        }

        protected void AppendParameter(MemberExpression node, Stack<MemberInfo> members)
        {
            members.Push(node.Member);
            switch (node.Expression.NodeType)
            {
                case ExpressionType.Constant:
                    this.AppendParameter((ConstantExpression)node.Expression, members);
                    break;
                case ExpressionType.MemberAccess:
                    this.AppendParameter((MemberExpression)node.Expression, members);
                    break;
                case ExpressionType.Parameter:
                    string fieldname = this.GetColumnName(node);
                    this.AddColumnName(this.MethodType, fieldname);
                    break;
                default:
                    break;
            }
        }

        protected void AppendParameter(ConstantExpression node, Stack<MemberInfo> members)
        {
            object value = node.Value;
            foreach (var item in members)
            {
                switch (item.MemberType)
                {
                    case MemberTypes.Field:
                        value = ((FieldInfo)item).GetValue(value) ?? string.Empty;
                        break;
                    case MemberTypes.Property:
                        value = ((PropertyInfo)item).GetValue(value) ?? string.Empty;
                        break;
                    default:
                        break;
                }
            }
            if (value != null)
            {
                this.AppendParameter(value);
            }
        }
        /// <summary>
        /// 组装参数化
        /// </summary>
        /// <param name="value"></param>
        protected void AppendParameter(object value)
        {
            if (value is ConstantExpression) value = ((ConstantExpression)value).Value;
            _field.Push($"@_p{paramIndex}");
            _parameters.Add($"@_p{paramIndex}", value);
            paramIndex++;
        }

        /// <summary>
        /// 获取字段名称
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected string GetColumnName(MemberExpression node)
        {
            ColumnAttribute column = node.Member.GetAttribute<ColumnAttribute>();
            return column == null ? node.Member.Name : column.Name;
        }
        protected string GetColumnName(PropertyInfo property)
        {
            ColumnAttribute column = property.GetAttribute<ColumnAttribute>();
            return column == null ? property.Name : column.Name;
        }

        /// <summary>
        /// 添加各种类型字段
        /// </summary>
        /// <param name="name"></param>
        protected void AddColumnName(MethodType type, string name)
        {
            switch (type)
            {
                case MethodType.None:
                    break;
                case MethodType.StartsWith:
                case MethodType.Contains:
                case MethodType.EndsWith:
                case MethodType.Where:
                    _field.Push(name);
                    break;
                case MethodType.OrderByDescending:
                case MethodType.OrderBy:
                case MethodType.Select:
                    if (!_method.ContainsKey(type))
                    {
                        _method.Add(type, new List<string>());
                    }
                    lock (_method)
                    {
                        _method[type].Add(name);
                    }
                    break;
                default:
                    break;
            }
        }
        protected void AddColumnName(MethodType type)
        {
            if (!_method.ContainsKey(type))
            {
                _method.Add(type, new List<string>());
            }
        }
        public void Dispose()
        {
            _parameters.Clear();
            _field.Clear();
            _method.Clear();
        }
    }
}
