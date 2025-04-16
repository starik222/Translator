using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Translator.IO;

namespace Translator
{
    public class GoogleTranslatorV2 : IDisposable
    {
        private CacheTrans cache;
        private string ch_zero;
        private HttpClient client;
        private const string googleTemplateUrl = "https://translate.google.com/m?hl=&sl={0}&tl={1}&ie=UTF-8&q={2}";

        public GoogleTranslatorV2(string AppPath)
        {
            ch_zero = ((char)8203).ToString();
            cache = new CacheTrans(AppPath);
            client = new HttpClient();
            //client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.5563.116 Mobile Safari/537.36");
            client.Timeout = new TimeSpan(0, 0, 10);
        }

        public GoogleTranslatorV2()
        {
            ch_zero = ((char)8203).ToString();
            client = new HttpClient();
            //client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.5563.116 Mobile Safari/537.36");
            client.Timeout = new TimeSpan(0, 0, 10);
        }

        private CacheTrans.Lang GetLang(string lang)
        {
            string nLang = string.Empty;
            switch (lang)
            {
                case "zh-CN":
                    nLang = "zhCN";
                    break;
                case "zh-TW":
                    nLang = "zhTW";
                    break;
                case "mni-Mtei":
                    nLang = "mniMtei";
                    break;
                default:
                    nLang = lang;
                    break;
            }
            return (CacheTrans.Lang)Enum.Parse(typeof(CacheTrans.Lang), nLang);
        }

        public string Translate(string str, string from, string to, bool caching)
        {
            var t = TranslateAsync(str, from, to, caching);
            t.Wait();
            return t.Result;
        }

        public string Translate(string str, string from, string to)
        {
            var t = TranslateAsync(str, from, to);
            t.Wait();
            return t.Result;
        }

        public async Task<string> TranslateAsync(string str, string from, string to, bool caching)
        {
            if (caching && cache != null)
            {
                string res = "";
                CacheTrans.Lang lg = GetLang(to);
                if (cache.IndexOf(str, out res, out _, lg))
                {
                    return res;
                }
                res = await TranslateAsync(str, from, to).ConfigureAwait(false);
                if (!res.Equals("error"))
                {
                    cache.Add(str, res, "-0123456789-", lg);
                    return res;
                }
                else
                {
                    return str;
                }
            }
            else
            {
                return await TranslateAsync(str, from, to);
            }
        }

        public async Task<string> TranslateAsync(string str, string from, string to)
        {

            //string val = string.Format(googleTemplateUrl, from, to, ConvertStringToHex(str, Encoding.UTF8));
            string val = string.Format(googleTemplateUrl, from, to, str);
            string data = null;
            try
            {
                data = await client.GetStringAsync(val).ConfigureAwait(false);
                String extracted = data.GetBetween("class=\"result-container\">", "</div>");//<div class="result-container">тестовая строка</div>
                string text = HttpUtility.HtmlDecode(extracted ?? string.Empty);
                text = text.Replace(ch_zero, "");
                return ReplaceOtherChar(text) ?? "error";
            }
            catch (Exception)
            {
                return "error";
            }
        }

        private string ReplaceOtherChar(string text)
        {
            int index = 0;
            string res = "";
            while (text.IndexOf("&#", index) != -1)
            {
                int old_ind = index;
                index = text.IndexOf("&#", index);
                res += text.Substring(old_ind, index);
                string chisl = text.Substring(index + 2, 2);
                byte[] ch = new byte[1];
                ch[0] = Convert.ToByte(chisl);
                string ret = Encoding.UTF8.GetString(ch);
                res += ret;
                index += 5;
            }
            if (index + 1 != text.Length)
            {
                res += text.Substring(index);
            }
            return res;
        }

        public void Dispose()
        {
            cache.Close();
            client.Dispose();
        }
    }
}
