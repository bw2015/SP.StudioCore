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
        [Description("千印尼盾"), CurrencyExchange(0.47)]
        KIDR = 14,
        [Description("金币"), CurrencyExchange(1)]
        COIN = 15,
        [Description("港币"), CurrencyExchange(0.82)]
        HKD
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
