using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ED7Editor
{
    public partial class Loader : Form
    {
        public Loader()
        {
            InitializeComponent();
        }

        private void Loader_Load(object sender, EventArgs e)
        {
            while (!Directory.Exists(Properties.Settings.Default.ED7Path) ||
                Directory.GetFiles(Properties.Settings.Default.ED7Path, "ED_ZERO.exe").Length == 0)
            {
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    Environment.Exit(1);
                Properties.Settings.Default.ED7Path = fbd.SelectedPath;
                Properties.Settings.Default.Save();
            }
            Hide();
            new ItemEditor().ShowDialog();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            new ItemEditor().ShowDialog();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
            new MagicEditor().ShowDialog();
            Close();
        }
    }
}
