using System;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace ED7Editor
{
    public class MyCollectionEditor : CollectionEditor
    {
        public MyCollectionEditor(Type type) : base(type) { }
        protected override CollectionForm CreateCollectionForm()
        {
            CollectionForm collectionForm = base.CreateCollectionForm();
            collectionForm.FormClosed += new FormClosedEventHandler(collection_FormClosed);
            return collectionForm;
        }

        void collection_FormClosed(object sender, FormClosedEventArgs e)
        {
            if ((sender as Form).DialogResult == DialogResult.OK)
                Helper.MakeDirty();
        }
    }
}