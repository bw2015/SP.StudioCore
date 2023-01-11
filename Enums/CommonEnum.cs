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
    /// 性别
    /// </summary>
    public enum Gender : byte
    {
        [Description("男")]
        Male = 1,
        [Description("女")]
        Female = 2
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
        [Description("维护中")]
        Maintain,
        [Description("当前账号被锁定")]
        UserLock,
        [Description("未绑定谷歌验证码")]
        SecretKey,
        [Description("未设置资金密码")]
        PayPassword
    }

    /// <summary>
    /// 语种
    /// </summary>
    public enum Language : byte
    {
        /// <summary>
        /// 简体中文
        /// </summary>
        [Description("简体中文"), ISO6391("zh-CN", "zh")]
        CHN = 0,
        /// <summary>
        /// 繁体中文
        /// </summary>
        [Description("正體中文"), ISO6391("zh-TW", "cht")]
        THN = 1,
        /// <summary>
        /// 英文
        /// </summary>
        [Description("English"), ISO6391("en", "en")]
        ENG = 2,
        /// <summary>
        /// 日语
        /// </summary>
        [Description("日本語"), ISO6391("ja", "jp")]
        JA = 3,
        /// <summary>
        /// 韩语
        /// </summary>
        [Description("한국어"), ISO6391("ko", "kor")]
        KO = 4,
        /// <summary>
        /// 越南语
        /// </summary>
        [Description("Tiếng việt"), ISO6391("vi", "vie")]
        VI = 5,
        /// <summary>
        /// 泰语
        /// </summary>
        [Description("ไทย"), ISO6391("th", "th")]
        TH = 6,
        /// <summary>
        /// 西班牙语
        /// </summary>
        [Description("Español"), ISO6391("es", "spa")]
        ES = 7,
        /// <summary>
        /// 葡萄牙语
        /// </summary>
        [Description("Português"), ISO6391("pt", "pt")]
        PT = 8,
        /// <summary>
        /// 法语
        /// </summary>
        [Description("Français"), ISO6391("fr", "fra")]
        FR = 9,
        /// <summary>
        /// 德语
        /// </summary>
        [Description("Deutsch"), ISO6391("de", "de")]
        DE = 10,
        /// <summary>
        /// 意大利语
        /// </summary>
        [Description("Italiano"), ISO6391("it", "it")]
        IT = 11,
        /// <summary>
        /// 俄语
        /// </summary>
        [Description("Русский"), ISO6391("ru", "ru")]
        RU = 12,
        /// <summary>
        /// 印尼语
        /// </summary>
        [Description("indonesia"), ISO6391("id", "id")]
        IND = 13,
        /// <summary>
        /// 丹麦语
        /// </summary>
        [Description("dansk"), ISO6391("da", "dan")]
        DA = 14,
        /// <summary>
        /// 芬兰文
        /// </summary>
        [Description("Suomalainen"), ISO6391("fi", "fin")]
        FI = 15,
        /// <summary>
        /// 荷兰文
        /// </summary>
        [Description("Nederlands"), ISO6391("nl", "nl")]
        NL = 16,
        /// <summary>
        /// 挪威文
        /// </summary>
        [Description("norsk"), ISO6391("no", "nor")]
        NO = 17,
        /// <summary>
        /// 波兰文
        /// </summary>
        [Description("Polski"), ISO6391("pl", "pl")]
        PL = 18,
        /// <summary>
        /// 罗马尼亚文
        /// </summary>
        [Description("rumunjski"), ISO6391("ro", "rom")]
        RO = 19,
        /// <summary>
        /// 瑞典文
        /// </summary>
        [Description("svenska"), ISO6391("sv", "swe")]
        SV = 20,
        /// <summary>
        /// 土耳其文
        /// </summary>
        [Description("Türk"), ISO6391("tr", "tr")]
        TR = 21,
        /// <summary>
        /// 缅甸文
        /// </summary>
        [Description("မြန်မာ"), ISO6391("my", "bur")]
        MY = 22,
        /// <summary>
        /// 阿拉伯语
        /// </summary>
        [Description("العربية"), ISO6391("ar", "ara")]
        AR = 23,
        /// <summary>
        /// 印地语
        /// </summary>
        [Description("हिन्दी"), ISO6391("hi", "hi")]
        HI = 24
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
        /// <summary>
        /// 韩元
        /// </summary>
        [Description("韩元"), CurrencyExchange(0.005)]
        KRW = 10,
        [Description("印度卢比"), CurrencyExchange(0.1)]
        INR = 11,
        /// <summary>
        /// 马来西亚林吉特
        /// </summary>
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
        /// <summary>
        /// 新加坡元
        /// </summary>
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
        [Description("比特币"), ChainCurrency, CurrencyExchange(135699.07)]
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
        [Description("汶莱元")]
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
        ZWD = 53,
        /// <summary>
        /// 阿富汗
        /// </summary>
        [Description("阿富汗")]
        AFN = 54,
        /// <summary>
        /// 阿尔巴尼亚列克
        /// </summary>
        [Description("阿尔巴尼亚列克")]
        ALL = 55,
        /// <summary>
        /// 亚美尼亚德拉姆
        /// </summary>
        [Description("亚美尼亚德拉姆")]
        AMD = 56,
        /// <summary>
        /// 安的列斯盾
        /// </summary>
        [Description("安的列斯盾")]
        ANG = 57,
        /// <summary>
        /// 安哥拉宽扎
        /// </summary>
        [Description("安哥拉宽扎")]
        AOA = 58,
        /// <summary>
        /// 阿根廷比索
        /// </summary>
        [Description("阿根廷比索")]
        ARS = 59,
        /// <summary>
        /// 阿鲁巴弗罗林
        /// </summary>
        [Description("阿鲁巴弗罗林")]
        AWG = 60,
        /// <summary>
        /// 阿塞拜疆马纳特
        /// </summary>
        [Description("阿塞拜疆马纳特")]
        AZN = 61,
        /// <summary>
        /// 波黑可兑换马克
        /// </summary>
        [Description("波黑可兑换马克")]
        BAM = 62,
        /// <summary>
        /// 巴巴多斯元
        /// </summary>
        [Description("巴巴多斯元")]
        BBD = 63,
        /// <summary>
        /// 巴林第纳尔
        /// </summary>
        [Description("巴林第纳尔")]
        BHD = 64,
        /// <summary>
        /// 布隆迪法郎
        /// </summary>
        [Description("布隆迪法郎")]
        BIF = 65,
        /// <summary>
        /// 千布隆迪法郎
        /// </summary>
        [Description("千布隆迪法郎")]
        KBIF = 66,
        [Description("保加利亚列弗")]
        BGN = 67,
        [Description("玻利维亚诺")]
        BOB = 68,
        [Description("巴哈马元")]
        BSD = 69,
        [Description("不丹努尔特鲁姆")]
        BTN = 70,
        [Description("博茨瓦纳普拉")]
        BWP = 71,
        [Description("白俄罗斯卢布")]
        BYN = 72,
        [Description("伯利兹元")]
        BZD = 73,
        [Description("刚果法郎")]
        CDF = 74,
        [Description("刚果法郎")]
        KCDF = 75,
        [Description("智利比索")]
        CLP = 76,
        [Description("哥斯达黎加科朗")]
        CRC = 77,
        [Description("千哥伦比亚比索")]
        KCRC = 78,
        [Description("塞尔维亚第纳尔")]
        CSD = 79,
        [Description("古巴比索")]
        CUP = 80,
        [Description("佛得角埃斯库多")]
        CVE = 81,
        [Description("捷克克朗")]
        CZK = 82,
        [Description("吉布提法郎")]
        DJF = 83,
        [Description("丹麦克朗")]
        DKK = 84,
        [Description("多米尼加比索")]
        DOP = 85,
        [Description("厄立特里亚纳克法")]
        ERN = 86,
        [Description("埃塞俄比亚比尔")]
        ETB = 87,
        [Description("斐济元")]
        FJD = 88,
        [Description("格鲁吉亚拉里")]
        GEL = 89,
        [Description("加纳塞地")]
        GHS = 90,
        [Description("直布罗陀庞德")]
        GIP = 91,
        [Description("冈比亚货币")]
        GMD = 92,
        /// <summary>
        /// 几内亚法郎
        /// </summary>
        [Description("几内亚法郎")]
        GNF = 93,
        /// <summary>
        /// 千几内亚法郎
        /// </summary>
        [Description("千几内亚法郎")]
        KGNF = 94,
        [Description("危地马拉格查尔")]
        GTQ = 95,
        [Description("圭亚那元")]
        GYD = 96,
        [Description("洪都拉斯伦皮拉")]
        HNL = 97,
        [Description("克罗地亚库纳")]
        HRK = 98,
        [Description("海地古德")]
        HTG = 99,
        [Description("匈牙利福林")]
        HUF = 100,
        /// <summary>
        /// 伊拉克第纳尔
        /// </summary>
        [Description("伊拉克第纳尔")]
        IQD = 101,
        /// <summary>
        /// 千伊拉克第纳尔
        /// </summary>
        [Description("千伊拉克第纳尔")]
        KIQD = 102,
        /// <summary>
        /// 伊朗里亚尔
        /// </summary>
        [Description("伊朗里亚尔")]
        IRR = 103,
        /// <summary>
        /// 千伊朗里亚尔
        /// </summary>
        [Description("千伊朗里亚尔")]
        KIRR = 104,
        [Description("冰岛克朗")]
        ISK = 105,
        [Description("牙买加元")]
        JMD = 106,
        [Description("约旦第纳尔")]
        JOD = 107,
        [Description("肯尼亚先令")]
        KES = 108,
        [Description("吉尔吉斯斯坦索姆")]
        KGS = 109,
        [Description("柬埔寨利尔斯")]
        KHR = 110,
        [Description("科摩罗法郎")]
        KMF = 111,
        [Description("北朝鲜元")]
        KPW = 112,
        /// <summary>
        /// 千韩元
        /// </summary>
        [Description("千韩元")]
        KKRW = 113,
        [Description("科威特第纳尔")]
        KWD = 114,
        [Description("开曼岛元")]
        KYD = 115,
        /// <summary>
        /// 黎巴嫩镑
        /// </summary>
        [Description("黎巴嫩镑")]
        LBP = 116,
        /// <summary>
        /// 千黎巴嫩镑
        /// </summary>
        [Description("千黎巴嫩镑")]
        KLBP = 117,
        [Description("黎巴嫩元")]
        LRD = 118,
        [Description("莱索托洛蒂")]
        LSL = 119,
        [Description("拉脱维亚拉特")]
        LVL = 120,
        [Description("利比亚第纳尔")]
        LYD = 121,
        [Description("摩洛哥迪拉姆")]
        MAD = 122,
        [Description("摩尔多瓦列伊")]
        MDL = 123,
        /// <summary>
        /// 马达加斯加阿里亚里
        /// </summary>
        [Description("马达加斯加阿里亚里")]
        MGA = 124,
        /// <summary>
        /// 千马达加斯加阿里亚里
        /// </summary>
        [Description("千马达加斯加阿里亚里")]
        KMGA = 125,
        [Description("马其顿第纳尔")]
        MKD = 126,
        [Description("毛里求斯卢比")]
        MUR = 127,
        [Description("马尔代夫罗非亚")]
        MVR = 128,
        [Description("马拉维克瓦查")]
        MWK = 129,
        [Description("莫桑比克梅蒂卡尔")]
        MZN = 130,
        [Description("纳米比亚元")]
        NAD = 131,
        [Description("尼加拉瓜科多巴")]
        NIO = 132,
        [Description("巴拿马巴波亚")]
        PAB = 133,
        [Description("巴布亚新几内亚基纳")]
        PGK = 134,
        [Description("波兰兹罗提")]
        PLN = 135,
        [Description("卡塔尔里亚尔")]
        QAR = 136,
        /// <summary>
        /// 巴拉圭瓜拉尼
        /// </summary>
        [Description("巴拉圭瓜拉尼")]
        PYG = 137,
        /// <summary>
        /// 千巴拉圭瓜拉尼
        /// </summary>
        [Description("千巴拉圭瓜拉尼")]
        KPYG = 138,
        /// <summary>
        /// 罗马尼亚列伊
        /// </summary>
        [Description("罗马尼亚列伊")]
        RON = 139,
        /// <summary>
        /// 塞尔维亚第纳尔
        /// </summary>
        [Description("塞尔维亚第纳尔")]
        RSD = 140,
        /// <summary>
        /// 卢旺达法郎
        /// </summary>
        [Description("卢旺达法郎")]
        RWF = 141,
        /// <summary>
        /// 千卢旺达法郎
        /// </summary>
        [Description("千卢旺达法郎")]
        KRWF = 142,
        [Description("沙特里亚尔")]
        SAR = 143,
        [Description("所罗门群岛元")]
        SBD = 144,
        [Description("塞舌尔卢比")]
        SCR = 145,
        [Description("苏丹镑")]
        SDG = 146,
        [Description("圣赫勒拿磅")]
        SHP = 147,
        /// <summary>
        /// 塞拉利昂利昂
        /// </summary>
        [Description("塞拉利昂利昂")]
        SLL = 148,
        /// <summary>
        /// 千塞拉利昂利昂
        /// </summary>
        [Description("千塞拉利昂利昂")]
        KSLL = 149,
        [Description("索马里先令")]
        SOS = 150,
        [Description("苏里南元")]
        SRD = 151,
        [Description("萨尔瓦多科朗")]
        SVC = 152,
        [Description("叙利亚镑")]
        SYP = 153,
        [Description("塔吉克斯坦索莫尼")]
        TJS = 154,
        [Description("土库曼斯坦新马纳特")]
        TMT = 155,
        [Description("汤加潘加")]
        TOP = 156,
        [Description("特立尼达与多巴哥元")]
        TTD = 157,
        [Description("TUSD"), ChainCurrency]
        TUSD = 158,
        /// <summary>
        /// 坦桑尼亚先令
        /// </summary>
        [Description("坦桑尼亚先令")]
        TZS = 159,
        /// <summary>
        /// 千坦桑尼亚先令
        /// </summary>
        [Description("千坦桑尼亚先令")]
        KTZS = 160,
        [Description("微比特币"), ChainCurrency]
        UBTC = 161,
        [Description("乌干达先令")]
        UGX = 162,
        [Description("千乌干达先令")]
        KUGX = 163,
        [Description("USDC"), ChainCurrency]
        USDC = 164,
        [Description("乌拉圭比索")]
        UYU = 165,
        /// <summary>
        /// 乌兹别克斯坦苏姆
        /// </summary>
        [Description("乌兹别克斯坦苏姆")]
        UZS = 166,
        /// <summary>
        /// 千乌兹别克斯坦苏姆
        /// </summary>
        [Description("千乌兹别克斯坦苏姆")]
        KUZS = 167,
        [Description("瓦努阿图瓦图")]
        VUV = 168,
        [Description("萨摩亚塔拉")]
        WST = 169,
        /// <summary>
        /// 中非金融合作法郎
        /// </summary>
        [Description("中非金融合作法郎")]
        XAF = 170,
        /// <summary>
        /// 东加勒比元
        /// </summary>
        [Description("东加勒比元")]
        XCD = 171,
        /// <summary>
        /// CFA法郎
        /// </summary>
        [Description("CFA法郎")]
        XOF = 172,
        /// <summary>
        /// 太平洋法郎
        /// </summary>
        [Description("太平洋法郎")]
        XPF = 173,
        /// <summary>
        /// 也门里亚尔
        /// </summary>
        [Description("也门里亚尔")]
        YER = 174,
        /// <summary>
        /// 赞比亚克瓦查
        /// </summary>
        [Description("赞比亚克瓦查")]
        ZMW = 175,
        /// <summary>
        /// 千哥伦比亚比索
        /// </summary>
        [Description("千哥伦比亚比索")]
        KCOP = 176,
        /// <summary>
        /// 百慕大元
        /// </summary>
        [Description("百慕大元")]
        BMD = 177,
        /// <summary>
        /// 千蒙古图格里克
        /// </summary>
        [Description("千蒙古图格里克")]
        KMNT = 178,
    }

    /// <summary>
    /// 区块链网络类型
    /// </summary>
    public enum ChainType : byte
    {
        /// <summary>
        /// 波场
        /// </summary>
        [Description("波场")]
        TRC = 1,
        /// <summary>
        /// 以太坊
        /// </summary>
        [Description("以太坊")]
        ERC = 2,
        /// <summary>
        /// 币安
        /// </summary>
        [Description("币安")]
        BSC = 3,
    }

    /// <summary>
    /// 银行卡的类型
    /// </summary>
    public enum BankCardType : byte
    {
        /// <summary>
        /// 储蓄卡/借记卡
        /// </summary>
        [Description("储蓄卡")]
        DC = 1,
        /// <summary>
        /// 信用卡/借记卡
        /// </summary>
        [Description("信用卡")]
        CC = 2,
        /// <summary>
        /// 准贷记卡
        /// 准贷记卡(Secured Credit Card)准贷记卡指持卡人在银行存一定备用金，不足支付时在信用额度内透支
        /// </summary>
        [Description("准贷记卡")]
        SCC = 3,

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
        public ISO6391Attribute(string code, string? baiduCode = null)
        {
            this.Code = code;
            this.BaiduCode = baiduCode;
        }

        /// <summary>
        /// ISO-639-1 语言代码
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// 百度翻译的语言代码
        /// </summary>
        public string? BaiduCode { get; private set; }

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

    /// <summary>
    /// 是否是区块链货币（虚拟货币）
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ChainCurrencyAttribute : Attribute
    {

    }
}
