using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace Translator
{
    public class VNRScriptParser
    {
        private string path;
        public List<ScriptItem> Items;
        public bool isError;
        public string TextError;
        private Dictionary<char, char> DeReplacemant;
        private char[] remChar;
        public List<string> Langs;
        public VNRScriptParser(string Path)
        {
            Langs = new List<string>();
            //string tempRemChars = " 　、。，．・：；？！゛゜´｀¨＾￣＿ヽヾゝゞ〃仝々〆〇ー―‐／＼～∥｜…‥‘’“”（）〔〕［］｛｝〈〉《》「」『』【】＋－±×・÷＝≠＜＞≦≧∞∴♂♀°′″℃￥＄￠￡％＃＆＊＠§☆★○●◎◇◆□■△▲▽▼※〒→←↑↓〓・・・・・・・・・・・∈∋⊆⊇⊂⊃∪∩・・・・・・・・∧∨￢⇒⇔∀∃・・・・・・・・・・・∠⊥⌒∂∇≡≒≪≫√∽∝∵∫∬・・・・・・・Å‰♯♭♪†‡¶・・・・◯・・" +
            //   "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~｡｢｣､･";
            string tempRemChars = " 　―…";
            /*  for (int i = 0; i < tempRemChars.Length; i++)
              {
                  char aaaw = tempRemChars[i];
              }*/
            DeReplacemant = new Dictionary<char, char>();
            DeReplacemant.Add('，', ',');
            DeReplacemant.Add('。', '.');
            DeReplacemant.Add('．', '.');
            DeReplacemant.Add('：', ':');
            DeReplacemant.Add('；', ';');
            DeReplacemant.Add('？', '?');
            DeReplacemant.Add('！', '!');
            DeReplacemant.Add('゛', '\"');
            DeReplacemant.Add('ー', '-');
            DeReplacemant.Add('―', '-');
            DeReplacemant.Add('‐', '-');
            DeReplacemant.Add('（', '(');
            DeReplacemant.Add('）', ')');
            DeReplacemant.Add('［', '[');
            DeReplacemant.Add('］', ']');
            DeReplacemant.Add('＋', '+');
            DeReplacemant.Add('－', '-');
            DeReplacemant.Add('＝', '=');
            remChar = new char[tempRemChars.Length];
            for (int i = 0; i < tempRemChars.Length; i++)
            {
                remChar[i] = tempRemChars[i];
            }
            path = Path;
            isError = false;
            TextError = string.Empty;
            Items = new List<ScriptItem>();
        }

        public void Parse()
        {
            /*DeReplacemant = new Dictionary<char, char>();
            string chars5 = "　！＠＃＄％＊″：；？〜）（，．―＋";
            string chars6 = " !@#$%*\":;?~)(,.-+";
            for (int i = 0; i < chars5.Length; i++)
            {
                DeReplacemant.Add(chars5[i], chars6[i]);
            }*/
            DirectoryInfo di = new DirectoryInfo(path + "\\VNRScript");
            FileInfo[] fi = di.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < fi.Length; i++)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fi[i].FullName);
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    foreach (XmlNode subnode in node.ChildNodes)
                    {
                        try
                        {
                            if (subnode.Attributes["type"].InnerText != "subtitle")
                                continue;
                            if (Langs.IndexOf(subnode["language"].InnerText) == -1)
                            {
                                Langs.Add(subnode["language"].InnerText);
                            }
                            Items.Add(new ScriptItem(subnode["language"].InnerText, subnode["context"].InnerText, subnode["text"].InnerText, subnode.Attributes["type"].InnerText, Convert.ToInt32(subnode["context"].Attributes["size"].InnerText), remChar, DeReplacemant));
                        }
                        catch (Exception ex)
                        {
                            isError = true;
                            TextError += ex.Message + "\n";
                        }
                    }
                }
            }
        }

        //public string GetReplacedString(string text)
        //{
        //    if (string.IsNullOrWhiteSpace(text))
        //        return text;
        //    for (int i = 0; i < Items.Count; i++)
        //    {
        //        if (Prepare(text).Length != Prepare(Items[i].context).Length)
        //            continue;
        //        if (Prepare(text) == Prepare(Items[i].context))
        //            return Items[i].text;
        //    }
        //    return text;
        //}

        //public string GetReplacedString(string text, out bool IsReplaced)
        //{
        //    IsReplaced = false;
        //    if (string.IsNullOrWhiteSpace(text))
        //        return text;
        //    for (int i = 0; i < Items.Count; i++)
        //    {
        //        if (Prepare(text).Length != Prepare(Items[i].context).Length)
        //            continue;
        //        if (Prepare(text) == Prepare(Items[i].context))
        //        {
        //            IsReplaced = true;
        //            return Items[i].text;
        //        }
        //    }
        //    return text;
        //}

        //public string GetReplacedString(string text, string lang)
        //{
        //    if (string.IsNullOrWhiteSpace(text))
        //        return text;
        //    for (int i = 0; i < Items.Count; i++)
        //    {
        //        if (Items[i].lang != lang)
        //            continue;
        //        if (Prepare(text).Length != Prepare(Items[i].context).Length)
        //            continue;
        //        if (Prepare(text) == Prepare(Items[i].context))
        //            return Items[i].text;
        //    }
        //    return text;
        //}

        //public string GetReplacedString(string text, string lang, out bool IsReplaced)
        //{
        //    IsReplaced = false;
        //    if (string.IsNullOrWhiteSpace(text))
        //        return text;
        //    for (int i = 0; i < Items.Count; i++)
        //    {
        //        if (Items[i].lang != lang)
        //            continue;
        //        if (Prepare(text).Length != Prepare(Items[i].context).Length)
        //            continue;
        //        if (Prepare(text) == Prepare(Items[i].context))
        //        {
        //            IsReplaced = true;
        //            return Items[i].text;
        //        }
        //    }
        //    return text;
        //}

        public string[] GetReplacedString(string[] text, out bool IsReplaced)
        {
            IsReplaced = false;
            bool[] TranslText = new bool[text.Length];
            for (int j = text.Length - 1; j >= 0; j--)
            {
                if (string.IsNullOrWhiteSpace(text[j]))
                {
                    continue;
                }
                string temp_text = PrepareInputText(text[j]);
                for (int i = 0; i < Items.Count; i++)
                {
                    if (temp_text.Length != Items[i].context.Length)
                        continue;
                    if (temp_text == Items[i].context)
                    {
                        if (Items[i].size > 1 && j >= Items[i].size - 1)
                        {
                            bool AllTrue = true;
                            for (int k = 0; k < Items[i].size - 1; k++)
                            {
                                if (PrepareInputText(text[j - ((Items[i].size - 1) - k)]) != Items[i].preText[k])
                                {
                                    AllTrue = false;
                                    break;
                                }
                            }
                            if (AllTrue)
                            {
                                IsReplaced = true;
                                text[j] = Items[i].text;
                                TranslText[j] = true;
                            }
                        }
                        else
                        {
                            IsReplaced = true;
                            text[j] = Items[i].text;
                            TranslText[j] = true;
                        }
                    }
                }
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (TranslText[i])
                    continue;
                string temp_text = PrepareInputText(text[i]);
                for (int j = 0; j < Items.Count; j++)
                {
                    if (Items[j].size <= 1)
                        continue;
                    if (temp_text == Items[j].fullText)
                    {
                        IsReplaced = true;
                        text[i] = Items[j].text;
                        TranslText[i] = true;
                    }

                }
            }
            return text;
        }

        public string[] GetReplacedString(string[] text, out bool IsReplaced, string lang)
        {
            IsReplaced = false;
            bool[] TranslText = new bool[text.Length];
            for (int j = text.Length - 1; j >= 0; j--)
            {
                if (string.IsNullOrWhiteSpace(text[j]))
                {
                    continue;
                }
                string temp_text = PrepareInputText(text[j]);
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].lang != lang)
                        continue;
                    if (temp_text.Length != Items[i].context.Length)
                        continue;
                    if (temp_text == Items[i].context)
                    {
                        if (Items[i].size > 1 && j >= Items[i].size - 1)
                        {
                            bool AllTrue = true;
                            for (int k = 0; k < Items[i].size - 1; k++)
                            {
                                if (PrepareInputText(text[j - ((Items[i].size - 1) - k)]) != Items[i].preText[k])
                                {
                                    AllTrue = false;
                                    break;
                                }
                            }
                            if (AllTrue)
                            {
                                IsReplaced = true;
                                text[j] = Items[i].text;
                                TranslText[j] = true;
                            }
                        }
                        else
                        {
                            IsReplaced = true;
                            text[j] = Items[i].text;
                            TranslText[j] = true;
                        }
                    }
                }
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (TranslText[i])
                    continue;
                string temp_text = PrepareInputText(text[i]);
                for (int j = 0; j < Items.Count; j++)
                {
                    if (Items[j].size <= 1)
                        continue;
                    if (Items[j].lang != lang)
                        continue;
                    if (temp_text == Items[j].fullText)
                    {
                        IsReplaced = true;
                        text[i] = Items[j].text;
                        TranslText[i] = true;
                    }

                }
            }
            return text;
        }


        private string PrepareInputText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            string temp_text = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                if (Array.IndexOf(remChar, text[i]) != -1)
                {
                    continue;
                }
                else if (DeReplacemant.ContainsKey(text[i]))
                {
                    temp_text += DeReplacemant[text[i]];
                }
                else
                {
                    temp_text += text[i].ToString().ToLower();
                }
            }
            return temp_text;
        }

        //private string DeInsertDoubleChars(string text)
        //{
        //    //char[] symbols = { '　', '！', '＠', '＃', '＄', '％', '＊', '″', '：', '；', '？', '〜', '）', '（', '，', '．', '―', '」', '\n' };
        //    char[] symbols = { '!', '@', '#', '$', '%', '*', '\"', ':', ';', '?', '~', ')', '(', ',', '.', '-', '」', '+' };
        //    string temp_text = string.Empty;
        //    for (int i = 0; i < text.Length; i++)
        //    {
        //        if (i < text.Length - 1)
        //        {
        //            if (Array.IndexOf(symbols, text[i + 1]) != -1)
        //            {
        //                if (text[i] == ' ')
        //                {
        //                    continue;
        //                }
        //                else
        //                {
        //                    temp_text += text[i];
        //                }
        //            }
        //            else if (text[i] == '「' && text[i + 1] == ' ')
        //            {
        //                temp_text += text[i];
        //                i++;
        //                continue;
        //            }
        //            else
        //            {
        //                temp_text += text[i];
        //            }
        //        }
        //        else
        //        {
        //            temp_text += text[i];
        //        }
        //    }
        //    return temp_text;
        //}

        //public string ConvertJpCharToEng(string text)
        //{
        //    string temp = string.Empty;
        //    for (int i = 0; i < text.Length; i++)
        //    {
        //        if (DeReplacemant.ContainsKey(text[i]))
        //        {
        //            temp += DeReplacemant[text[i]];
        //        }
        //        else
        //        {
        //            temp += text[i];
        //        }
        //    }
        //    return temp;
        //}

    }
    public class ScriptItem
    {
        public string lang;
        public string[] preText;
        public string context;
        public string text;
        public string commentType;
        public string fullText;
        public int size;

        public ScriptItem(string Lang, string Context, string Text, string CommentType, int Size, char[] remChar, Dictionary<char, char> DeReplacemant)
        {
            lang = Lang;
            size = Size;
            context = GetContext(Context, Size, out preText);
            //if (context.IndexOf("他者を降して力を示せ、夢を求めて円環を揃えろ。") != -1)
            //{
            //    int aaaa = 1;
            //}
            string temp_text = string.Empty;
            if (!string.IsNullOrWhiteSpace(context))
            {
                for (int i = 0; i < context.Length; i++)
                {
                    if (Array.IndexOf(remChar, context[i]) != -1)
                    {
                        continue;
                    }
                    else if (DeReplacemant.ContainsKey(context[i]))
                    {
                        temp_text += DeReplacemant[context[i]];
                    }
                    else
                    {
                        temp_text += context[i].ToString().ToLower();
                    }
                }
                context = temp_text;
            }
            if (preText.Length > 0)
            {
                for (int j = 0; j < preText.Length; j++)
                {
                    if (!string.IsNullOrWhiteSpace(preText[j]))
                    {
                        temp_text = string.Empty;
                        for (int i = 0; i < preText[j].Length; i++)
                        {
                            if (Array.IndexOf(remChar, preText[j][i]) != -1)
                            {
                                continue;
                            }
                            else if (DeReplacemant.ContainsKey(preText[j][i]))
                            {
                                temp_text += DeReplacemant[preText[j][i]];
                            }
                            else
                            {
                                temp_text += preText[j][i].ToString().ToLower();
                            }
                        }
                        preText[j] = temp_text;
                    }
                }
            }
            text = Text;
            fullText = string.Empty;
            for (int i = 0; i < preText.Length; i++)
            {
                fullText += preText[i];
            }
            fullText += context;
            commentType = CommentType;
        }


        private string GetContext(string text, int size, out string[] PreText)
        {
            PreText = new string[size - 1];
            string temp = string.Empty;
            string[] ch = { "||" };
            if (size > 1)
            {
                string[] temp_arr = text.Split(ch, StringSplitOptions.None);
                temp = temp_arr[size - 1];
                for (int i = 0; i < size - 1; i++)
                {
                    if (temp_arr[i].IndexOf('「') != -1)
                    {
                        PreText[i] = temp_arr[i].Substring(temp_arr[i].IndexOf('「'));
                    }
                    else
                    {
                        PreText[i] = temp_arr[i];
                    }
                }
            }
            else
            {
                temp = text;
            }
            if (temp.IndexOf('「') != -1)
            {
                text = temp.Substring(temp.IndexOf('「'));
            }
            else
            {
                text = temp;
            }
            return text;
        }
    }
}
