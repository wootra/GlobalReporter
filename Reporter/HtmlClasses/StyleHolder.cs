using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using wXmlHandlers;

namespace GlobalReporter
{
    public class StyleHolder : TextHolder
    {
        Dictionary<String, StyleItemCollection> _styles = new Dictionary<String, StyleItemCollection>();

        public StyleHolder(String styleText)
            : base(styleText)
        {
            Text = styleText;
        }

        public Dictionary<String, StyleItemCollection> Styles
        {
            get { return _styles; }
        }
        public override TextHolder Clone()
        {
            StyleHolder holder = new StyleHolder(Text);
            return holder;
        }

        public String GetStyleText(String styleName)
        {
            if (_styles.ContainsKey(styleName))
            {
                string txt = "";
                foreach (String itemKey in _styles[styleName].Keys)
                {
                    txt += itemKey + ":" + _styles[styleName][itemKey] + ";\n";
                }
                return txt;
            }
            else return "";
        }

        public StyleItemCollection this[String styleName]
        {
            get
            {
                if (_styles.ContainsKey(styleName)) return _styles[styleName];
                else return new StyleItemCollection();
            }
        }


        public override String Text
        {
            get
            {
                String text = "\n";
                foreach (String name in _styles.Keys)
                {
                    text += name + "\n";
                    text += "{\n";
                    text += _styles[name].Text;
                    text += "}\n";
                }
                return text;
            }
            set
            {
                string text = value.Trim();
                string name;
                int end = -1;
                int beg = text.IndexOf('{');

                string temp = "";
                _styles.Clear();

                while (beg >= 0)
                {
                    try
                    {
                        name = text.Substring(end + 1, beg - end - 1).Trim();
                        end = text.IndexOf('}', beg + 1);
                        temp = text.Substring(beg + 1, end - beg - 1);
                        StyleItemCollection styleItem = GetStyleItemCollection(temp);
                        _styles[name] = styleItem;
                        beg = text.IndexOf('{', end + 1);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }

        }

        public static StyleItemCollection GetStyleItemCollection(string temp)
        {
            StyleItemCollection styleItem = new StyleItemCollection();

            string[] lines = temp.Split(';');


            foreach (String aLine in lines)
            {
                string itemTxt = aLine.Trim();
                string[] itemToken = itemTxt.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (itemToken.Length == 2)
                {
                    styleItem[itemToken[0].Trim().ToLower()] = itemToken[1].Trim();
                }
            }
            return styleItem;
        }
        public void AddStyle(Dictionary<String, StyleItemCollection> stylesToAdd, bool overwrite = true)
        {
            foreach (String key in stylesToAdd.Keys)
            {
                if (_styles.ContainsKey(key))//기존에 있는 style
                {
                    foreach (String itemKey in stylesToAdd[key].Keys)
                    {
                        if (overwrite)
                            _styles[key][itemKey] = stylesToAdd[key][itemKey];//기존에있던 항목은 덮어씌우고 추가할 항목은 추가한다.
                        else
                        {
                            if (_styles[key].ContainsKey(itemKey) == false)
                            {
                                _styles[key][itemKey] = stylesToAdd[key][itemKey];//기존에있던 항목은 무시하고 추가할 항목만 추가한다.
                            }
                            else
                            {
                                //무시.
                            }
                        }
                    }
                }
                else
                {
                    _styles[key] = stylesToAdd[key];
                }
            }
        }
    }

}
