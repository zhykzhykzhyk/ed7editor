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

namespace ED7Editor.Models
{
    public class Cook
    {
        List<Recipe> Recipes { get; set; }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Recipe
    {
        public RecipeField Field { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return Field.Product.ToString();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RecipeField
    {
        ushort id;
        [ReadOnly(true)]
        public ushort ID
        {
            get { return id; }
            set { id = value; }
        }
        ItemReference product;

        public ItemReference Product
        {
            get { return product; }
            set { product = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        ItemCount[] material;

        public ItemCount[] Material
        {
            get { return material; }
            internal set { material = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        ushort[] level;

        public ushort[] Level
        {
            get { return level; }
            internal set { level = value; }
        }
        ushort description;
        [Browsable(false)]
        public ushort Description
        {
            get { return description; }
            set { description = value; }
        }
        [Browsable(false)]
        ushort unknown;

        public ushort Unknown
        {
            get { return unknown; }
            set { unknown = value; }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CookProbability
    {
        CharacterReference character;
        [ReadOnly(true)]
        public CharacterReference Character
        {
            get { return character; }
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        LevelProbability[] levels;

        public LevelProbability[] Levels
        {
            get { return levels; }
            internal set { levels = value; }
        }
    }
    [TypeConverter(typeof(ValueTypeConverter))]
    public struct LevelProbability
    {
        ushort prefect;

        public ushort Prefect
        {
            get { return prefect; }
            set { prefect = value; }
        }
        ushort success;

        public ushort Success
        {
            get { return success; }
            set { success = value; }
        }
        ushort failed;

        public ushort Failed
        {
            get { return failed; }
            set { failed = value; }
        }
        ushort unexpected;

        public ushort Unexpected
        {
            get { return unexpected; }
            set { unexpected = value; }
        }
    }
    public class CookEditor : EditorBase
    {
        List<Recipe> recipes;
        CookProbability[] probability;


        public override void Load()
        {
            using (var stream = ReadFile("t_cook._dt"))
            using (var reader = new BinaryReader(stream))
            {
                recipes = new List<Recipe>();
                ushort pos1 = reader.ReadUInt16();
                ushort pos2 = reader.ReadUInt16();
                stream.Position = pos1;
                while (stream.Position < pos2)
                {
                    Recipe recipe = new Recipe();
                    recipe.Field = ReadStrcuture<RecipeField>(stream);
                    long pos = stream.Position;
                    stream.Position = recipe.Field.Description;
                    recipe.Description = ReadString(stream);
                    stream.Position = pos;
                    recipes.Add(recipe);
                }
                stream.Position = pos2;
                probability = new CookProbability[4];
                for (int i = 0; i < 4; i++)
                    probability[i] = ReadStrcuture<CookProbability>(stream);
            }
        }

        public override IEnumerable<IndexedItem> GetList()
        {
            List<IndexedItem> items = new List<IndexedItem>();
            items.Add(new IndexedItem { Index = -1, Item = probability, Name = "Probability" });
            foreach (var recipe in recipes)
                if (recipe.Field.ID != 999)
                    items.Add(new IndexedItem { Index = recipe.Field.ID, Item = recipe });
            return items;
        }

        public override IEnumerable<SelectorItem> GetSelector()
        {
            throw new NotImplementedException();
        }

        public override object GetById(int id)
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
            using (var stream = WriteFile("t_cook._dt"))
            using (var writer = new BinaryWriter(stream))
            {
                long epos = 4;
                writer.Write((ushort)epos);
                epos += Marshal.SizeOf(typeof(RecipeField)) * recipes.Count;
                writer.Write((ushort)epos);
                epos += Marshal.SizeOf(typeof(CookProbability)) * probability.Length;
                foreach (var recipe in recipes)
                {
                    if (recipe.Field.ID != 999)
                        recipe.Field.Description = (ushort)epos;
                    WriteStruct(stream, recipe.Field);
                    if (recipe.Field.ID == 999) break;
                    long pos = stream.Position;
                    stream.Position = epos;
                    var bytes = Helper.Encoding.GetBytes(recipe.Description);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.WriteByte(0);
                    epos = (ushort)stream.Position;
                    stream.Position = pos;
                }
                for (int i = 0; i < probability.Length; i++)
                    WriteStruct(stream, probability[i]);
            }
        }
    }
}
