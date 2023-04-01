using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Translator;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace TestTranslator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] testArr = { "「お金の問題は気にしなくても大丈夫ですよ。私は貴方が怪我をしてしまうことの方が辛い」", "「わかったー」" };
            VNRScriptParser parser = new VNRScriptParser(Application.StartupPath);
            parser.Parse();
            bool repl = false;
            string[] res = parser.GetReplacedString(testArr, out repl);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] ch = new byte[1];
            string res = string.Empty;
            for (byte i = 0x00; i < 0xFF; i++)
            {
                ch[0] = i;
                res += Encoding.GetEncoding(932).GetString(ch);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string text = "コスト0、威力補正100%。敵1体に炎属性ダメージを与えシールドを1pt削る3213131コスト0、威力補正100%。敵1体に炎属性ダメージを与えシールドを1pt削る";
            TextTool tools = new TextTool(Application.StartupPath);
            tools.CompareAndReplaceMask(text, tools.RepText);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string text = "コスト0、威力補正100%。敵1体に炎属性ダメージを与えシールドを1pt削る3213131コスト0、威力補正100%。敵1体に炎属性ダメージを与えシールドを1pt削る";
            Regex regex = new Regex(@"敵1体に\S{1}属性ダメージを与えシールドを\S{3}削る");
            MatchCollection co = regex.Matches(text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string test ="「お金の問題は気にしなくても大丈夫ですよ。私は貴方が怪我をしてしまうことの方が辛い」";
            TextTool tools = new TextTool(Application.StartupPath);
            tools.TranslateText(test, "ja", "ru");

        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form_regex_replace f_r = new Form_regex_replace();
            f_r.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            DeeplTranslator translator = new DeeplTranslator();
            translator.Translate("EN", "RU", "test translation");
            //CustomDeeplClientTCP webClientTCP = new CustomDeeplClientTCP();
            //webClientTCP.OpenConnection("https://www2.deepl.com/jsonrpc");
            //webClientTCP.SendData("");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string serverName = "google.com";
            TcpClient client = new TcpClient(serverName, 443);
            //Console.WriteLine("Client connected.");
            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false
                );
            // The server name must match the name on the server certificate.
            try
            {
                sslStream.AuthenticateAsClient(serverName);

                byte[] buffer = new byte[2048];
                int bytes;
                byte[] request = Encoding.UTF8.GetBytes(String.Format("GET https://{0}/  HTTP/1.1\r\nHost: {0}\r\n\r\n", serverName));
                sslStream.Write(request, 0, request.Length);
                sslStream.Flush();
                StringBuilder sb = new StringBuilder();
                // Read response
                do
                {
                    bytes = sslStream.Read(buffer, 0, buffer.Length);
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                } while (bytes == 2048);

            }
            catch (AuthenticationException ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", ex.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                return;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            GoogleTranslator googleTranslator = new GoogleTranslator(Application.StartupPath);
            Task<string> t = googleTranslator.Translate("test string (as test=word&car)", "en", "ru");
            t.Wait();
            string res = t.Result;
        }
    }
}
