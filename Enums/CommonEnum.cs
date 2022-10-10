using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SP.StudioCore.Enums
{
    /// <summary>
    /// 用户的通用状态
    /// </summary>
    public enum UserStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("锁定")]
        Lock = 1,
        [Description("删除")]
        Deleted = 10
    }

    /// <summary>
    /// 审核状态
    /// </summary>
    public enum CheckStatus : byte
    {
        [Description("待处理")]
        None = 0,
        [Description("成功")]
        Success = 1,
        [Description("失败")]
        Faild = 2
    }

    /// <summary>
    /// 通用的错误信息
    /// </summary>
    public enum ErrorType
    {
        [Description("请先登录")]
        Login,
        [Description("没有权限")]
        Permission,
        [Description("地区限制")]
        IP,
        [Description("请求无效")]
        BadRequest,
        [Description("无效授权")]
        Authorization,
        [Description("系统异常")]
        Exception,
        /// <summary>
        /// 不支持
        /// </summary>
        [Description("不支持")]
        NoSupport,
        Maintain,
        USERLOCK
    }

    /// <summary>
    /// 语种
    /// </summary>
    public enum Language : byte
    {
        [Description("简体中文"), ISO6391("zh-CN")]
        CHN = 0,
        [Description("正體中文"), ISO6391("zh-TW")]
        THN = 1,
        [Description("English"), ISO6391("en")]
        ENG = 2,
        [Description("日本語"), ISO6391("ja")]
        JP = 3,
        [Description("한국어"), ISO6391("ko")]
        KR = 4,
        [Description("Tiếng việt"), ISO6391("vi")]
        VN = 5,
        [Description("ไทย"), ISO6391("th")]
        TH = 6,
        [Description("Español"), ISO6391("es")]
        ES = 7,
        [Description("Português"), ISO6391("pt")]
        PT = 8,
        [Description("Français"), ISO6391("fr")]
        FR = 9,
        [Description("Deutsch"), ISO6391("de")]
        DE = 10,
        [Description("Italiano"), ISO6391("it")]
        IT = 11,
        [Description("Русский"), ISO6391("ru")]
        RU = 12,
        [Description("indonesia"), ISO6391("id")]
        ID = 13
    }

    /// <summary>
    /// 币种
    /// </summary>
    public enum Currency : byte
    {
        [Description("人民币"), CurrencyExchange(1)]
        CNY = 0,
        [Description("美元"), CurrencyExchange(7)]
        USD = 1,
        [Description("新台币"), CurrencyExchange(0.25)]
        TWD = 2,
        [Description("欧元"), CurrencyExchange(8)]
        EUR = 3,
        [Description("泰铢"), CurrencyExchange(0.2)]
        THB = 4,
        [Description("越南盾"), CurrencyExchange(0.0003)]
        VND = 5,
        [Description("印尼盾"), CurrencyExchange(0.0005)]
        IDR = 6,
        [Description("菲律宾比索"), CurrencyExchange(0.125)]
        PHP = 7,
        [Description("俄罗斯卢布"), CurrencyExchange(0.1)]
        RUB = 8,
        [Description("日元"), CurrencyExchange(0.05)]
        JPY = 9,
        [Description("韩元"), CurrencyExchange(0.005)]
        KRW = 10,
        [Description("印度卢比"), CurrencyExchange(0.1)]
        INR = 11,
        [Description("马来西亚林吉特"), CurrencyExchange(1.66)]
        MYR = 12,
        [Description("千越南盾"), CurrencyExchange(0.3)]
        KVND = 13,
        /// <summary>
        /// 千印尼盾
        /// </summary>
        [Description("千印尼盾"), CurrencyExchange(0.47)]
        KIDR = 14,
        [Description("金币"), CurrencyExchange(1)]
        COIN = 15,
        [Description("港币"), CurrencyExchange(0.82)]
        HKD = 16,
        [Description("USDT"), CurrencyExchange(7)]
        USDT = 17,
        [Description("新加坡元"), CurrencyExchange(5)]
        SGD = 18,
        /// <summary>
        /// 英镑
        /// </summary>
        [Description("英镑"), CurrencyExchange(5)]
        GBP = 19,
        /// <summary>
        /// 比特币
        /// </summary>
        [Description("比特币"), CurrencyExchange(135699.07)]
        BTC = 20,
        /// <summary>
        /// 澳元
        /// </summary>
        [Description("澳元"), CurrencyExchange(4.67)]
        AUD = 21,
        /// <summary>
        /// 加拿大元
        /// </summary>
        [Description("加拿大元"), CurrencyExchange(5.25)]
        CAD = 22,
        /// <summary>
        /// 埃及镑
        /// </summary>
        [Description("埃及镑"), CurrencyExchange(0.36)]
        EGP = 23,
        /// <summary>
        /// 阿曼里亚尔
        /// </summary>
        [Description("阿曼里亚尔"), CurrencyExchange(18.51)]
        OMR = 24,
        /// <summary>
        /// 阿尔及利亚第纳尔
        /// </summary>
        [Description("阿尔及利亚第纳尔"), CurrencyExchange(0.051)]
        DZD = 25,
        /// <summary>
        /// 澳门元
        /// </summary>
        [Description("澳门元"), CurrencyExchange(0.88)]
        MOP = 26,
        /// <summary>
        /// 阿联酋迪拉姆
        /// </summary>
        [Description("阿联酋迪拉姆"), CurrencyExchange(1.94)]
        AED = 27,
        /// <summary>
        /// 孟加拉国塔卡
        /// </summary>
        [Description("孟加拉国塔卡")]
        BDT = 28,
        /// <summary>
        /// 汶莱元
        /// </summary>
        BND = 29,
        /// <summary>
        /// 巴西雷亚尔
        /// </summary>
        [Description("巴西雷亚尔")]
        BRL = 30,
        /// <summary>
        /// 瑞士法郎
        /// </summary>
        [Description("瑞士法郎")]
        CHF = 31,
        /// <summary>
        /// 哥伦比亚比索
        /// </summary>
        [Description("哥伦比亚比索")]
        COP = 32,
        /// <summary>
        /// 哈萨克坦吉
        /// </summary>
        [Description("哈萨克坦吉")]
        KZT = 33,
        /// <summary>
        /// 千柬埔寨瑞尔
        /// </summary>
        [Description("千柬埔寨瑞尔")]
        KKHR = 34,
        /// <summary>
        /// 柬埔寨老挝基普
        /// </summary>
        [Description("柬埔寨老挝基普")]
        LAK = 35,
        /// <summary>
        /// 千柬埔寨老挝基普
        /// </summary>
        [Description("千柬埔寨老挝基普")]
        KLAK = 36,
        /// <summary>
        /// 斯里兰卡卢比
        /// </summary>
        [Description("斯里兰卡卢比")]
        LKR = 37,
        /// <summary>
        /// 缅甸元
        /// </summary>
        [Description("缅甸元")]
        MMK = 38,
        /// <summary>
        /// 千缅甸元
        /// </summary>
        [Description("千缅甸元")]
        KMMK = 39,
        /// <summary>
        /// 墨西哥比索
        /// </summary>
        [Description("墨西哥比索")]
        MXN = 40,
        /// <summary>
        /// 蒙古图格里克
        /// </summary>
        [Description("蒙古图格里克")]
        MNT = 41,
        /// <summary>
        /// 挪威克朗
        /// </summary>
        [Description("挪威克朗")]
        NOK = 42,
        /// <summary>
        /// 新西兰元
        /// </summary>
        [Description("新西兰元")]
        NZD = 43,
        /// <summary>
        /// 尼泊尔卢比
        /// </summary>
        [Description("尼泊尔卢比")]
        NPR = 44,
        /// <summary>
        /// 奈及利亚奈拉
        /// </summary>
        [Description("奈及利亚奈拉")]
        NGN = 45,
        /// <summary>
        /// 巴基斯坦卢比
        /// </summary>
        [Description("巴基斯坦卢比")]
        PKR = 46,
        /// <summary>
        /// 秘鲁新索尔
        /// </summary>
        [Description("秘鲁新索尔")]
        PEN = 47,
        /// <summary>
        /// 瑞典克朗
        /// </summary>
        [Description("瑞典克朗")]
        SEK = 48,
        /// <summary>
        /// 土耳其里拉
        /// </summary>
        [Description("土耳其里拉")]
        TRY = 49,
        /// <summary>
        /// 突尼斯第纳尔
        /// </summary>
        [Description("突尼斯第纳尔")]
        TND = 50,
        /// <summary>
        /// 乌克兰赫夫纳
        /// </summary>
        [Description("乌克兰赫夫纳")]
        UAH = 51,
        /// <summary>
        /// 南非兰特
        /// </summary>
        [Description("南非兰特")]
        ZAR = 52,
        /// <summary>
        /// 津巴布韦元
        /// </summary>
        [Description("津巴布韦元")]
        ZWD = 53
    }

    /// <summary>
    /// 排序规则
    /// </summary>
    public enum SortType
    {
        /// <summary>
        /// 正序
        /// </summary>
        ASC,
        /// <summary>
        /// 倒序
        /// </summary>
        DESC
    }

    /// <summary>
    /// ISO-639-1 语言代码
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ISO6391Attribute : Attribute
    {
        public ISO6391Attribute(string code)
        {
            this.Code = code;
        }

        /// <summary>
        /// ISO-639-1 语言代码
        /// </summary>
        public string Code { get; private set; }

        public static implicit operator string(ISO6391Attribute IOS6391)
        {
            return IOS6391.Code;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CurrencyExchangeAttribute : Attribute
    {
        public CurrencyExchangeAttribute(double exchange)
        {
            this.Exchange = (decimal)exchange;
        }

        /// <summary>
        /// 汇率
        /// </summary>
        public decimal Exchange { get; private set; }
    }
}
