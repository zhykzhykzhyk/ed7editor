using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ED7Editor
{
    public class IndexedItem
    {
        public int Index { get; set; }
        public object Item { get; set; }
        public override string ToString()
        {
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
            Update(this, new EventArgs());
        }
        public event EventHandler Update;
        public abstract void Load();
        public abstract IEnumerable<IndexedItem> GetList();
        public abstract bool Add(int id);
        public abstract bool CopyTo(object src, object dest);
        public abstract bool Remove(int item);
        public abstract void Save();
        public static string ReadString(Stream stream)
        {
            byte b;
            List<byte> a = new List<byte>();
            while ((b = (byte)stream.ReadByte()) != 0) a.Add(b);
            string s = Encoding.Default.GetString(a.ToArray());
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
            byte[] buffer = new byte[Marshal.SizeOf(sct.GetType())];
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
}
