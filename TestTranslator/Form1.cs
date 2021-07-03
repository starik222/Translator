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
            tools.TranslateText(test, TextTool.TransMethod.JaRu);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form_regex_replace f_r = new Form_regex_replace();
            f_r.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            YandexTranslate yandex = new YandexTranslate();
            string text = yandex.Translate("新一", "ja", "ru");
        }
    }
}
