using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace ED7Editor
{
    public partial class Editor : Form
    {
        public Editor(EditorBase editor)
        {
            this.editor = editor;
            InitializeComponent();
            Text = editor.ToString();
            this.editor.Update += euh = new EventHandler(editor_Update);
        }

        EventHandler euh;

        void editor_Update(object sender, EventArgs e)
        {
            var sel = listBox1.SelectedItem as IndexedItem;
            listBox1.Items.Clear();
            foreach (var item in editor.GetList())
            {
                listBox1.Items.Add(item);
            }
            if (sel != null)
                SelectItem(sel.Index);
        }

        EditorBase editor;

        private void ItemEditor_Load(object sender, EventArgs e)
        {
            editor.Refresh();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var x = listBox1.SelectedItem as IndexedItem;
            propertyGrid1.SelectedObject = x == null ? null : x.Item;
            propertyGrid1.ExpandAllGridItems();
        }

        private void ItemEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.editor.Update -= euh;
        }

        private void Add_Click(object sender, EventArgs e)
        {
            int index;
            string result = Interaction.InputBox("请输入编号：");
            if (string.IsNullOrEmpty(result)) return;
            if (!int.TryParse(result, out index))
            {
                MessageBox.Show("请输入一个数字");
                return;
            }
            if (!editor.Add(index))
            {
                MessageBox.Show("请输入合法的编号（编号重复）");
                return;
            }
            Helper.MakeDirty();
            editor.Refresh();
            SelectItem(index);
        }

        private void SelectItem(int index)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
                if ((listBox1.Items[i] as IndexedItem).Index == index)
                {
                    listBox1.SelectedIndex = i;
                }
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(String.Format("确定要删除“{0}”吗？", listBox1.SelectedItem),
                "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                if (editor.Remove((listBox1.SelectedItem as IndexedItem).Index))
                {
                    Helper.MakeDirty();
                    editor.Refresh();
                }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            editor.Refresh();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            Remove.Visible = listBox1.SelectedItems.Count == 1;
        }

        object clipboard;

        private void Copy_Click(object sender, EventArgs e)
        {
            clipboard = propertyGrid1.SelectedObject;
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            if (editor.CopyTo(clipboard, propertyGrid1.SelectedObject))
                Helper.MakeDirty();
            propertyGrid1.Refresh();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            Paste.Enabled = clipboard != null;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Helper.MakeDirty();
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

    }
}
