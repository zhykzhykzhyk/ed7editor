using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace ED7Editor
{
    public partial class Editor : Form
    {
        public Editor(EditorBase editor)
        {
            this.editor = editor;
            InitializeComponent();
        }

        EditorBase editor;

        private void ItemEditor_Load(object sender, EventArgs e)
        {
            foreach (var item in editor.GetList())
            {
                listBox1.Items.Add(item);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = listBox1.SelectedItem;
            propertyGrid1.ExpandAllGridItems();
        }

        private void ItemEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void Add_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(new Item { Name = " ", Description = " ", Field = new ItemField() });
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(String.Format("确定要删除“{0}”吗？", ((Item)listBox1.SelectedItem).Name),
                "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            List<Item> list = new List<Item>();
            foreach (var item in listBox1.Items)
            {
                Item i = (Item)item;
                if (i.Field.ID < 100 || i.Field.ID >= 200) i.Quert = null;
                else if (i.Quert == null) i.Quert = new ItemQuart();
                list.Add((Item)item);
            }
            list.Sort();
            listBox1.Items.Clear();
            listBox1.Items.AddRange(list.ToArray());
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            Remove.Visible = listBox1.SelectedItems.Count == 1;
        }


    }
}
