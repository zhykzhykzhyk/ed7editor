using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

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
            foreach (var type in Helper.Editors)
            {
                comboBox1.Items.Add(type);
            }
            comboBox1.SelectedIndex = 0;
            //Hide();
            //new Editor().ShowDialog();
            //Close();
        }

        void WarnDirty(object s, CancelEventArgs e)
        {
            var result = MessageBox.Show("已更改，是否保存？", "警告", MessageBoxButtons.YesNoCancel);
            e.Cancel = result == DialogResult.Cancel;
            if (result == DialogResult.Yes) Helper.Save();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            Helper.Load(WarnDirty);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Editor((EditorBase)comboBox1.SelectedItem).Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Helper.Save();
        }

        private void Loader_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = Helper.CheckDirty(WarnDirty);
        }
    }
}
