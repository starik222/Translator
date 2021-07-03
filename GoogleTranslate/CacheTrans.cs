using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Translator.Crypto;
using System.IO;
using Translator.IO;

namespace Translator
{
    internal class CacheTrans
    {
        public string signarure; // DT2 FILE
        public string FileName;
        public List<CacheRecord> Records;
        public Stream stream;
        private CustomBinaryReader reader;
        private CustomBinaryWriter writer;
        private Encoding enc = Encoding.UTF8;
        private cCRC64 crc64;

        public CacheTrans(string AppPath)
        {
            FileName = AppPath + "\\cache.dt2";
            Records = new List<CacheRecord>();
            bool newFile = false;
            if (File.Exists(FileName))
            {
                stream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite);
                newFile = false;
            }
            else
            {
                stream = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite);
                newFile = true;
            }
            reader = new CustomBinaryReader(stream);
            writer = new CustomBinaryWriter(stream);
            if (newFile)
            {
                writer.WriteString("DT2 FILE", enc);
            }
            else
            {
                signarure = reader.ReadString(8, enc);
                if (!signarure.Equals("DT2 FILE"))
                    throw new FileLoadException("Неизвестный формат файла.");
            }
            LoadRecords();
            ImportOnPrevVersion();
            OptimizeCache();
        }

        private void OptimizeCache()
        {
            List<CacheRecord> errRec = Records.FindAll(a => a.TranslatedText.Equals("error"));
            if (errRec == null || errRec.Count == 0)
                return;
            Records.RemoveAll(a => a.TranslatedText.Equals("error"));


            stream = new FileStream(FileName, FileMode.Create);
            writer = new CustomBinaryWriter(stream);
            writer.WriteString("DT2 FILE", enc);
            foreach (var item in Records)
            {
                item.WriteTo(writer);
            }
            stream.Close();
        }

        private void ImportOnPrevVersion()
        {
            if (!File.Exists(Path.GetDirectoryName(FileName) + "\\cache.dat") || !File.Exists(Path.GetDirectoryName(FileName) + "\\cache.idx"))
                return;
            Cache ch = new Cache(Path.GetDirectoryName(FileName));
            for (int i = 0; i < ch.Data.Count; i++)
            {
                Add(ch.Data[i].original, ch.Data[i].translated, ch.Data[i].translit, (Lang)ch.Index[i].To_Lang);
            }
            File.Delete(Path.GetDirectoryName(FileName) + "\\cache.dat");
            File.Delete(Path.GetDirectoryName(FileName) + "\\cache.idx");
        }

        private void LoadRecords()
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                try
                {
                    Records.Add(new CacheRecord(reader));
                }
                catch (EndOfStreamException)
                {
                    stream.Close();
                    RestoreBadCacheFile();
                    return;
                }
            }
            stream.Close();
        }

        private void RestoreBadCacheFile()
        {
            stream = new FileStream(FileName, FileMode.Create);
            writer = new CustomBinaryWriter(stream);
            writer.WriteString("DT2 FILE", enc);
            foreach (var item in Records)
            {
                item.WriteTo(writer);
            }
            stream.Close();
        }

        private void ReopenCache()
        {
            stream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite);
            stream.Seek(0, SeekOrigin.End);
            //reader = new CustomBinaryReader(stream);
            writer = new CustomBinaryWriter(stream);
        }

        public void Close()
        {
            Records.Clear();
            stream.Close();
        }

        public void Add(string OrigText, string TransText, string translit, Lang toLang)
        {
            ReopenCache();
            OrigText = OrigText.Trim();
            CacheRecord rec = new CacheRecord(OrigText, TransText, translit, toLang);
            Records.Add(rec);
            rec.WriteTo(writer);
            stream.Close();
        }

        public bool IndexOf(string find_origin, out string translated_text, out string translit_text, Lang to_lang)
        {
            translated_text = string.Empty;
            translit_text = string.Empty;
            find_origin = find_origin.Trim();
            crc64 = new cCRC64();
            UInt64 hash = crc64.GenerateCRC64(find_origin + Enum.Format(typeof(Lang), to_lang, "g"), enc);
            int index = Records.FindIndex(x => x.OrigTextCRC64.Equals(hash));
            if (index == -1)
                return false;
            translated_text = Records[index].TranslatedText;
            translit_text = Records[index].TranslitText;
            return true;
        }

        public class CacheRecord
        {
            public UInt64 OrigTextCRC64;
            public string OriginalText;
            public string TranslatedText;
            public string TranslitText;
            public Lang ToLang;

            private cCRC64 crc64;
            private Encoding enc = Encoding.UTF8;

            public CacheRecord(string OrigText, string TransText, string translit, Lang toLang)
            {
                crc64 = new cCRC64();
                OrigTextCRC64 = crc64.GenerateCRC64(OrigText+Enum.Format(typeof(Lang), toLang,"g") , enc);
                OriginalText = OrigText;
                TranslatedText = TransText;
                TranslitText = translit;
                ToLang = toLang;
            }
            public CacheRecord(CustomBinaryReader reader)
            {
                OrigTextCRC64 = reader.ReadUInt64();
                OriginalText = reader.ReadStringWithPreLen(enc, PreType.INT32, false);
                TranslatedText = reader.ReadStringWithPreLen(enc, PreType.INT32, false);
                TranslitText = reader.ReadStringWithPreLen(enc, PreType.INT32, false);
                ToLang = (Lang)reader.ReadUInt32();
            }
            public void WriteTo(CustomBinaryWriter writer)
            {
                writer.Write(OrigTextCRC64);
                writer.WriteStringWithPreLen(OriginalText, enc, PreType.INT32, false);
                writer.WriteStringWithPreLen(TranslatedText, enc, PreType.INT32, false);
                writer.WriteStringWithPreLen(TranslitText, enc, PreType.INT32, false);
                writer.Write((uint)ToLang);
            }
        }

        public enum Lang : uint
        {
            ru = 1,
            en = 2,
            jp = 3
        }
    }
}
