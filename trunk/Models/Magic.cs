using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Reflection;

namespace ED7Editor
{
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MagicField
    {

        public MagicField Duplicate()
        {
            return (MagicField)this.MemberwiseClone();
        }
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
        ushort unknown;

        public ushort Unknown
        {
            get { return unknown; }
            set { unknown = value; }
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
        [Browsable(false)]
        public ushort Str1
        {
            get { return str1; }
            set { str1 = value; }
        }
        ushort str2;
        [Browsable(false)]
        public ushort Str2
        {
            get { return str2; }
            set { str2 = value; }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Editor(typeof(MagicQuartzEditor), typeof(UITypeEditor))] 
    public class MagicQuartz
    {
        public MagicQuartz()
        {
            Quartz = new ushort[7];
        }
        public ushort ID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        private ushort[] quartz;

        public ushort[] Quartz
        {
            get { return quartz; }
            internal set { quartz = value; }
        }

    }

    class MagicQuartzEditor:UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return (context.Instance as Magic).ID <= 150 ? 
                UITypeEditorEditStyle.Modal : UITypeEditorEditStyle.None;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,
            object value)
        {
            if (value == null)
                return new MagicQuartz();
            else if (MessageBox.Show("是否删除？", "提醒", MessageBoxButtons.YesNo) == DialogResult.Yes)
                return null;
            else return value;
        }
    }

    public class Magic
    {
        public MagicField Field { get; set; }
        public MagicQuartz Quartz { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [ReadOnly(true)]
        public int ID { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    [DisplayName("Magic Editor")]
    public class MagicEditor : EditorBase<Magic>
    {
        public override bool Add(int id)
        {
            if (id < 0 || id >= magics.Length) return false;
            lock (this)
            {
                if (magics[id] != null) return false;
                magics[id] = new Magic { Field = new MagicField(), ID = id, Description = " ", Name = " " };
            }
            return true;
        }
        public override bool CopyTo(object src, object dest)
        {
            Magic si, di;
            try
            {
                si = (Magic)src;
                di = (Magic)dest;
            }
            catch { return false; }
            di.Name = si.Name;
            di.Description = si.Description;
            di.Field = si.Field.Duplicate();
            return true;
        }
        public override object GetById(int id)
        {
            lock (this)
                if (magics == null) Load();
            return magics[id];
        }
        public override IEnumerable<IndexedItem> GetList()
        {
            lock(this)
                if (magics == null) Load();
            List<IndexedItem> list = new List<IndexedItem>();
            for (int i = 0; i < magics.Length; i++)
                if (magics[i] != null)
                    list.Add(new IndexedItem { Index = i, Item = magics[i] });
            return list;
        }
        public override IEnumerable<SelectorItem> GetSelector()
        {
            throw new NotImplementedException();
        }
        Magic[] magics;
        public override void Load()
        {
            Magic[] magics = new Magic[350];
            MagicQuartz[] quartz = new MagicQuartz[151];
            ushort[] lp = new ushort[350];
            using (var stream = File.OpenRead(GetFile("t_magqrt._dt")))
            using (var reader = new BinaryReader(stream))
            {
                for (int i = 0; i < quartz.Length; i++)
                    lp[i] = reader.ReadUInt16();
                for (int i = 0; i < quartz.Length; i++)
                {
                    stream.Position = lp[i];
                    MagicQuartz quart = ReadStrcuture<MagicQuartz>(stream);
                    if (quart.ID != 999)
                        quartz[quart.ID] = quart;
                }
            }
            using (var stream = File.OpenRead(GetFile("t_magic._dt")))
            using (var reader = new BinaryReader(stream))
            {
                for (int i = 0; i < lp.Length; i++)
                    lp[i] = reader.ReadUInt16();
                for (int i = 0; i < lp.Length; i++)
                {
                    if (i == lp.Length - 1 || lp[i + 1] - lp[i] <= 4) continue;
                    Magic magic = new Magic();
                    stream.Seek(lp[i], SeekOrigin.Begin);
                    magic.Field = ReadStrcuture<MagicField>(stream);
                    stream.Seek(magic.Field.Str1, SeekOrigin.Begin);
                    magic.Name = ReadString(stream);
                    stream.Seek(magic.Field.Str2, SeekOrigin.Begin);
                    magic.Description = ReadString(stream);
                    if (i < quartz.Length) magic.Quartz = quartz[i];
                    magic.ID = i;
                    magics[i] = magic;
                }
            }
            this.magics = magics;
        }
        public override bool Remove(int item)
        {
            magics[item] = null;
            return true;
        }
        public override void Save()
        {
            MagicQuartz[] quartz = new MagicQuartz[151];
            using (var stream = File.OpenWrite(GetFile("t_magic._dt")))
            using (var writer = new BinaryWriter(stream))
            {
                long p = magics.Length * 2;
                for (int i = 0; i < magics.Length; i++)
                {
                    writer.Write((ushort)p);
                    long pos = stream.Position;
                    stream.Position = p;
                    if (magics[i] != null)
                    {
                        if (i < quartz.Length) quartz[i] = magics[i].Quartz;
                        WriteStruct(stream, magics[i].Field);
                        p = stream.Position;
                    }
                    stream.Position = pos;
                }
            }
            using (var stream = File.OpenWrite(GetFile("t_magqrt._dt")))
            using (var writer = new BinaryWriter(stream))
            {
                long p = quartz.Length * 2;
                for (int i = 0; i < quartz.Length; i++)
                {
                    writer.Write((ushort)p);
                    long pos = stream.Position;
                    stream.Position = p;
                    if (quartz[i] != null)
                    {
                        quartz[i].ID = (ushort)i;
                        WriteStruct(stream, quartz[i]);
                        p = stream.Position;
                    }
                    stream.Position = pos;
                }
            }
        }
    }
}
