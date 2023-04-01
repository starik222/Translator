using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Translator
{
    public class DeeplTranslator
    {
        public const uint MAGIC_NUMBER = 654187;
        //public const uint MAGIC_NUMBER = 77280001;
        public const string API_URL = "https://www2.deepl.com/jsonrpc";

        private readonly HttpClient client;
        private CustomDeeplClientTCP deeplClient;

        private string err;
        private bool istranslated;


        public DeeplTranslator()
        {
            client = new HttpClient();
            SetDefaultHeaders();
            deeplClient = new CustomDeeplClientTCP();
        }


        private void SetDefaultHeaders()
        {
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Mobile Safari/537.36");
            //client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Add("accept-language", "en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("authority", "www2.deepl.com");
            //client.DefaultRequestHeaders.Add("content-type", "application/json");
            client.DefaultRequestHeaders.Add("origin", "https://www.deepl.com");
            client.DefaultRequestHeaders.Add("referer", "https://www.deepl.com/translator");
            client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
            //webreque.Timeout = 20000;
            //webreque.Method = "POST";
        }

        //    private JObject generate_split_sentences_request_data(string text, uint identifier = MAGIC_NUMBER)
        //    {
        //        string data = @"{" +
        //    "\"jsonrpc\": \"2.0\"," +
        //    "\"method\": \"LMT_split_into_sentences\"," +
        //    "\"params\": {" +
        //            "\"texts\": [\""+text+"\"]," +
        //        "\"lang\": { \"lang_user_selected\": \"auto\", \"user_preferred_langs\": []}," +
        //    "}," +
        //    "\"id\": "+identifier+"," +
        //"}";
        //        JObject dataObj = JObject.Parse(data);
        //        return dataObj;
        //    }

        private JObject generate_split_sentences_request_data(string text, uint identifier = MAGIC_NUMBER)
        {
            string data = "{\"jsonrpc\": \"2.0\", \"method\": \"LMT_split_into_sentences\", \"params\": {\"texts\": [\"" + text + "\"], \"lang\": {\"lang_user_selected\": \"auto\", \"user_preferred_langs\": []}}, \"id\": " + identifier + "}";
            JObject dataObj = JObject.Parse(data);
            return dataObj;
        }

        //public async Task<string> GetRandomNumber(string Token)
        //{
        //    using (var request = new HttpRequestMessage(HttpMethod.Get, randomNumberUrl))
        //    {
        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        //        var response = await HttpClient.SendAsync(request);

        //        response.EnsureSuccessStatusCode();

        //        return await response.Content.ReadAsStringAsync();
        //    }
        //}


        public string Translate(string from, string to, string text, bool formal = true)
        {
            return request_translation(from, to, text);
        }

        private string request_translation(string source_language, string target_language, string text)
        {
            var data = split_into_sentences(text);
            return data;
        }


        private string generate_translation_request_data(string source_language, string target_language, string sentences)
        {
            string data = "{" +
        "\"jsonrpc\": \"2.0\"," +
        "\"method\": \"LMT_handle_jobs\"," +
      "  \"params\": {" +
                "\"jobs\": generate_jobs(sentences, beams = alternatives)," +
            "\"lang\": {" +
                    "\"user_preferred_langs\": [target_language, source_language]," +
                "\"source_lang_computed\": source_language," +
                "\"target_lang\": target_language," +
            "}," +
            "\"priority\": 1," +
            "\"commonJobParams\": generate_common_job_params(formality_tone)," +
            "\"timestamp\": generate_timestamp(sentences)," +
        "}," +
        "\"id\": identifier," +
    "}";
            return "";
        }

        private string split_into_sentences(string text)
        {
            var data = generate_split_sentences_request_data(text);
            if (!deeplClient.IsConnected)
                deeplClient.OpenConnection(API_URL);
            string response = deeplClient.SendData(data.ToString(Formatting.None));
            if (response == null)
                return null;
            var resJsonData = JObject.Parse(response);
            var sentences = extract_split_sentences(resJsonData);
            return sentences;
        }

        private string extract_split_sentences(JObject data)
        {
            return data["result"]["splitted_texts"][0].ToString();
        }

        private string SendRequest2(JObject jsonData)
        {
            //HttpContent body = new StringContent(jsonData.ToString(Formatting.None));
            //body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = client.PostAsync(API_URL + "?method=LMT_split_into_sentences", new StringContent(jsonData.ToString(Formatting.None), Encoding.UTF8, "application/json"));
            response.Wait();
            return "";
        }


        private string SendRequest(JObject jsonData)
        {
            HttpWebRequest webreque = (HttpWebRequest)WebRequest.Create(API_URL);
            // webreque.UserAgent = "Opera/9.80 (Windows NT 5.1; U; MRA 5.7 (build 03797); ru) Presto/2.10.229 Version/11.60";
            // webreque.UserAgent = "Opera / 9.80(J2ME / MIDP; Opera Mini/ 5.1.21214 / 28.2725; U; en) Presto / 2.8.119 Version / 11.10";
            //webreque.Accept = "*/*";
            webreque.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Mobile Safari/537.36";
            webreque.Accept = "*/*";
            webreque.Headers.Add("accept-language", "en-US;q=0.8,en;q=0.7");
            webreque.Headers.Add("authority", "www2.deepl.com");
            webreque.ContentType = "application/json";
            webreque.Headers.Add("origin", "https://www.deepl.com");
            webreque.Referer = "https://www.deepl.com/translator";
            //webreque.Headers.Add("sec-fetch-dest", "empty");
            //webreque.Headers.Add("sec-fetch-mode", "cors");
            //webreque.Headers.Add("sec-fetch-site", "same-site");
            webreque.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
            //webreque.Timeout = 20000;
            webreque.Method = "POST";
            webreque.KeepAlive = true;
            webreque.ProtocolVersion = HttpVersion.Version11;
            var data = Encoding.UTF8.GetBytes(jsonData.ToString(Formatting.None));
            using (var stream = webreque.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse webrespon;
            string str = null;
            try
            {
                webrespon = (HttpWebResponse)webreque.GetResponse();
                StreamReader str_read = new StreamReader(webrespon.GetResponseStream(), Encoding.UTF8);
                str = str_read.ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var errResponse = ex.Response as HttpWebResponse;

                    if ((int)errResponse.StatusCode == 429)
                    {
                        MessageBox.Show("Капча, поменяйте vpn");
                        //throw;
                        //form_webBrowser browserForm = new form_webBrowser();
                        //browserForm.webBrowser1.DocumentStream = errResponse.GetResponseStream();
                        //browserForm.webBrowser1.Document.Domain = "translate.google.ru";
                        //browserForm.ShowDialog();

                    }

                }
                err = ex.Message;
                istranslated = false;
                return "error";
            }
            catch (Exception ex)
            {
                err = ex.Message;
                istranslated = false;
                return "error";
            }
            return str;
        }

    }



}
