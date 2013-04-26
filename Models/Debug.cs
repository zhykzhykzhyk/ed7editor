using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace ED7Editor.Models
{
#if !DEBUG
    [Browsable(false)]
#endif
    public class DebugPlugin : Plugin
    {
        public override void Launch()
        {
            new SoundSelector().ShowDialog();
        }
    }
}
