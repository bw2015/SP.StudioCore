using SP.StudioCore.API;
using SP.StudioCore.API.Translates;
using SP.StudioCore.Enums;
using SP.StudioCore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace SP.StudioCore.Model
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    public class AdminMenu
    {
        public AdminMenu(XElement root, bool menuOnly, params string[] permission) : this(Language.CHN, root, menuOnly, permission)
        {

        }

        public AdminMenu(Language language, XElement root, bool menuOnly, params string[] permission)
        {
            this.Name = root.GetAttributeValue("name").Get(language);
            this.ID = root.GetAttributeValue("ID");
            this.Href = root.GetAttributeValue("href");
            this.Icon = root.GetAttributeValue("icon");
            this.IsChecked = permission == null ? true : permission.Contains(this.ID);
            this.menu = new List<AdminMenu>();
            foreach (XElement item in root.Elements())
            {
                if (menuOnly && (item.Name.ToString() != "menu" ||
                      (permission != null && !permission.Contains(item.GetAttributeValue("ID"))))) continue;
                this.menu.Add(new AdminMenu(language, item, menuOnly, permission));
            }
        }

        /// <summary>
        /// 权限名字
        /// </summary>
        private readonly string Name;

        /// <summary>
        /// 权限ID
        /// </summary>
        private readonly string ID;

        /// <summary>
        /// 链接地址
        /// </summary>
        private readonly string Href;

        /// <summary>
        /// 图标
        /// </summary>
        private readonly string Icon;

        /// <summary>
        /// 是否已经拥有该权限
        /// </summary>
        private readonly bool IsChecked;

        /// <summary>
        /// 下级菜单
        /// </summary>
        private readonly List<AdminMenu> menu;


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

        public static IEnumerable<string> GetPermissions(XElement root)
        {
            foreach (XElement item in root.Elements())
            {
                string id = item.GetAttributeValue("ID");
                if (!string.IsNullOrEmpty(id)) yield return id;
                foreach (string itemId in GetPermissions(item))
                {
                    yield return itemId;
                }
            }
        }
    }
}
