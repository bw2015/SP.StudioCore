using SP.StudioCore.Array;
using SP.StudioCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Web
{
    /// <summary>
    /// 昵称（反序列化资源文件 nickname.json）
    /// </summary>
    internal class NickNameModel
    {
        public ChineseNickNameModel? Chinese { get; set; }

        public EnglishNickNameModel? English { get; set; }

        public string Generate(Language language)
        {
            string name = string.Empty;
            if (language == Language.CHN && this.Chinese != null)
            {
                name = this.Chinese.ToString();
            }
            if (!string.IsNullOrEmpty(name)) return name;
            if (this.English != null) name = this.English.ToString();
            return name;
        }
    }

    internal class ChineseNickNameModel
    {
        public string[]? FirstName { get; set; }

        public string[]? Male { get; set; }

        public string[]? Fename { get; set; }

        public override string ToString()
        {
            string firstName = this.FirstName == null ? string.Empty : FirstName.GetRandom();
            List<string[]> namelist = new List<string[]>();
            if (this.Male != null) namelist.Add(this.Male);
            if (this.Fename != null) namelist.Add(this.Fename);
            if (!namelist.Any()) return firstName;
            string name = namelist.GetRandom().GetRandom();
            return string.Concat(firstName, name);
        }
    }

    internal class EnglishNickNameModel
    {
        public string[]? Name { get; set; }

        public override string ToString()
        {
            return this.Name?.GetRandom() ?? string.Empty;
        }
    }

}
