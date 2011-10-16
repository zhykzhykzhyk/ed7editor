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
//#if !AONOKISEKI
namespace ED7Editor
{
    [ReadOnly(true)]
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class NameField
    {
        ushort id;

        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        ushort name;

        public ushort Name
        {
            get { return name; }
            set { name = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        byte[] unknown;

        public byte[] Unknown
        {
            get { return unknown; }
            set { unknown = value; }
        }
    }

    [ReadOnly(true)]
    public class Name
    {
        public NameField Field { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return Value;
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class OrbLine
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        sbyte[] orbs;

        public sbyte[] Orbs
        {
            get { return orbs; }
            set { orbs = value; }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class OrbField
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        byte[] slots;

        public byte[] Slots
        {
            get { return slots; }
            set { slots = value; }
        }
        byte length;

        public byte Length
        {
            get { return length; }
            set { length = value; }
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Orb
    {
        public OrbField Field { get; set; }
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<OrbLine> Lines { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CraftGet
    {
        ushort level;

        public ushort Level
        {
            get { return level; }
            set { level = value; }
        }
        MagicReference craft;

        public MagicReference Craft
        {
            get { return craft; }
            set { craft = value; }
        }

        public override string ToString()
        {
            return Craft.Name;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Slot
    {
        uint zero;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        ushort[] cost1;
        [ReadOnly(true)]
        public ushort[] Cost1
        {
            get { return cost1; }
            set { cost1 = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        ushort[] cost2;

        [ReadOnly(true)]
        public ushort[] Cost2
        {
            get { return cost2; }
            set { cost2 = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        ushort[] cost3;

        [ReadOnly(true)]
        public ushort[] Cost3
        {
            get { return cost3; }
            set { cost3 = value; }
        }

    }

    public class Character
    {
        public Name Name { get; set; }
        public Orb Orb { get; set; }
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<CraftGet> Crafts { get; set; }
        [ReadOnly(true)]
        public Slot[] Slots { get; set; }
        public override string ToString()
        {
            return Name.Value;
        }
    }

    public class CharacterEditor : EditorBase<Character>
    {
        public override bool Add(int id)
        {
            throw new NotImplementedException();
        }

        public override bool CopyTo(object src, object dest)
        {
            throw new NotImplementedException();
        }

        SortedDictionary<ushort, Character> characters;

        const int MainCharacter = 10;

        public override void Load()
        {
            SortedDictionary<ushort, Character> characters = new SortedDictionary<ushort, Character>();
            using (var stream = ReadFile("t_name._dt"))
            using (var reader = new BinaryReader(stream))
            {
                while (true)
                {
                    Character character = new Character();
                    character.Name = new Name();
                    character.Name.Field = ReadStrcuture<NameField>(stream);
                    long pos = stream.Position;
                    stream.Position = character.Name.Field.Name;
                    character.Name.Value = ReadString(stream);
                    stream.Position = pos;
                    if (character.Name.Field.ID == 999) break;
                    characters.Add(character.Name.Field.ID, character);
                }
            }
            using (var stream = ReadFile("t_orb._dt"))
            using (var reader = new BinaryReader(stream))
            {
                ushort i = 0;
                while (true)
                {
                    ushort pos = reader.ReadUInt16();
                    unchecked { if (pos == (ushort)-1) break; }
                    long p = stream.Position;
                    stream.Position = pos;
                    Orb orb = new Orb();
                    orb.Field = ReadStrcuture<OrbField>(stream);
                    orb.Lines = new List<OrbLine>();
                    for (int l = 0; l < orb.Field.Length; l++)
                    {
                        orb.Lines.Add(ReadStrcuture<OrbLine>(stream));
                    }
                    characters[i++].Orb = orb;
                    stream.Position = p;
                }
            }
            using (var stream = ReadFile("t_crfget._dt"))
            using (var reader = new BinaryReader(stream))
            {
                ushort i = 0;
                while (true)
                {
                    ushort pos = reader.ReadUInt16();
                    unchecked { if (pos == (ushort)-1) break; }
                    long p = stream.Position;
                    stream.Position = pos;
                    List<CraftGet> crafts = new List<CraftGet>();
                    while (true)
                    {
                        CraftGet craft = ReadStrcuture<CraftGet>(stream);
                        unchecked { if (craft.Craft.ID == (ushort)-1) break; }
                        crafts.Add(craft);
                    }
                    characters[i++].Crafts = crafts;
                    stream.Position = p;
                }
            }
            using (var stream = ReadFile("t_sltget._dt"))
            using (var reader = new BinaryReader(stream))
            {
                ushort i = 0;
                while (true)
                {
                    ushort pos = reader.ReadUInt16();
                    unchecked { if (pos == (ushort)-1) break; }
                    long p = stream.Position;
                    stream.Position = pos;
                    Slot[] slots = new Slot[7];
                    for (int j = 0; j < slots.Length;j++)
                        slots[j] = ReadStrcuture<Slot>(stream);
                    characters[i++].Slots = slots;
                    stream.Position = p;
                }
            }
            this.characters = characters;
        }

        public override IEnumerable<IndexedItem> GetList()
        {
            List<IndexedItem> list = new List<IndexedItem>();
            foreach (var character in characters.Values)
            {
                list.Add(new IndexedItem { Index = character.Name.Field.ID, Item = character });
            }
            return list;
        }

        public override IEnumerable<SelectorItem> GetSelector()
        {
            throw new NotImplementedException();
        }

        public override object GetById(int id)
        {
            if (characters == null) Load();
            return characters.ContainsKey((ushort)id) ? characters[(ushort)id] : null;
        }

        public override bool Remove(int item)
        {
            return false;
        }

        public override void Save()
        {
            using (var stream = WriteFile("t_orb._dt"))
            using (var writer = new BinaryWriter(stream))
            {
                long epos = (MainCharacter + 1) * 2;
                for (ushort i = 0; i < MainCharacter;i++){
                    writer.Write((ushort)epos);
                    long p = stream.Position;
                    stream.Position=epos;
                    characters[i].Orb.Field.Length=(byte)characters[i].Orb.Lines.Count;
                    WriteStruct(stream, characters[i].Orb.Field);
                    foreach(var line in characters[i].Orb.Lines)
                        WriteStruct(stream, line);
                    unchecked { writer.Write((ushort)-1); }
                    epos=stream.Position;
                    stream.Position = p;
                }
                unchecked { writer.Write((ushort)-1); }
            }
            using (var stream = WriteFile("t_crfget._dt"))
            using (var writer = new BinaryWriter(stream))
            {
                long epos = (MainCharacter + 1) * 2;
                for (ushort i = 0; i < MainCharacter; i++)
                {
                    writer.Write((ushort)epos);
                    long p = stream.Position;
                    stream.Position = epos;
                    foreach (var craft in characters[i].Crafts)
                        WriteStruct(stream, craft);
                    unchecked { writer.Write((uint)-1); }
                    epos = stream.Position;
                    stream.Position = p;
                }
                unchecked { writer.Write((ushort)-1); }
            }
            using (var stream = WriteFile("t_sltget._dt"))
            using (var writer = new BinaryWriter(stream))
            {
                long epos = (MainCharacter + 1) * 2;
                for (ushort i = 0; i < MainCharacter; i++)
                {
                    writer.Write((ushort)epos);
                    long p = stream.Position;
                    stream.Position = epos;
                    foreach(var slot in characters[i].Slots)
                        WriteStruct(stream, slot);
                    epos = stream.Position;
                    stream.Position = p;
                }
                unchecked { writer.Write((ushort)-1); }

            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CharacterReference
    {
        public override string ToString()
        {
            return Name;
        }
        public ushort ID { get; set; }
        [Browsable(false)]
        public Character Character
        {
            get
            {
                return (Character)Helper.GetEditorByType(typeof(CharacterEditor)).GetById(ID);
            }
        }
        public string Name
        {
            get
            {
                return Character != null ? Character.Name.Value : null;
            }
        }
    }
}
//#endif