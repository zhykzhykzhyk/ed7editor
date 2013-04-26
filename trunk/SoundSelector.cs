using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace ED7Editor
{
    public partial class SoundSelector : Form
    {
        public SoundSelector()
        {
            InitializeComponent();
            var path = Properties.Settings.Default.ED7Path + @"\data\se";
            int pos = path.Length + @"\ed7v".Length;
            foreach (var se in Directory.GetFiles(path))
                listBox1.Items.Add(new SE { name = se.Substring(pos, 4), path = se });
        }

        SoundPlayer soundPlayer = new SoundPlayer();

        public void SetSelect(ushort id)
        {
            initValue = id;
            for (int i = 0; i < listBox1.Items.Count; i++)
                if (int.Parse((listBox1.Items[i] as SE).name) == id)
                    listBox1.SelectedIndex = i;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var se = listBox1.SelectedItem as SE;
            if (se == null) return;
            var path = se.path;
            soundPlayer.Stop();
            soundPlayer.SoundLocation = path;
            soundPlayer.Play();
            Result = ushort.Parse(se.name);
        }

        private void SoundSelector_Load(object sender, EventArgs e)
        {

        }

        public ushort Result { get; private set; }

        ushort? initValue = null;

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Result == initValue) DialogResult = DialogResult.Cancel;
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }

    class SE
    {
        public string name;
        public string path;
        public override string ToString()
        {
            return name;
        }
    }
}
