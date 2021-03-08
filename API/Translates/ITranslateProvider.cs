using SP.StudioCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Translates
{
    /// <summary>
    /// 翻译注入基类
    /// 2020.4.3 暂未实现，没有想到第二种翻译方式，暂时使用TranslateUtils静态工具方法进行处理
    /// </summary>
    public interface ITranslateProvider
    {
        /// <summary>
        /// 加载语言包数据
        /// </summary>
        void LoadLanguageData();

        /// <summary>
        /// 获取翻译后的内容
        /// </summary>
        /// <returns></returns>
        string Get(string keyword, Language language);

    }
}
