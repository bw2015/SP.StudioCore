using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Enums
{
    /// <summary>
    /// 中国的银行列表
    /// </summary>
    public enum BankType : byte
    {
        [Description("中国农业银行")]
        ABC = 1,
        [Description("安徽省农村信用社")]
        ARCU = 2,
        [Description("鞍山银行")]
        ASCB = 3,
        [Description("安阳银行")]
        AYCB = 4,
        [Description("潍坊银行")]
        BANKWF = 5,
        [Description("广西北部湾银行")]
        BGB = 6,
        /// <summary>
        /// 河北银行
        /// </summary>
        [Description("河北银行")]
        BHB = 7,
        /// <summary>
        /// 北京银行
        /// </summary>
        [Description("北京银行")]
        BJBANK = 8,
        /// <summary>
        /// 北京农村商业银行
        /// </summary>
        [Description("北京农村商业银行")]
        BJRCB = 9,
        [Description("中国银行")]
        BOC = 10,
        [Description("承德银行")]
        BOCD = 11,
        [Description("朝阳银行")]
        BOCY = 12,
        [Description("东莞银行")]
        BOD = 13,
        [Description("丹东银行")]
        BODD = 14,
        /// <summary>
        /// 渤海银行
        /// </summary>
        [Description("渤海银行")]
        BOHAIB = 15,
        [Description("锦州银行")]
        BOJZ = 16,
        [Description("平顶山银行")]
        BOP = 17,
        [Description("青海银行")]
        BOQH = 18,
        [Description("苏州银行")]
        BOSZ = 19,
        [Description("营口银行")]
        BOYK = 20,
        [Description("周口银行")]
        BOZK = 21,
        [Description("包商银行")]
        BSB = 22,
        [Description("驻马店银行")]
        BZMD = 23,
        [Description("城市商业银行资金清算中心")]
        CBBQS = 24,
        [Description("开封市商业银行")]
        CBKF = 25,
        /// <summary>
        /// 建设银行
        /// </summary>
        [Description("中国建设银行")]
        CCB = 26,
        [Description("重庆三峡银行")]
        CCQTGB = 27,
        [Description("国家开发银行")]
        CDB = 28,
        /// <summary>
        /// 成都银行
        /// </summary>
        [Description("成都银行")]
        CDCB = 29,
        [Description("成都农商银行")]
        CDRCB = 30,
        /// <summary>
        /// 中国光大银行
        /// </summary>
        [Description("中国光大银行")]
        CEB = 31,
        /// <summary>
        /// 兴业银行
        /// </summary>
        [Description("兴业银行")]
        CIB = 32,
        /// <summary>
        /// 中信银行
        /// </summary>
        [Description("中信银行")]
        CITIC = 33,
        /// <summary>
        /// 招商银行
        /// </summary>
        [Description("招商银行")]
        CMB = 34,
        /// <summary>
        /// 中国民生银行
        /// </summary>
        [Description("中国民生银行")]
        CMBC = 35,
        /// <summary>
        /// 交通银行
        /// </summary>
        [Description("交通银行")]
        COMM = 36,
        [Description("重庆银行")]
        CQBANK = 37,
        [Description("重庆农村商业银行")]
        CRCBANK = 38,
        [Description("长沙银行")]
        CSCB = 39,
        [Description("常熟农村商业银行")]
        CSRCB = 40,
        /// <summary>
        /// 浙商银行
        /// </summary>
        [Description("浙商银行")]
        CZBANK = 41,
        /// <summary>
        /// 浙江稠州商业银行
        /// </summary>
        [Description("浙江稠州商业银行")]
        CZCB = 42,
        [Description("常州农村信用联社")]
        CZRCB = 43,
        [Description("龙江银行")]
        DAQINGB = 44,
        [Description("大连银行")]
        DLB = 45,
        [Description("东莞农村商业银行")]
        DRCBCL = 46,
        [Description("德阳商业银行")]
        DYCB = 47,
        [Description("东营市商业银行")]
        DYCCB = 48,
        [Description("德州银行")]
        DZBANK = 49,
        [Description("恒丰银行")]
        EGBANK = 50,
        [Description("富滇银行")]
        FDB = 51,
        [Description("福建海峡银行")]
        FJHXBC = 52,
        [Description("抚顺银行")]
        FSCB = 53,
        [Description("阜新银行")]
        FXCB = 54,
        /// <summary>
        /// 广州银行
        /// </summary>
        [Description("广州银行")]
        GCB = 55,
        /// <summary>
        /// 广东发展银行
        /// </summary>
        [Description("广东发展银行")]
        GDB = 56,
        [Description("广东省农村信用社联合社")]
        GDRCC = 57,
        [Description("桂林银行")]
        GLBANK = 58,
        /// <summary>
        /// 广州农商银行
        /// </summary>
        [Description("广州农商银行")]
        GRCB = 59,
        [Description("甘肃省农村信用")]
        GSRCU = 60,
        [Description("广西省农村信用")]
        GXRCU = 61,
        [Description("贵阳市商业银行")]
        GYCB = 62,
        [Description("赣州银行")]
        GZB = 63,
        [Description("贵州省农村信用社")]
        GZRCU = 64,
        [Description("内蒙古银行")]
        H3CB = 65,
        [Description("韩亚银行")]
        HANABANK = 66,
        [Description("湖北银行")]
        HBC = 67,
        [Description("湖北银行黄石分行")]
        HBHSBANK = 68,
        [Description("河北省农村信用社")]
        HBRCU = 69,
        [Description("湖北银行宜昌分行")]
        HBYCBANK = 70,
        [Description("邯郸银行")]
        HDBANK = 71,
        [Description("汉口银行")]
        HKB = 72,
        /// <summary>
        /// 东亚银行
        /// </summary>
        [Description("东亚银行")]
        HKBEA = 73,
        [Description("湖南省农村信用社")]
        HNRCC = 74,
        [Description("河南省农村信用")]
        HNRCU = 75,
        [Description("华融湘江银行")]
        HRXJB = 76,
        /// <summary>
        /// 徽商银行
        /// </summary>
        [Description("徽商银行")]
        HSBANK = 77,
        [Description("衡水银行")]
        HSBK = 78,
        [Description("湖北省农村信用社")]
        HURCB = 79,
        /// <summary>
        /// 华夏银行
        /// </summary>
        [Description("华夏银行")]
        HXBANK = 80,
        /// <summary>
        /// 杭州银行
        /// </summary>
        [Description("杭州银行")]
        HZCB = 81,
        [Description("湖州市商业银行")]
        HZCCB = 82,
        [Description("中国工商银行")]
        ICBC = 83,
        [Description("金华银行")]
        JHBANK = 84,
        [Description("晋城银行JCBANK")]
        JINCHB = 85,
        [Description("九江银行")]
        JJBANK = 86,
        [Description("吉林银行")]
        JLBANK = 87,
        [Description("吉林农信")]
        JLRCU = 88,
        [Description("济宁银行")]
        JNBANK = 89,
        [Description("江苏江阴农村商业银行")]
        JRCB = 90,
        [Description("晋商银行")]
        JSB = 91,
        [Description("江苏银行")]
        JSBANK = 92,
        [Description("江苏省农村信用联合社")]
        JSRCU = 93,
        [Description("嘉兴银行")]
        JXBANK = 94,
        [Description("江西省农村信用")]
        JXRCU = 95,
        [Description("晋中市商业银行")]
        JZBANK = 96,
        [Description("昆仑银行")]
        KLB = 97,
        [Description("库尔勒市商业银行")]
        KORLABANK = 98,
        [Description("昆山农村商业银行")]
        KSRB = 99,
        [Description("廊坊银行")]
        LANGFB = 100,
        [Description("莱商银行")]
        LSBANK = 101,
        [Description("临商银行")]
        LSBC = 102,
        [Description("乐山市商业银行")]
        LSCCB = 103,
        [Description("洛阳银行")]
        LYBANK = 104,
        [Description("辽阳市商业银行")]
        LYCB = 105,
        [Description("兰州银行")]
        LZYH = 106,
        [Description("浙江民泰商业银行")]
        MTBANK = 107,
        /// <summary>
        /// 宁波银行
        /// </summary>
        [Description("宁波银行")]
        NBBANK = 108,
        [Description("鄞州银行")]
        NBYZ = 109,
        [Description("南昌银行")]
        NCB = 110,
        [Description("南海农村信用联社")]
        NHB = 111,
        [Description("农信银清算中心")]
        NHQS = 112,
        /// <summary>
        /// 南京银行
        /// </summary>
        [Description("南京银行")]
        NJCB = 113,
        [Description("宁夏银行")]
        NXBANK = 114,
        [Description("宁夏黄河农村商业银行")]
        NXRCU = 115,
        [Description("广东南粤银行")]
        NYNB = 116,
        [Description("鄂尔多斯银行")]
        ORBANK = 117,
        /// <summary>
        /// 中国邮政储蓄银行
        /// </summary>
        [Description("中国邮政储蓄银行")]
        PSBC = 118,
        [Description("青岛银行")]
        QDCCB = 119,
        [Description("齐鲁银行")]
        QLBANK = 120,
        [Description("三门峡银行")]
        SCCB = 121,
        [Description("四川省农村信用")]
        SCRCU = 122,
        /// <summary>
        /// 顺德农商银行
        /// </summary>
        [Description("顺德农商银行")]
        SDEB = 123,
        [Description("山东农信")]
        SDRCU = 124,
        /// <summary>
        /// 上海银行
        /// </summary>
        [Description("上海银行")]
        SHBANK = 125,
        /// <summary>
        /// 上海农村商业银行
        /// </summary>
        [Description("上海农村商业银行")]
        SHRCB = 126,
        [Description("盛京银行")]
        SJBANK = 127,
        /// <summary>
        /// 平安银行
        /// </summary>
        [Description("平安银行")]
        SPABANK = 128,
        /// <summary>
        /// 上海浦东发展银行
        /// </summary>
        [Description("上海浦东发展银行")]
        SPDB = 129,
        [Description("上饶银行")]
        SRBANK = 130,
        [Description("深圳农村商业银行")]
        SRCB = 131,
        [Description("绍兴银行")]
        SXCB = 132,
        [Description("陕西信合")]
        SXRCCU = 133,
        [Description("石嘴山银行")]
        SZSBK = 134,
        [Description("泰安市商业银行")]
        TACCB = 135,
        /// <summary>
        /// 天津银行
        /// </summary>
        [Description("天津银行")]
        TCCB = 136,
        [Description("江苏太仓农村商业银行")]
        TCRCB = 137,
        [Description("天津农商银行")]
        TRCB = 138,
        [Description("台州银行")]
        TZCB = 139,
        [Description("乌鲁木齐市商业银行")]
        URMQCCB = 140,
        [Description("威海市商业银行")]
        WHCCB = 141,
        [Description("武汉农村商业银行")]
        WHRCB = 142,
        [Description("吴江农商银行")]
        WJRCB = 143,
        [Description("无锡农村商业银行")]
        WRCB = 144,
        [Description("温州银行")]
        WZCB = 145,
        [Description("西安银行")]
        XABANK = 146,
        [Description("许昌银行")]
        XCYH = 147,
        [Description("中山小榄村镇银行")]
        XLBANK = 148,
        [Description("邢台银行")]
        XTB = 149,
        [Description("新乡银行")]
        XXBANK = 150,
        [Description("信阳银行")]
        XYBANK = 151,
        [Description("宜宾市商业银行")]
        YBCCB = 152,
        [Description("尧都农商行")]
        YDRCB = 153,
        [Description("云南省农村信用社")]
        YNRCC = 154,
        [Description("阳泉银行")]
        YQCCB = 155,
        [Description("玉溪市商业银行")]
        YXCCB = 156,
        [Description("齐商银行")]
        ZBCB = 157,
        [Description("自贡市商业银行")]
        ZGCCB = 158,
        /// <summary>
        /// 张家口市商业银行
        /// </summary>
        [Description("张家口市商业银行")]
        ZJKCCB = 159,
        /// <summary>
        /// 浙江省农村信用社联合社
        /// </summary>
        [Description("浙江省农村信用社联合社")]
        ZJNX = 160,
        /// <summary>
        /// 浙江泰隆商业银行
        /// </summary>
        [Description("浙江泰隆商业银行")]
        ZJTLCB = 161,
        [Description("张家港农村商业银行")]
        ZRCBANK = 162,
        [Description("遵义市商业银行")]
        ZYCBANK = 163,
        [Description("郑州银行")]
        ZZBANK = 164,
        [Description("南充市商业银行")]
        CGNB = 165
    }
}
