using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GlobalReporter
{
    public partial class PropertyGetter : Form
    {
        Dictionary<String, String> _more = new Dictionary<String, String>();//추가된 속성.
        //Dictionary<String, String> _morePropertiesToAsk = new Dictionary<String, string>();

        public Dictionary<String, String> Properties
        {
            get
            {
                return _more;
            }
        }
        String _currentPath;
        public PropertyGetter(String currentPath)
        {
            InitializeComponent();
            _currentPath = currentPath;
            this.DialogResult = DialogResult.Cancel;

        }

        public void GetMoreProperties(Dictionary<string,String> propertiesToAsk, Dictionary<String, String> userProperties)
        {
             String fileToRead = _currentPath + "\\Config\\moreProperties.txt";

             foreach (String key in propertiesToAsk.Keys)
            {
                _more[key] = "";
            }

            if (File.Exists(fileToRead))
            {
                String[] lines = File.ReadAllLines(fileToRead);
                
                foreach (String line in lines)
                {
                    String[] tokens = line.Split(":".ToCharArray());
                    if (tokens.Length == 2)
                    {
                        String key = tokens[0].Trim().ToUpper();
                        String val = tokens[1].Trim();
                        if(userProperties.ContainsKey(key)==false) _more[key] = val;
                    }
                }
            }
            int loc = 0;

            foreach (String key in _more.Keys)
            {
                Label label = new Label();
                label.Location = new Point(10, loc * 30 + 10);
                if (propertiesToAsk.ContainsKey(key))
                {
                    label.Text = propertiesToAsk[key];//alias
                }
                else
                {
                    label.Text = key;
                }
                
                label.AutoSize = false;
                label.Size = new Size(100, 20);
                label.TextAlign = ContentAlignment.MiddleRight;
                
                this.Controls.Add(label);
                TextBox text = new TextBox();
                text.Location = new Point(130, loc * 30 + 10);
                text.Size = new Size(300, 20);

                text.Text = _more[key];
                text.Tag = key;
                text.TextChanged += new EventHandler(text_TextChanged);
                text.DoubleClick += new EventHandler(text_DoubleClick);
                text.TabIndex = loc;
                this.Controls.Add(text);
                loc++;  
            }
            Button b = new Button();
            b.Text = "OK";
            b.Click += new EventHandler(b_Click);
            b.Location = new Point(150, loc * 30 + 10);
            b.Size = new Size(100, 30);
            b.TabIndex = loc;
            this.Controls.Add(b);

            this.Size = new Size(450, loc * 30 + 100);
            ShowDialog();
            foreach (String key in Properties.Keys)
            {
                userProperties[key] = Properties[key];
            }
        }

        private Dictionary<String, string> SetPageBaseByUserProperties(HtmlNode pageBase, Dictionary<string, string> userProperties)
        {
            Dictionary<String, string> more = new Dictionary<String, string>();
            GetMoreProperties(pageBase, userProperties, more);
            return more;
        }

        private void GetMoreProperties(HtmlNode node, Dictionary<string, string> userProperties, Dictionary<String, string> more)
        {
            if (node.Children.Count > 0)
            {
                foreach (HtmlNode child in node.Children)
                {
                    GetMoreProperties(child, userProperties, more);
                }
            }
            else
            {
                string inside = node.InnerText.ToUpper();
                int beg = inside.IndexOf('[');
                if (beg == -1) return;
                int end = inside.IndexOf(']', beg);
                int cursor = beg;
                String temp = "";
                while (beg < inside.Length && beg >= 0)
                {
                    end = inside.IndexOf(']', beg);
                    if (end < 0) break;
                    temp = inside.Substring(beg + 1, end - beg - 1).Trim();

                    if (temp.IndexOf('@') == 0)//속성.
                    {
                        temp = temp.Substring(1);
                        string tempTrim = temp.Trim();
                        int aliasBeg = tempTrim.IndexOf(':');
                        string tempName;
                        string tempAlias;
                        if (aliasBeg > 0)
                        {
                            tempName = tempTrim.Substring(0, aliasBeg).ToUpper().Trim();
                            tempAlias = tempTrim.Substring(aliasBeg + 1).Trim();
                        }
                        else
                        {
                            tempName = tempTrim.ToUpper();
                            tempAlias = tempTrim;
                        }

                        bool isChanged = false;
                        foreach (String prop in userProperties.Keys)
                        {
                            string propUpper = prop.Trim().ToUpper();
                            if (tempName.Equals(propUpper))
                            {
                                node.InnerText = userProperties[prop].Trim();//일단 앞뒤공백 자르고
                                if (node.InnerText.IndexOf("\"") == 0 && node.InnerText.LastIndexOf("\"") == node.InnerText.Length - 1)
                                {
                                    node.InnerText = node.InnerText.Substring(1, node.InnerText.Length - 2);//따옴표 안에 있다면 따옴표 삭제.
                                }
                                else if (node.InnerText.IndexOf("'") == 0 && node.InnerText.LastIndexOf("'") == node.InnerText.Length - 1)
                                {
                                    node.InnerText = node.InnerText.Substring(1, node.InnerText.Length - 2);//따옴표 안에 있다면 따옴표 삭제.
                                }
                                isChanged = true;
                                break;
                            }
                        }
                        if (isChanged == false)
                        {
                            //주어진 속성에 없으면
                            if (more.ContainsKey(tempName))
                            {
                                if ((more[tempName].Length == 0 && tempAlias.Length > 0)
                                    ||more[tempName].Equals(tempName))
                                {
                                    more[tempName] = tempAlias;//alias가 이전에는 없었지만 나중에 추가될 때..
                                }
                            }
                            else
                            {
                                more.Add(tempName, tempAlias);//나중에 더 받아와야 될 속성에 추가.
                            }
                            
                        }
                    }
                    beg = inside.IndexOf('[', end);
                }

            }
        }



        void b_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            String txt = "";

            String fileToWrite = _currentPath + "\\Config\\moreProperties.txt";

            foreach (String key in _more.Keys)
            {
                txt += key + ":" + _more[key] + "\n";
            }
            File.WriteAllText(fileToWrite, txt);
            this.Close();
        }

        void text_DoubleClick(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            txt.SelectAll();
        }

        void text_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            string key = (txt).Tag as String;
            _more[key] = txt.Text;
        }

        void T_Content_Click(object sender, EventArgs e)
        {
            TextBox t = sender as TextBox;
            t.SelectAll();
        }



        internal void GetProperties(List<HtmlNode> nodes, Dictionary<string, string> userProperties)
        {
            Dictionary<String, String> morePropertiesToAsk = new Dictionary<String, String>();
            foreach (HtmlNode node in nodes)
            {
                Dictionary<String, String> propsToAsk = SetPageBaseByUserProperties(node, userProperties);
                foreach (String key in propsToAsk.Keys)
                {
                    morePropertiesToAsk[key] = propsToAsk[key];//추가 및 중복처리.
                }
            }
            

            if (morePropertiesToAsk.Count > 0) GetMoreProperties(morePropertiesToAsk, userProperties);
        }
    }
}
