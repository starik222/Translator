using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translator;

namespace DictJoiner
{
    public class DictManager
    {
        public TranslationDictonary transDict;

        private List<NAMES> originalDictonary;
        public DataTable tableToCheck;

        private string origFilePath;

        public DictManager()
        {
            transDict = new TranslationDictonary();
            tableToCheck = new DataTable();
            tableToCheck.Columns.Add("Id", typeof(int));
            tableToCheck.Columns.Add("Replace", typeof(bool));
            tableToCheck.Columns.Add("Original", typeof(string));
            tableToCheck.Columns.Add("ExistTrans", typeof(string));
            tableToCheck.Columns.Add("AddTrans", typeof(string));
        }

        public void LoadOriginalDictonary(string fName)
        {
            originalDictonary = transDict.LoadDictonary(fName);
            origFilePath = fName;
        }

        public bool AddFromText(string textData)
        {
            tableToCheck.Rows.Clear();
            string[] lines = textData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var toAdd = transDict.LoadDictonary(lines);
            List<NAMES> newData = new List<NAMES>();
            foreach (var item in toAdd)
            {
                item.orig_name = item.orig_name.Trim();
                item.translit_name = item.translit_name.Trim();
                int index = transDict.FindIndex(originalDictonary, item.orig_name);
                if (index == -1)
                {
                    newData.Add(item);
                }
                else if (originalDictonary[index].translit_name.Trim() != item.translit_name.Trim())
                {
                    tableToCheck.Rows.Add(index, true, originalDictonary[index].orig_name.Trim(), originalDictonary[index].translit_name.Trim(), item.translit_name.Trim());
                }
            }
            Form_comparer itemsComparer = new Form_comparer();
            itemsComparer.dataGridView1.DataSource = tableToCheck;
            if (itemsComparer.ShowDialog() != DialogResult.OK)
            {
                itemsComparer.dataGridView1.DataSource = null;
                itemsComparer.Close();
                return false;
            }
            for (int i = 0; i < tableToCheck.Rows.Count; i++)
            {
                if ((bool)tableToCheck.Rows[i]["Replace"])
                {
                    originalDictonary[(int)tableToCheck.Rows[i]["Id"]].translit_name = (string)tableToCheck.Rows[i]["AddTrans"];
                }
            }
            originalDictonary.AddRange(newData);
            itemsComparer.dataGridView1.DataSource = null;
            itemsComparer.Close();
            return true;
        }

        public void SaveOriginalDictonary()
        {
            transDict.SaveDictonary(originalDictonary, origFilePath);
        }
    }
}
