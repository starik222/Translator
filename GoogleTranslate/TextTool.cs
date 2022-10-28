using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Translator;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading;
using System.Configuration;

namespace Translator
{
    public class TextTool
    {
        public string GetSplitterChar()
        {
            return splitChar;
        }


        public enum TranslateService
        {
            Google,
            Microsoft
        }

        public struct RepConst
        {
            public string orig;
            public string repl;
            public RepConst(string Original, string Replacment)
            {
                orig = Original;
                repl = Replacment;
            }
        }

        public enum TransMethod
        {
            JaRu,
            JaEnRu
        }

        public List<NAMES> names { get; set; }
        public List<NAMES> otherChar { get; set; }
        public List<NAMES> RepText { get; set; }
        public GoogleTrans gt { get; set; }
        Dictionary<char, char> replacemant { get; set; }
        Dictionary<char, char> smartReplacemant { get; set; }
        public List<RepConst> Prefix;
        public List<RepConst> NameConst;

        private string ApplicationPath;

        private List<char> smartChars;
        private string splitChar = "|";

        private TranslationConfig _config;

        public TextTool(string appPath)
        {
            _config = new TranslationConfig();
            splitChar = _config.GetAppSetting("TextSplitter");
            ApplicationPath = appPath;
            Prefix = new List<RepConst>();
            NameConst = new List<RepConst>();
            NameConst.Add(new RepConst("お兄ちゃん", "Oni-chan"));
            NameConst.Add(new RepConst("お兄さん", "Oni-san"));
            NameConst.Add(new RepConst("お兄様", "Oni-sama"));
            Prefix.Add(new RepConst("さん", "-san"));
            Prefix.Add(new RepConst("君", "-kun"));
            Prefix.Add(new RepConst("ちゃん", "-chan"));
            Prefix.Add(new RepConst("さま", "-sama"));
            Prefix.Add(new RepConst("様", "-sama"));
            Prefix.Add(new RepConst("先輩", "-senpai"));
            gt = new GoogleTrans(appPath);
            names = new List<NAMES>();
            RepText = new List<NAMES>();
            otherChar = new List<NAMES>();
            if (!File.Exists(appPath + "\\ReplaceNames.txt"))
            {
                File.Create(appPath + "\\ReplaceNames.txt").Close();
            }
            string[] lines = File.ReadAllLines(appPath + "\\ReplaceNames.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                names.Add(new NAMES(lines[i], splitChar));
            }
            names = TestDictonary(names, appPath + "\\ReplaceNames.txt");
            names.Sort((y, x) => x.orig_name.Length.CompareTo(y.orig_name.Length));
            if (!File.Exists(appPath + "\\ReplaceChar.txt"))
            {
                File.Create(appPath + "\\ReplaceChar.txt").Close();
            }
            string[] lines2 = File.ReadAllLines(appPath + "\\ReplaceChar.txt");
            for (int i = 0; i < lines2.Length; i++)
            {
                otherChar.Add(new NAMES(lines2[i], splitChar));
            }

            if (!File.Exists(appPath + "\\ReplaceText.txt"))
            {
                File.Create(appPath + "\\ReplaceText.txt").Close();
                Thread.Sleep(200);
            }
            string[] lines3 = File.ReadAllLines(appPath + "\\ReplaceText.txt");
            List<string> nLines = new List<string>();
            for (int i = 0; i < lines3.Length; i++)
            {
                if (!nLines.Contains(lines3[i]))
                    nLines.Add(lines3[i]);
            }
            for (int i = 0; i < nLines.Count; i++)
            {
                RepText.Add(new NAMES(nLines[i], splitChar));
            }
            RepText = TestDictonary(RepText, appPath + "\\ReplaceText.txt");
            RepText.Sort((y, x) => x.orig_name.Length.CompareTo(y.orig_name.Length));

            replacemant = new Dictionary<char, char>();
            smartReplacemant = new Dictionary<char, char>();
            string chars1 = "　０１２３４５６７８９ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ！＠＃＄％＊″：；？〜）（，．―";
            string chars2 = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%*\":;?~)(,.-";
            string jpChar = "　０１２３４５６７８９ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ！＠＃＄％＊″：；？〜）（，．―＋。";
            string enChar = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%*\":;?~)(,.-+.";
            for (int i = 0; i < chars1.Length; i++)
            {
                replacemant.Add(chars2[i], chars1[i]);
            }
            for (int i = 0; i < jpChar.Length; i++)
            {
                smartReplacemant.Add(jpChar[i], enChar[i]);
            }
            smartChars = new List<char>();
            smartChars.AddRange(new char[] { '　', ' ', '|', '，', ',', '。', '.', '・' });
        }

