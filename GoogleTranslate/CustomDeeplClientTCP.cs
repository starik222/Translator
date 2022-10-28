using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Translator
{
    public class CustomDeeplClientTCP
    {
        private readonly TcpClient client;
        private Uri baseUri;
        private SslStream sslStream;

        public bool IsConnected
        {
            get => client.Connected;
        }

        public string ErrorMessage { get; private set; }
        public CustomDeeplClientTCP()
        {
            
            client = new TcpClient();
            ErrorMessage = string.Empty;
        }

        public bool OpenConnection(string url)
        {
            ErrorMessage = string.Empty;
            baseUri = new Uri(url);
            client.Connect(baseUri.Host, 443);
            sslStream = new SslStream(client.GetStream(), false);
            try
            {
                sslStream.AuthenticateAsClient(baseUri.Host);
            }
            catch (AuthenticationException ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
            return true;
        }

        public string SendData(string body)
        {
            byte[] buffer = new byte[2048];
            int bytes;
            //string body = "{\"jsonrpc\": \"2.0\", \"method\": \"LMT_split_into_sentences\", \"params\": {\"texts\": [\"test translation\"], \"lang\": {\"lang_user_selected\": \"auto\", \"user_preferred_langs\": []}}, \"id\": 3405691582}";
            //body = "{\"jsonrpc\":\"2.0\",\"method\": \"LMT_handle_jobs\",\"params\":{\"jobs\":[{\"kind\":\"default\",\"sentences\":[{\"text\":\"test one\",\"id\":0,\"prefix\":\"\"}],\"raw_en_context_before\":[],\"raw_en_context_after\":[],\"preferred_num_beams\":4}],\"lang\":{\"preference\":{\"weight\":{},\"default\":\"default\"},\"source_lang_computed\":\"JA\",\"target_lang\":\"RU\"},\"priority\":1,\"commonJobParams\":{\"browserType\":1,\"formality\":null},\"timestamp\":1650956619560},\"id\":99310005}";
            //body = "{\"jsonrpc\": \"2.0\", \"method\": \"LMT_handle_jobs\", \"params\": {\"jobs\": [{\"kind\": \"default\", \"raw_en_sentence\": \"Why you name?\", \"raw_en_context_before\": [], \"raw_en_context_after\": [], \"preferred_num_beams\": 1}], \"lang\": {\"user_preferred_langs\": [\"RU\", \"JA\"], \"source_lang_computed\": \"JA\", \"target_lang\": \"RU\"}, \"priority\": 1, \"commonJobParams\": {\"formality\": \"formal\"}, \"timestamp\": 1650957434050}, \"id\": 3405691582}";
            //JObject b = JObject.Parse(body);
            //b["timestamp"] = generate_timestamp(new string[] { "Why you name?" });
            byte[] bBody = Encoding.UTF8.GetBytes(body);
            string data = $"POST /jsonrpc HTTP/1.1\r\nHost: www2.deepl.com\r\nuser-agent: Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Mobile Safari/537.36\r\nAccept-Encoding: gzip, deflate\r\naccept: */*\r\nConnection: keep-alive\r\naccept-language: en-US;q=0.8,en;q=0.7\r\nauthority: www2.deepl.com\r\ncontent-type: application/json\r\norigin: https://www.deepl.com\r\nreferer: https://www.deepl.com/translator\r\nsec-fetch-dest: empty\r\nsec-fetch-mode: cors\r\nsec-fetch-site: same-site\r\nContent-Length: {bBody.Length}\r\n\r\n";
            sslStream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
            sslStream.Flush();
            Thread.Sleep(100);
            sslStream.Write(bBody, 0, bBody.Length);
            sslStream.Flush();
            StringBuilder sb = new StringBuilder();
            // Read response
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
            } while (bytes == 2048);

            return sb.ToString();
        }

        public long generate_timestamp(string[] sentences)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int i_count = 1;
            foreach (var item in sentences)
            {
                i_count += (item.Split(new string[] { "i" }, StringSplitOptions.None).Count() - 1);
            }
            try
            {
                return now + (i_count - now % i_count);
            }
            catch
            {
                return now;
            }
        }
    }
}
