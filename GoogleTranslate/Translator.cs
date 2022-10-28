using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Translator.IO;
using System.Xml;
using System.Windows.Forms;

namespace Translator
{
    public class GoogleTrans
    {
        //private Cache cache;
        private CacheTrans cache;
        public struct TranslateInfo
        {
            public string output_text;
            public string translit;
            public TranslateInfo(string out_tex, string trans)
            {
                output_text = out_tex;
                translit = trans;
            }
        }
        private bool istranslated;
        public bool isTranslated
        {
            get { return istranslated; }
            set { istranslated = value; }
        }

        private string err;
        public string ErrorLog
        {
            get { return err; }
            set { err = value; }
        }

        private string trtext;
        public string TranslatedText
        {
            get { return trtext; }
            set { trtext = value; }
        }
        private string ch_zero;
        public GoogleTrans(string AppPath)
        {
            ch_zero = ((char)8203).ToString();
            istranslated = true;
            err = null;
            trtext = null;
            cache = new CacheTrans(AppPath);
        }
        //public void ClearCache()
        //{
        //    cache.ClearCache();
        //}

        /// <summary>
        /// Перевод текста с указанного языка
        /// </summary>
        /// <param name="str">Строка для перевода</param>
        /// <param name="from">Указывает, с какого языка происходит перевод(например eu)</param>
        /// <param name="to">Указывает, на какой язык происходит перевод(например ru)</param>
        /// <returns>Возвращает переведенный текст</returns>
        public string Translate(string str, string from, string to)
        {
            string res = "";
            string res_translit = "";
            CacheTrans.Lang lg;
            switch (to)
            {
                case "ru": lg = CacheTrans.Lang.ru;
                    break;
                case "en": lg = CacheTrans.Lang.en;
                    break;
                case "ja": lg = CacheTrans.Lang.jp;
                    break;
                default: lg = CacheTrans.Lang.ru;
                    break;
            }
            if (cache.IndexOf(str, out res, out res_translit, lg))
            {
                return res;
            }
            string hex;
            string final_str = "";
            hex = ConvertStringToHex(str, Encoding.UTF8);
            for (int i = 0; i < hex.Length; i = i + 2)
            {
                final_str += "%" + hex[i] + hex[i + 1];
            }
            res = dowload_page(final_str, from, to);
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
        /// <summary>
        /// Перевод текста с указанного языка на русский язык
        /// </summary>
        /// <param name="str">Строка для перевода</param>
        /// <param name="from">Указывает, с какого языка происходит перевод(например eu)</param>
        /// <returns>Возвращает переведенный текст</returns>
        public string Translate(string str, string from)
        {
            string res = "";
            string res_translit = "";
            if (cache.IndexOf(str, out res, out res_translit, CacheTrans.Lang.ru))
            {
                return res;
            }
            string hex;
            string final_str = "";
            hex = ConvertStringToHex(str, Encoding.UTF8);
            for (int i = 0; i < hex.Length; i = i + 2)
            {
                final_str += "%" + hex[i] + hex[i + 1];
            }
            res = dowload_page(final_str, from, "ru");
            if (!res.Equals("error"))
            {
                cache.Add(str, res, "-0123456789-", CacheTrans.Lang.ru);
                return res;
            }
            else
                return str;
        }

        /// <summary>
        /// Перевод текста с указанного языка
        /// </summary>
        /// <param name="str">Строка для перевода</param>
        /// <param name="from">Указывает, с какого языка происходит перевод(например eu)</param>
        /// <param name="to">Указывает, на какой язык происходит перевод(например ru)</param>
        /// <param name="translit">Указывает, нужен ли транслит</param>
        /// <returns>Возвращает переведенный текст</returns>
        public TranslateInfo Translate(string str, string from, string to, bool translit)
        {
            string res = "";
            string res_translit = "";
            CacheTrans.Lang lg;
            switch (to)
            {
                case "ru": lg = CacheTrans.Lang.ru;
                    break;
                case "en": lg = CacheTrans.Lang.en;
                    break;
                case "ja": lg = CacheTrans.Lang.jp;
                    break;
                default: lg = CacheTrans.Lang.ru;
                    break;
            }
            if (cache.IndexOf(str, out res, out res_translit, lg))
            {
                return new TranslateInfo(res,res_translit);
            }
            string hex;
            string final_str = "";
            hex = ConvertStringToHex(str, Encoding.UTF8);
            for (int i = 0; i < hex.Length; i = i + 2)
            {
                final_str += "%" + hex[i] + hex[i + 1];
            }
            TranslateInfo ti = dowload_page(final_str, from, to, translit);
            if (isTranslated)
                if (ti.output_text != null && ti.translit != null)
                    cache.Add(str, ti.output_text, ti.translit, lg);
            return ti;
        }

        string googleTemplateUrl = "https://translate.google.com/m?hl=&sl={0}&tl={1}&ie=UTF-8&q={2}";
        Random r = new Random();

        private string dowload_page(string hex, string from, string to)
        {
            ////Задержка для снижения риска отображения recapcha
            //int sleepVal = r.Next(500, 1500);
            //Thread.Sleep(sleepVal);
            int count = 0;
            label1:
            string val = string.Format(googleTemplateUrl, from, to, hex);
            // string val = "https://translate.google.ru/translate_t?hl=&ie=UTF-8&text=" + hex + "&sl="+from+"&tl="+to;
            HttpWebRequest webreque = (HttpWebRequest)WebRequest.Create(val);
            // webreque.UserAgent = "Opera/9.80 (Windows NT 5.1; U; MRA 5.7 (build 03797); ru) Presto/2.10.229 Version/11.60";
            // webreque.UserAgent = "Opera / 9.80(J2ME / MIDP; Opera Mini/ 5.1.21214 / 28.2725; U; en) Presto / 2.8.119 Version / 11.10";
            //webreque.Accept = "*/*";
            webreque.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            webreque.Timeout = 20000;
            HttpWebResponse webrespon;
            string str;
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
                        goto label1;
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
            try
            {
                //String extracted = str.GetBetween("class=\"t0\">", "</div>");
                String extracted = str.GetBetween("class=\"result-container\">", "</div>");
                if (string.IsNullOrEmpty(extracted))
                {
                    Thread.Sleep(2000);
                    count++;
                    goto label1;

                }
                string text = HttpUtility.HtmlDecode(extracted ?? string.Empty);
                text = text.Replace("&quot;", "\"");
                text = text.Replace("\\x26quot;", "\"");
                text = text.Replace("\\x26#39;", "");
                text = text.Replace("\\u003c", "<");
                text = text.Replace("\\u003e", ">");
                text = text.Replace("\\u0026#39;", "'");
                text = text.Replace("\\u0026amp;", "&");
                text = text.Replace("\\u0026quot;", "\"");
                text = text.Replace("\\u0026apos;", "'");
                text = text.Replace("\\u0026lt;", "<");
                text = text.Replace("\\u0026gt;", ">");
                text = text.Replace("\\u003d", "=");
                text = text.Replace("\\u200b", string.Empty);

                text = text.Replace("\\x3c", "<");
                text = text.Replace("\\x3e", ">");
                text = text.Replace("\\x26amp;", "&");
                text = text.Replace("\\x26quot;", "\"");
                text = text.Replace("\\x26apos;", "'");
                text = text.Replace("\\x26lt;", "<");
                text = text.Replace("\\x26gt;", ">");
                text = text.Replace("\\x3d", "=");
                text = text.Replace("\\x200b", string.Empty);
                text = text.Replace(ch_zero, "");

                trtext = ReplaceOtherChar(text);
                return ReplaceOtherChar(text);
            }
            catch (Exception ex)
            {
                err = ex.Message;
                istranslated = false;
                return "error";
            }
        }

        private string ReplaceOtherChar(string text)
        {
            int index = 0;
            string res = "";
            while (text.IndexOf("&#",index) != -1)
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
        [Obsolete("Использование функции с транслитом больше не поддерживается")]
        private TranslateInfo dowload_page(string hex, string from, string to, bool translit)
        {
            int count = 0;
            label1:
            TranslateInfo ti = new TranslateInfo();
            string val = string.Format(googleTemplateUrl, from, to, hex);
            // string val = "https://translate.google.ru/translate_t?hl=&ie=UTF-8&text=" + hex + "&sl="+from+"&tl="+to;
            HttpWebRequest webreque = (HttpWebRequest)WebRequest.Create(val);
            //webreque.UserAgent = "Opera/9.80 (Windows NT 5.1; U; MRA 5.7 (build 03797); ru) Presto/2.10.229 Version/11.60";
            //webreque.UserAgent = "Opera / 9.80(J2ME / MIDP; Opera Mini/ 5.1.21214 / 28.2725; U; ru) Presto / 2.8.119 Version / 11.10";
            //webreque.Accept = "*/*";
            webreque.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            webreque.Timeout = 20000;
            HttpWebResponse webrespon;
            string str;
            try
            {
                webrespon = (HttpWebResponse)webreque.GetResponse();
                StreamReader str_read = new StreamReader(webrespon.GetResponseStream(), Encoding.UTF8);
                str = str_read.ReadToEnd();
            }
            catch (Exception ex)
            {
                err = ex.Message;
                istranslated = false;
                return ti;
            }
            try
            {
                //String extracted = str.GetBetween("class=\"t0\">", "</div>");
                String extracted = str.GetBetween("class=\"result-container\">", "</div>");
                if (string.IsNullOrEmpty(extracted))
                {
                    Thread.Sleep(2000);
                    count++;
                    goto label1;

                }
                string text = HttpUtility.HtmlDecode(extracted ?? string.Empty);



                text = text.Replace("&quot;", "\"");
                text = text.Replace("\\x26quot;", "\"");
                text = text.Replace("\\x26#39;", "");
                text = text.Replace("\\u003c", "<");
                text = text.Replace("\\u003e", ">");
                text = text.Replace("\\u0026#39;", "'");
                text = text.Replace("\\u0026amp;", "&");
                text = text.Replace("\\u0026quot;", "\"");
                text = text.Replace("\\u0026apos;", "'");
                text = text.Replace("\\u0026lt;", "<");
                text = text.Replace("\\u0026gt;", ">");
                text = text.Replace("\\u003d", "=");
                text = text.Replace("\\u200b", string.Empty);

                text = text.Replace("\\x3c", "<");
                text = text.Replace("\\x3e", ">");
                text = text.Replace("\\x26amp;", "&");
                text = text.Replace("\\x26quot;", "\"");
                text = text.Replace("\\x26apos;", "'");
                text = text.Replace("\\x26lt;", "<");
                text = text.Replace("\\x26gt;", ">");
                text = text.Replace("\\x3d", "=");
                text = text.Replace("\\x200b", string.Empty);
                trtext = ReplaceOtherChar(text);
                ti.output_text = ReplaceOtherChar(text);
                ti.translit = string.Empty;
                //temp = "id=src-translit class=translit";
                //if (str.IndexOf(temp) != -1 && translit)
                //{
                //    ind = str.IndexOf(temp);
                //    ind_last = str.IndexOf("</div>", ind + temp.Length);
                //    startInd = ind + temp.Length;
                //    text_leng = ind_last - startInd;
                //    text = str.Substring(startInd, text_leng);
                //    text = text.Substring(text.IndexOf(">")+1);
                //    ti.translit = ReplaceOtherChar(text);
                //}
                return ti;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                istranslated = false;
                return ti;
            }
        }
        /// <summary>
        /// Преобразует строку в Hex формат
        /// </summary>
        /// <param name="input">Исходный текст</param>
        /// <param name="encoding">Кодировка текста</param>
        /// <returns>Hex представление строки</returns>
        public static string ConvertStringToHex(String input, System.Text.Encoding encoding)
        {
            Byte[] stringBytes = encoding.GetBytes(input);
            StringBuilder sbBytes = new StringBuilder(stringBytes.Length * 2);
            foreach (byte b in stringBytes)
            {
                sbBytes.AppendFormat("{0:X2}", b);
            }
            return sbBytes.ToString();
        }

    }
    internal class Cache
    {
        private object ReadStruct(FileStream fs, Type t)
        {
            byte[] buffer = new byte[Marshal.SizeOf(t)];
            fs.Read(buffer, 0, Marshal.SizeOf(t));
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Object temp = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), t);
            handle.Free();
            return temp;
        }
        private object ReadStructFromMem(MemoryStream fs, Type t)
        {
            byte[] buffer = new byte[Marshal.SizeOf(t)];
            fs.Read(buffer, 0, Marshal.SizeOf(t));
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Object temp = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), t);
            handle.Free();
            return temp;
        }

        private static byte[] RawSerialize(object anything)
        {
            int rawsize = Marshal.SizeOf(anything);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(anything, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return rawdata;
        }
        public enum lang : uint
        {
            ru = 1,
            en = 2,
            jp = 3
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sindex
        {
            public lang To_Lang;
            public uint position_orig;
            public uint len_orig;
            public uint position_trans;
            public uint len_trans;
            public uint position_translit;
            public uint len_translit;
            public sindex(lang to_lang, uint pos_or, uint len_or, uint pos_tr, uint len_tr, uint pos_lit, uint len_lit)
            {
                To_Lang = to_lang;
                position_orig = pos_or;
                len_orig = len_or;
                position_trans = pos_tr;
                len_trans = len_tr;
                position_translit = pos_lit;
                len_translit = len_lit;
            }
        }
        public struct sdata
        {
            public string original;
            public string translated;
            public string translit;
            public sdata(string origin, string transl, string _translit)
            {
                original = origin;
                translated = transl;
                translit = _translit;
            }
        }
        long data_file_size;
        long current_position;
        FileStream fs_index;
        FileStream fs_data;
        public List<sindex> Index;
        public List<sdata> Data;
        string path;
        public Cache(string AppPath)
        {
            ch_zero = ((char)8203).ToString();
            path = AppPath;
            if (File.Exists(path + "\\cache.idx") && File.Exists(path + "\\cache.dat"))
            {
                fs_index = new FileStream(path + "\\cache.idx", FileMode.Open, FileAccess.ReadWrite);
                fs_data = new FileStream(path + "\\cache.dat", FileMode.Open, FileAccess.ReadWrite);
            }
            else
            {
                fs_index = new FileStream(path + "\\cache.idx", FileMode.Create, FileAccess.ReadWrite);
                fs_data = new FileStream(path + "\\cache.dat", FileMode.Create, FileAccess.ReadWrite);
            }
            data_file_size = fs_data.Length;
            current_position = fs_data.Position;
            LoadCache();
        }
        private void LoadCache()
        {
            Index = new List<sindex>();
            Data = new List<sdata>();
            while (fs_index.Position < fs_index.Length)
            {
                Index.Add((sindex)ReadStruct(fs_index, typeof(sindex)));
            }
            for (int i = 0; i < Index.Count; i++)
            {
                byte[] b_origin = new byte[Index[i].len_orig];
                fs_data.Seek(Index[i].position_orig, SeekOrigin.Begin);
                fs_data.Read(b_origin, 0, b_origin.Length);
                byte[] b_trans = new byte[Index[i].len_trans];
                fs_data.Seek(Index[i].position_trans, SeekOrigin.Begin);
                fs_data.Read(b_trans, 0, b_trans.Length);

                byte[] b_translit = new byte[Index[i].len_translit];
                fs_data.Seek(Index[i].position_translit, SeekOrigin.Begin);
                fs_data.Read(b_translit, 0, b_translit.Length);

                Data.Add(new sdata(Encoding.Unicode.GetString(b_origin), Encoding.Unicode.GetString(b_trans), Encoding.Unicode.GetString(b_translit)));
            }
            fs_index.Close();
            fs_index = null;
            fs_data.Close();
            fs_data = null;
        }
        public void Add(string origin, string translation, string translit, lang to_lang)
        {
            if (translit == null)
                translit = "";
            if (fs_data == null && fs_index == null)
            {
                fs_index = new FileStream(path + "\\cache.idx", FileMode.Open, FileAccess.ReadWrite);
                fs_data = new FileStream(path + "\\cache.dat", FileMode.Open, FileAccess.ReadWrite);
            }
            fs_data.Seek(0, SeekOrigin.End);
            fs_index.Seek(0, SeekOrigin.End);
            current_position = fs_data.Length;
            long old_pos = current_position;
            byte[] b_orig = Encoding.Unicode.GetBytes(origin);
            byte[] b_trans = Encoding.Unicode.GetBytes(translation);
            byte[] b_translit = Encoding.Unicode.GetBytes(translit);
            sindex ind = new sindex();
            ind.To_Lang = to_lang;
            ind.len_orig = (uint)b_orig.Length;
            ind.len_trans = (uint)b_trans.Length;
            ind.len_translit = (uint)b_translit.Length;
            ind.position_orig = Convert.ToUInt32(current_position);
            current_position += ind.len_orig + 1;
            ind.position_trans = Convert.ToUInt32(current_position);
            current_position += ind.len_trans + 1;
            ind.position_translit = Convert.ToUInt32(current_position);
            current_position += ind.len_translit + 1;

            long temp = current_position;
            current_position = old_pos;
            old_pos = temp;
            byte[] buffer_ind = RawSerialize(ind);
            fs_data.Position = current_position;
            fs_index.Write(buffer_ind, 0, buffer_ind.Length);
            fs_data.Write(b_orig, 0, b_orig.Length);
            fs_data.WriteByte(0x00);
            fs_data.Write(b_trans, 0, b_trans.Length);
            fs_data.WriteByte(0x00);
            fs_data.Write(b_translit, 0, b_translit.Length);
            fs_data.WriteByte(0x00);
            current_position = fs_data.Position;
            if(fs_data.Position!=old_pos)
                throw new Exception("Произошла рассинхронизация в данных");
            Index.Add(ind);
            Data.Add(new sdata(origin, translation, translit));
            data_file_size = fs_data.Length;
            fs_index.Close();
            fs_index = null;
            fs_data.Close();
            fs_data = null;
            
        }
        private string ch_zero;
        public bool IndexOf(string find_origin, out string translated_text, out string translit_text, lang to_lang)
        {
            //for (int i = 0; i < Data.Count; i++)
            //{
            //    if (find_origin.Trim().ToUpper() == Data[i].original.Trim().ToUpper())
            //    {
            //        if (Index[i].To_Lang == to_lang)
            //        {
            //            translated_text = Data[i].translated;
            //            translated_text = translated_text.Replace(ch_zero, "");
            //            translit_text = Data[i].translit;
            //            return true;
            //        }
            //    }
            //}
            //translated_text = "";
            //translit_text = "";
            //return false;
            find_origin = find_origin.Trim().ToUpper();
            List<sdata> found = Data.FindAll(x => x.original.Trim().ToUpper().Equals(find_origin));
            if (found.Count > 0)
            {
                for (int i = 0; i < found.Count; i++)
                {
                    int ind = Data.IndexOf(found[i]);
                    if (Index[ind].To_Lang.Equals(to_lang))
                    {
                        translated_text = Data[ind].translated;
                        translated_text = translated_text.Replace(ch_zero, "");
                        translit_text = Data[ind].translit;
                        return true;
                    }
                }
                translated_text = "";
                translit_text = "";
                return false;
            }
            else
            {
                translated_text = "";
                translit_text = "";
                return false;
            }
        }
        public void ClearCache()
        {
            fs_data.Close();
            fs_index.Close();
            fs_index = new FileStream(path + "\\cache.idx", FileMode.Create, FileAccess.ReadWrite);
            fs_data = new FileStream(path + "\\cache.dat", FileMode.Create, FileAccess.ReadWrite);
            Index.Clear();
            Data.Clear();
            current_position = 0;
            data_file_size = 0;
        }

    }
}
