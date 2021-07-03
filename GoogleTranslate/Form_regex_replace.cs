using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Translator
{
    public partial class Form_regex_replace : Form
    {
        public Form_regex_replace()
        {
            InitializeComponent();
        }

        public TextTool tools;

        private void Form_regex_replace_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            StringBuilder shablon = new StringBuilder();
            StringBuilder res_shablon = new StringBuilder();
            shablon.Append("(.*)");
            res_shablon.Append("$1");
            int param = 2;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '「')
                {
                    shablon.Append("(「.*」)");
                    res_shablon.Append("$" + param);
                    param++;
                    while (i < text.Length && text[i] != '」')
                        i++;
                    continue;
                }
                else if (!char.IsDigit(text[i]))
                {
                    shablon.Append(text[i]);
                    res_shablon.Append(text[i]);
                }
                else
                {
                    shablon.Append("(\\d+)");
                    res_shablon.Append("$" + param);
                    param++;
                    while (i < text.Length && char.IsDigit(text[i]))
                        i++;
                    i--;
                }
            }
            shablon.Append("(.*)");
            res_shablon.Append("$" + param);
            textBox2.Text = shablon.ToString();
            textBox3.Text = res_shablon.ToString();
        }

        public void SetOriginalString(string text)
        {
            textBox1.Text = text;
        }

        public string GetRegexShablon()
        {
            return textBox2.Text;
        }
        public string GetRaplacedShablon()
        {
            return textBox3.Text;
        }

        public string ReplaceString(string originalText, bool useReplaceParam)
        {
            Regex r = new Regex(textBox2.Text);
            if (!useReplaceParam)
            {
                if (r.IsMatch(originalText))
                {
                    return r.Replace(originalText, textBox3.Text);
                }
                else
                    return originalText;
            }
            else
            {
                Match mText = r.Match(originalText);
                if (mText.Success && mText.Groups[0].Value == originalText)
                {
                    return replaceRegex(textBox3.Text, mText);
                }
                else
                    return originalText;
            }

        }

        private string replaceRegex(string text, Match mText)
        {
            StringBuilder sb = new StringBuilder();
            string textValue = textBox3.Text;
            for (int i = 0; i < textValue.Length; i++)
            {
                if (textValue[i] != '$')
                    sb.Append(textValue[i]);
                else
                {
                    i++;
                    int GroupId = -1;
                    GroupId = int.Parse(textValue[i].ToString());
                    if (GroupId < mText.Groups.Count)
                    {
                        string DictResult = string.Empty;
                        //TranslationDataBase DictResult = TextTranslator.dictForRegex[match2.Groups[GroupId].Value] as TranslationDataBase;
                        if (tools.isReplace(mText.Groups[GroupId].Value, tools.RepText, out DictResult, false))
                        {
                            if (!String.IsNullOrEmpty(DictResult))
                                sb.Append(DictResult);
                            else
                            {
                                sb.Append(mText.Groups[GroupId].Value);
                            }
                        }
                        else
                        {
                            sb.Append(mText.Groups[GroupId].Value);
                        }
                    }
                    else
                    {
                        sb.Append("$" + textValue[i].ToString());
                    }

                }

            }
            return sb.ToString();
        }
    }
}
