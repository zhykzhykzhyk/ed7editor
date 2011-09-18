using System;
using System.Collections.Generic;
using System.Text;

namespace ED7Editor
{
    public class SelectorItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
