using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using wXmlHandlers;

namespace GlobalReporter
{


    public class HtmlNode
    {
        static int ID_BASE = 0;

        
        static int BR_HEIGHT = 40;
        static int P_HEIGHT = 80;

        public int ID;
        public bool _canBeAlone = false;
        public bool CanBeAlone{
            get
            {
                if (_specialTags.Contains(Name.ToLower())) return true;
                else return _canBeAlone;
            }
        }
        
        /// <summary>
        /// canbeAlone이 false이면 안에 내용이 없어도 태그 두개로 마무리된다.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="canBeAlone"></param>
        public HtmlNode(String name)
        {
            Name = name;
            ID_BASE++;
            ID = ID_BASE;
            if (name.ToLower().Equals("br")) _canBeAlone = true;
            else if (name.ToLower().Equals("link")) _canBeAlone = true;
            else if (name.ToLower().Equals("p")) _canBeAlone = true;
            else _canBeAlone = false;
        }


        public int GetLinesInNode()
        {
            if (Children.Count == 0)
            {
                return 0;
            }
            else
            {
                int count = 0;
                foreach (HtmlNode node in Children)
                {
                    if (node.Name.ToLower().Equals("br")) count++;
                    else if (node.Name.ToLower().Equals("p")) count += 2;
                }
                return count;
            }
        }



        public virtual String Name { get; set; }
        public Dictionary<String, String> Attributes = new Dictionary<String, String>();

