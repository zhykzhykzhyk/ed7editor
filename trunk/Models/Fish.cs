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
using System.Diagnostics;

namespace ED7Editor
{
    public class Fish
    {
        public FishField Field { get; set; }
        [Editor(typeof(NormalStringEditor), typeof(UITypeEditor))]
        public string Description { get; set; }
        public override string ToString()
        {
            return Field.Fish.ToString();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FishField
    {
        ushort id;

        [ReadOnly(true)]
        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        ushort spos;
        [Browsable(false)]
        public ushort Spos
        {
            get { return spos; }
            set { spos = value; }
        }
        ItemReference fish;

        public ItemReference Fish
        {
            get { return fish; }
            set { fish = value; }
        }
        ushort unknown;

        public ushort Unknown
        {
            get { return unknown; }
            set { unknown = value; }
        }
        ushort small;

        public ushort MinSize
        {
            get { return small; }
            set { small = value; }
        }
        ushort large;

        public ushort MaxSize
        {
            get { return large; }
            set { large = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        FishingAward[] award;

        public FishingAward[] Award
        {
            get { return award; }
            internal set { award = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        Bait[] bait;

        public Bait[] Bait
        {
            get { return bait; }
            internal set { bait = value; }
        }
    }
    [TypeConverter(typeof(ValueTypeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public struct FishingAward
    {
        ushort unknown;

        public ushort Unknown
        {
            get { return unknown; }
            set { unknown = value; }
        }
        ItemReference item;

        public ItemReference Item
        {
            get { return item; }
            set { item = value; }
        }
        ushort amount;

        public ushort Amount
        {
            get { return amount; }
            set { amount = value; }
        }
        ushort zero;

        public ushort Zero
        {
            get { return zero; }
            set { zero = value; }
        }
    }

    [TypeConverter(typeof(ValueTypeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public struct Bait
    {
        ItemReference item;

        public ItemReference Item
        {
            get { return item; }
            set { item = value; }
        }
        ushort unknown;

        public ushort Unknown
        {
            get { return unknown; }
            set { unknown = value; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Pole
    {
        ushort id;
        [ReadOnly(true)]
        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        ushort unknown;

        public ushort Unknown
        {
            get { return unknown; }
            set { unknown = value; }
        }
#if AONOKISEKI
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=10)]
#else
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=30)]
#endif
        ushort[] bait;

        [TypeConverter(typeof(ReferenceArrayConverter<Item, ushort>))]
        public ushort[] Bait
        {
            get { return bait; }
            set { bait = value; }
        }

        public string Name
        {
            get
            {  
#if AONOKISEKI
                var aid = 20 + id;
#else
                var aid = 50 + id;
#endif
                var item = Helper.GetEditorByType<ItemEditor>().GetById(aid);
                if (item != null)
                    return item.Name;
                else
                    return "";
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Place
    {
        ushort id;
        [ReadOnly(true)]
        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        TownReference town;
        [ReadOnly(true)]
        public TownReference Town
        {
            get { return town; }
            set { town = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        sbyte[] fish;
        [TypeConverter(typeof(ReferenceArrayConverter<Fish, sbyte>))]
        public sbyte[] Fish
        {
            get { return fish; }
            set { fish = value; }
        }

        public override string ToString()
        {
            return Town.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class TownReference : Reference<Town, ushort> { }

    [StructLayout(LayoutKind.Sequential)]
    public class FishReference : Reference<Fish, sbyte> { }


#if AONOKISEKI
    [ReadOnly(true)]
#endif
    public class FishEditor : EditorBase<Fish>
    {
        List<Fish> fishes;
        List<Place> places;
        List<Pole> poles;
        public override void Load()
        {
            //return;
            using (var stream = ReadFile("t_fish._dt"))
            using (var reader = new BinaryReader(stream))
            {
                ushort pos1 = reader.ReadUInt16();
                ushort pos2 = reader.ReadUInt16();
                ushort pos3 = reader.ReadUInt16();
                stream.Position = pos1;
                fishes = new List<Fish>();
                places = new List<Place>();
                poles = new List<Pole>();
                Fish fish;
                do
                {
                    fish = new Fish { Field = ReadStrcuture<FishField>(stream) };
                    if (fish.Field.Spos != 0)
                    {
                        long pos = stream.Position;
                        stream.Position = fish.Field.Spos;
                        fish.Description = ReadString(stream);
                        stream.Position = pos;
                    }
                    else fish.Description = "";// null;
                    fishes.Add(fish);
                } while (fish.Field.ID != 99);
                Pole pole;
                do
                {
                    pole = ReadStrcuture<Pole>(stream);
                    poles.Add(pole);
                } while (pole.ID != 99);
                Place place;
                do
                {
                    place = ReadStrcuture<Place>(stream);
                    places.Add(place);
#if AONOKISEKI
                } while (place.ID != 0x22);
#else
                } while (place.ID != 99);
#endif
            }
        }

        public override IEnumerable<IndexedItem> GetList()
        {
            foreach (var i in fishes)
                if (i.Field.ID != 99) yield return new IndexedItem { Index = i.Field.ID, Item = i };
            yield return new IndexedItem { Index = -2, Name = "--鱼竿--" };
            foreach (var i in poles)
                if (i.ID != 99) yield return new IndexedItem { Index = 100 + i.ID, Item = i };
            yield return new IndexedItem { Index = -3, Name = "--钓点--" };
            foreach (var i in places)
                if (i.ID != 99) yield return new IndexedItem { Index = 200 + i.ID, Item = i };

        }

        public override IEnumerable<SelectorItem> GetSelector()
        {
            yield return new SelectorItem { ID = -1, Name = "(无)" };
            foreach (var fish in fishes)
                yield return new SelectorItem { ID = fish.Field.ID, Name = fish.ToString(),
                    Description = fish.Description.Replace(@"\n", "\r\n") };
        }

        public override Fish GetById(int id)
        {
            if (id < 0) return null;
            else return fishes[id];
        }

        public override bool Add(int id)
        {
            return false;
        }

        public override bool CopyTo(object src, object dest)
        {
            return false;
        }

        public override bool Remove(int item)
        {
            return false;
        }

        public override void Save()
        {
#if AONOKISEKI
            return;
#endif
            using (var stream = WriteFile("t_fish._dt"))
            using (var writer = new BinaryWriter(stream))
            {
                long epos = 6;
                writer.Write((ushort)epos);
                epos += Marshal.SizeOf(typeof(FishField)) * fishes.Count;
                writer.Write((ushort)epos);
                epos += Marshal.SizeOf(typeof(Pole)) * poles.Count;
                writer.Write((ushort)epos);
                epos += Marshal.SizeOf(typeof(Place)) * places.Count;
                foreach (var fish in fishes)
                {
                    fish.Field.Spos = (ushort)(fish.Description == null ? 0 : epos);
                    WriteStruct(stream, fish.Field);
                    if (fish.Description != null)
                    {
                        long pos = stream.Position;
                        stream.Position = epos;
                        var bytes = Helper.Encoding.GetBytes(fish.Description);
                        stream.Write(bytes, 0, bytes.Length);
                        stream.WriteByte(0);
                        epos = (ushort)stream.Position;
                        stream.Position = pos;
                    }
                }
                foreach (var pole in poles)
                    WriteStruct(stream, pole);
                foreach (var place in places)
                    WriteStruct(stream, place);
            }
            //throw new NotImplementedException();
        }
    }
}
