using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Translator.IO;
using System.Configuration;


namespace Translator
{
    public partial class Form_EditText : Form
    {
        public Form_EditText()
        {
            InitializeComponent();
        }
        public GoogleTrans gt;
        public TextTool tools;
        public bool Loaded = false;
        public string tmpData
        {
            get
            {
                return Application.StartupPath + "\\modData_"+dataGridView1.RowCount +"_"+dataGridView1.ColumnCount+".tmp";   
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.F && e.Control)
            {
                if (!tableLayoutPanel2.Visible)
                    FindTextDialog();
                else
                    tableLayoutPanel2.Visible = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.R && e.Control)
            {
                if (!tableLayoutPanel1.Visible)
                    ReplaceTextDialog();
                else
                    tableLayoutPanel1.Visible = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                button3_Click(null, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.D && e.Control)
            {
                button1_Click(null, EventArgs.Empty);
            }
        }

        private void FindTextDialog()
        {
            tableLayoutPanel2.Visible = true;
            textBox6.Focus();
        }

        private void ReplaceTextDialog()
        {
            tableLayoutPanel1.Visible = true;
            textBox4.Focus();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            int max_len = -1;
            try
            {
                max_len = Convert.ToInt32(toolStripTextBox1.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            int col_index = dataGridView1.SelectedCells[0].ColumnIndex;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1[col_index, i].Style.BackColor = Color.White;
                if (string.IsNullOrWhiteSpace((string)dataGridView1[col_index, i].Value))
                    continue;
                if (Encoding.GetEncoding(932).GetByteCount((string)dataGridView1[col_index, i].Value) > max_len - 1)
                {
                    dataGridView1[col_index, i].Style.BackColor = Color.Yellow;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0 && dataGridView1.Columns.Count > 0)
                {
                    if (dataGridView1.SelectedCells.Count == 0)
                        return;
                    int x_index = dataGridView1.SelectedCells[0].RowIndex;
                    int y_index = dataGridView1.SelectedCells[0].ColumnIndex;
                    if (x_index == -1 && y_index <= 0)
                        return;
                    string text = dataGridView1[y_index, x_index].Value.ToString();
                    GoogleTrans.TranslateInfo ti = gt.Translate(text, "ja", "ru", true);
                    textBox1.Text = ti.output_text;
                    textBox2.Text = ti.translit;
                    //textBox3.Text = mt.Translate(text, "ja", "ru");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetVisiblePanel(bool isVisible)
        {
            toolStripButton6.Visible = isVisible;
            toolStripButton7.Visible = isVisible;
            toolStripButton9.Visible = isVisible;
            toolStripComboBox1.Visible = isVisible;
            //button1.Visible = isVisible;
            button2.Visible = isVisible;
        }

        private void Form_EditText_Load(object sender, EventArgs e)
        {
            Type t = null;
            if (dataGridView1.DataSource != null)
                t = dataGridView1.DataSource.GetType();
            gt = new GoogleTrans(Application.StartupPath);
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            if (t != null)
            {
                if (LoadTmpFile(t.FullName))
                {
                    MessageBox.Show("Загружены несохраненные данные!");
                }
                WriteHeader(t.FullName);
            }
            else
            {
                //if (LoadTmpFile("NULL"))
                //{
                //    MessageBox.Show("Загружены несохраненные данные!");
                //}
                //WriteHeader(t.FullName);
            }
            Loaded = true;
        }

        private bool LoadTmpFile(string name)
        {
            if (File.Exists(tmpData))
            {
                CustomBinaryReader reader = new CustomBinaryReader(new FileStream(tmpData, FileMode.Open));
                string OldName = reader.ReadString0(Encoding.UTF8);
                if (!name.Equals(OldName))
                {
                    reader.Close();
                    File.Delete(tmpData);
                    return false;
                }
                else
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        int rowIndex = reader.ReadInt32();
                        int columnIndex = reader.ReadInt32();
                        string data = reader.ReadString0(Encoding.UTF8);
                        dataGridView1[columnIndex, rowIndex].Value = data;
                        dataGridView1[columnIndex, rowIndex].Style.BackColor = Color.Yellow;
                    }
                    reader.Close();
                    File.Delete(tmpData);
                    return true;
                }
            }
            else
                return false;
        }

        private void WriteModData(int RowIndex, int ColumnIndex, string data)
        {
            CustomBinaryWriter writer = new CustomBinaryWriter(new FileStream(tmpData, FileMode.OpenOrCreate));
            writer.BaseStream.Seek(0, SeekOrigin.End);
            writer.Write(RowIndex);
            writer.Write(ColumnIndex);
            writer.WriteString0(data, Encoding.UTF8);
            writer.Close();
        }

        private void WriteHeader(string data)
        {
            CustomBinaryWriter writer = new CustomBinaryWriter(new FileStream(tmpData, FileMode.Create));
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            writer.WriteString0(data, Encoding.UTF8);
            writer.Close();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            DeleteTmpFile();
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
                return;

                for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                    {
                        if (!checkBox3.Checked)
                        {
                            if (toolStripComboBox1.Text == "JaRu")
                                dataGridView1.SelectedCells[i].Value = tools.TranslateText((string)dataGridView1.SelectedCells[i].Value, TextTool.TransMethod.JaRu);
                            else if (toolStripComboBox1.Text == "JaEnRu")
                                dataGridView1.SelectedCells[i].Value = tools.TranslateText((string)dataGridView1.SelectedCells[i].Value, TextTool.TransMethod.JaEnRu);
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace((string)comboBox1.SelectedItem))
                                return;
                            if (toolStripComboBox1.Text == "JaRu")
                                dataGridView1.Rows[dataGridView1.SelectedCells[i].RowIndex].Cells[(string)comboBox1.SelectedItem].Value = tools.TranslateText((string)dataGridView1.SelectedCells[i].Value, TextTool.TransMethod.JaRu);
                            else if (toolStripComboBox1.Text == "JaEnRu")
                                dataGridView1.Rows[dataGridView1.SelectedCells[i].RowIndex].Cells[(string)comboBox1.SelectedItem].Value = tools.TranslateText((string)dataGridView1.SelectedCells[i].Value, TextTool.TransMethod.JaEnRu);
                        }
                    }
                }

        }
        public Form_replace f_rep;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (!tableLayoutPanel1.Visible)
                ReplaceTextDialog();
            else
                tableLayoutPanel1.Visible = false;
            //if (f_rep == null)
            //{
            //    f_rep = new Form_replace();
            //    f_rep.Owner = this;
            //}
            //if (f_rep.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //    {
            //        for (int j = 1; j < dataGridView1.Columns.Count; j++)
            //        {
            //            if (string.IsNullOrWhiteSpace((string)dataGridView1[j, i].Value))
            //                continue;
            //            dataGridView1[j, i].Value = ((string)dataGridView1[j, i].Value).Replace(f_rep.textBox1.Text, f_rep.textBox2.Text);
            //        }
            //    }
            //}
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(toolStripTextBox2.Text) || string.IsNullOrEmpty(toolStripTextBox3.Text))
                return;
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                    dataGridView1.SelectedCells[i].Value = tools.PrepareText((string)dataGridView1.SelectedCells[i].Value, Convert.ToInt32(toolStripTextBox3.Text), toolStripTextBox2.Text);
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(toolStripTextBox2.Text))
                return;
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                    dataGridView1.SelectedCells[i].Value = ((string)dataGridView1.SelectedCells[i].Value).Replace(toolStripTextBox2.Text, "");
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            string res = string.Empty;
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                {
                    if (toolStripComboBox1.Text == "JaRu")
                        res += (string)dataGridView1.SelectedCells[i].Value + ConfigurationManager.AppSettings["TextSplitter"] + tools.TranslateText((string)dataGridView1.SelectedCells[i].Value, TextTool.TransMethod.JaRu) + "\r\n";
                    else if (toolStripComboBox1.Text == "JaEnRu")
                        res += (string)dataGridView1.SelectedCells[i].Value + ConfigurationManager.AppSettings["TextSplitter"] + tools.TranslateText((string)dataGridView1.SelectedCells[i].Value, TextTool.TransMethod.JaEnRu) + "\r\n";
                }
            }
            Clipboard.SetText(res);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            string res = string.Empty;
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                {
                    res += (string)dataGridView1.SelectedCells[i].Value + ConfigurationManager.AppSettings["TextSplitter"] + tools.TranslateName((string)dataGridView1.SelectedCells[i].Value) + "\r\n";
                }
            }
            Clipboard.SetText(res);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            string res = string.Empty;
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                {
                    res += (string)dataGridView1.SelectedCells[i].Value + ConfigurationManager.AppSettings["TextSplitter"] + tools.CompareAndReplace((string)dataGridView1.SelectedCells[i].Value, tools.RepText) + "\r\n";
                }
            }
            Clipboard.SetText(res);
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace((string)dataGridView1.SelectedCells[i].Value))
                {
                    if (!checkBox3.Checked)
                        dataGridView1.SelectedCells[i].Value = tools.CompareAndReplace((string)dataGridView1.SelectedCells[i].Value, tools.RepText);
                    else
                    {
                        string repVal = tools.CompareAndReplace((string)dataGridView1.SelectedCells[i].Value, tools.RepText);
                        if ((string)dataGridView1.SelectedCells[i].Value != repVal)
                            dataGridView1.Rows[dataGridView1.SelectedCells[i].RowIndex].Cells[(string)comboBox1.SelectedItem].Value = tools.CompareAndReplace((string)dataGridView1.SelectedCells[i].Value, tools.RepText);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int startRow = 0, startColumn = 0;
            if (dataGridView1.CurrentCell.ColumnIndex != -1 && dataGridView1.CurrentCell.RowIndex != -1)
            {
                startRow = dataGridView1.CurrentCell.RowIndex;
                startColumn = dataGridView1.CurrentCell.ColumnIndex;
                if (startRow > 0)
                {
                    if (startColumn == 0)
                        startColumn++;
                    else
                    {
                        startRow++;
                        startColumn--;
                    }
                }
            }
            for (int i = startRow; i < dataGridView1.RowCount - 1; i++)
            {
                for (int j = startColumn; j < dataGridView1.ColumnCount; j++)
                {
                    if (dataGridView1[j, i].Value == null)
                        continue;
                    if (dataGridView1[j, i].Value.GetType() == typeof(string))
                    {
                        if (!checkBox2.Checked)
                        {
                            if (((string)dataGridView1[j, i].Value).ToLower().IndexOf(textBox6.Text.ToLower()) != -1)
                            {
                                if (dataGridView1.CurrentCell != dataGridView1[j, i])
                                {
                                    dataGridView1.ClearSelection();
                                    dataGridView1.CurrentCell = dataGridView1[j, i];
                                }
                                return;
                            }
                        }
                        else
                        {
                            if (((string)dataGridView1[j, i].Value).Equals(textBox6.Text))
                            {
                                if (dataGridView1.CurrentCell != dataGridView1[j, i])
                                {
                                    dataGridView1.ClearSelection();
                                    dataGridView1.CurrentCell = dataGridView1[j, i];
                                }
                                return;
                            }
                        }
                    }
                    else if (dataGridView1[j, i].Value.GetType() == typeof(int))
                    {
                        int testOut = -1;
                        if (int.TryParse(textBox6.Text, out testOut))
                        {
                            if (((int)dataGridView1[j, i].Value).Equals(testOut))
                            {
                                if (dataGridView1.CurrentCell != dataGridView1[j, i])
                                {
                                    dataGridView1.ClearSelection();
                                    dataGridView1.CurrentCell = dataGridView1[j, i];
                                }
                                return;
                            }
                        }
                    }
                    
                }
                startColumn = 0;
            }

            if (startRow > 0)
            {
                for (int i = 0; i < startRow; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        if (dataGridView1[j, i].Value == null)
                            continue;
                        if (dataGridView1[j, i].Value.GetType() == typeof(string))
                        {
                            if (!checkBox2.Checked)
                            {
                                if (((string)dataGridView1[j, i].Value).ToLower().IndexOf(textBox6.Text.ToLower()) != -1)
                                {
                                    if (dataGridView1.CurrentCell != dataGridView1[j, i])
                                    {
                                        dataGridView1.ClearSelection();
                                        dataGridView1.CurrentCell = dataGridView1[j, i];
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                if (((string)dataGridView1[j, i].Value).Equals(textBox6.Text))
                                {
                                    if (dataGridView1.CurrentCell != dataGridView1[j, i])
                                    {
                                        dataGridView1.ClearSelection();
                                        dataGridView1.CurrentCell = dataGridView1[j, i];
                                    }
                                    return;
                                }
                            }
                        }
                        else if (dataGridView1[j, i].Value.GetType() == typeof(int))
                        {
                            int testOut = -1;
                            if (int.TryParse(textBox6.Text, out testOut))
                            {
                                if (((int)dataGridView1[j, i].Value).Equals(testOut))
                                {
                                    if (dataGridView1.CurrentCell != dataGridView1[j, i])
                                    {
                                        dataGridView1.ClearSelection();
                                        dataGridView1.CurrentCell = dataGridView1[j, i];
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    startColumn = 0;
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string ftext = textBox4.Text;
            string rtext = textBox5.Text;
            int count = 0;
            if (checkBox1.Checked)
            {
                if (dataGridView1.SelectedCells.Count == 0)
                    return;
                int ColIndex = dataGridView1.SelectedCells[0].ColumnIndex;
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {
                    if (dataGridView1[ColIndex, j].Value.GetType() != typeof(string))
                        continue;
                    if (((string)dataGridView1[ColIndex, j].Value).IndexOf(ftext) != -1)
                    {
                        dataGridView1[ColIndex, j].Value = ((string)dataGridView1[ColIndex, j].Value).Replace(ftext, rtext);
                        count++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        if (dataGridView1[i, j].Value.GetType() != typeof(string))
                            continue;
                        if (((string)dataGridView1[i, j].Value).IndexOf(ftext) != -1)
                        {
                            dataGridView1[i, j].Value = ((string)dataGridView1[i, j].Value).Replace(ftext, rtext);
                            count++;
                        }
                    }
                }
            }
            MessageBox.Show("Произведено " + count.ToString() + " замен.");
            tableLayoutPanel1.Visible = false;
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                textBox5.Focus();
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                button11.Focus();
            }
        }

        private void редактироватьКлассToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
                return;
            Form_EditText f_edit = new Form_EditText();
            f_edit.dataGridView1.DataSource = dataGridView1.SelectedCells[0].Value;
            f_edit.ShowDialog();
        }

        private void dataGridView1_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            comboBox1.Items.Add(e.Column.Name);
        }

        private void dataGridView1_ColumnRemoved(object sender, DataGridViewColumnEventArgs e)
        {
            comboBox1.Items.Remove(e.Column.Name);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (Loaded)
                if (!string.IsNullOrWhiteSpace((string)dataGridView1[e.ColumnIndex, e.RowIndex].Value))
                    if (dataGridView1.DataSource != null)
                        WriteModData(e.RowIndex, e.ColumnIndex, (string)dataGridView1[e.ColumnIndex, e.RowIndex].Value);
        }

        private void Form_EditText_FormClosing(object sender, FormClosingEventArgs e)
        {
            DeleteTmpFile();
        }

        private void DeleteTmpFile()
        {
            Loaded = false;
            if (File.Exists(tmpData))
                File.Delete(tmpData);
        }
    }
}