        public HtmlNode GetChildNodeByName(String name)
        {
            if (Children.Count > 0)
            {
                foreach (HtmlNode child in Children)
                {
                    if (child.Name.ToLower().Equals(name.ToLower()))
                    {
                        return child;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }

        }


        public List<HtmlNode> GetChildrenNodeByName(String name)
        {
            List<HtmlNode> nodes = new List<HtmlNode>();
            if (Children.Count > 0)
            {
                foreach (HtmlNode child in Children)
                {
                    if (child.Name.ToLower().Equals(name.ToLower()))
                    {
                        nodes.Add(child);
                    }
                }
            }
            return nodes;
        }

        public HtmlNode GetNodeByContent(string p)
        {
            if (Children.Count > 0)
            {
                HtmlNode selected = null;
                foreach (HtmlNode child in Children)
                {
                    selected = child.GetNodeByContent(p);
                    if (selected != null) break;
                }
                return selected;
            }
            else
            {
                if (InnerText.Contains(p)) return this;
                else return null;
            }
        }

        public List<HtmlNode> GetNodesByContent(string p, List<HtmlNode> nodes = null)
        {
            if (nodes == null) nodes = new List<HtmlNode>();

            if (Children.Count > 0)
            {
                foreach (HtmlNode child in Children)
                {
                    child.GetNodesByContent(p, nodes);
                }
                return nodes;
            }
            else
            {
                if (InnerText.Contains(p))
                {
                    nodes.Add(this);
                }
                return nodes;
            }
        }
        public static void SplitFunctionNodes(HtmlNode orgNode, HtmlNode holderNode, List<HtmlNode> funcNodes)
        {
            foreach (string attr_name in holderNode.Attributes.Keys)
            {
                orgNode.Attributes[attr_name.ToLower()] = holderNode.Attributes[attr_name];
            }
            orgNode.Name = holderNode.Name;

            if (holderNode.Children.Count > 0)
            {

                //List<HtmlNode> newChildren = new List<HtmlNode>();

                foreach (HtmlNode child in holderNode.Children)
                {

                    if (child.Children.Count == 0 && child.InnerText.IndexOf("[%") >= 0)
                    {//tableName이 있으면 text tag를 3개로 나누어 추가.

                        string innerText = child.InnerText.Trim();
                        int end = -1;
                        int beg = innerText.IndexOf("[%");

                        while (beg >= 0)
                        {
                            HtmlNode node = child.Clone();// new HtmlNode(child.Name);
                            node.InnerText = innerText.Substring(end + 1, beg - end - 1).Trim();
                            if (node.InnerText.Length > 0) orgNode.Children.Add(node);
                            end = innerText.IndexOf("]", beg + 1);
                            node = child.Clone();// new HtmlNode(child.Name);
                            node.InnerText = innerText.Substring(beg, end - beg + 1);
                            orgNode.Children.Add(node);
                            funcNodes.Add(node);
                            beg = innerText.IndexOf("[%", end + 1);
                        }

                    }
                    else if (child.Name.ToLower().Equals("table"))
                    {
                        HtmlTable table = new HtmlTable(child);
                        orgNode.Children.Add(table);

                    }
                    else
                    {
                        HtmlNode clone = child.Clone();
                        clone.Children.Clear();
                        SplitFunctionNodes(clone, child, funcNodes);
                        orgNode.Children.Add(clone);

                    }
                }

            }
            else
            {
                orgNode.InnerText = holderNode.InnerText;
            }

        }

        protected List<HtmlNode> _children = new List<HtmlNode>();
        public virtual List<HtmlNode> Children
        {
            get
            {
                return _children;
            }
        }

        public virtual HtmlNode Clone()
        {
            HtmlNode node = new HtmlNode(Name);
            foreach (string attr_name in Attributes.Keys)
            {
                node.Attributes.Add(attr_name.ToLower(), Attributes[attr_name]);
            }
            node.Name = Name;
            if (Children.Count > 0)
            {
                foreach (HtmlNode child in Children)
                {
                    node.Children.Add(child.Clone());
                }
            }
            else
            {
                node._innerText = (_innerText == null) ? null : _innerText.Clone();
            }


            return node;

        }

        public TextHolder GetTextHolder()
        {
            return _innerText;
        }

        internal void SetTextHolder(TextHolder holder)
        {
            _innerText = holder;
        }

        protected TextHolder _innerText = null;
        public String InnerText
        {
            get
            {
                if (Children.Count>0)
                {
                    if (Children.Count > 0)
                    {
                        String txt = "";
                        int i = 0;
                        foreach (HtmlNode child in Children)
                        {
                            i++;
                            if (i < Children.Count)
                            {
                                txt += XmlToHtml(child).Replace("\n", "\n  ");
                            }
                            else
                            {
                                txt += XmlToHtml(child);
                            }


                        }
                        return txt;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    if (_innerText != null)
                    {
                        return _innerText.Text;
                    }
                    else
                    {
                        return "";
                    }
                    
                }
            }
            set
            {
                XmlDocument xDoc = new XmlDocument();
                try
                {
                    String html = "<" + Name;
                    foreach (String attr in Attributes.Keys)
                    {
                        html += " " + attr + "=\"" + Attributes[attr] + "\"";
                    }
                    html += ">" + HtmlToXml(value) + "</" + Name + ">";//html형식으로 가져오기 위해서 
                    xDoc.LoadXml(html);
                    XmlNode node = xDoc.FirstChild;
                    if (node.ChildNodes.Count == 0)
                    {
                        if (_innerText == null) _innerText = new TextHolder(value);
                        else _innerText.Text = value;
                    }
                    else
                    {
                        GetContentFrom(node);
                    }
                    if (Children.Count > 0) _innerText = null;
                }
                catch
                {
                    if (_innerText == null) _innerText = new TextHolder(value);
                    else _innerText.Text = value;
                }
            }
        }

        private string XmlToHtml(HtmlNode child)
        {
            if (child.Name.Equals("text"))
            {
                return child.InnerText;
            }
            if (child.Name.Equals("comment"))
            {
                return "<!--" + child.InnerText + "-->\n";
            }
            else if (_specialTags.Contains(child.Name.ToLower()))
            {
                return "&" + child.Name.ToLower() + ";";
            }
            else
            {
                return child.OuterText;
            }
        }

        public string OuterText
        {
            get
            {
                if (Children.Count == 0 && (_innerText == null || _innerText.Text.Length == 0))
                {
                    String html = "<" + Name;
                    foreach (String attr in Attributes.Keys)
                    {
                        html += " " + attr + "=\"" + Attributes[attr] + "\"";
                    }
                    if (CanBeAlone)
                    {
                        html += "/>\n";
                    }
                    else
                    {
                        html += "></" + Name + ">\n";//두개의 태그로 나뉜다.
                    }

                    return html;
                }
                else
                {
                    string lineFeed = (Children.Count == 0) ? "" : "\n";
                    String html = "<" + Name;
                    foreach (String attr in Attributes.Keys)
                    {
                        html += " " + attr + "=\"" + Attributes[attr] + "\"";
                    }
                    html += ">";
                    if (Children.Count == 0)
                    {
                        if (_innerText != null)
                        {
                            if (_innerText.Text.IndexOf('\n') > 0) //줄바꿈이 하나라도 있으면 보기좋게 하기 위해 태그이후 한줄 띄운다.
                            {
                                html += "\n";
                            }
                            html += _innerText.Text;
                        }
                    }
                    else
                    {
                        html += lineFeed;
                        int i = 0;
                        foreach (HtmlNode node in Children)
                        {
                            i++;
                            if (i < Children.Count)
                            {
                                html += XmlToHtml(node).Replace("\n", "\n  ");// node.OuterText;
                            }
                            else
                            {
                                html += XmlToHtml(node);// node.OuterText;
                            }

                        }

                        //html += lineFeed;
                    }


                    html += "</" + Name + ">\n";//html형식으로 가져오기 위해서 

                    return html;
                }

            }
        }
        public enum TagTypes { Normal, Comment, Info };
        public static List<String> _specialTags = new List<String>();
        public string HtmlToXml(string xml)
        {
            string newStr = "";
            Stack<String> open_tag_stack = new Stack<string>();
            String temp = "";
            xml = xml.Trim();
            int lineIndex = 0;
            int colEnd = 0;

            for (int i = 0; i < xml.Length; i++)
            {
                if (xml[i] == '<') //<발견.
                {

                    i++;
                    //temp = "";
                    bool is_open_tag;
                    TagTypes is_comment;
                    if (xml[i] == '/')
                    {
                        i++;
                        is_open_tag = false;
                        is_comment = TagTypes.Normal;
                    }
                    else
                    {
                        if (xml.Substring(i, 3).Equals("!--"))
                        {
                            is_comment = TagTypes.Comment;
                            is_open_tag = false;
                        }
                        else if (xml[i] == '!')// <!doctype html>과 같은 형식.
                        {
                            is_comment = TagTypes.Info;
                            is_open_tag = false;
                        }
                        else
                        {
                            is_comment = TagTypes.Normal;
                            is_open_tag = true;
                        }

                    }

                    if (temp.Length > 0 && is_open_tag)
                    {
                        if (temp.Trim().Length > 0)
                        {
                            newStr += "<text>" + temp + "</text>\n";
                        }
                        else
                        {
                            newStr += temp;
                        }

                        temp = "";//이전것은 모두 삭제..
                    }
                    else
                    {
                        newStr += temp;
                        temp = "";
                    }
                    string tag = "";
                    string temp_in_tag = "";
                    if (is_comment == TagTypes.Comment)
                    {
                        while (xml.Length > i && (i<5 || xml.Substring(i - 2, 3).Equals("-->") == false))//  -->를 찾음. //마지막 문자는 >가 되며, 저장되지 않음.
                        {
                            temp += xml[i];
                            i++;
                        }
                        newStr += "<" + temp + ">\n";//stack에 넣지도 빼지도 않고 그냥 포함.

                        temp = "";
                    }
                    else if (is_comment == TagTypes.Info)
                    {
                        while (xml.Length > i && xml[i] != '>')//  -->를 찾음. //마지막 문자는 >가 되며, 저장되지 않음.
                        {
                            temp += xml[i];
                            i++;
                        }
                        newStr += "<" + temp + ">\n";//stack에 넣지도 빼지도 않고 그냥 포함.

                        temp = "";
                    }
                    else
                    {
                        while (xml.Length > i && xml[i] != '>')
                        {

                            if (xml[i] != ' ' && tag.Length == 0)
                            {
                                temp_in_tag += xml[i];
                            }
                            else
                            {
                                tag = temp_in_tag;
                            }
                            temp += xml[i];
                            i++;
                        }
                        //i--;//xml[i]=='>'
                        if (tag.Length == 0)
                        {
                            tag = temp_in_tag;
                        }

                        if (i > 0 && xml[i - 1] == '/')//닫는태그..
                        {
                            newStr += "<" + temp + ">\n";//stack에 넣지도 빼지도 않고 그냥 포함.
                            temp = "";
                        }
                        else
                        {

                            if (is_open_tag) //여는태그.
                            {

                                open_tag_stack.Push(tag);
                                newStr += "<" + temp + ">";
                            }
                            else
                            {
                                if (open_tag_stack.Count == 0)
                                {
                                    throw new Exception("wrong html format! on[line(" + lineIndex + "), col(" + (i - colEnd) + ")]\n" + "<" + tag + "> closed without opened...");
                                }
                                string opened_tag = open_tag_stack.Pop();
                                if (tag.Equals(opened_tag) == false)
                                {
                                    throw new Exception("wrong html format! on[line(" + lineIndex + "), col(" + (i - colEnd) + ")]\n" + "<" + tag + "> closed without opened...");
                                }
                                newStr += "</" + temp + ">\n";
                            }
                            temp = "";
                        }
                    }




                }
                else if (xml[i] == '&')
                {
                    i++;
                    string tag = "";
                    while (xml[i] != ';')
                    {
                        tag += xml[i];
                        i++;
                    }
                    if (_specialTags.Contains(tag.ToLower()) == false)
                    {
                        _specialTags.Add(tag);
                    }
                    
                    newStr += "<" + tag + "/>";

                }
                else
                {
                    if (xml[i].Equals("\n")) lineIndex++;
                    colEnd = i - 1;
                    temp += xml[i];
                }
            }
            return newStr;

        }
        public void GetContentFrom(XmlNode xNode)
        {
            Name = xNode.Name;
            Attributes.Clear();
            if (xNode.Attributes != null)
            {
                foreach (XmlAttribute attr in xNode.Attributes)
                {
                    Attributes.Add(attr.Name.ToLower(), attr.Value);
                }
            }

            Children.Clear();
            if (xNode.ChildNodes.Count == 0)
            {
                _innerText = new TextHolder(xNode.InnerText);
            }
            else
            {
                foreach (XmlNode child in xNode.ChildNodes)
                {
                    if (child.Name.ToLower().Equals("#text"))
                    {
                        _innerText = new TextHolder(child.InnerText);
                    }
                    else if (child.Name.ToLower().Equals("#comment"))
                    {
                        _innerText = new TextHolder("<!--" + child.InnerText + "-->");
                    }
                    else
                    {
                        HtmlNode node = new HtmlNode(child.Name);
                        Children.Add(node);
                        node.GetContentFrom(child);
                    }

                }
            }

        }

        public virtual void SetXmlTo(XmlDocument xDoc, XmlNode parentNode)
        {
            //Name은 바깥에서 이미 지정했음.
            foreach (String attr in Attributes.Keys)
            {
                XmlAdder.Attribute(xDoc, attr, Attributes[attr], parentNode);
            }
            if (Children.Count > 0)
            {
                foreach (HtmlNode node in Children)
                {
                    XmlElement ele = XmlAdder.Element(xDoc, node.Name, parentNode);
                    node.SetXmlTo(xDoc, ele);
                }
            }
            else
            {
                parentNode.InnerText = (_innerText != null) ? _innerText.Text : "";
            }
        }

        internal virtual void Clear()
        {
            Children.Clear();
            if (_innerText != null) _innerText = null;
        }

        /// <summary>
        /// [%CONTENT , [%PAGE_NUM 등의 FUNCTION을 가진 node를 가져온다.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal HtmlNode GetNodeByFunction(string function)
        {
            if (_innerText != null && _innerText.Text.Contains(function))
            {
                return this;
            }
            else
            {
                HtmlNode nodeToReturn = null;
                foreach (HtmlNode child in Children)
                {
                    nodeToReturn = child.GetNodeByFunction(function);
                    if (nodeToReturn != null)
                    {
                        break;
                    }
                }
                if (nodeToReturn != null)
                {
                    if (nodeToReturn.Children.Count == 0 && nodeToReturn.Name.ToLower().Equals("text"))//text이고 내부에 function을 가지고 있을 때..
                    {
                        int beg = nodeToReturn.InnerText.IndexOf(function);
                        int end = nodeToReturn.InnerText.IndexOf(']', beg + 1);
                        int index = Children.IndexOf(nodeToReturn);
                        HtmlNode before = new HtmlNode("text");
                        before.InnerText = nodeToReturn.InnerText.Substring(0, beg);
                        HtmlNode newNode = new HtmlNode("text");
                        newNode.InnerText = nodeToReturn.InnerText.Substring(beg, end - beg + 1);
                        HtmlNode after = new HtmlNode("text");
                        after.InnerText = nodeToReturn.InnerText.Substring(end + 1);
                        Children.Insert(index, before);
                        Children.Insert(index + 1, before);
                        Children.Insert(index + 2, before);
                        Children.Remove(nodeToReturn);//대체한다.
                        return newNode;
                    }
                    else return nodeToReturn;
                }
            }
            return null;
        }

        internal List<HtmlNode> GetNodesByFunction(string function, List<HtmlNode> nodeList = null)
        {
            List<HtmlNode> newList = (nodeList == null) ? new List<HtmlNode>() : nodeList;
            if (_innerText != null && _innerText.Text.Contains(function))
            {
                newList.Add(this);
            }
            else
            {
                foreach (HtmlNode child in Children)
                {
                    child.GetNodesByFunction(function, newList);
                }
            }
            return newList;
        }

        /// <summary>
        /// special tag는 태그가 그대로 나간다.
        /// </summary>
        /// <returns></returns>
        internal String GetAllInnerText()
        {
            if (Children.Count > 0)
            {
                string innerText = "";
                foreach (HtmlNode node in Children)
                {
                    innerText += node.GetAllInnerText();
                }
                if (_innerText != null) innerText += _innerText.Text;
                return innerText;
            }
            else
            {
                if (_innerText != null) return _innerText.Text;
                else return "";
            }
        }
        /*
        /// <summary>
        /// 자기 자신 이외에 추가적으로 높이가 추가되는 태그(BR/P등)의 높이를 추가한다.
        /// </summary>
        /// <returns></returns>
        public int GetTagHeight(int defaultHeight=-1)
        {
            if (defaultHeight < 0) defaultHeight = DEFAULT_CELL_HEIGHT;

            int brCount = GetNodesByName("BR").Count;
            int pCount = GetNodesByName("P").Count;
            List<HtmlNode> specialNodes = new List<HtmlNode>();
            int maxHeight = GetHeightStyle(defaultHeight);
            foreach (String nodeName in _specialTags)
            {
                GetNodesByName(nodeName, specialNodes);
            }
            if (brCount == 0 && pCount == 0)
            {
                if (specialNodes.Count > 0)
                {
                    return defaultHeight;
                }
                else
                {
                    if (GetAllInnerText().Trim().Length == 0) return 0;
                    else return defaultHeight;
                }
            }
            else
            {
                int max = brCount * BR_HEIGHT + pCount * P_HEIGHT;
                return brCount * BR_HEIGHT + pCount * P_HEIGHT;
            }
            
        }
        */
        public int GetTagHeight(int defaultHeight, int nowHeight=0)
        {
            if (this.Name.ToLower().Equals("tr"))
            {
                if (this.Children.Count > 0)
                {
                    int max = nowHeight;
                    foreach (HtmlNode td in Children)
                    {
                        if (td.Name.ToLower().Equals("td"))
                        {
                            int hig = td.GetTagHeight(defaultHeight);
                            if(hig==0 && td.GetAllInnerText().Length>0) hig = defaultHeight; //내용이 있는데 hig이 0이라면 br이 내부에 없는 것. 하지만 테이블에서는 크기를 count해야 함.
                            if (max < hig) max = hig;
                        }
                        else
                        {
                            continue;//tr및에는 td가 오는 것이 정상. 다른 태그는 comment나 비정상태그. 그냥 무시할 것.
                        }
                    }
                    return max;
                }
                else
                {
                    //do nothing.. 
                    return 0;//td가 채워지지 않았으므로 defaultSize도 없음.
                }
            }
            else //tr이 아니면 내부의 크기는 모두 합치면 됨.
            {
                int tagHeight = nowHeight;
                if (this.Attributes.ContainsKey("style"))
                {
                    StyleItemCollection styles = new StyleItemCollection(this.Attributes["style"]);
                    if (styles.ContainsKey("height"))
                    {
                        String style = styles["height"].Replace("px", "").Replace("pt", "");
                        int height;
                        if (int.TryParse(style, out height))
                        {
                            tagHeight =  height;
                        }
                        else
                        {
                            tagHeight = defaultHeight;
                        }
                    }
                }
                if (this.Attributes.ContainsKey("height"))
                {
                    int height;
                    if (int.TryParse(this.Attributes["height"], out height))
                    {
                        if (tagHeight < height) tagHeight = height;
                    }
                }
                if (Children.Count > 0)
                {
                    int childHeight = 0;
                    foreach (HtmlNode child in Children)
                    {

                        childHeight = child.GetTagHeight(defaultHeight, childHeight);
                        

                        //childHeight = 0;
                    }
                    if (childHeight > tagHeight) tagHeight = childHeight;
                }
                else
                {

                    if (_specialTags.Contains(this.Name))//special tag면 기본적으로 defaultHeight가 됨.
                    {
                        //tagHeight = nowHeight;// defaultHeight;
                    }
                    else if (this.Name.ToLower().Equals("br"))
                    {
                        tagHeight += BR_HEIGHT;
                    }
                    else if (this.Name.ToLower().Equals("p"))
                    {
                        tagHeight +=  P_HEIGHT;
                    }
                    else
                    {
                       // if(nowHeight>tagHeight) tagHeight = nowHeight;
                        /*
                        if (InnerText.Trim().Length > 0)//공백제외한 일반 text길이가 0보다 크면
                        {
                            if (tagHeight < defaultHeight) tagHeight = defaultHeight;
                        }
                        else
                        {
                            //tagHeight를 수정하지 않는다.
                        }
                         */
                    }
                }
                if (tagHeight > 0 && tagHeight < defaultHeight) tagHeight = defaultHeight;
                if (tagHeight < nowHeight) tagHeight = nowHeight;
                return tagHeight;
                //return (nowHeight>tagHeight)? nowHeight : tagHeight;
            }
            
        }

        public List<HtmlNode> GetNodesByName(string p, List<HtmlNode> nodes=null)
        {
            if (nodes == null) nodes = new List<HtmlNode>();
            if (Children.Count > 0)
            {
                foreach (HtmlNode child in Children)
                {
                    child.GetNodesByName(p, nodes);
                }

            }
            else
            {
                if (Name.ToUpper().Equals(p.ToUpper().Trim())) nodes.Add(this);
            }
            return nodes;
        }


    } 
}
