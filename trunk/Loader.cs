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
#if !DEBUG
            if (!Helper.CheckPath())
#endif
                do
                    if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        Environment.Exit(1);
                while (!Helper.SetPath(fbd.SelectedPath));


            foreach (var type in Helper.Editors)
            {
                comboBox1.Items.Add(type);
            }
            comboBox1.SelectedIndex = 0;
            Helper.Load(WarnDirty);
#if AONOKISEKI
            using (var writer = new StreamWriter(@"D:\item.txt"))
            {
                foreach (var f in Helper.GetEditorByType(typeof(ItemEditor)).GetList())
                {
                    var i = f.Item as Item;
                    writer.WriteLine("{0}\t{1}\t{2}", i.Field.ID, i.Name, i.Description);
                }
            }
#endif
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
