using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Design;

namespace ED7Editor
{
    public class MyCollectionEditor : CollectionEditor
    {
        public MyCollectionEditor(Type type) : base(type) { }
        void EnumControl<T>(Control control, Action<T> action)
        {
            foreach (var c in control.Controls)
            {
                if (c is T) action((T)c);
                else if (c is Control) EnumControl((Control)c, action);
            }
        }
        protected override CollectionForm CreateCollectionForm()
        {
            CollectionForm collectionForm = base.CreateCollectionForm();
            collectionForm.FormClosed += new FormClosedEventHandler(collection_FormClosed);
            int state = 0;
            collectionForm.Load += (s, e) => state = 1;
            collectionForm.FormClosed += (s, e) => state = 2;
            EnumControl<PropertyGrid>(collectionForm,
                p => p.SelectedObjectsChanged += (s, e) =>
                    new Thread(() =>
                    {
                        lock (collectionForm)
                        {
                            if (state == 0) collectionForm.Load += (a, b) => p.ExpandAllGridItems();
                            else if (state == 1) p.Invoke((ThreadStart)p.ExpandAllGridItems);
                        }
                    }
                        ).Start());
            return collectionForm;
        }

        bool dirty;

        public override object EditValue(ITypeDescriptorContext context,
            IServiceProvider provider, object value)
        {
            if (value == null) return null;
            var v = base.EditValue(context, provider, value);
            if (dirty && v == value) v = value.GetType().GetConstructor(new[] { value.GetType() })
                .Invoke(new[] { value });
            return v;
        }

        void collection_FormClosed(object sender, FormClosedEventArgs e)
        {
            if ((sender as Form).DialogResult == DialogResult.OK)
                dirty = true;
        }
    }
}