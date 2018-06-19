using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using wXmlHandlers;

namespace GlobalReporter
{

    public class TextHolder
    {
        String _text;
        public virtual String Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }
        public TextHolder(String text)
        {
            _text = text;
        }

        public virtual TextHolder Clone()
        {
            return new TextHolder(_text);
        }
    }

}
