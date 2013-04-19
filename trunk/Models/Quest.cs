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
using System.Linq;

namespace ED7Editor
{
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class QuestField
    {
        sbyte id;

        public sbyte ID
        {
            get { return id; }
            set { id = value; }
        }
        byte flag;

        public byte Flag
        {
            get { return flag; }
            set { flag = value; }
        }
        ushort money;

        public ushort Money
        {
            get { return money; }
            set { money = value; }
        }
        byte bp;

        public byte Bp
        {
            get { return bp; }
            set { bp = value; }
        }
        byte sub;

        public byte Sub
        {
            get { return sub; }
            set { sub = value; }
        }
        ushort zero;

        public ushort Zero
        {
            get { return zero; }
            set { zero = value; }
        }
        ushort start;

        public ushort Start
        {
            get { return start; }
            set { start = value; }
        }
        ushort end;

        public ushort End
        {
            get { return end; }
            set { end = value; }
        }
        /*
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=7)]
        byte[] unkown;

        public byte[] Unkown
        {
            get { return unkown; }
            set { unkown = value; }
        }
         */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        uint[] str;
        [Browsable(false)]
        public uint[] Str
        {
            get { return str; }
            set { str = value; }
        }
        uint strs;

        [Browsable(false)]
        public uint Strs
        {
            get { return strs; }
            set { strs = value; }
        }
    }

    [Editor(typeof(CompressedStringEditor), typeof(UITypeEditor))]
    public class CompressedString
    {
        string s;
        public static implicit operator CompressedString(string s)
        {
            return new CompressedString{s=s};
        }
        public static implicit operator string(CompressedString s)
        {
            return s.s;
        }
        public override string ToString()
        {
            return s;
        }
    }

    public class Quest
    {
        public QuestField Field { get; set; }
        public CompressedString[] Str { get; set; }
        public CompressedString[] Strs { get; set; }
        public override string ToString()
        {
            return Str[0];
        }
    }

    [ReadOnly(true)]
    public class QuestEditor : EditorBase<Quest>
    {
        public override Quest GetById(int id)
        {
            throw new NotImplementedException();
        }

        List<Quest> quests;

        public override void Load()
        {
            return;
            using (var stream = ReadFile("t_quest._dt"))
            using (var reader = new BinaryReader(stream))
            {
                quests = new List<Quest>();
                while (true)
                {
                    var field = ReadStrcuture<QuestField>(stream);
                    if (field.ID == -1) break;
                    quests.Add(new Quest { Field = field });
                }
                foreach (var quest in quests)
                {
                    quest.Str = new CompressedString[quest.Field.Str.Length];
                    for (int i = 0; i < quest.Str.Length; i++)
                    {
                        stream.Position = quest.Field.Str[i];
                        quest.Str[i] = ReadString(stream);
                    }
                    uint[] pos = new uint[32];
                    quest.Strs = new CompressedString[pos.Length];
                    stream.Position = quest.Field.Strs;
                    for (int i = 0; i < pos.Length; i++)
                    {
                        pos[i] = reader.ReadUInt32();
                    }
                    for (int i = 0; i < pos.Length; i++)
                    {
                        stream.Position = pos[i];
                        quest.Strs[i] = ReadString(stream);
                    }

                }
            }
        }

        public override IEnumerable<IndexedItem> GetList()
        {
            return from q in quests 
                   select new IndexedItem { Index = q.Field.ID, Item = q };
        }

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

        public override bool Remove(int item)
        {
            return false;
        }

        public override void Save()
        {

        }
    }
}
