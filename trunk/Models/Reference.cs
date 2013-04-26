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
using System.Linq;
using System.Globalization;
using System.Security.Permissions;

namespace ED7Editor
{
    [StructLayout(LayoutKind.Sequential)]
    [Editor(typeof(ReferenceEditor), typeof(UITypeEditor))]
    public class Reference<T, Ti>
        where T : class
        where Ti : struct
    {
        public override string ToString()
        {
            var value = Value;
            if (value != null) return value.ToString();
            else return "（无）";
        }

        [Editor(typeof(ReferenceIDEditor), typeof(UITypeEditor))]
        public Ti ID { get; set; }

        public T Value
        {
            get
            {
                return Helper.GetEditorsOfType<T>().Single().GetById(Convert.ToInt32(ID));
            }
        }
    }
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ReferenceEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context.PropertyDescriptor != null &&
                context.PropertyDescriptor.IsReadOnly)
                return UITypeEditorEditStyle.None;
            return UITypeEditorEditStyle.Modal;
        }

        static readonly string GenericName = Helper.GetGenericName(typeof(Reference<object, int>));

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,
            object value)
        {
            var type = context.PropertyDescriptor.PropertyType;
            var t = type;
            while (Helper.GetGenericName(t) != GenericName)
                t = t.BaseType;
            var types = t.GetGenericArguments();
            using (var selector = new Selector(Helper.GetEditorsOfType(types[0]).Single()))
            {
                var id = Convert.ToInt32(value.GetType().GetProperty("ID").GetValue(value, null));
                selector.SetSelect(id);
                if (selector.ShowDialog() == DialogResult.OK && selector.Result != id)
                {
                    value = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    type.GetProperty("ID")
                        .SetValue(value, Convert.ChangeType(selector.Result, types[1]), null);
                }
                return value;
            }
        }
    }
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class ReferenceIDEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context.PropertyDescriptor.IsReadOnly)
                return UITypeEditorEditStyle.None;
            return UITypeEditorEditStyle.Modal;
        }

        static readonly string GenericName = Helper.GetGenericName(typeof(Reference<object, int>));

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,
            object value)
        {
            var type = context.Instance.GetType();
            while (Helper.GetGenericName(type) != GenericName)
                type = type.BaseType;
            var types = type.GetGenericArguments();
            using (var selector = new Selector(Helper.GetEditorsOfType(types[0]).Single()))
            {
                var id = Convert.ToInt32(value);
                selector.SetSelect(id);
                if (selector.ShowDialog() == DialogResult.OK && selector.Result != id)
                {
                    return Convert.ChangeType(selector.Result, types[1]);
                }
                return value;
            }
        }
    }

    public class ReferenceArrayConverter<T, Ti> : ArrayConverter
        where T:class where Ti : struct
    {
        private class ArrayPropertyDescriptor : TypeConverter.SimplePropertyDescriptor
        {
            private int index;
            public ArrayPropertyDescriptor(Type arrayType, Type elementType, int index)
                : base(arrayType, "[" + index + "]", elementType, null)
            {
                this.index = index;
            }
            public override object GetValue(object instance)
            {
                if (instance is Array)
                {
                    Array array = (Array)instance;
                    if (array.GetLength(0) > this.index)
                    {
                        return new Reference<T, Ti> { ID = (Ti)array.GetValue(this.index) };
                    }
                }
                return null;
            }
            public override void SetValue(object instance, object value)
            {
                if (instance is Array)
                {
                    Array array = (Array)instance;
                    if (array.GetLength(0) > this.index)
                    {
                        array.SetValue((value as Reference<T, Ti>).ID, this.index);
                    }
                    this.OnValueChanged(instance, EventArgs.Empty);
                }
            }
        }
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context,
            object value, Attribute[] attributes)
        {
            PropertyDescriptor[] properties = null;
            if (value.GetType().IsArray)
            {
                int length = ((Array)value).GetLength(0);
                properties = new PropertyDescriptor[length];
                Type arrayType = value.GetType();
                Type elementType = typeof(Reference<T, Ti>);
                for (int i = 0; i < length; i++)
                {
                    properties[i] = new ArrayPropertyDescriptor(arrayType, elementType, i);
                }
            }
            return new PropertyDescriptorCollection(properties);
        }
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    [Editor(typeof(SEReferenceEditor), typeof(UITypeEditor))]
    public struct SEReference
    {
        public override string ToString()
        {
            return ID.ToString();
        }

        public ushort ID { get; set; }
    }

    public class SEReferenceEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context.PropertyDescriptor != null &&
                context.PropertyDescriptor.IsReadOnly)
                return UITypeEditorEditStyle.None;
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider,
            object value)
        {
            var se = (SEReference)value;
            using (var selector = new SoundSelector())
            {
                var id = se.ID;
                selector.SetSelect(id);
                if (selector.ShowDialog() == DialogResult.OK && selector.Result != id)
                {
                    se.ID = id;
                }
                return se;
            }
        }
    }
}
