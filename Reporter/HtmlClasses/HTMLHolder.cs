using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using wXmlHandlers;

namespace GlobalReporter
{
    public class HtmlHolder : HtmlNode
    {
        public HtmlHolder():base("HTML")
        {

        }
        public override String Name
        {
            get
            {
                return "HTML";
            }
        }

        public void SetXmlTo(XmlDocument xDoc)
        {
            XmlElement root = XmlAdder.Element(xDoc, "HTML");
            //Name은 바깥에서 이미 지정했음.
            foreach (String attr in Attributes.Keys)
            {
                XmlAdder.Attribute(xDoc, attr, Attributes[attr], root);
            }

            if (Children.Count > 0)
            {
                foreach (HtmlNode node in Children)
                {
                    XmlElement ele = XmlAdder.Element(xDoc, node.Name, root);
                    node.SetXmlTo(xDoc, ele);
                }
            }
            else
            {
                root.InnerText = _innerText.Text;
            }
        }

        internal void GetContentFrom(string xml)
        {

            xml = RemoveDocType(xml);
            string newStr = HtmlToXml(xml);
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(newStr);
            GetContentFrom(xDoc.FirstChild);
            HtmlNode head = GetChildNodeByName("head");
            HtmlNode style;
            if (head != null){
                style = head.GetChildNodeByName("style");
                if(style!=null) style.SetTextHolder(new StyleHolder(style.InnerText));//style의 innerText(TextHolder)를 styleHolder로 바꿈.
                else{
                    style = new HtmlNode("style");
                    head.Children.Add(style);
                    style.SetTextHolder(new StyleHolder(""));
                }
            }
            else
            {
                head = new HtmlNode("head");
                Children.Insert(0,head);//body앞에 넣어야 하므로.
                style = new HtmlNode("style");
                head.Children.Add(style);
                style.SetTextHolder(new StyleHolder(""));
            }
        }

        private string RemoveDocType(string newStr)
        {
            newStr = newStr.TrimStart();
            if (newStr.Substring(0, 9).ToUpper().Equals("<!DOCTYPE"))
            {
                int firstEnd = newStr.IndexOf('>');
                newStr = newStr.Substring(firstEnd + 1);
            }
            return newStr;
        }
    }

}
