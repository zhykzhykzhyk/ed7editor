using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Drawing.Design;
using System.IO;

namespace ED7Editor
{
    [DisplayName("Shop Editor")]
    class ShopEditor : EditorBase<Shop>
    {
        public override IEnumerable<SelectorItem> GetSelector()
        {
            throw new NotImplementedException();
        }
        public override bool Add(int id)
        {
            return false;
        }
        public override bool CopyTo(object src, object dest)
        {
            return false;
        }
        public override IEnumerable<IndexedItem> GetList()
        {
            if (shops == null) Load();
            List<IndexedItem> list = new List<IndexedItem>();
            foreach (var v in shops) list.Add(new IndexedItem { Index = v.Key, Item = v.Value });
            return list;
        }
        SortedDictionary<ushort, Shop> shops;
        public override void Load()
        {
            SortedDictionary<ushort, Shop> shops = new SortedDictionary<ushort, Shop>();
            using (var stream = File.OpenRead(EditorBase.GetFile("t_shop._dt")))
            using (var reader = new BinaryReader(stream))
            {
                ushort end = 0x200;
                List<ushort> lp = new List<ushort>();
                do
                {
                    ushort pos = reader.ReadUInt16();
                    if (pos != 0) lp.Add(pos);
                } while (stream.Position < end);
                //lp.Add(pos);
                foreach (var p in lp)
                {
                    Shop shop = new Shop();
                    stream.Seek(p, SeekOrigin.Begin);
                    shop.Field = ReadStrcuture<ShopField>(stream);
                    stream.Seek(shop.Field.Name, SeekOrigin.Begin);
                    shop.Name = ReadString(stream);
                    stream.Seek(shop.Field.Items, SeekOrigin.Begin);
                    if (shop.Field.Count != 100 && shop.Field.Count != 200)
                    {
                        shop.Items = new List<ItemReference>();
                        for (int i = 0; i < shop.Field.Count; i++)
                            shop.Items.Add(new ItemReference { ID = reader.ReadUInt16() });
                    }
                    shops[shop.Field.ID] = shop;
                }
            }
            this.shops = shops;
        }
        public override object GetById(int id)
        {
            throw new NotImplementedException();
        }
        public override bool Remove(int item)
        {
            return false;
        }
        public override void Save()
        {
            using (var stream = File.OpenWrite(EditorBase.GetFile("t_shop._dt")))
            using (var writer = new BinaryWriter(stream))
            {
                long epos = 0x200;
                for (ushort i = 0; i < 0x100;i++){
                    stream.Seek(i * 2, SeekOrigin.Begin);
                    if (shops.ContainsKey(i)) writer.Write((ushort)epos);
                    else
                    {
                        writer.Write((ushort)0);
                        continue;
                    }
                    byte[] s = Helper.Encoding.GetBytes(shops[i].Name);
                    if (shops[i].Items != null)
                        shops[i].Field.Count = (byte)shops[i].Items.Count;
                    shops[i].Field.ID = i;
                    shops[i].Field.Name = (ushort)(epos + 16);
                    shops[i].Field.Items = (ushort)(epos + 17 + s.Length);
                    stream.Position = epos;
                    WriteStruct(stream, shops[i].Field);
                    stream.Write(s, 0, s.Length);
                    writer.Write((byte)0);
                    if (shops[i].Items != null)
                        foreach (var item in shops[i].Items)
                            writer.Write((ushort)item.ID);
                    epos = stream.Position;
                }
            }
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    class ShopField
    {
        private ushort id;
        [ReadOnly(true)]
        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        private byte type;

        public byte Type
        {
            get { return type; }
            set { type = value; }
        }
        public byte Count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        private byte[] exchange;

        public byte[] Exchange
        {
            get { return exchange; }
            internal set { exchange = value; }
        }
        public ushort Items;
        public ushort Name;
    }

    class Shop
    {
        public ShopField Field { get; set; }
        public string Name { get; set; }
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<ItemReference> Items { get; internal set; }
        public override string ToString()
        {
            return String.Format("{0:000} {1}", Field.ID, Name);
        }
    }
}
