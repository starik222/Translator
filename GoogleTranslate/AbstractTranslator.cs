using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Translator.IO;

namespace Translator
{
    public abstract class AbstractTranslator : IDisposable
    {
        public abstract void Dispose();

        public abstract string Translate(string str, string from, string to, bool caching);

        public abstract string Translate(string str, string from, string to);

        public abstract Task<string> TranslateAsync(string str, string from, string to, bool caching);

        public abstract Task<string> TranslateAsync(string str, string from, string to);

        protected string ReplaceOtherChar(string text)
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

        internal CacheTrans.Lang GetLang(string lang)
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
    }
}
