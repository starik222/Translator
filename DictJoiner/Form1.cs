using System.Windows.Forms;

namespace DictJoiner
{
    public partial class Form1 : Form
    {
        private DictManager manager;
        public Form1()
        {
            InitializeComponent();
            manager = new DictManager();
        }

        private void äîáàâèòüÈçÔàéëàToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files|*.txt";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            manager.AddFromText(File.ReadAllText(openFileDialog.FileName));
        }

        private void îòêğûòüÈñõîäíûéÑëîâàğüToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files|*.txt";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            manager.LoadOriginalDictonary(openFileDialog.FileName);

        }

        private void ñîõğàíèòüToolStripMenuItem_Click(object sender, EventArgs e)
        {
            manager.SaveOriginalDictonary();
        }

        private void äîáàâèòüÈçÒåêñòàToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_addText addText = new Form_addText();
            if (addText.ShowDialog() == DialogResult.OK)
            {
                manager.AddFromText(addText.textBox1.Text);
            }
            addText.Close();

        }
    }
}