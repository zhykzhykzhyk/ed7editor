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
    public partial class MagicEditor : Form
    {
        public MagicEditor()
        {
            InitializeComponent();
        }
        [StructLayout(LayoutKind.Sequential)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        class MagicField
        {
            ushort type;

            public ushort Type
            {
                get { return type; }
                set { type = value; }
            }
            byte target;

            public byte Target
            {
                get { return target; }
                set { target = value; }
            }
            byte shape;

            public byte Shape
            {
                get { return shape; }
                set { shape = value; }
            }
            byte attr;

            public byte Attr
            {
                get { return attr; }
                set { attr = value; }
            }
            byte what;

            public byte What
            {
                get { return what; }
                set { what = value; }
            }
            byte aff1;

            public byte Aff1
            {
                get { return aff1; }
                set { aff1 = value; }
            }
            byte aff2;

            public byte Aff2
            {
                get { return aff2; }
                set { aff2 = value; }
            }
            ushort rng;

            public ushort Rng
            {
                get { return rng; }
                set { rng = value; }
            }
            ushort region;

            public ushort Region
            {
                get { return region; }
                set { region = value; }
            }
            ushort drive;

            public ushort Drive
            {
                get { return drive; }
                set { drive = value; }
            }
            ushort wait;

            public ushort Wait
            {
                get { return wait; }
                set { wait = value; }
            }
            ushort ep;

            public ushort Ep
            {
                get { return ep; }
                set { ep = value; }
            }
            ushort id;

            public ushort ID
            {
                get { return id; }
                set { id = value; }
            }
            short amount1;

            public short Amount1
            {
                get { return amount1; }
                set { amount1 = value; }
            }
            ushort time1;

            public ushort Time1
            {
                get { return time1; }
                set { time1 = value; }
            }
            short amount2;

            public short Amount2
            {
                get { return amount2; }
                set { amount2 = value; }
            }
            ushort time2;

            public ushort Time2
            {
                get { return time2; }
                set { time2 = value; }
            }
            ushort str1;

            public ushort Str1
            {
                get { return str1; }
                set { str1 = value; }
            }
            ushort str2;

            public ushort Str2
            {
                get { return str2; }
                set { str2 = value; }
            }
        }

        class Magic
        {
            public MagicField Field { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        string ReadString(Stream stream)
        {
            byte b;
            List<byte> a = new List<byte>();
            while ((b = (byte)stream.ReadByte()) != 0) a.Add(b);
            string s = Encoding.Default.GetString(a.ToArray());
            return s;
        }

        private void MagicEditor_Load(object sender, EventArgs e)
        {
            string ext = "._dt";
            string textPath = @"E:\Program Files\Joyoland\ed_zero\data\text\";
            using (var stream = File.OpenRead(textPath + "t_magic" + ext))
            using (var reader = new BinaryReader(stream))
            {
                ushort end = reader.ReadUInt16();
                ushort pos = end;
                List<ushort> lp = new List<ushort>();
                do
                {
                    lp.Add(pos);
                    pos = reader.ReadUInt16();
                } while (stream.Position < end);
                //lp.Add(pos);
                foreach (var p in lp)
                {
                    Magic magic = new Magic();
                    stream.Seek(p, SeekOrigin.Begin);
                    byte[] buffer = new byte[Marshal.SizeOf(typeof(MagicField))];
                    stream.Read(buffer, 0, buffer.Length);
                    var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        magic.Field = (MagicField)
                            Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), typeof(MagicField));
                    }
                    finally
                    {
                        handle.Free();
                    }
                    if (magic.Field.Type == 0) continue;
                    stream.Seek(magic.Field.Str1, SeekOrigin.Begin);
                    magic.Name = ReadString(stream);
                    stream.Seek(magic.Field.Str2, SeekOrigin.Begin);
                    magic.Description = ReadString(stream);
                    listBox1.Items.Add(magic);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = listBox1.SelectedItem;
        }
    }
}
