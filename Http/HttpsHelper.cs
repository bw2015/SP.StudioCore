using SP.StudioCore.Json;
using SP.StudioCore.Model;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace SP.StudioCore.Http
{
    /// <summary>
    /// Https的解析类
    /// </summary>
    public static class HttpsHelper
    {
        /// <summary>
        /// 读取证书文件内容
        /// </summary>
        /// <param name="content"></param>
        public static List<CertInfo> ReadCert(string content)
        {
            
            var certList = new List<CertInfo>();
            var certStrList = new List<string>();

            Regex regex = new Regex(@"-----BEGIN CERTIFICATE-----(?<Content>.+?)-----END CERTIFICATE-----", RegexOptions.Singleline);
            foreach (Match match in regex.Matches(content))
            {
                string base64 = match.Groups["Content"].Value;
                using (X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(base64)))
                {
                    Console.WriteLine(cert.Subject);
                    Console.WriteLine(cert.HasPrivateKey);
                    Console.WriteLine(cert.Thumbprint);
                    Console.WriteLine(cert.GetExpirationDateString());

                    var simpleName = cert.GetNameInfo(X509NameType.SimpleName,false);
                    var expireAt = cert.NotAfter;

                    foreach (X509Extension ext in cert.Extensions)
                    {
                        if (ext.Oid.FriendlyName == "使用者可选名称" || ext.Oid.FriendlyName == "Subject Alternative Name")
                        {
                            string dnsnames = "";
                            for (int i = 0; i < ext.RawData.Length; i++)
                            {
                                dnsnames += ext.RawData[i].ToString("X2");
                            }
                            string[] dnsname = Regex.Split(dnsnames, @"82\d\w", RegexOptions.None);

                            foreach (string dns in dnsname)
                            {
                                if (dns.Length > 6)
                                {
                                    //16进制转换为字符串
                                    var arr = Regex.Matches(dns, @"\w\w").Cast<Match>().Select(m => m.Value);
                                    var domainsStr = new StringBuilder();

                                    foreach (var m in arr)
                                    {
                                        char c = (char)Int32.Parse(m, System.Globalization.NumberStyles.HexNumber);
                                        Console.WriteLine(c);
                                        domainsStr.Append(c);
                                    }

                                    certStrList.Add(domainsStr.ToString());
                                }
                            }
                        }
                    }

                    if (certStrList.Count <= 0) continue;

                    certList.Add(new CertInfo
                    {
                        Success = true,
                        Domain = certStrList.ToArray(),
                        ExpireAt = expireAt,
                        Message = simpleName
                    }) ;
                }
            }

            return certList;
        }

        /// <summary>
        /// 解析SSL证书的内容
        /// </summary>
        /// <param name="content">证书内容原串</param>
        /// <returns></returns>
        public static CertInfo GetFirstCertInfo(string content)
        {
            var certList = HttpsHelper.ReadCert(content);
            if (certList.Count > 0) return certList[0];
            return new CertInfo
            {
                Message = "证书解析错误"
            };
        }

        /// <summary>
        /// 解析SSL证书的内容，根据传入的证书内容解析成实体
        /// </summary>
        /// <param name="content">证书内容原串</param>
        /// <returns></returns>
        public static List<CertInfo> GetCertList(string content)
        {
            return HttpsHelper.ReadCert(content);
        }
    }

    /// <summary>
    /// 证书内容
    /// </summary>
    public struct CertInfo
    {
        /// <summary>
        /// 解析成功
        /// </summary>
        public bool Success;

        /// <summary>
        /// 返回的信息
        /// </summary>
        public string Message;

        public DateTime ExpireAt;

        public string[] Domain;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Name\":\"{0}\",", Message)
                .AppendFormat("\"Expire\":\"{0}\",", ExpireAt)
                .AppendFormat("\"Domain\":[{0}]", string.Join(",", this.Domain.Select(t => $"\"{t}\"")))
                .Append("}");
            return sb.ToString();
        }
    }
}
