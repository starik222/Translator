using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Translator
{
    public partial class Form_select_var : Form
    {
        public Form_select_var()
        {
            InitializeComponent();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.S && e.Control)
            {
                if (dataGridView1.SelectedRows.Count == 0)
                    return;
                SelectIndex = dataGridView1.SelectedRows[0].Index;
                DialogResult = System.Windows.Forms.DialogResult.OK;
                e.Handled = true;
            }
        }
        public int SelectIndex;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;
            SelectIndex = dataGridView1.SelectedRows[0].Index;
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void Form_select_var_Load(object sender, EventArgs e)
        {
            SelectIndex = -1;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;
            SelectIndex = dataGridView1.SelectedRows[0].Index;
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
