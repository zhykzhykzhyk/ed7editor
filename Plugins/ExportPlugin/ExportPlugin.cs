using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using ED7Editor;

namespace ExportPlugin
{
    [DisplayName("导出至改之理")]
    public class ExportPlugin : Plugin
    {
        public override void Launch()
        {
            using (var writer = new StreamWriter(File.Open("全物品代码.txt", FileMode.Create,
                FileAccess.Write, FileShare.None), Encoding.Default))
            {
                foreach (var item in Helper.GetEditorByType<ItemEditor>().GetList())
                    if (item.Index != 0)
                    {
                        var i = (Item)item.Item;
                        writer.WriteLine("{0}\t{1}\t{2}", i.Field.ID, i.Name, i.Description);
                    }
            }
            using (var writer = new StreamWriter(File.Open("人物代码.txt", FileMode.Create,
                FileAccess.Write, FileShare.None), Encoding.Default))
            {
                foreach (var item in Helper.GetEditorByType<CharacterEditor>().GetList())
                {
                    var i = (Character)item.Item;
                    writer.WriteLine("{0}\t{1}", item.Index, i.Name);
                }
            }
        }
    }
}
