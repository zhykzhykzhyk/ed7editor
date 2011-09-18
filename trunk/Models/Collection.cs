using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using System.Threading;

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
            collectionForm.Load+=(s,e)=>state=1;
            collectionForm.FormClosed+=(s,e)=>state=2;
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

        void collection_FormClosed(object sender, FormClosedEventArgs e)
        {
            if ((sender as Form).DialogResult == DialogResult.OK)
                Helper.MakeDirty();
        }
    }
}