using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Translator.IO;

namespace Translator
{
    public class BridgeTranslator : AbstractTranslator
    {

        private CacheTrans cache;
        private string ch_zero;
        private HttpClient client;
        private string _bridgeUrl = "";

        public BridgeTranslator(string AppPath, string bridgeUrl)
        {
            _bridgeUrl = bridgeUrl;
            ch_zero = ((char)8203).ToString();
            cache = new CacheTrans(AppPath);
            client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
        }

        public BridgeTranslator(string bridgeUrl)
        {
            _bridgeUrl = bridgeUrl;
            ch_zero = ((char)8203).ToString();
            client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
        }


        public override void Dispose()
        {
            cache.Close();
            client.Dispose();
        }

        public override string Translate(string str, string from, string to, bool caching)
        {
            var t = TranslateAsync(str, from, to, caching);
            t.Wait();
            return t.Result;
        }

        public override string Translate(string str, string from, string to)
        {
            var t = TranslateAsync(str, from, to);
            t.Wait();
            return t.Result;
        }

        public override async Task<string> TranslateAsync(string str, string from, string to, bool caching)
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

        public override async Task<string> TranslateAsync(string str, string from, string to)
        {
            RequestData request = new RequestData();
            request.FromLang = from;
            request.ToLang = to;
            request.Text = str;
            string data = null;
            try
            {
                data = await client.GetStringAsync(JsonConvert.SerializeObject(request)).ConfigureAwait(false);
                var jData =  JsonConvert.DeserializeObject<ResponseData>(data);
                if (jData.Success)
                {
                    string text = jData.TranslatedText;
                    text = text.Replace(ch_zero, "");
                    return ReplaceOtherChar(text) ?? "error";
                }
                else
                    return "error";

            }
            catch (Exception)
            {
                return "error";
            }
        }

        private class RequestData
        {
            public string FromLang { get; set; } = "ja";
            public string ToLang { get; set; } = "ru";
            public string Text { get; set; } = "";
            public string Service { get; set; } = "";
        }

        private class ResponseData
        {
            public bool Success { get; set; } = false;
            public string Error { get; set; } = "";
            public string OriginalText { get; set; } = "";
            public string TranslatedText { get; set; } = "";
            public string Translit { get; set; } = "";
        }
    }
}
