using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using wXmlHandlers;

namespace GlobalReporter
{
    public class StyleItemCollection : Dictionary<String, String>
    {
        public StyleItemCollection()
            : base()
        {

        }

        public StyleItemCollection(string styleItemsText)
            : base()
        {
            string[] items = styleItemsText.Split(';');
            foreach (String itemPair in items)
            {
                string[] pair = itemPair.Trim().Split(':');
                if (pair.Length > 1)
                {
                    this[pair[0].ToLower().Trim()] = pair[1].ToLower().Trim();//중복시 덮어씌움.
                }
            }
        }

        public new String this[String key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return base[key];
                }
                else
                {
                    return "";
                }
            }
            set
            {
                base[key] = value;
            }
        }
        public String Text
        {
            get
            {
                string txt = "";
                foreach (String key in this.Keys)
                {
                    txt += key + ":" + base[key] + ";\n";
                }
                return txt;
            }
        }
        public String LineText
        {
            get
            {
                string txt = "";
                foreach (String key in this.Keys)
                {
                    txt += key + ":" + base[key] + ";";
                }
                return txt;
            }
        }

    }

}