        private List<NAMES> TestDictonary(List<NAMES> dict, string DictPath)
        {
            bool NeedSave = false;
            List<NAMES> ndict = new List<NAMES>();
            for (int i = 0; i < dict.Count; i++)
            {
                int pos = IsDictContainsValue(ndict, dict[i].orig_name);
                if (pos == -1)
                {
                    ndict.Add(dict[i]);
                }
                else
                {
                    if (ndict[pos].translit_name == dict[i].translit_name)
                    {
                        NeedSave = true;
                        continue;
                    }
                    BindingList<NAMES> bl = new BindingList<NAMES>();
                    bl.Add(ndict[pos]);
                    bl.Add(dict[i]);
                    Form_select_var f_sel = new Form_select_var();
                    f_sel.dataGridView1.DataSource = bl;
                    if (f_sel.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        NeedSave = true;
                        if (pos == -1)
                            throw new Exception("Ожидалось не отрицательное значение");
                        ndict[pos].translit_name = bl[f_sel.SelectIndex].translit_name;
                    }
                }
            }
            if (NeedSave)
            {
                StreamWriter sw = new StreamWriter(DictPath, false, Encoding.UTF8);
                for (int i = 0; i < ndict.Count; i++)
                {
                    sw.WriteLine(ndict[i].orig_name + splitChar + ndict[i].translit_name);
                }
                sw.Close();
            }
            return ndict;
        }

        private int IsDictContainsValue(List<NAMES> dict, string text)
        {
            for (int i = 0; i < dict.Count; i++)
            {
                if (dict[i].orig_name == text)
                    return i;
            }
            return -1;
        }

        private List<NAMES> Reload(string file)
        {
            List<NAMES> data = new List<NAMES>();
            if (!File.Exists(ApplicationPath + "\\"+ file))
            {
                File.Create(ApplicationPath + "\\" + file);
            }
            string[] lines = File.ReadAllLines(ApplicationPath + "\\" + file);
            for (int i = 0; i < lines.Length; i++)
            {
                data.Add(new NAMES(lines[i], splitChar));
            }
            data = TestDictonary(data, ApplicationPath + "\\" + file);
            return data;
        }

        private void AddToDict(List<NAMES> dict, string file, string original, string transl)
        {
            dict.Add(new NAMES(original + splitChar + transl, splitChar));
            StreamWriter sw = new StreamWriter(ApplicationPath + "\\" + file, false, Encoding.UTF8);
            for (int i = 0; i < dict.Count; i++)
            {
                sw.WriteLine(dict[i].orig_name + splitChar + dict[i].translit_name);
            }
            sw.Close();
        }

        public void AddToDictText(string original, string transl)
        {
            AddToDict(RepText, "ReplaceText.txt", original, transl);
        }

        public void AddToDictNames(string original, string transl)
        {
            AddToDict(names, "ReplaceNames.txt", original, transl);
        }

        public void ReloadReplacedNames()
        {
            names = Reload("ReplaceNames.txt");
        }

        public void ReloadReplacedText()
        {
            RepText = Reload("ReplaceText.txt");
        }

        public void ReloadReplacedOtherChar()
        {
            otherChar = Reload("ReplaceChar.txt");
        }

        public void ReloadAll()
        {
            ReloadReplacedNames();
            ReloadReplacedText();
            ReloadReplacedOtherChar();
        }

