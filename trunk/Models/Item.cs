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
using System.Linq;

namespace ED7Editor
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public class ItemField
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

    [Browsable(false)]
    [ReadOnly(true)]
    public class MasterEffectsEditor : StaticListEditor<string>
    {
        public void Load(string[] e) { masterEffects = e; }
        string[] masterEffects;

        public string[] MasterEffects
        {
            get { return masterEffects; }
        }
        public override string GetById(int id)
        {
            return masterEffects[id];
        }
        public override IEnumerable<IndexedItem> GetList()
        {
            for (int i = 0; i < masterEffects.Length; i++)
                yield return new IndexedItem
                {
                    Index = i,
                    Item = masterEffects[i]
                };
        }
        public override IEnumerable<SelectorItem> GetSelector()
        {
            for (int i = 0; i < masterEffects.Length; i++)
                yield return new SelectorItem
                {
                    ID = i,
                    Name = masterEffects[i],
                    Description = masterEffects[i]
                };
        }
    }
    
    public class ItemEditor : EditorBase<Item>
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
        public override Item GetById(int id)
        {
            lock (this) if (items == null) Load();
            var x = checked(id - 990);
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
#if AONOKISEKI
            ItemQuartz[] quartz = new ItemQuartz[120];
            MasterQuartz[] master = new MasterQuartz[22];
            QuartzLevel[][] mqattr = new QuartzLevel[22][];
            for (int i = 0; i < mqattr.Length; i++)
                mqattr[i] = new QuartzLevel[5];
#else
            ItemQuartz[] quartz = new ItemQuartz[200];
#endif
            using (var stream = ReadFile("t_quartz._dt"))
            {
                int i = 0;
#if AONOKISEKI
                var b1 = stream.ReadByte();
                var b2 = stream.ReadByte();
                var length = (ushort)b2 << 8 | b1;
#else
                var length = stream.Length;
#endif
                while (stream.Position < length)
                {
                    quartz[i++] = ReadStrcuture<ItemQuartz>(stream);
                }
#if AONOKISEKI
                i = 0;
                while (i < master.Length)
                {
                    master[i++] = ReadStrcuture<MasterQuartz>(stream);
                }
#endif
            }
            using (var stream = ReadFile("t_item._dt"))
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
                    Item item = new Item { Name = "", Description = "" };
                    stream.Seek(p, SeekOrigin.Begin);
                    item.Field = ReadStrcuture<ItemField>(stream);
                    if (item.Field.ID >= 100 && item.Field.ID < 100 + quartz.Length)
                    {
                        item.Quartz = quartz[item.Field.ID - 100];
#if AONOKISEKI
                    } else if (item.Field.ID >= 220 && item.Field.ID < 242) {
                        item.Quartz = master[item.Field.ID - 220];
#endif
                    }
                    Item[item.Field.ID] = item;
                }
            }
            using (var stream = ReadFile("t_item2._dt"))
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
                    Item item = new Item { Name = "", Description = "" };
                    stream.Seek(p, SeekOrigin.Begin);
                    item.Field = ReadStrcuture<ItemField>(stream);
                    Item[item.Field.ID] = item;
                }
            }
            using (var stream = ReadFile("t_ittxt._dt"))
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
            using (var stream = ReadFile("t_ittxt2._dt"))
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
                Item.Remove(9999);
            }
            using (var stream = ReadFile("t_mstqrt._dt"))
            using (var reader = new BinaryReader(stream))
            {
                for (int i = 0; i < mqattr.Length; i++)
                {
                    for (int j = 0; j < 5; j++)
                        mqattr[i][j] = ReadStrcuture<QuartzLevel>(stream);
                    Item[(ushort)(220 + i)].Levels = mqattr[i];
                }
                List<ushort> pos = new List<ushort>();
                do
                {
                    pos.Add(reader.ReadUInt16());
                } while (stream.Position < pos[0]);
                string[] desc = new string[pos.Count];
                for (int i = 0; i < desc.Length; i++)
                {
                    stream.Position = pos[i];
                    desc[i] = ReadString(stream);
                }
                Helper.GetEditorByType<MasterEffectsEditor>().Load(desc);
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
#if AONOKISEKI
            ItemQuartz[] quartz = new ItemQuartz[120];
            MasterQuartz[] master = new MasterQuartz[22];
            QuartzLevel[][] mqattr = new QuartzLevel[22][];
            for (int i = 0; i < mqattr.Length; i++)
                mqattr[i] = items[(ushort)(i + 220)].Levels;
#else
            ItemQuartz[] quartz = new ItemQuartz[200];
#endif
            for (int i = 0; i < quartz.Length; i++) quartz[i] = new ItemQuartz { ID = (ushort)i };
            SortedList<ushort, Item>[] Item = new SortedList<ushort, Item>[18];
            for (int i = 0; i < Item.Length; i++)
                Item[i] = new SortedList<ushort, Item>();
            foreach (Item item in items.Values)
            {
                Item[item.Field.ID / 100][item.Field.ID] = item;
            }
            for (ushort i = 0; i < quartz.Length; i++)
                try
                {
                    quartz[i] = items[(ushort)(i + 100)].quartz;
                }
                catch (KeyNotFoundException)
                {
                }
#if AONOKISEKI
            for (ushort i = 0; i < master.Length; i++)
                try{
                    master[i] = items[(ushort)(i + 220)].master;
                }
                catch (KeyNotFoundException)
                {
                }
#endif
            using (var stream = WriteFile("t_quartz._dt"))
            using (var writer = new BinaryWriter(stream))
            {
#if AONOKISEKI
                writer.Write((ushort)0);
#endif
                foreach (var q in quartz)
                {
                    WriteStruct(stream, q);
                }
#if AONOKISEKI
                long pos = stream.Position;
                stream.Position = 0;
                writer.Write((ushort)pos);
                stream.Position = pos;
                foreach (var q in master)
                {
                    WriteStruct(stream, q);
                }
#endif
            }
            //Item[9][999] = new Item { Field = new ItemField { ID = 999 } };
            //Item[17][9999] = new Item { Field = new ItemField { ID = 9999 } };
            using (var stream = WriteFile("t_item._dt"))
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
            using (var stream = WriteFile("t_item2._dt"))
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
            using (var stream = WriteFile("t_ittxt._dt"))
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
            using (var stream = WriteFile("t_ittxt2._dt"))
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
            using (var stream = WriteFile("t_mstqrt._dt"))
            using (var writer = new BinaryWriter(stream))
            {
                for (int i = 0; i < mqattr.Length; i++)
                    for (int j = 0; j < 5; j++)
                        WriteStruct(stream, mqattr[i][j]);
                var effects = Helper.GetEditorByType<MasterEffectsEditor>().MasterEffects;
                var epos = stream.Position + effects.Length * sizeof(ushort);
                for (int i = 0; i < effects.Length; i++)
                {
                    writer.Write((ushort)epos);
                    long pos = stream.Position;
                    stream.Position = epos;
                    var bytes = Helper.Encoding.GetBytes(effects[i]);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.WriteByte(0);
                    epos = stream.Position;
                    stream.Position = pos;
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
#if AONOKISEKI
                    Quartz = id >= 220 && id < 242 ? (object)new MasterQuartz() : 
                            id >= 100 && id < 220 ? new ItemQuartz() : null
#else
                    Quartz = id >= 100 && id < 300 ? new ItemQuartz() : null
#endif
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
            if (si.quartz != null && di.quartz != null)
            {
                di.quartz.Attr = si.quartz.Attr;
                di.quartz.Cost = si.quartz.Cost;
                di.quartz.Quartz = si.quartz.Quartz;
            }
            if (si.master != null && di.master != null)
            {
                di.master.Attr = si.master.Attr;
                si.master.Levels.CopyTo(di.master.Levels, 0);
            }
            if (si.Levels != null && di.Levels != null)
            {
                for (int i = 0; i < 5; i++)
                    si.Levels[i].CopyTo(di.Levels[i]);
            }
            return true;
        }
        public override IEnumerable<IndexedItem> GetList()
        {
            lock (this) if (items == null) Load();
            return from i in items
                   select new IndexedItem { Index = i.Key, Item = i.Value };
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

    [TypeConverter(typeof(ValueTypeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    [AutoExpand]
    public struct ExpandedQuartz<T>
    {
        T dummy, earth, water, fire, wind, time, space, mirage;

        public T Earth
        {
            get { return earth; }
            set { earth = value; }
        }

        public T Water
        {
            get { return water; }
            set { water = value; }
        }

        public T Fire
        {
            get { return fire; }
            set { fire = value; }
        }

        public T Wind
        {
            get { return wind; }
            set { wind = value; }
        }

        public T Time
        {
            get { return time; }
            set { time = value; }
        }

        public T Space
        {
            get { return space; }
            set { space = value; }
        }

        public T Mirage
        {
            get { return mirage; }
            set { mirage = value; }
        }
    }

    [TypeConverter(typeof(ValueTypeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    [AutoExpand]
    public struct Quartz<T>
    {
        T earth, water, fire, wind, time, space, mirage;

        public T Earth
        {
            get { return earth; }
            set { earth = value; }
        }

        public T Water
        {
            get { return water; }
            set { water = value; }
        }

        public T Fire
        {
            get { return fire; }
            set { fire = value; }
        }

        public T Wind
        {
            get { return wind; }
            set { wind = value; }
        }

        public T Time
        {
            get { return time; }
            set { time = value; }
        }

        public T Space
        {
            get { return space; }
            set { space = value; }
        }

        public T Mirage
        {
            get { return mirage; }
            set { mirage = value; }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public class ItemQuartz
    {
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
        
        ExpandedQuartz<ushort> cost;
        public ExpandedQuartz<ushort> Cost
        {
            get { return cost; }
            set { cost = value; }
        }

        private ExpandedQuartz<byte> quartz;
        public ExpandedQuartz<byte> Quartz
        {
            get { return quartz; }
            set { quartz = value; }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public class MasterQuartz
    {
        public MasterQuartz()
        {
            levels = new ExpandedQuartz<byte>[5];
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

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        ExpandedQuartz<byte>[] levels;

        [AutoExpand]
        public ExpandedQuartz<byte>[] Levels
        {
            get { return levels; }
            internal set { levels = value; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    [Editor(typeof(MEReferenceEditor), typeof(UITypeEditor))]
    public struct MEReference
    {
        public override string ToString()
        {
            var value = Value;
            if (value != null) return value.ToString();
            else return string.Format("（ID：{0}）", ID);
        }

        [Editor(typeof(ReferenceIDEditor), typeof(UITypeEditor))]
        public byte ID { get; set; }

        public string Value
        {
            get
            {
                return Helper.GetEditorByType<MasterEffectsEditor>().GetById(Convert.ToInt32(ID));
            }
        }
    }
    public class MEReferenceEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context.PropertyDescriptor != null &&
                context.PropertyDescriptor.IsReadOnly)
                return UITypeEditorEditStyle.None;
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,
            object value)
        {
            using (var selector = new Selector(Helper.GetEditorByType<MasterEffectsEditor>()))
            {
                var id = ((MEReference)value).ID;
                selector.SetSelect(id);
                if (selector.ShowDialog() == DialogResult.OK && selector.Result != id)
                    return new MEReference { ID = (byte)selector.Result };
                return value;
            }
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public class QuartzLevel
    {
        public void CopyTo(QuartzLevel level)
        {
            baseAttr.CopyTo(level.baseAttr);
            effectAttr.CopyTo(level.effectAttr);
            effects.CopyTo(level.effects, 0);
            level.magic.ID = magic.ID;
        }
        static byte CDiv10(ushort s)
        {
            int y = s / 10;
            if (y * 10 < s) y++;
            return (byte)(y > 255 ? 255 : y);
        }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [StructLayout(LayoutKind.Sequential)]
        [AutoExpand]
        public class Base
        {
            public void CopyTo(Base b)
            {
                b.hp = hp;
                b.ep = ep;
                b.str = str;
                b.def = def;
                b.ats = ats;
                b.adf = adf;
                b.spd = spd;
            }
            ushort hp;

            public ushort Hp
            {
                get { return hp; }
                set { hp = value; }
            }
            byte ep, str, def, ats, adf, spd;

            public ushort Ep
            {
                get { return (ushort)(ep * 10); }
                set { ep = CDiv10(value); }
            }

            public ushort Str
            {
                get { return (ushort)(str * 10); }
                set { str = CDiv10(value); }
            }

            public ushort Def
            {
                get { return (ushort)(def * 10); }
                set { def = CDiv10(value); }
            }

            public ushort Ats
            {
                get { return (ushort)(ats * 10); }
                set { ats = CDiv10(value); }
            }

            public ushort Adf
            {
                get { return (ushort)(adf * 10); }
                set { adf = CDiv10(value); }
            }

            public byte Spd
            {
                get { return spd; }
                set { spd = value; }
            }
        }
        Base baseAttr;

        public Base BaseAttr
        {
            get { return baseAttr; }
            set { baseAttr = value; }
        }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [StructLayout(LayoutKind.Sequential)]
        [AutoExpand]
        public class Effect
        {
            public void CopyTo(Effect e)
            {
                e.at1 = at1;
                e.at2 = at2;
                e.at5 = at5;
                e.at6 = at6;
            }
            byte at1, at2, at5, at6;

            public byte At1
            {
                get { return at1; }
                set { at1 = value; }
            }

            public byte At2
            {
                get { return at2; }
                set { at2 = value; }
            }

            public ushort At3
            {
                get { return (ushort)(at2 << 8 | at1); }
                set { at2 = (byte)(value >> 8); at1 = (byte)(value & 0xFF); }
            }

            public double At4
            {
                get { return (double)At3 / 100; }
                set { checked { At3 = (ushort)(value * 100); } }
            }

            public byte At5
            {
                get { return at5; }
                set { at5 = value; }
            }

            public byte At6
            {
                get { return at6; }
                set { at6 = value; }
            }

            public ushort At7
            {
                get { return (ushort)(at6 << 8 | at5); }
                set { at6 = (byte)(value >> 8); at5 = (byte)(value & 0xFF); }
            }

            public double At8
            {
                get { return (double)At7 / 100; }
                set { checked { At7 = (ushort)(value * 100); } }
            }
        }
        Effect effectAttr;

        public Effect EffectAttr
        {
            get { return effectAttr; }
            set { effectAttr = value; }
        }
        MagicReference magic;

        public MagicReference Magic
        {
            get { return magic; }
            set { magic = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
        MEReference[] effects;

        public MEReference[] Effects
        {
            get { return effects; }
            internal set { effects = value; }
        }
    }
    
    public class Item : IComparable<Item>
    {
        [AutoExpand]
        public ItemField Field { get; set; }
        public string Name { get; set; }
        public ItemQuartz quartz;
        public MasterQuartz master;
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public object Quartz
        {
            get
            {
                return master ?? (object)quartz;
            }
            set
            {
                quartz = value as ItemQuartz;
                master = value as MasterQuartz;
            }
        }
        [AutoExpand]
        public QuartzLevel[] Levels { get; internal set; }
        [Editor(typeof(NormalStringEditor), typeof(UITypeEditor))]
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
    [StructLayout(LayoutKind.Sequential)]
    public class ItemReference : Reference<Item, ushort>
    { }

    [TypeConverter(typeof(ValueTypeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    [AutoExpand]
    public struct ItemCount
    {
        ItemReference item;
        public ItemReference Item
        {
            get { return item; }
            set { item = value; }
        }
        public ushort Count { get; set; }
    }
    
}
