using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service
{
    /// <summary>
    /// Https请求证书验证
    /// </summary>
    public class HttpCertificate
    {
        /// <summary>
        /// 是否需要进行验证
        /// </summary>
        public static bool IsCertificate { get; set; } = true;

        /// <summary>
        /// 证书信息
        /// </summary>
        public static X509Store Scertificate = null;

        /// <summary>
        /// 加载的证书集合
        /// </summary>
        public static X509Certificate2Collection Certificattes { get; set; }

        /// <summary>
        /// 构造设置证书信息
        /// </summary>
        static HttpCertificate()
        {
            Scertificate = GetScertificate();
        }

        /// <summary>
        /// 证书认证
        /// </summary>
        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors errors)
        {
            // 总是接受 [如果设置为不做证书校验，则进行通过] 
            if (!IsCertificate) { return true; }
            if (Scertificate == null)
            {
                //系统设置必须验证，但是证书为空，未获取到，则返回未空
                return false;
            }
            //返回
            if (errors == SslPolicyErrors.None)
            {
                //验证成功
                return true;
            }
            //返回具体错误
            return false;
        }

        /// <summary>
        /// 获取证书信息
        /// </summary>
        /// <returns>证书信息</returns>
        private static X509Store GetScertificate()
        {
            X509Store certificate = null;
            try
            {
                var checkDir = $@"{System.AppDomain.CurrentDomain.BaseDirectory}crt_appkey\";
                if (!System.IO.Directory.Exists(checkDir))
                {
                    return null;
                }
                var dirFiles = Directory.GetFiles(checkDir, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".crt") || s.EndsWith(".cer") || s.EndsWith(".CRT") || s.EndsWith(".CER")).ToArray();
                if (dirFiles.Length == 0)
                {
                    return null;
                }
                //获取证书存储区域
                certificate = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certificate.Open(OpenFlags.MaxAllowed);
                if (Certificattes == null)
                {
                    Certificattes = new X509Certificate2Collection();
                }
                //循环加载
                foreach (var crt in dirFiles.Select(GetCrt).Where(crt => crt != null))
                {
                    //添加
                    certificate.Add(crt);
                    Certificattes.Add(crt);
                }
                //关闭证书区域
                certificate.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"Get X509Certificate2 Error:{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                certificate = null;
            }
            //返回
            return certificate;
        }

        /// <summary>
        /// 方法说明：获取证书信息
        /// </summary>
        /// <param name="crtFile">证书路径</param>
        /// <returns></returns>
        private static X509Certificate2 GetCrt(string crtFile)
        {
            try
            {
                var crt = new X509Certificate2(crtFile);
                return crt;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"X509Certificate2 GetCrt Error:{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
    }
}
