using Aliyun.OSS;
using SP.StudioCore.Enums;
using SP.StudioCore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace SP.StudioCore.Model
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    public class AdminMenu
    {
        public AdminMenu() { }

        public AdminMenu(XElement root, bool menuOnly, params string[] permission) : this(root, menuOnly, null, permission)
        {

        }

        /// <summary>
        /// 自带选中效果的输出全部权限菜单，一般用于权限编辑页面
        /// </summary>
        public AdminMenu(XElement root, string[] permissions, Func<string, string>? getMenuName = null)
        {
            this.ID = root.GetAttributeValue("ID");
            this.Href = root.GetAttributeValue("href");
            this.Icon = root.GetAttributeValue("icon");
            string? name = root.GetAttributeValue("name");
            if (getMenuName != null)
            {
                name = getMenuName(this.ID ?? string.Empty);
                if (string.IsNullOrEmpty(name)) name = root.GetAttributeValue("name");
            }
            this.Name = name;
            this.IsChecked = permissions.Contains(this.ID);

            this.menu = new List<AdminMenu>();
            foreach (XElement item in root.Elements())
            {
                this.menu.Add(new AdminMenu(item, permissions, getMenuName));
            }
        }

        public AdminMenu(XElement root, bool menuOnly, Func<string, string>? getMenuName, params string[] permission)
        {
            this.ID = root.GetAttributeValue("ID");
            this.Href = root.GetAttributeValue("href");
            this.Icon = root.GetAttributeValue("icon");
            string? name = root.GetAttributeValue("name");
            if (getMenuName != null)
            {
                name = getMenuName(this.ID ?? string.Empty);
                if (string.IsNullOrEmpty(name)) name = root.GetAttributeValue("name");
            }
            this.Name = name;
            this.IsChecked = permission == null || permission.Contains(this.ID);
            this.menu = new List<AdminMenu>();
            foreach (XElement item in root.Elements())
            {
                // 如果只输出菜单
                if (menuOnly && item.Name.ToString() != "menu") continue;
                // 如果没有权限
                if (permission != null && menuOnly && !permission.Contains(item.GetAttributeValue("ID"))) continue;

                this.menu.Add(new AdminMenu(item, menuOnly, getMenuName, permission));
            }
        }
        public AdminMenu(string? name, string? id, string? href, string? icon, bool isChecked, List<AdminMenu>? menu)
        {
            Name = name;
            ID = id;
            Href = href;
            Icon = icon;
            IsChecked = isChecked;
            this.menu = menu;
        }

        /// <summary>
        /// 权限名字
        /// </summary>
        public string? Name { get; private set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        public string? ID { get; private set; }

        /// <summary>
        /// 链接地址
        /// </summary>
        public string? Href { get; private set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; private set; }

        /// <summary>
        /// 是否已经拥有该权限
        /// </summary>
        public bool IsChecked { get; private set; }

        /// <summary>
        /// 下级菜单
        /// </summary>
        public List<AdminMenu>? menu { get; private set; }


        /// <summary>
        /// 转化成为JSON字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"name\":\"{0}\"", this.Name)
                .AppendFormat(",\"id\":\"{0}\"", this.ID);
            if (!string.IsNullOrEmpty(this.Href))
            {
                sb.AppendFormat(",\"href\":\"{0}\"", this.Href);
            }
            if (!string.IsNullOrEmpty(this.Icon))
            {
                sb.AppendFormat(",\"icon\":\"{0}\"", this.Icon);
            }
            sb.AppendFormat(",\"checked\":{0}", this.IsChecked ? 1 : 0);
            if (this.menu != null && this.menu.Count > 0)
            {
                sb.AppendFormat(",\"menu\":[{0}]",
                    string.Join(",", this.menu.Select(t => t.ToString())));
            }
            sb.Append("}");
            return sb.ToString();
        }
        public List<AdminMenu> ToList()
        {
            return this.menu;
        }
        /// <summary>
        /// 获取权限
        /// </summary>
        /// <param name="root"></param>
        /// <param name="filterPermission">过滤那些权限</param>
        /// <returns></returns>
        public static IEnumerable<string> GetPermissions(XElement root, params string[] filterPermission)
        {
            foreach (XElement item in root.Elements())
            {
                string id = item.GetAttributeValue("ID");
                if (filterPermission != null) if (filterPermission.Any(c => c == id)) continue;
                if (!string.IsNullOrEmpty(id)) yield return id;
                foreach (string itemId in GetPermissions(item, filterPermission))
                {
                    yield return itemId;
                }
            }
        }

        /// <summary>
        /// 获取权限列表
        /// </summary>
        /// <param name="root"></param>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public static IEnumerable<string> GetPermissions(XElement root, Func<XElement, bool> filter)
        {
            foreach (XElement item in root.Elements())
            {
                string? id = item.GetAttributeValue("ID");
                if (string.IsNullOrEmpty(id)) continue;
                if (!filter(item)) continue;
                if (!string.IsNullOrEmpty(id)) yield return id;
                foreach (string itemId in GetPermissions(item, filter))
                {
                    yield return itemId;
                }
            }
        }
    }
}
