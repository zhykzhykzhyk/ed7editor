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
    [DisplayName("Trade Editor")]
    class TradeEditor : EditorBase<Trade>
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
            if (trades == null) Load();
            List<IndexedItem> list = new List<IndexedItem>();
            for (int i = 0; i < trades.Count; i++)
                list.Add(new IndexedItem { Index = i, Item = trades[i] });
            return list;
        }
        IList<Trade> trades;
        public readonly static string[] TradeNames = new string[] { "交换", "改造" };
        public override void Load()
        {
            using (var stream = File.OpenRead(EditorBase.GetFile("t_trade._dt")))
            using (var reader = new BinaryReader(stream))
            {
                ushort[] pos = new ushort[2];
                for (int i = 0; i < pos.Length; i++) pos[i] = reader.ReadUInt16();
                Trade[] trades = new Trade[pos.Length];
                for (int i = 0; i < pos.Length; i++)
                {
                    stream.Position = pos[i];
                    trades[i] = new Trade { Name = TradeNames[i], Trades = new List<TradeField>() };
                    while (true)
                    {
                        TradeField field = ReadStrcuture<TradeField>(stream);
                        if (field.ID == 999) break;
                        trades[i].Trades.Add(field);
                    }
                }
                this.trades = trades;
            }
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
            using (var stream = File.OpenWrite(EditorBase.GetFile("t_trade._dt")))
            using (var writer = new BinaryWriter(stream))
            {
                ushort pos = 4;
                byte[] end = new byte[18];
                for (int i = 0; i < trades.Count; i++)
                {
                    writer.Write(pos);
                    pos += (ushort)((trades[i].Trades.Count + 1) * 20);
                }
                foreach (var trade in trades)
                {
                    foreach (var field in trade.Trades) WriteStruct(stream, field);
                    writer.Write((ushort)999);
                    stream.Write(end, 0, end.Length);
                }
            }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    class TradeField
    {
        private ushort id;

        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        private ItemReference target;

        public ItemReference Target
        {
            get { return target; }
            set { target = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private ItemCount[] source;

        public ItemCount[] Source
        {
            get { return source; }
            internal set { source = value; }
        }

        public override string ToString()
        {
            return Target.Name;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    class Trade
    {
        [Browsable(false)]
        public string Name { get; set; }
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<TradeField> Trades { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
