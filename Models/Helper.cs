using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.ComponentModel;
using System.Security.Cryptography;

namespace ED7Editor
{
    public class IndexedItem
    {
        public int Index { get; set; }
        public object Item { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            if (Name != null) return Name;
            if (Item == null) return "";
            return Item.ToString();
        }
    }
    public class IndexedItem<T> : IndexedItem
    {
        public new T Item
        {
            get
            {
                return (T)base.Item;
            }
            set
            {
                base.Item = value;
            }
        }
    }
    public abstract class EditorBase<T> : EditorBase where T : class
    {
        public abstract T GetById(int id);
        public override object GetObjById(int id)
        {
            return GetById(id);
        }
    }
    public abstract class EditorBase
    {
        public void Refresh()
        {
            if (Update != null)
                Update(this, new EventArgs());
        }
        public event EventHandler Update;
        public abstract void Load();
        public abstract IEnumerable<IndexedItem> GetList();
        public abstract IEnumerable<SelectorItem> GetSelector();
        public abstract object GetObjById(int id);
        public abstract bool Add(int id);
        public abstract bool CopyTo(object src, object dest);
        public abstract bool Remove(int item);
        public abstract void Save();
        public static string ReadString(Stream stream)
        {
            byte b;
            List<byte> a = new List<byte>();
            while ((b = (byte)stream.ReadByte()) != 0) a.Add(b);
            string s = Helper.Encoding.GetString(a.ToArray());
            return s;
        }
        public static T ReadStrcuture<T>(Stream stream)
        {
            byte[] buf = new byte[Marshal.SizeOf(typeof(T))];
            stream.Read(buf, 0, buf.Length);
            var handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
        public static void WriteStruct(Stream stream, object sct)
        {
            byte[] buffer = new byte[Marshal.SizeOf(sct)];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(sct, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), false);
            }
            finally
            {
                handle.Free();
            }
            stream.Write(buffer, 0, buffer.Length);
        }
        public static bool ElevateProcess()
        {
            Process process = new Process();
            process.StartInfo.Verb = "runas";
            process.StartInfo.FileName = Application.ExecutablePath;
            try
            {
                return process.Start();
            }
            catch { return false; }
        }
        public static FileStream ReadFile(string filename)
        {
            return File.OpenRead(GetFile(filename, true));
        }

        public static FileStream WriteFile(string filename)
        {
            return File.OpenWrite(GetFile(filename));
        }

        private static string GetFile(string filename, bool readOnly = false)
        {
            try
            {
                string textPath = Properties.Settings.Default.ED7Path + @"\data\text\";
                string org = String.Format(@"\{0}\", Helper.Encoding.CodePage);
                if (!File.Exists(textPath + filename))
                {
                    File.Copy(Application.StartupPath + org + filename, textPath + filename);
                    return textPath + filename;
                }
                byte[] header = new byte[4];
                using (var stream = File.Open(textPath + filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Read(header, 0, 4);
                }
                if (header[0] == 'S' && header[1] == 'D' && header[2] == 'F' && header[3] == 'A')
                {
                    File.Move(textPath + filename, textPath + filename + ".bak");
                    File.Copy(Application.StartupPath + org + filename, textPath + filename);
                }
                return textPath + filename;
            }
            catch (UnauthorizedAccessException)
            {
                if (!ElevateProcess())
                    MessageBox.Show("拒绝访问！请以管理员权限运行此程序。");
                Environment.Exit(0);
                throw;
            }
        }
        public override string ToString()
        {
            Type type = GetType();
            try
            {
                return ((DisplayNameAttribute)type
                    .GetCustomAttributes(typeof(DisplayNameAttribute), true)[0]).DisplayName;
            }
            catch
            {
                return type.Name;
            }
        }

        public bool Browsable
        {
            get
            {
                try
                {
                    return ((BrowsableAttribute)GetType()
                        .GetCustomAttributes(typeof(BrowsableAttribute), true)[0]).Browsable;
                }
                catch
                {
                    return true;
                }
            }
        }
    }
    public static class Helper
    {
        static Helper()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(EditorBase))) {
                    var cons = type.GetConstructor(Type.EmptyTypes);
                    if (cons != null)
                        editors.Add(type, (EditorBase)cons.Invoke(new object[0]));
                }
            }
        }
