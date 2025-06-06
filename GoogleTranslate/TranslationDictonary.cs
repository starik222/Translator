﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading;

namespace Translator
{
    public class TranslationDictonary
    {
        private string splitChar = "|";
        private TranslationConfig _config;

        public TranslationDictonary()
        {
            _config = new TranslationConfig();
            splitChar = _config.GetAppSetting("TextSplitter");

        }

        public NAMES NewValue(string orig, string trans)
        {
            NAMES val = new NAMES();
            val.orig_name = orig;
            val.translit_name = trans;
            return val;
        }

        public List<NAMES> LoadDictonary(string dictPath, bool checkUnique = true)
        {
            List<NAMES> dict = new List<NAMES>();
            if (!File.Exists(dictPath))
            {
                File.Create(dictPath).Close();
            }
            string[] lines = File.ReadAllLines(dictPath);
            if (checkUnique)
            {
                List<string> nLines = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!nLines.Contains(lines[i]))
                        nLines.Add(lines[i]);
                }
                for (int i = 0; i < nLines.Count; i++)
                {
                    dict.Add(new NAMES(nLines[i], splitChar));
                }
                dict = TestDictonary(dict, dictPath);
                dict.Sort((y, x) => x.orig_name.Length.CompareTo(y.orig_name.Length));
                return dict;
            }
            else
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    dict.Add(new NAMES(lines[i], splitChar));
                }
                return dict;
            }
        }

        public List<NAMES> LoadDictonary(string[] lines)
        {
            List<NAMES> dict = new List<NAMES>();
            for (int i = 0; i < lines.Length; i++)
            {
                dict.Add(new NAMES(lines[i], splitChar));
            }

            return dict.Distinct(new NamesComparer()).ToList();
        }

        public NAMES FindItem(List<NAMES> dict, string original)
        {
            int index = dict.FindIndex(a => a.orig_name.Equals(original));
            if (index != -1)
                return dict[index];
            return null;
        }

        public int FindIndex(List<NAMES> dict, string original)
        {
            return dict.FindIndex(a => a.orig_name.Equals(original));
        }

        public string GetTranslation(List<NAMES> dict, string original)
        {
            var item = FindItem(dict, original);
            return item != null ? item.translit_name : null;
        }

        public void SetTranslation(ref List<NAMES> dict, string original, string trans)
        {
            if (string.IsNullOrWhiteSpace(original))
                return;
            else if (!string.IsNullOrWhiteSpace(original) && string.IsNullOrWhiteSpace(trans))
            {
                int index = FindIndex(dict, original);
                if (index != -1)
                    dict.RemoveAt(index);
            }
            else
            {
                int index = FindIndex(dict, original);
                if (index != -1)
                    dict[index].translit_name = trans;
                else
                    dict.Add(new NAMES() { orig_name = original, translit_name = trans });
            }
        }

        public void SaveDictonary(List<NAMES> dict, string dictPath)
        {
            StreamWriter sw = new StreamWriter(dictPath, false, Encoding.UTF8);
            for (int i = 0; i < dict.Count; i++)
            {
                sw.WriteLine(dict[i].orig_name + splitChar + dict[i].translit_name);
            }
            sw.Close();
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

        public string GetSplitterChar()
        {
            return splitChar;
        }
    }

    class NamesComparer : IEqualityComparer<NAMES>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(NAMES x, NAMES y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.orig_name == y.orig_name;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(NAMES product)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(product, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashProductName = product.orig_name == null ? 0 : product.orig_name.GetHashCode();

            //Get hash code for the Code field.
            //int hashProductCode = product.Code.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductName;
        }
    }
}
