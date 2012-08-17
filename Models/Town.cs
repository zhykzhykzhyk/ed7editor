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
    public class Town
    {
        public string Name { get; set; }
        public override string  ToString()
        {
            return Name;
        }
    }
    [Browsable(false)]
    public class TownEditor : EditorBase<Town>
    {
        Town[] towns;
        public override Town GetById(int id)
        {
            if (towns == null) Load();
            return towns[id];
        }

        public override void Load()
        {
            using (var stream = ReadFile("t_town._dt"))
            using (var reader = new BinaryReader(stream))
            {
                ushort len = reader.ReadUInt16();
                towns = new Town[len];
                ushort[] pos = new ushort[len];
                for (int i = 0; i < len; i++)
                    pos[i] = reader.ReadUInt16();
                for (int i = 0; i < len; i++)
                {
                    stream.Position = pos[i];
                    towns[i] = new Town { Name = ReadString(stream) };
                }
            }
        }

        public override IEnumerable<IndexedItem> GetList()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<SelectorItem> GetSelector()
        {
            throw new NotImplementedException();
        }

        public override bool Add(int id)
        {
            throw new NotImplementedException();
        }

        public override bool CopyTo(object src, object dest)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(int item)
        {
            throw new NotImplementedException();
        }

        public override void Save()
        {
        }
    }
}