#if !AONOKISEKI
        public static Encoding Encoding
        {
            get;
            private set;
        }
#else
        public readonly static Encoding Encoding = Encoding.GetEncoding("Shift-JIS");
#endif
        public static EditorBase GetEditorByType(Type type)
        {
            lock (editors)
                if (editors.ContainsKey(type)) return editors[type]; else return null;
        }

        public static T GetEditorByType<T>() where T:EditorBase
        {
            return (T)GetEditorByType(typeof(T));
        }

        public static string GetGenericName(Type type)
        {
            var name = type.AssemblyQualifiedName;
            int x = name.IndexOf('[');
            int y = name.LastIndexOf(']');
            if (x != y) name = name.Remove(x, y - x + 1);
            return name;
        }
        
        static readonly string GenericName = Helper.GetGenericName(typeof(Reference<object, int>));

        public static IEnumerable<EditorBase> GetEditorsOfType(Type type)
        {
            lock (editors)
                foreach (var editor in editors)
                {
                    Type t = editor.Key;
                    while (t.BaseType != typeof(EditorBase))
                        t = t.BaseType;
                    if (t.GetGenericArguments()[0] == type)
                        yield return editor.Value;
                }
        }

        public static IEnumerable<EditorBase<T>> GetEditorsOfType<T>() where T:class
        {
            lock (editors)
                foreach (var editor in editors.Values)
                {
                    var e = editor as EditorBase<T>;
                    if (e != null) yield return e;
                }
        }

        public static IEnumerable<EditorBase> Editors { get { return editors.Values; } }

        public static void Load(CancelEventHandler handler)
        {
            lock (editors)
            {
                if (CheckDirty(handler)) return;
                foreach (var editor in editors.Values)
                {
                    editor.Load();
                    editor.Refresh();
                }
                dirty = false;
            }
        }
        public static bool CheckDirty(CancelEventHandler handler)
        {
            lock (editors)
            {
                if (dirty)
                {
                    var args = new CancelEventArgs();
                    handler(null, args);
                    return args.Cancel;
                }
                else return false;
            }
        }
        public static void MakeDirty()
        {
            lock(editors)
                dirty = true;
        }
        public static void Save()
        {
            lock (editors)
            {
                foreach (var editor in editors.Values) editor.Save();
                dirty = false;
            }
        }
        static bool dirty;
        static Dictionary<Type, EditorBase> editors = new Dictionary<Type, EditorBase>();

        public static bool CheckPath()
        {
            DirectoryInfo info;
            try
            {
                info = new DirectoryInfo(Properties.Settings.Default.ED7Path);
            }
            catch
            {
                return false;
            }
            if (!info.Exists)
                return false;
            var files = info.GetFiles("ED_ZERO.exe", SearchOption.TopDirectoryOnly);
            if (files.Length != 1) return false;
            var file = files[0];
            byte[] hash;
            using (var stream = file.OpenRead())
                hash = new MD5CryptoServiceProvider().ComputeHash(stream);
            switch (BitConverter.ToString(hash).Replace("-", ""))
            {
                case "DA16661AEF931C86645E3B5D41A00B45":
                    Encoding = Encoding.GetEncoding(950);
                    return true;
                case "1E17B6101C3BC6CE779AF192F7F5E8BF":
                    Encoding = Encoding.GetEncoding(936);
                    return true;
                default:
                    return false;
            }
        }

        public static bool SetPath(string p)
        {
            Properties.Settings.Default.ED7Path = p;
            Properties.Settings.Default.Save();
            return CheckPath();
        }
    }

    class ValueTypeConverter : TypeConverter
    {
        class ValueTypePropertyDescriptor : SimplePropertyDescriptor
        {
            public ValueTypePropertyDescriptor(PropertyInfo property, ITypeDescriptorContext context,
                IEnumerable<Attribute> attributes) :
                base(property.ReflectedType, property.Name, property.PropertyType,
                GetAttributes(property, attributes))
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
}
