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

namespace ED7Editor
{
    public class IndexedItem
    {
        public int Index { get; set; }
        public object Item { get; set; }
        public override string ToString()
        {
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
        public abstract object GetById(int id);
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
                return (T)Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0), typeof(T));
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
        public static string GetFile(string filename)
        {
            try
            {
                string textPath = Properties.Settings.Default.ED7Path + @"\data\text\";
                if (!File.Exists(textPath + filename))
                {
                    File.Copy(Application.StartupPath + @"\org\" + filename, textPath + filename);
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
                    File.Copy(Application.StartupPath + @"\org\" + filename, textPath + filename);
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
            return GetType().Name;
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
        public readonly static Encoding Encoding = Encoding.GetEncoding(936);
        public static EditorBase GetEditorByType(Type type)
        {
            lock (editors)
                if (editors.ContainsKey(type)) return editors[type]; else return null;
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
    }
}
