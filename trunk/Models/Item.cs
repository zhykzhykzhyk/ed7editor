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
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    class ItemField
    {
        private ushort id;
        [ReadOnly(true)]
        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        private byte type;

        public ItemField Duplicate()
        {
            return (ItemField)this.MemberwiseClone();
        }

        public byte Type
        {
            get { return type; }
            set { type = value; }
        }
        private byte limit;

        public byte Limit
        {
            get { return limit; }
            set { limit = value; }
        }
        private byte unload;

        public byte Unload
        {
            get { return unload; }
            set { unload = value; }
        }
        private byte pic;

        public byte Pic
        {
            get { return pic; }
            set { pic = value; }
        }
        private byte work;

        public byte Work
        {
            get { return work; }
            set { work = value; }
        }
        private byte attr;

        public byte Attr
        {
            get { return attr; }
            set { attr = value; }
        }
        private byte sub;

        public byte Sub
        {
            get { return sub; }
            set { sub = value; }
        }
        private byte unknown;

        public byte Unknown
        {
            get { return unknown; }
            set { unknown = value; }
        }
        private sbyte rng;

        public sbyte Rng
        {
            get { return rng; }
            set { rng = value; }
        }
        private byte region;

        public byte Region
        {
            get { return region; }
            set { region = value; }
        }
        private short str;

        public short Str
        {
            get { return str; }
            set { str = value; }
        }
        private short def;

        public short Def
        {
            get { return def; }
            set { def = value; }
        }
        private short ats;

        public short Ats
        {
            get { return ats; }
            set { ats = value; }
        }
        private short adf;

        public short Adf
        {
            get { return adf; }
            set { adf = value; }
        }
        private sbyte dex;

        public sbyte Dex
        {
            get { return dex; }
            set { dex = value; }
        }
        private sbyte agl;

        public sbyte Agl
        {
            get { return agl; }
            set { agl = value; }
        }
        private sbyte mov;

        public sbyte Mov
        {
            get { return mov; }
            set { mov = value; }
        }
        private sbyte spd;

        public sbyte Spd
        {
            get { return spd; }
            set { spd = value; }
        }
        private uint price;

        public uint Price
        {
            get { return price; }
            set { price = value; }
        }
    }

    [DisplayName("Item Editor")]
    class ItemEditor : EditorBase<Item>
    {
        private void WriteItemStrings(BinaryWriter writer, Item item)
        {
            writer.Write(item.Field.ID);
            writer.Write((ushort)0);
            byte[] s1 = Helper.Encoding.GetBytes(item.Name);
            byte[] s2 = Helper.Encoding.GetBytes(item.Description);
            writer.Write((ushort)(writer.BaseStream.Position + 4));
            writer.Write((ushort)(writer.BaseStream.Position + 3 + s1.Length));
            writer.Write(s1);
            writer.Write((byte)0);
            writer.Write(s2);
            writer.Write((byte)0);
        }
        public readonly static char[] QuartzName = "地水火风时空幻".ToCharArray();
        public override IEnumerable<SelectorItem> GetSelector()
        {
            SortedDictionary<int, SelectorItem> items = new SortedDictionary<int, SelectorItem>();
            foreach (var item in this.items)
            {
                items.Add(item.Key, new SelectorItem
                {
                    ID = item.Key,
                    Name = item.Value.Name,
                    Description = item.Value.Description.Replace(@"\n", "\r\n")
                });
            }
            for (int i = 0; i < QuartzName.Length; i++)
            {
                items.Add(i + 990, new SelectorItem
                {
                    ID = i + 990,
                    Name = QuartzName[i] + "之耀晶片",
                    Description = "改造用"
                });
            }
            return items.Values;
        }
        public override object GetById(int id)
        {
            lock (this) if (items == null) Load();
            var x = id - 990;
            if (x >= 0 && x < QuartzName.Length) return new Item
            {
                Name = QuartzName[x] + "之耀晶片",
                Description = "改造用"
            };
            return items.ContainsKey((ushort)id) ? items[(ushort)id] : null;
        }
        public override void Load()
        {
            SortedDictionary<ushort, Item> Item = new SortedDictionary<ushort, Item>();
            ItemQuartz[] quartz = new ItemQuartz[200];
            using (var stream = File.OpenRead(EditorBase.GetFile("t_quartz._dt")))
            {
                int i = 0;
                while (stream.Position < stream.Length)
                {
                    quartz[i++] = ReadStrcuture<ItemQuartz>(stream);
                }
            }
            using (var stream = File.OpenRead(EditorBase.GetFile("t_item._dt")))
            using (var reader = new BinaryReader(stream))
            {
                ushort header = reader.ReadUInt16();
                stream.Seek(header, SeekOrigin.Begin); //Skip Index Table;
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
                    Item item = new Item();
                    stream.Seek(p, SeekOrigin.Begin);
                    item.Field = ReadStrcuture<ItemField>(stream);
                    if (item.Field.ID >= 100 && item.Field.ID < 300)
                    {
                        item.Quartz = quartz[item.Field.ID - 100];
                    }
                    Item[item.Field.ID] = item;
                }
            }
            using (var stream = File.OpenRead(EditorBase.GetFile("t_item2._dt")))
            using (var reader = new BinaryReader(stream))
            {
                ushort header = reader.ReadUInt16();
                stream.Seek(header, SeekOrigin.Begin); //Skip Index Table;
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
                    Item item = new Item();
                    stream.Seek(p, SeekOrigin.Begin);
                    item.Field = ReadStrcuture<ItemField>(stream);
                    Item[item.Field.ID] = item;
                }
            }
            using (var stream = File.OpenRead(EditorBase.GetFile("t_ittxt._dt")))
            using (var reader = new BinaryReader(stream))
            {
                ushort header = reader.ReadUInt16();
                stream.Seek(header, SeekOrigin.Begin); //Skip Index Table;
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
                    stream.Seek(p, SeekOrigin.Begin);
                    ushort id = reader.ReadUInt16();
                    reader.ReadUInt16(); //skip 0s
                    ushort p1 = reader.ReadUInt16();
                    ushort p2 = reader.ReadUInt16();
                    stream.Seek(p1, SeekOrigin.Begin);
                    Item[id].Name = EditorBase.ReadString(stream);
                    stream.Seek(p2, SeekOrigin.Begin);
                    Item[id].Description = EditorBase.ReadString(stream);
                }
            }
            using (var stream = File.OpenRead(EditorBase.GetFile("t_ittxt2._dt")))
            using (var reader = new BinaryReader(stream))
            {
                ushort header = reader.ReadUInt16();
                stream.Seek(header, SeekOrigin.Begin); //Skip Index Table;
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
                    stream.Seek(p, SeekOrigin.Begin);
                    ushort id = reader.ReadUInt16();
                    reader.ReadUInt16(); //skip 0s
                    ushort p1 = reader.ReadUInt16();
                    ushort p2 = reader.ReadUInt16();
                    stream.Seek(p1, SeekOrigin.Begin);
                    Item[id].Name = EditorBase.ReadString(stream);
                    stream.Seek(p2, SeekOrigin.Begin);
                    Item[id].Description = EditorBase.ReadString(stream);
                }
            }
            items = Item;
        }
        int GetLength(SortedList<ushort, Item> list)
        {
            if (list.Count == 0) return 0;
            else return list.Values[list.Count - 1].Field.ID % 100 * 2 + 2;
        }
        public override void Save()
        {
            if (items == null) return;
            ItemQuartz[] quartz = new ItemQuartz[200];
            for (int i = 0; i < quartz.Length; i++) quartz[i] = new ItemQuartz { ID = (ushort)i };
            SortedList<ushort, Item>[] Item = new SortedList<ushort, Item>[18];
            for (int i = 0; i < Item.Length; i++)
                Item[i] = new SortedList<ushort, Item>();
            foreach (Item item in items.Values)
            {
                Item[item.Field.ID / 100][item.Field.ID] = item;
            }
            foreach (Item item in Item[1].Values)
            {
                item.Quartz.ID = (ushort)(item.Field.ID - 100);
                quartz[item.Quartz.ID] = item.Quartz;
            }
            foreach (Item item in Item[2].Values)
            {
                item.Quartz.ID = (ushort)(item.Field.ID - 100);
                quartz[item.Quartz.ID] = item.Quartz;
            }
            using (var stream = File.OpenWrite(EditorBase.GetFile("t_quartz._dt")))
            {
                foreach (var q in quartz)
                {
                    WriteStruct(stream, q);
                }
            }
            //Item[9][999] = new Item { Field = new ItemField { ID = 999 } };
            //Item[17][9999] = new Item { Field = new ItemField { ID = 9999 } };
            using (var stream = File.OpenWrite(EditorBase.GetFile("t_item._dt")))
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(20, SeekOrigin.Begin);
                long epos = 22;
                for (int i = 0; i <= 9; i++)
                    epos += GetLength(Item[i]);
                for (int i = 0; i <= 9; i++)
                {
                    long bpos = stream.Position;
                    stream.Seek(i * 2, SeekOrigin.Begin);
                    writer.Write((ushort)bpos);
                    stream.Position = bpos;
                    int prev = i * 100;
                    foreach (var item in Item[i].Values)
                    {
                        for (int j = prev; j < item.Field.ID; j++)
                        {
                            writer.Write((ushort)epos);
                        }
                        prev = item.Field.ID + 1;
                        writer.Write((ushort)epos);
                        long ipos = stream.Position;
                        stream.Position = epos;
                        WriteStruct(stream, item.Field);
                        epos = stream.Position;
                        stream.Position = ipos;
                    }
                    if (i == 9)
                    {
                        writer.Write((ushort)epos);
                        long ipos = stream.Position;
                        stream.Position = epos;
                        WriteStruct(stream, new ItemField { ID = 999 });
                    }
                }
            }
            using (var stream = File.OpenWrite(EditorBase.GetFile("t_item2._dt")))
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(16, SeekOrigin.Begin);
                long epos = 18;
                for (int i = 0; i <= 7; i++)
                    epos += GetLength(Item[i + 10]);
                for (int i = 0; i <= 7; i++)
                {
                    long bpos = stream.Position;
                    stream.Seek(i * 2, SeekOrigin.Begin);
                    writer.Write((ushort)bpos);
                    stream.Position = bpos;
                    int prev = 1000 + i * 100;
                    foreach (var item in Item[i + 10].Values)
                    {
                        for (int j = prev; j < item.Field.ID; j++)
                        {
                            writer.Write((ushort)epos);
                        }
                        prev = item.Field.ID + 1;
                        writer.Write((ushort)epos);
                        long ipos = stream.Position;
                        stream.Position = epos;
                        WriteStruct(stream, item.Field);
                        epos = stream.Position;
                        stream.Position = ipos;
                    }
                    if (i == 7)
                    {
                        writer.Write((ushort)epos);
                        long ipos = stream.Position;
                        stream.Position = epos;
                        WriteStruct(stream, new ItemField { ID = 9999 });
                    }
                }
            }
            using (var stream = File.OpenWrite(EditorBase.GetFile("t_ittxt._dt")))
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(20, SeekOrigin.Begin);
                long epos = 22;
                for (int i = 0; i <= 9; i++)
                    epos += GetLength(Item[i]);
                for (int i = 0; i <= 9; i++)
                {
                    long bpos = stream.Position;
                    stream.Seek(i * 2, SeekOrigin.Begin);
                    writer.Write((ushort)bpos);
                    stream.Position = bpos;
                    int prev = i * 100;
                    foreach (var item in Item[i].Values)
                    {
                        for (int j = prev; j < item.Field.ID; j++)
                        {
                            writer.Write((ushort)epos);
                        }
                        prev = item.Field.ID + 1;
                        writer.Write((ushort)epos);
                        long ipos = stream.Position;
                        stream.Position = epos;
                        WriteItemStrings(writer, item);
                        epos = stream.Position;
                        stream.Position = ipos;
                    }
                    if (i == 9)
                    {
                        writer.Write((ushort)epos);
                        long ipos = stream.Position;
                        stream.Position = epos;
                        WriteItemStrings(writer, new Item { Field = new ItemField { ID = 999 }, Description = " ", Name = " " });
                    }
                }
            }
            using (var stream = File.OpenWrite(EditorBase.GetFile("t_ittxt2._dt")))
            using (var writer = new BinaryWriter(stream))
            {
                stream.Seek(16, SeekOrigin.Begin);
                long epos = 18;
                for (int i = 0; i <= 7; i++)
                    epos += GetLength(Item[i + 10]);
                for (int i = 0; i <= 7; i++)
                {
                    long bpos = stream.Position;
                    stream.Seek(i * 2, SeekOrigin.Begin);
                    writer.Write((ushort)bpos);
                    stream.Position = bpos;
                    int prev = 1000 + i * 100;
                    foreach (var item in Item[i + 10].Values)
                    {
                        for (int j = prev; j < item.Field.ID; j++)
                        {
                            writer.Write((ushort)epos);
                        }
                        prev = item.Field.ID + 1;
                        writer.Write((ushort)epos);
                        long ipos = stream.Position;
                        stream.Position = epos;
                        WriteItemStrings(writer, item);
                        epos = stream.Position;
                        stream.Position = ipos;
                    }
                    if (i == 7)
                    {
                        writer.Write((ushort)epos);
                        long ipos = stream.Position;
                        stream.Position = epos;
                        WriteItemStrings(writer, new Item { Field = new ItemField { ID = 9999 }, Description = " ", Name = " " });
                    }
                }
            }
        }
        public override bool Add(int id)
        {
            try
            {
                if (id < 0) return false;
                if (id >= 1700) return false;
                if (id == 999) return false;
                items.Add((ushort)id, new Item
                {
                    Description = " ",
                    Name = " ",
                    Field = new ItemField { ID = (ushort)id },
                    Quartz = id >= 100 && id < 300 ? new ItemQuartz() : null
                });
                return true;
            }
            catch { return false; }
        }
        public override bool CopyTo(object src, object dest)
        {
            Item si, di;
            try
            {
                si = (Item)src;
                di = (Item)dest;
            }
            catch { return false; }
            di.Name = si.Name;
            di.Description = si.Description;
            ushort id = di.Field.ID;
            di.Field = si.Field.Duplicate();
            di.Field.ID = id;
            if (si.Quartz != null && di.Quartz != null)
            {
                di.Quartz.Attr = si.Quartz.Attr;
                si.Quartz.Cost.CopyTo(di.Quartz.Cost, 0);
                si.Quartz.Quartz.CopyTo(di.Quartz.Quartz, 0);
            }
            return true;
        }
        public override IEnumerable<IndexedItem> GetList()
        {
            lock(this) if (items == null) Load();
            List<IndexedItem> list = new List<IndexedItem>();
            foreach (var v in items) list.Add(new IndexedItem { Index = v.Key, Item = v.Value });
            return list;
        }
        public override bool Remove(int id)
        {
            try
            {
                return items.Remove((ushort)id);
            }
            catch { return false; }
        }
        SortedDictionary<ushort, Item> items;
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    class ItemQuartz
    {
        public ItemQuartz()
        {
            Cost = new ushort[8];
            Quartz = new byte[8];
        }
        private ushort id;
        [Browsable(false)]
        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        private ushort attr;

        public ushort Attr
        {
            get { return attr; }
            set { attr = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        private ushort[] cost;
        public ushort[] Cost
        {
            get { return cost; }
            internal set { cost = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        private byte[] quartz;
        public byte[] Quartz
        {
            get { return quartz; }
            internal set { quartz = value; }
        }
    }

    class Item : IComparable<Item>
    {
        public ItemField Field { get; set; }
        public string Name { get; set; }
        public ItemQuartz Quartz { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(Item other)
        {
            return Field.ID - other.Field.ID;
        }
    }

    [Editor(typeof(ItemReferenceEditor), typeof(UITypeEditor))]
    [StructLayout(LayoutKind.Sequential)]
    class ItemReference
    {
        public override string ToString()
        {
            return Name;
        }
        [Editor(typeof(ItemIDSelector), typeof(UITypeEditor))]
        public ushort ID { get; set; }
        [Browsable(false)]
        public Item Item
        {
            get
            {
                return (Item)Helper.GetEditorByType(typeof(ItemEditor)).GetById(ID);
            }
        }
        public string Name
        {
            get
            {
                return Item != null ? Item.Name : null;
            }
        }
        public string Description
        {
            get
            {
                return Item != null ? Item.Description : null;
            }
        }
    }
    //[TypeConverter(typeof(ExpandableObjectConverter))]
    [TypeConverter(typeof(ValueTypeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    struct ItemCount
    {
        ItemReference item;
        public ItemReference Item
        {
            get { return item; }
            set { item = value; }
        }
        public ushort Count { get; set; }
    }
    class ValueTypeConverter : TypeConverter
    {
        class ValueTypePropertyDescriptor : SimplePropertyDescriptor
        {
            public ValueTypePropertyDescriptor(PropertyInfo property, ITypeDescriptorContext context,
                IEnumerable<Attribute> attributes) :
                base(property.ReflectedType, property.Name, property.PropertyType,
                GetAttributes(property,attributes))
            {
                this.property = property;
                this.context = context;
            }
            static Attribute[] GetAttributes(PropertyInfo property, IEnumerable<Attribute> attributes)
            {
                var list = new List<Attribute>(attributes);
                foreach (var attr in property.GetCustomAttributes(true))
                {
                    if (attr is Attribute) list.Add((Attribute)attr);
                }
                return list.ToArray();
            }
            PropertyInfo property;
            ITypeDescriptorContext context;
            public override void SetValue(object component, object value)
            {
                property.SetValue(component, value, new object[0]);
                context.PropertyDescriptor.SetValue(context.Instance, component);
            }
            public override object GetValue(object component)
            {
                return property.GetValue(component, new object[0]);
            }
        }
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context,
            object value, Attribute[] attributes)
        {
            List<PropertyDescriptor> list = new List<PropertyDescriptor>();
            foreach (var property in context.PropertyDescriptor.PropertyType.GetProperties())
            {
                var descriptor = new ValueTypePropertyDescriptor(property, context, attributes);
                if (descriptor.IsBrowsable) list.Add(descriptor);
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    class ItemReferenceEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,
            object value)
        {
            var selector = new Selector(Helper.GetEditorByType(typeof(ItemEditor)));
            var id = ((ItemReference)value).ID;
            selector.SetSelect(id);
            if (selector.ShowDialog() == DialogResult.OK && selector.Result != id)
                return value = new ItemReference { ID = (ushort)selector.Result };
            return value;
        }
    }
    class ItemIDSelector : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,
            object value)
        {
            var selector = new Selector(Helper.GetEditorByType(typeof(ItemEditor)));
            var id = (ushort)value;
            selector.SetSelect(id);
            if (selector.ShowDialog() == DialogResult.OK && selector.Result != id)
                return (ushort)selector.Result;
            return value;
        }
    }
}
