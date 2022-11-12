## 第三方接口的封装

### Ali
> 阿里云市场的供应商封装

#### QRCodeAPI
> 二维码识别的组件

```
    /// <summary>
    /// 识别二维码
    /// </summary>
    /// <param name="src">图片base64编码或者url（http或者https开头）</param>
    /// <returns>识别之后的内容</returns>
    public string Execute(string src)
```

#### TranslateAPI
> 文字翻译

```
    /// <summary>
    /// 执行翻译
    /// </summary>
    /// <param name="content">要翻译的内容</param>
    /// <param name="source">来源语种</param>
    /// <param name="target">目标语种</param>
    /// <param name="result">翻译结果</param>
    /// <returns>是否执行成功</returns>
    public bool Execute(string content, Language source, Language target, out string result)
```

### BaiduAI
> 百度AI的接口封装

---

### Wallets
> 免转钱包的解决方案封装


### OSSAgent
> 阿里云OSS 存储的方法封装

