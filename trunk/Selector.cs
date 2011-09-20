using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ED7Editor
{
    public partial class Selector : Form
    {
        public Selector(EditorBase editor)
        {
            this.editor = editor;
            InitializeComponent();
            foreach (var i in editor.GetSelector())
                listBox1.Items.Add(i);
        }

        EditorBase editor;

        private void Selector_Load(object sender, EventArgs e)
        {
        }

        public int Result { get; private set; }

        int? initValue = null;

        public void SetSelect(int id)
        {
            initValue = id;
            for (int i = 0; i < listBox1.Items.Count; i++)
                if ((listBox1.Items[i] as SelectorItem).ID == id)
                    listBox1.SelectedIndex = i;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Result == initValue) DialogResult = DialogResult.Cancel;
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = listBox1.SelectedItem as SelectorItem;
            textBox1.Text = item.Description;
            Result = item.ID;
        }
    }
}
