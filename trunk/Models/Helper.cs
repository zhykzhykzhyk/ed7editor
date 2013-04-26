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
using System.ComponentModel.Design;
using System.Security.Cryptography;
using System.Drawing.Design;
using System.Linq;
using System.Security.Permissions;

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
    public abstract class Component
    {
        public Component(string s = null)
        {
            Type = s;
        }

        public string Type { get; protected set; }

        public abstract void Launch();

        public override string ToString()
        {
            return (Type == null ? "" : Type + ": ") + 
                Name + (ReadOnly ? "(Read-only)" : "");
        }

        public string Name
        {
            get
            {
                Type type = GetType();
                var attributes = type.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                if (attributes.Length != 0)
                    return ((DisplayNameAttribute)attributes[0]).DisplayName;

                var name = type.Name;
                if (Type != null && name.EndsWith(Type))
                {
                    name = name.Substring(0, name.Length - Type.Length);
                }
                return name;
            }
        }

        public bool ReadOnly
        {
            get
            {
                var attributes = GetType().GetCustomAttributes(typeof(ReadOnlyAttribute), true);
                if (attributes.Length == 0)
                    return false;
                return ((ReadOnlyAttribute)attributes[0]).IsReadOnly;
            }
        }

        public bool Browsable
        {
            get
            {
                var attributes = GetType().GetCustomAttributes(typeof(BrowsableAttribute), true);
                if (attributes.Length == 0)
                    return true;
                return ((BrowsableAttribute)attributes[0]).Browsable;
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
    public abstract class EditorBase : Component
    {
        public EditorBase() : base("Editor") { }
        private Form editor;
        public override void Launch()
        {
            lock (this)
            {
                if (editor == null)
                    editor = new Editor(this);
            }
            editor.Hide();
            editor.Show();
        }
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
            if (Environment.GetCommandLineArgs().Contains("--no-elevate"))
                return false;
            using (var process = new Process())
            {
                process.StartInfo.Verb = "runas";
                process.StartInfo.FileName = Application.ExecutablePath;
                process.StartInfo.Arguments = "--no-elevate";
                try
                {
                    return process.Start();
                }
                catch { return false; }
            }
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
                Environment.Exit(1);
                throw;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("资源文件不存在！请下载对应版本的资源文件。");
                Environment.Exit(1);
                throw;
            }
        }
    }
    public abstract class Plugin : Component
    {
        public Plugin() : base("Plugin") { }
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
                if (type.IsSubclassOf(typeof(Plugin)))
                {
                    var cons = type.GetConstructor(Type.EmptyTypes);
                    if (cons != null)
                        plugins.Add(type, (Plugin)cons.Invoke(new object[0]));
                }
            }
            var dir = new DirectoryInfo(Application.StartupPath + @"\plugins");
            if (!dir.Exists) dir.Create();
            foreach (var file in dir.GetFiles("*.dll"))
            {
                try
                {
                    foreach (var type in Assembly.LoadFile(file.FullName).GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(Plugin)))
                        {
                            var cons = type.GetConstructor(Type.EmptyTypes);
                            plugins.Add(type, (Plugin)cons.Invoke(null));
                        }
                    }
                }
                catch (BadImageFormatException e)
                {
                    MessageBox.Show("无法加载文件 " + e.FileName);
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
        public readonly static Encoding Encoding = Encoding.GetEncoding("GB18030");
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

        public static IEnumerable<Plugin> Plugins { get { return plugins.Values; } }

        public static IEnumerable<Component> Components
        {
            get
            {
                foreach (var x in Editors) yield return x;
                foreach (var x in Plugins) yield return x;
            }
        }

        public static Plugin GetPluginByType(Type type)
        {
            return plugins.ContainsKey(type) ? plugins[type] : null;
        }

        public static T GetPluginByType<T>() where T : Plugin
        {
            return (T)GetPluginByType(typeof(T));
        }

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
        static Dictionary<Type, Plugin> plugins = new Dictionary<Type, Plugin>();
        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

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
#if AONOKISEKI
            var files = info.GetFiles("ED_AO.exe", SearchOption.TopDirectoryOnly);
#else
            var files = info.GetFiles("ED_ZERO.exe", SearchOption.TopDirectoryOnly);
#endif
            if (files.Length == 0)
            {
#if !AONOKISEKI
                MessageBox.Show("找不到ED_ZERO.exe");
#endif
                return false;
            }
            var file = files[0];
            byte[] hash;
            using (var stream = file.OpenRead())
                hash = md5.ComputeHash(stream);
            var md5hash = BitConverter.ToString(hash).Replace("-", "");
            switch (md5hash)
            {
#if AONOKISEKI
                case "7E2537C01477BD883A0301AC43563C48":
                    return true;
#else
                case "DA16661AEF931C86645E3B5D41A00B45":
                    Encoding = Encoding.GetEncoding(950);
                    return true;
                case "1E17B6101C3BC6CE779AF192F7F5E8BF":
                    Encoding = Encoding.GetEncoding(936);
                    return true;
#endif
                default:
                    MessageBox.Show("未知版本：" + md5hash);
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

    public class ValueTypeConverter : TypeConverter
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

    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class MultilineStringEditorBase : UITypeEditor
    {
        public MultilineStringEditorBase(string lineBreak)
        {
            this.lineBreak = lineBreak;
        }

        public static object FromString(Type type, string str)
        {
            return GetMethod(type, "op_Implicit", BindingFlags.Public | BindingFlags.Static,
                type, new[] { typeof(string) }).Invoke(null, new[] { str });
        }

        public static MethodInfo GetMethod(Type type, string name, BindingFlags bindingFlags,
            Type returnType, Type[] paramters)
        {
            foreach (var v in from m in type.GetMethods(bindingFlags)
                              where m.Name == name && m.ReturnType == returnType
                              select m)
            {
                if (paramters != null)
                {
                    if (v.GetParameters().Select(p => p.ParameterType).SequenceEqual(paramters))
                        return v;
                }
                else if (!v.GetParameters().Any())
                    return v;
            }
            return null;
        }


        public static string ToString(object obj)
        {
            var type = obj.GetType();
            return (string)GetMethod(type, "op_Implicit", BindingFlags.Public | BindingFlags.Static,
                typeof(string), new[] { type }).Invoke(null, new[] { obj });
            
        }

        string lineBreak;

        private string ConvertToMultiline(string str)
        {
            if (str == null) return null;
            return str.Replace(lineBreak, "\r\n");
        }

        private string ConvertFromMultiline(string str)
        {
            if (str == null) return null;
            return str.Replace("\r\n", lineBreak);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            string str = value as string;
            bool needConvert = str == null;
            if (needConvert) str = ToString(value);
            var before = ConvertToMultiline(str);
            var after = editor.EditValue(context, provider, before);
            if (after.Equals(before)) return value;
            if (needConvert) value = FromString(context.PropertyDescriptor.PropertyType, after as string);
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return editor.GetEditStyle(context);
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return editor.GetPaintValueSupported(context);
        }
        public override void PaintValue(PaintValueEventArgs e)
        {
            editor.PaintValue(e);
        }
        public override bool IsDropDownResizable
        {
            get
            {
                return editor.IsDropDownResizable;
            }
        }
        MultilineStringEditor editor = new MultilineStringEditor();
    }
    public class NormalStringEditor : MultilineStringEditorBase
    {
        public NormalStringEditor() : base(@"\n") { }
    }
    public class CompressedStringEditor : MultilineStringEditorBase
    {
        public CompressedStringEditor() : base("\x1") { }
    }
}