        public string ConvertEngCharToJp(string text)
        {
            string temp = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                if (replacemant.ContainsKey(text[i]))
                {
                    temp += replacemant[text[i]];
                }
                else
                {
                    temp += text[i];
                }
            }
            return temp;
        }

        public string TranslateName(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                return name;
            GoogleTrans.TranslateInfo ti = new GoogleTrans.TranslateInfo();
            ti = gt.Translate(name, "ja", "en", true);
            if (string.IsNullOrEmpty(ti.translit))
                return "error";
            return CompareAndReplace(ti.translit, otherChar);

        }


        public string TranslateText(string text, TransMethod method)
        {
            string ntext = "";
            if(string.IsNullOrWhiteSpace(text))
                return text;
            //text = text.Replace(" ", "");
            text = CompareAndReplace(text, RepText);
            if (text.Trim()[0] == '「' && text.Trim()[text.Trim().Length - 1] == '」')
            {
                //text = text.Trim().Remove(0, 1);
                //text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    if (method == TransMethod.JaRu)
                    {
                        ti = gt.Translate(text, "ja", "ru");
                    }
                    else if (method == TransMethod.JaEnRu)
                    {
                        ti = gt.Translate(text, "ja", "en");
                        ti = gt.Translate(ti, "en", "ru");
                    }
                }
                catch (Exception ex)
                {

                }
                //ti = ti.Replace("\"", "");
                //ti = ti.Trim();
                //ntext = '「' + ti + '」';

                if (ti.StartsWith("\"") && ti.EndsWith("\""))
                {
                    ti = '「' + ti + '」';
                    ti = ti.Replace("\"", "");
                }
                ti = ti.Replace('«', '「');
                ti = ti.Replace('»', '」');

                ti = ti.Replace("\"", "“");
                ntext = ti;
            }
            else if (text.Trim()[0] == '（' && text.Trim()[text.Trim().Length - 1] == '）')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    if (method == TransMethod.JaRu)
                    {
                        ti = gt.Translate(text, "ja", "ru");
                    }
                    else if (method == TransMethod.JaEnRu)
                    {
                        ti = gt.Translate(text, "ja", "en");
                        ti = gt.Translate(ti, "en", "ru");
                    }
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '（' + ti + '）';
            }
            else if (text.Trim()[0] == '《' && text.Trim()[text.Trim().Length - 1] == '》')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    if (method == TransMethod.JaRu)
                    {
                        ti = gt.Translate(text, "ja", "ru");
                    }
                    else if (method == TransMethod.JaEnRu)
                    {
                        ti = gt.Translate(text, "ja", "en");
                        ti = gt.Translate(ti, "en", "ru");
                    }
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '《' + ti + '》';
            }
            else if (text.Trim()[0] == '『' && text.Trim()[text.Trim().Length - 1] == '』')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    if (method == TransMethod.JaRu)
                    {
                        ti = gt.Translate(text, "ja", "ru");
                    }
                    else if (method == TransMethod.JaEnRu)
                    {
                        ti = gt.Translate(text, "ja", "en");
                        ti = gt.Translate(ti, "en", "ru");
                    }
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '『' + ti + '』';
            }
            else
            {
                string ti = "";
                try
                {
                    if (method == TransMethod.JaRu)
                    {
                        ti = gt.Translate(text, "ja", "ru");
                    }
                    else if (method == TransMethod.JaEnRu)
                    {
                        ti = gt.Translate(text, "ja", "en");
                        ti = gt.Translate(ti, "en", "ru");
                    }
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = ti;
            }
            if (ntext == "error" || ntext == null)
            {
                int aa = 1;
            }

            for (int i = 0; i < otherChar.Count; i++)
            {
                ntext = ntext.Replace(otherChar[i].orig_name, otherChar[i].translit_name);
            }
           /* ntext = ntext.Replace(":", "");
            ntext = ntext.Replace("~", "");
            ntext = ntext.Replace("\\x3d", "＝");
            ntext = ntext.Replace("...", "…");*/

            return ntext;
        }

        public string TranslateText(string text, string ToLang, bool UseReplaceText = true)
        {
            return TranslateText(text, "ja", ToLang, UseReplaceText);
        }


        public string TranslateText(string text, string FromLang, string ToLang, bool UseReplaceText = true)
        {
            string ntext = "";
            if (string.IsNullOrWhiteSpace(text))
                return text;
            //text = text.Replace(" ", "");
            if (UseReplaceText)
                text = CompareAndReplace(text, RepText);
            if (text.Trim()[0] == '「' && text.Trim()[text.Trim().Length - 1] == '」')
            {
                //text = text.Trim().Remove(0, 1);
                //text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    ti = gt.Translate(text, FromLang, ToLang);
                }
                catch (Exception ex)
                {

                }
                //ti = ti.Replace("\"", "");
                //ti = ti.Trim();
                //ntext = '「' + ti + '」';
                if (ti.StartsWith("\"") && ti.EndsWith("\""))
                {
                    ti = '「' + ti + '」';
                    ti = ti.Replace("\"", "");
                }
                ti = ti.Replace('«', '「');
                ti = ti.Replace('»', '」');

                ti = ti.Replace("\"", "“");
                ntext = ti;
            }
            else if (text.Trim()[0] == '（' && text.Trim()[text.Trim().Length - 1] == '）')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    ti = gt.Translate(text, FromLang, ToLang);
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '（' + ti + '）';
            }
            else if (text.Trim()[0] == '《' && text.Trim()[text.Trim().Length - 1] == '》')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    ti = gt.Translate(text, FromLang, ToLang);
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '《' + ti + '》';
            }
            else if (text.Trim()[0] == '『' && text.Trim()[text.Trim().Length - 1] == '』')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    ti = gt.Translate(text, FromLang, ToLang);
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '『' + ti + '』';
            }
            else
            {
                string ti = "";
                try
                {
                    ti = gt.Translate(text, FromLang, ToLang);
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = ti;
            }
            if (ntext == "error" || ntext == null)
            {
                int aa = 1;
            }

            for (int i = 0; i < otherChar.Count; i++)
            {
                ntext = ntext.Replace(otherChar[i].orig_name, otherChar[i].translit_name);
            }
            /* ntext = ntext.Replace(":", "");
             ntext = ntext.Replace("~", "");
             ntext = ntext.Replace("\\x3d", "＝");
             ntext = ntext.Replace("...", "…");*/

            return ntext;
        }



        public GoogleTrans.TranslateInfo GoogleTranslate(string text, string from, string to)
        {
            GoogleTrans.TranslateInfo ti = new GoogleTrans.TranslateInfo();
            if (string.IsNullOrWhiteSpace(text))
                return ti;
            text = CompareAndReplace(text, RepText);
            ti = gt.Translate(text, from, to, true);
            return ti;
        }




        public string TranslateText(string text, TranslateService service, string ToLang)
        {
            string ntext = "";
            if (string.IsNullOrWhiteSpace(text))
                return text;
            //text = text.Replace(" ", "");
            text = CompareAndReplace(text, RepText);
            if (text.Trim()[0] == '「' && text.Trim()[text.Trim().Length - 1] == '」')
            {
                //text = text.Trim().Remove(0, 1);
                //text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    switch (service)
                    {
                        case TranslateService.Google:
                            {
                                ti = gt.Translate(text, "ja", ToLang);
                                break;
                            }
                        case TranslateService.Microsoft:
                            {
                                throw new NotImplementedException("Удалено за ненадобностью.");
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {

                }
                //ti = ti.Replace("\"", "");
                //ti = ti.Trim();
                //ntext = '「' + ti + '」';
                if (ti.StartsWith("\"") && ti.EndsWith("\""))
                {
                    ti = '「' + ti + '」';
                    ti = ti.Replace("\"", "");
                }
                ti = ti.Replace('«', '「');
                ti = ti.Replace('»', '」');

                ti = ti.Replace("\"", "“");
                ntext = ti;
            }
            else if (text.Trim()[0] == '（' && text.Trim()[text.Trim().Length - 1] == '）')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    switch (service)
                    {
                        case TranslateService.Google:
                            {
                                ti = gt.Translate(text, "ja", ToLang);
                                break;
                            }
                        case TranslateService.Microsoft:
                            {
                                throw new NotImplementedException("Удалено за ненадобностью.");
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '（' + ti + '）';
            }
            else if (text.Trim()[0] == '《' && text.Trim()[text.Trim().Length - 1] == '》')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    switch (service)
                    {
                        case TranslateService.Google:
                            {
                                ti = gt.Translate(text, "ja", ToLang);
                                break;
                            }
                        case TranslateService.Microsoft:
                            {
                                throw new NotImplementedException("Удалено за ненадобностью.");
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '《' + ti + '》';
            }
            else if (text.Trim()[0] == '『' && text.Trim()[text.Trim().Length - 1] == '』')
            {
                text = text.Trim().Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                string ti = "";
                try
                {
                    switch (service)
                    {
                        case TranslateService.Google:
                            {
                                ti = gt.Translate(text, "ja", ToLang);
                                break;
                            }
                        case TranslateService.Microsoft:
                            {
                                throw new NotImplementedException("Удалено за ненадобностью.");
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = '『' + ti + '』';
            }
            else
            {
                string ti = "";
                try
                {
                    switch (service)
                    {
                        case TranslateService.Google:
                            {
                                ti = gt.Translate(text, "ja", ToLang);
                                break;
                            }
                        case TranslateService.Microsoft:
                            {
                                throw new NotImplementedException("Удалено за ненадобностью.");
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                }
                ti = ti.Replace("\"", "“");
                ti = ti.Trim();
                ntext = ti;
            }
            if (ntext == "error" || ntext == null)
            {
                int aa = 1;
            }

            for (int i = 0; i < otherChar.Count; i++)
            {
                ntext = ntext.Replace(otherChar[i].orig_name, otherChar[i].translit_name);
            }
            /* ntext = ntext.Replace(":", "");
             ntext = ntext.Replace("~", "");
             ntext = ntext.Replace("\\x3d", "＝");
             ntext = ntext.Replace("...", "…");*/

            return ntext;
        }

        public string PrepareText(string text, int maxLen, string separator)
        {
            string[] ch = { " " };
            List<string> lines = new List<string>();
            string[] slova = text.Split(ch, StringSplitOptions.RemoveEmptyEntries);
            if (slova.Length <= 1)
                return text;
            string aa = "", bb = "";
            for (int i = 0; i < slova.Length; i++)
            {
                if (slova[i].Length >= maxLen)
                {
                    if (!string.IsNullOrWhiteSpace(bb))
                        lines.Add(bb.Trim());
                    lines.Add(slova[i].Trim());
                    bb = "";
                    continue;
                }
                aa = bb;
                bb += slova[i] + " ";
                if (bb.Length > maxLen)
                {
                    lines.Add(aa.Trim());
                    aa = "";
                    bb = "";
                    i--;
                }
                if (slova.Length - 1 == i)
                {
                    lines.Add(bb.Trim());
                }
            }
            return string.Join(separator, lines.ToArray());
        }

        public string PrepareText(string text, int maxLen, int MaxLines, string separator)
        {
            string[] ch = { " " };
            List<string> lines = new List<string>();
            string[] slova = text.Split(ch, StringSplitOptions.RemoveEmptyEntries);
            if (slova.Length <= 1)
                return text;
            string aa = "", bb = "";
            for (int i = 0; i < slova.Length; i++)
            {
                if (slova[i].Length >= maxLen)
                {
                    if (!string.IsNullOrWhiteSpace(bb))
                        lines.Add(bb.Trim());
                    lines.Add(slova[i].Trim());
                    bb = "";
                    continue;
                }
                aa = bb;
                bb += slova[i] + " ";
                if (bb.Length > maxLen)
                {
                    lines.Add(aa.Trim());
                    aa = "";
                    bb = "";
                    i--;
                }
                if (slova.Length - 1 == i)
                {
                    lines.Add(bb.Trim());
                }
            }
            if (lines.Count > MaxLines)
            {
                lines.Clear();

                decimal d_sl_count = (decimal)slova.Length / MaxLines;
                int sl_count = Convert.ToInt32(Math.Round(d_sl_count));
                int k=0;
                for (int i = 0; i < MaxLines; i++)
                {
                    string temp = "";
                    while (((k < sl_count * (i + 1)) && k < slova.Length) || (i == MaxLines - 1 && k < slova.Length))
                    {
                        temp += slova[k] + " ";
                        k++;
                    }
                    lines.Add(temp);
                }
            }
            return string.Join(separator, lines.ToArray());
        }


        //private string EndText(string text)
        //{
        //    switch (text[text.Length - 1])
        //    {
        //        case '。':
        //        case '！':
        //        case '…':
        //        case '？':
        //    }
        //}


        public string[] PrepareText(string text, int maxLen)
        {
            string[] ch = { " " };
            List<string> lines = new List<string>();
            string[] slova = text.Split(ch, StringSplitOptions.RemoveEmptyEntries);
            if (slova.Length <= 1)
            {
                string[] temp = { text };
                return temp;
            }
            string aa = "", bb = "";
            for (int i = 0; i < slova.Length; i++)
            {
                if (slova[i].Length >= maxLen)
                {
                    if (!string.IsNullOrWhiteSpace(bb))
                        lines.Add(bb.Trim());
                    lines.Add(slova[i].Trim());
                    bb = "";
                    continue;
                }
                aa = bb;
                bb += slova[i] + " ";
                if (bb.Length > maxLen)
                {
                    lines.Add(aa.Trim());
                    aa = "";
                    bb = "";
                    i--;
                }
                if (slova.Length - 1 == i)
                {
                    lines.Add(bb.Trim());
                }
            }
            return lines.ToArray();
        }

        public string[] PrepareText(string text, int maxLen, int MaxLines)
        {
            string[] ch = { " " };
            List<string> lines = new List<string>();
            string[] slova = text.Split(ch, StringSplitOptions.RemoveEmptyEntries);
            if (slova.Length <= 1)
            {
                string[] temp = { text };
                return temp;
            }
            string aa = "", bb = "";
            for (int i = 0; i < slova.Length; i++)
            {
                if (slova[i].Length >= maxLen)
                {
                    if (!string.IsNullOrWhiteSpace(bb))
                        lines.Add(bb.Trim());
                    lines.Add(slova[i].Trim());
                    bb = "";
                    continue;
                }
                aa = bb;
                bb += slova[i] + " ";
                if (bb.Length > maxLen)
                {
                    lines.Add(aa.Trim());
                    aa = "";
                    bb = "";
                    i--;
                }
                if (slova.Length - 1 == i)
                {
                    lines.Add(bb.Trim());
                }
            }
            if (lines.Count > MaxLines)
            {
                lines.Clear();

                decimal d_sl_count = (decimal)slova.Length / MaxLines;
                int sl_count = Convert.ToInt32(Math.Round(d_sl_count));
                int k = 0;
                for (int i = 0; i < MaxLines; i++)
                {
                    string temp = "";
                    while (((k < sl_count * (i + 1)) && k < slova.Length) || (i == MaxLines - 1 && k < slova.Length))
                    {
                        temp += slova[k] + " ";
                        k++;
                    }
                    lines.Add(temp);
                }
            }
            return lines.ToArray();
        }

        public string RemoveRepeatingChars(string text, int repeatCount)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int rep = 0;
            char c = (char)0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != c)
                {
                    c = text[i];
                    stringBuilder.Append(text[i]);
                    rep = 0;
                }
                else
                {
                    if (rep < repeatCount)
                    {
                        rep++;
                        stringBuilder.Append(text[i]);
                    }
                }
            }
            return stringBuilder.ToString();
        }


        public string CompareAndReplace(string OriginalText, List<NAMES> names, bool smartReplacment = false)
        {
            string str = OriginalText;
            bool replaced = false;
            for (int j = 0; j < NameConst.Count; j++)
            {
                if (str.IndexOf(NameConst[j].orig) != -1)
                {
                    str = str.Replace(NameConst[j].orig, NameConst[j].repl);
                    replaced = true;
                }
            }
            if (smartReplacment)
            {
                str = PrepareStringToReplace(str);
                for (int i = 0; i < names.Count; i++)
                {
                    string smartOrigName = PrepareStringToReplace(names[i].orig_name);
                    for (int j = 0; j < Prefix.Count; j++)
                    {
                        if (str.IndexOf(smartOrigName + Prefix[j].orig) != -1)
                        {
                            str = str.Replace(smartOrigName + Prefix[j].orig, names[i].translit_name + Prefix[j].repl);
                            replaced = true;
                        }
                    }
                    if (str.IndexOf(smartOrigName) != -1)
                    {
                        str = str.Replace(smartOrigName, names[i].translit_name);
                        replaced = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < names.Count; i++)
                {
                    for (int j = 0; j < Prefix.Count; j++)
                    {
                        if (str.IndexOf(names[i].orig_name + Prefix[j].orig) != -1)
                        {
                            str = str.Replace(names[i].orig_name + Prefix[j].orig, names[i].translit_name + Prefix[j].repl);
                            replaced = true;
                        }
                    }
                    if (str.IndexOf(names[i].orig_name) != -1)
                    {
                        str = str.Replace(names[i].orig_name, names[i].translit_name);
                        replaced = true;
                    }
                    //for (int j = 0; j < Prefix.Count; j++)
                    //{
                    //    if (str.IndexOf(Prefix[j].orig) != -1)
                    //        str = str.Replace(Prefix[j].orig, Prefix[j].repl);
                    //}
                }
            }
            return (replaced ? str : OriginalText);
        }

        private string PrepareStringToReplace(string text)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (!smartChars.Contains(text[i]))
                {
                    if (smartReplacemant.ContainsKey(text[i]))
                        sb.Append(smartReplacemant[text[i]]);
                    else
                        sb.Append(text[i]);
                }

            }
            return sb.ToString();
        }

        public bool isReplace(string source, List<NAMES> dict, out string result, bool strictReplace)
        {
            result = source;
            foreach (var item in dict)
            {
                if (strictReplace)
                {
                    if (item.orig_name.Equals(source))
                    {
                        result = item.translit_name;
                        return true;
                    }
                }
                else
                {
                    if (source.IndexOf(item.orig_name) != -1)
                    {
                        result = source.Replace(item.orig_name, item.translit_name);
                        return true;
                    }
                }
            }
            return false;
        }

        public class PARAM
        {
            public int index;
            public string param;
            public int Nomer;
            public int Len;
            public string Value;

            public PARAM(int id, string p)
            {
                char[] ch = {'-'};
                index = id;
                param = p;
                p = p.Substring(1, p.Length - 2);
                string[] items = p.Split(ch);
                Nomer = Convert.ToInt32(items[0].Trim());
                Len = Convert.ToInt32(items[1].Trim());
            }
            public void SetValue(string val)
            {
                Value = val;
            }

            public void SetIndex(int val)
            {
                index = val;
            }
        }

        private List<PARAM> GetParam(string text)
        {
            List<PARAM> param = new List<PARAM>();
            string mask = @"\{[0-9]\-[0-9]\}";
            Regex reg = new Regex(mask);
            MatchCollection mc = reg.Matches(text);
            foreach (Match item in mc)
            {
                param.Add(new PARAM(item.Index, item.Value));
            }
            return param;
        }

        private void CalcRealIndex(ref List<PARAM> param)
        {
            int sdvig = 0;
            for (int i = 0; i < param.Count; i++)
            {
                param[i].SetIndex(param[i].index + sdvig);
                sdvig += param[i].Len - 5;
            }
        }

        public string CompareAndReplaceMask(string str, List<NAMES> names)
        {
            List<PARAM> param = new List<PARAM>();
            for (int i = 0; i < names.Count; i++)
            {
                param = GetParam(names[i].orig_name);
                if (param.Count == 0)
                {
                    if (str.IndexOf(names[i].orig_name) != -1)
                    {
                        str = str.Replace(names[i].orig_name, names[i].translit_name);
                    }
                    continue;
                }
                List<string> substr = new List<string>();
                int cur_pos = 0;
                int real_size = 0;
                string mask = string.Empty;
                foreach (PARAM val in param)
                {
                    substr.Add(names[i].orig_name.Substring(cur_pos, val.index - cur_pos));
                    mask+=names[i].orig_name.Substring(cur_pos, val.index - cur_pos);
                    mask += "\\S{" + val.Len + "}";
                    cur_pos = val.index + 5;
                    real_size += substr[substr.Count - 1].Length;
                    real_size += val.Len;
                }
                mask += names[i].orig_name.Substring(cur_pos);
                substr.Add(names[i].orig_name.Substring(cur_pos));
                real_size += substr[substr.Count - 1].Length;
                mask = mask.Replace(".", @"\.");
                mask = mask.Replace("+", @"\+");
                mask = mask.Replace("?", @"\?");
                mask = mask.Replace("$", @"\$");
                mask = mask.Replace("^", @"\^");
                mask = mask.Replace("*", @"\*");
                mask = mask.Replace("(", @"\(");
                if (real_size > str.Length)
                    continue;
                Regex reg = new Regex(mask, RegexOptions.IgnoreCase);
                MatchCollection collect = reg.Matches(str);
                if (collect.Count == 0)
                    continue;
                CalcRealIndex(ref param);
                foreach (Match item in collect)
                {
                    int start_pos = item.Index;
                    string rep_string = names[i].translit_name;
                    for (int j = 0; j < param.Count; j++)
                    {
                        string temp_val = str.Substring(start_pos + param[j].index, param[j].Len);
                        param[j].SetValue(temp_val);
                        rep_string = rep_string.Replace(param[j].param, param[j].Value);
                    }
                    string n_str = str.Substring(0, item.Index);
                    n_str += rep_string;
                    n_str += str.Substring(item.Index + item.Length);
                    str = n_str;
                }

            }
            return str;



            //List<PARAM> param = new List<PARAM>();
            //for (int i = 0; i < names.Count; i++)
            //{
            //    param = GetParam(names[i].orig_name);

            //    if (param.Count == 0)
            //        return CompareAndReplace(str, names);
            //    List<string> substr = new List<string>();
            //    int cur_pos = 0;
            //    int real_size = 0;
            //    foreach (PARAM val in param)
            //    {
            //        substr.Add(names[i].orig_name.Substring(cur_pos, val.index - cur_pos));
            //        cur_pos = val.index + 5;
            //        real_size += substr[substr.Count - 1].Length;
            //        real_size += val.Len;
            //    }
            //    substr.Add(names[i].orig_name.Substring(cur_pos));
            //    real_size += substr[substr.Count - 1].Length;
            //    if (real_size > str.Length)
            //        continue;
            //    int index = -1;
            //    int[] f_index = new int[substr.Count];
            //    for (int j = 0; j < substr.Count; j++)
            //    {
            //        if(string.IsNullOrEmpty(substr[j]))
            //            f_index[j] = -2;
            //    }
            //}
            //return "";
        }

    }
    public class NAMES
    {
        private string Orig;
        private string Trans;
        public string orig_name
        {
            get { return Orig; }
            set { Orig = value; }
        }
        public string translit_name
        {
            get { return Trans; }
            set { Trans = value; }
        }
        public NAMES(string val, string splitter)
        {
            int firstIndex = val.IndexOf(splitter);
            if (firstIndex == -1)
                throw new Exception("Строка не содержит разделителя");
            Orig = val.Substring(0, firstIndex);
            Trans = val.Substring(firstIndex + 1);
        }

        public NAMES()
        {

        }
    }
}
