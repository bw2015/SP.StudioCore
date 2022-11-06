using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Enums
{
    /// <summary>
    /// 生肖（从0开始）
    /// </summary>
    public enum Zodiac : byte
    {
        [Description("鼠")]
        Rat,
        [Description("牛")]
        Ox,
        [Description("虎")]
        Tiger,
        [Description("兔")]
        Rabbit,
        [Description("龙")]
        Dragon,
        [Description("蛇")]
        Snake,
        [Description("马")]
        Horse,
        [Description("羊")]
        Goat,
        [Description("猴")]
        Monkey,
        [Description("鸡")]
        Rooster,
        [Description("狗")]
        Dog,
        [Description("猪")]
        Pig
    }

    /// <summary>
    /// 天干 甲乙丙丁戊己庚辛壬癸
    /// </summary>
    public enum CelestialStems : byte
    {
        [Description("甲")]
        A,
        [Description("乙")]
        B,
        [Description("丙")]
        C,
        [Description("丁")]
        D,
        [Description("戊")]
        W,
        [Description("己")]
        G,
        [Description("庚")]
        Xin,
        [Description("壬")]
        Ren,
        [Description("癸")]
        Gui
    }

    /// <summary>
    /// 地支 子丑寅卯辰巳午未申酉戌亥
    /// </summary>
    public enum TerrestrialBranches : byte
    {
        [Description("子")]
        Zi,
        [Description("丑")]
        Chou,
        [Description("寅")]
        Yin,
        [Description("卯")]
        Mao,
        [Description("辰")]
        Chen,
        [Description("巳")]
        Si,
        [Description("午")]
        Wei,
        [Description("未")]
        Shen,
        [Description("申")]
        You,
        [Description("酉")]
        Xu,
        [Description("亥")]
        Hai
    }
}
