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
#if AONOKISEKI
            fbd.Description = fbd.Description
                .Replace("零之轨迹", "碧之轨迹")
                .Replace("ed_zero", "ED_AO");
#endif
        }

        bool loaded;

        private void Loader_Load(object sender, EventArgs e)
        {
#if !DEBUG
            if (!Helper.CheckPath())
#endif
            do
            {
                fbd.SelectedPath = Properties.Settings.Default.ED7Path;
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    Environment.Exit(1);
            } while (!Helper.SetPath(fbd.SelectedPath));


            foreach (var type in Helper.Components)
            {
                if (type.Browsable)
                    comboBox1.Items.Add(type);
            }
            comboBox1.SelectedIndex = 0;
            Helper.Load(WarnDirty);
#if __AONOKISEKI
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
            loaded = true;
            this.Opacity = 1;
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
            ((Component)comboBox1.SelectedItem).Launch();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Helper.Save();
        }

        private void Loader_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = Helper.CheckDirty(WarnDirty);
        }

        private void Loader_Activated(object sender, EventArgs e)
        {
            if (this.loaded)
                this.Opacity = 1;
        }

        private void Loader_Deactivate(object sender, EventArgs e)
        {
            if (this.loaded)
                this.Opacity = 0.8;
        }
    }
}
