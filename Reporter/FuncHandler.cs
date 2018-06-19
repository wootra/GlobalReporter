using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IOHandling;
using System.IO;
using wXmlHandlers;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;


namespace GlobalReporter
{
    public class FuncHandler
    {
        public static FuncHandler This;
        public FuncHandler()
        {
            if (_currentPath == null)
            {
                _currentPath = Directory.GetCurrentDirectory();//default
            }
            This = this;
        }
        public String _currentPath;
        public int _pageNum = 1;
        int _imgIndex = 0;
        public DateTime _testDate = DateTime.Now;
        public HtmlNode _currentPage = null;
        public List<String> _tableResults = new List<string>();
        public TableHolder _currentTableHolder = null;
        public HtmlNode _currentContent = null;
        public TableHolder _currentTableHolderBase = null;//원본은 변하지 않아야 한다.
        public List<HtmlNode> _bodyContents = new List<HtmlNode>();
        public Dictionary<String, ChartHolder> _chartList = new Dictionary<String, ChartHolder>();
        public int _currentRowHeight;
        public int _pageHeight;
        List<String> _fTableForms = new List<String>();
        public String _pdfPathToSave = "";
        public String _pathToSave = "";
        bool _isNewTableMaking = false;
        TableHolder _baseTableHolder = null;
        public bool _makePdf = false;
        public bool _printNow = false;

        
        public String _dirToSaveHtml = "";
        //public String _currentPath;
        public String _fProperties = "";
        public List<String> _fFilesToParse = new List<String>();
        public Dictionary<String, Encoding> _fEncodingsToParse = new Dictionary<String, Encoding>();

        public String _fbasicForm = "";
        public String _fcoverPage = "";
        
        public int DEFAULT_LINE_HEIGHT = 35;
        public int DEFAULT_CELL_HEIGHT = 26;

        public int _nowHeight = 0;
        public String _cond = "";
        public List<String> _content_results = new List<String>();
        public int _total_pass_cut_line = 7;
        public bool _closeOnStart = false;
        public bool _isLoaded = false;
        int _contentRowIndex = 0;
        int _currentTitleRowIndex = 0;
        int _addIndex = 0;
        public HtmlHolder _basicHtml = null;
       // public HtmlHolder _coverHtml = null;
        public HtmlNode _basicPageBase;
        public HtmlNode _pageBase=null;
        public Dictionary<String, String> _userProperties;

        public Dictionary<String, TableHolder> _tables;

        public HtmlNode _coverPageBase;

        
        public String PdfPathToSave
        {
            get
            {
                return _pdfPathToSave;
            }
        }
        public String PathToSave{
            get
            {
                return _pathToSave;
            }
            set
            {
                DateTime now = DateTime.Now;
                _pathToSave = value;
                _pathToSave = _pathToSave.Replace("%yyyy%", now.ToString("yyyy"));
                _pathToSave = _pathToSave.Replace("%MM%", now.ToString("MM"));
                _pathToSave = _pathToSave.Replace("%dd%", now.ToString("dd"));
                _pathToSave = _pathToSave.Replace("%HH%", now.ToString("HH"));
                _pathToSave = _pathToSave.Replace("%mm%", now.ToString("mm"));
                _pathToSave = _pathToSave.Replace("%ss%", now.ToString("ss"));
                _pathToSave = _pathToSave.Replace("%.fff%", now.ToString(".fff"));

                foreach (String name in _userProperties.Keys)
                {
                    _pathToSave = _pathToSave.Replace("%@" + name + "%", _userProperties[name]);
                }
                if (_pathToSave.Contains("%"))
                {
                    throw new Exception("the format of /dst or config/path_to_save.txt is wrong! :\n" + value);
                }

                _pathToSave = GetRelativePath(_pathToSave, _currentPath);

                string exceptExt = (value.Contains('.')) ? value.Substring(0, value.LastIndexOf('.')) : _pathToSave;
                _pdfPathToSave = exceptExt + ".pdf";
                string pathToCopy = exceptExt + ".html";
                string newPath="";
                for (int i = 0; i < pathToCopy.Length; i++)
                {
                    if ((pathToCopy[i] >= '0' && pathToCopy[i] <= '9')
                        || (pathToCopy[i] >= 'a' && pathToCopy[i] <= 'z')
                        || (pathToCopy[i] >= 'A' && pathToCopy[i] <= 'Z')
                        || (pathToCopy[i] == '\\' || pathToCopy[i] == ':' || pathToCopy[i] == '.' || pathToCopy[i] == '_' || pathToCopy[i] == '-'))
                    {
                        newPath += pathToCopy[i];
                    }
                    else
                    {
                        newPath += ('0' + pathToCopy[i] % 10);
                    }
                }
                String tempPath = GetTempPath();

                newPath = tempPath + newPath.Substring(2);//c:를 없앰.
                String newDir = Path.GetDirectoryName(newPath);
                
                
                
                if (Directory.Exists(newDir))
                {
                    Directory.Delete(newDir, true);//기존에 있던 것 지움..
                    Thread.Sleep(3000);
                }
                Directory.CreateDirectory(newDir);
                _pathToSave = newPath;
                try
                {
                    _dirToSaveHtml = newDir;
                    if(Directory.Exists(_dirToSaveHtml)==false) Directory.CreateDirectory(_dirToSaveHtml);
                    String dirToSavePdf = _pdfPathToSave.Substring(0, _pdfPathToSave.LastIndexOf('\\'));
                    if (Directory.Exists(dirToSavePdf) == false) Directory.CreateDirectory(dirToSavePdf);
                    File.WriteAllText(_pdfPathToSave, "");
                    File.Delete(_pdfPathToSave);
                }
                catch (Exception ex)
                {
                    throw new Exception("the file format of /dst or config/path_to_save.txt is wrong! :\n" + PathToSave);
                }

                
            }
        }
        


        




        public int GetCount(string src, IEnumerable<string> txtsToFind)
        {
            int count = 0;
            int index;
            foreach (String txtToFind in txtsToFind)
            {
                index = src.IndexOf(txtToFind);
                if (index >= 0) count++;
                while (index > 0)
                {
                    index = src.IndexOf(txtToFind, index + 1);
                    if (index > 0) count++;
                }
            }
            return count;
        }

        public void SetPropertiesOnNode(HtmlNode node)
        {
            if (node == null) return;
            Dictionary<String, String> newAttr = new Dictionary<String, String>();
            foreach (String key in node.Attributes.Keys)
            {
                string value;
                bool? ret = TextReplaced(node.Attributes[key], null, out value);
                if (ret != false) newAttr[key] = value;
            }
            if (newAttr.Count > 0)
            {
                foreach (String key in newAttr.Keys)
                {
                    node.Attributes[key] = newAttr[key];
                }
            }
            if (node.Children.Count == 0)
            {
                ReplaceFunctions(node);
            }
            else
            {
                foreach (HtmlNode child in node.Children)
                {
                    SetPropertiesOnNode(child);
                }
            }
        }

        public int GetHeightFromContentFunc(HtmlNode currentContent)
        {

            String contentFunc = currentContent.InnerText.Trim();
            int end = contentFunc.IndexOf(']');
            contentFunc = contentFunc.Substring(2, end - 2);
            string[] tokens = contentFunc.Split(':');
            string heightTxt = tokens[1];
            string newHeight;
            bool? ret = TextReplaced("["+heightTxt+"]", null, out newHeight);
            if (ret != false) heightTxt = newHeight;
            int height;
            if (tokens.Length != 2 || int.TryParse(heightTxt, out height) == false)
            {
                throw new Exception("[%CONTENT:height] is needed in basic form...");
            }
            return height;
        }

        void ReplaceFunctions(HtmlNode node)
        {
            IsCellReplaced(node, null);
        }
        /// <summary>
        /// 기능이나 user-property로 대체되었으면 null리턴.
        /// txtToReplace로 대체되었으면 true리턴.
        /// 대체되지 않았으면 false리턴.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="txtToReplace"></param>
        /// <returns></returns>
        public bool? IsCellReplaced(HtmlNode cell, string txtToReplace = "")
        {
            String newText;
            bool? ret = false;
            bool? tempRet=false;
            Dictionary<String, String> attrToModify = new Dictionary<String, String>();
            foreach (String attr in cell.Attributes.Keys)
            {
             
                tempRet = TextReplaced(cell.Attributes[attr], txtToReplace, out newText);
                if (tempRet != false)
                {
                    attrToModify[attr] = newText;
                }
                
                //cell.Attributes[attr] = newText;
                if (tempRet==true) ret = true;
                else if (ret != true && tempRet == null) ret = tempRet;
            }
            
            foreach (String key in attrToModify.Keys)
            {
                cell.Attributes[key] = attrToModify[key];
            }
            if (cell.Children.Count > 0)
            {
                bool? childRet=false;
                foreach(HtmlNode child in cell.Children){
                    childRet = IsCellReplaced(child, txtToReplace);
                    if (tempRet != true && childRet != false) tempRet = childRet;
                }
            }
            else
            {
                string cellTxt = cell.InnerText.Trim();
                tempRet = TextReplaced(cellTxt, txtToReplace, out newText);
                cell.InnerText = newText;
            }
            
            if (ret != true && tempRet != false) ret = tempRet;
            //string test = cell.InnerText;
            return ret;

        }

        public bool? TextReplaced(string cellTxt, string txtToReplace, out string newText)
        {
            int end = -1;
            int beg = cellTxt.IndexOf('[');
            string temp;
            bool? ret = false;

            cellTxt = cellTxt.Replace("%c", ",");
            cellTxt = cellTxt.Replace("%s", "&nbsp;");
            if (beg >= 0)
            {
                newText = cellTxt.Substring(0, beg);//[이전까지 삽입.
            }
            else
            {
                newText = cellTxt;
            }


            while (beg >= 0)
            {

                end = cellTxt.IndexOf(']', beg + 1);
                if (end < 0) end = cellTxt.Length - 1;
                try
                {
                    temp = cellTxt.Substring(beg + 1, end - beg - 1);
                }
                catch
                {
                    throw;
                }
                

                if (temp.IndexOf("%") == 0)//기능
                {
                    if (temp.ToUpper().Equals("%BLANK"))
                    {
                        if (txtToReplace == null)
                        {
                            newText += "[" + temp + "]";//그대로 추가. 변경안함.
                        }
                        else
                        {
                            newText += txtToReplace;
                            if (ret != true) ret = true;
                        }

                    }
                    else if (temp.ToUpper().Equals("%H"))
                    {

                        newText += "";// "[" + temp + "]";//그대로 추가. 변경안함.
                        

                    }
                    else if (temp.ToUpper().Equals("%CURRENT_DIR"))
                    {
                        newText += "FILE:///" + _currentPath.Replace("\\", "/");
                        if (ret != true) ret = null;
                    }
                    else if (temp.ToUpper().Equals("%COND"))
                    {
                        if (txtToReplace == null)
                        {
                            newText += "[" + temp + "]";//그대로 추가. 변경안함.
                        }
                        else
                        {
                            newText += txtToReplace;
                            _cond = txtToReplace;//condition일 때는 일단 저장한다.
                            if (ret != true) ret = true;
                        }

                    }
                    else if (temp.ToUpper().IndexOf("%IMG") == 0)
                    {
                        if (txtToReplace == null)
                        {
                            newText += "[" + temp + "]";//그대로 추가. 변경안함.
                        }
                        else
                        {
                            HtmlNode imgNode = new HtmlNode("img");
                            int optBeg = temp.IndexOf(":");
                            if (optBeg > 0)
                            {
                                string opt = temp.Substring(optBeg + 1).Trim();

                                string[] tokens = opt.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);//img의 option은 공백으로 나눔.

                                if (tokens.Length > 0) imgNode.Attributes["width"] = tokens[0];//첫번째는 width
                                if (tokens.Length > 1)
                                {
                                    imgNode.Attributes["height"] = tokens[1];//첫번째는 height.
                                    int height = int.Parse(tokens[1]);
                                    if (height > _currentRowHeight)
                                    {
                                        _currentRowHeight = height;
                                    }
                                }
                            }
                            String imgFile = GetRelativePath(txtToReplace, _currentPath);
                           
                            if (File.Exists(imgFile))
                            {
                                 String ext = "";
                                if(txtToReplace.LastIndexOf('.')>=0){//확장자 있을 때 확장자 저장.(.포함)
                                    ext = txtToReplace.Substring(txtToReplace.LastIndexOf('.'));
                                }

                                String fileName = GetOnlyFileName(PathToSave);
                                String newFile = _dirToSaveHtml+"\\images\\"+fileName +"_"+ (_imgIndex) +ext; //새 경로로 이동.
                                if(Directory.Exists(_dirToSaveHtml+"\\images")==false){
                                    Directory.CreateDirectory(_dirToSaveHtml+"\\images");//image폴더 없으면 생성.
                                }
                                File.Copy(imgFile, newFile);
                                imgNode.Attributes["src"] = "images\\"+fileName + "_" + (_imgIndex) + ext;//url..
                            }
                            else imgNode.Attributes["src"] = txtToReplace;//url..
                            newText += imgNode.OuterText;
                            ret = true;
                            _imgIndex++;//이미지의 번호를 증가하면서 이름을 replace함.
                        }
                    }
                    else if (temp.ToUpper().Trim().IndexOf("%CHARTLIST") == 0)
                    {
                        foreach (ChartHolder chart in _chartList.Values)
                        {
                            newText += chart.Text;

                        }

                    }
                    else if (temp.ToUpper().IndexOf("%CHART") == 0)
                    {
                        if (txtToReplace == null)
                        {
                            newText += "[" + temp + "]";//그대로 추가. 변경안함.
                        }
                        else
                        {
                            HtmlNode imgNode = new HtmlNode("div");
                            int optBeg = temp.IndexOf(":");
                            if (optBeg > 0)
                            {
                                string opt = temp.Substring(optBeg + 1).Trim();

                                string[] tokens = opt.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);//img의 option은 공백으로 나눔.
                                string style = "";
                                if (tokens.Length > 0)
                                {
                                    imgNode.Attributes["width"] = tokens[0];//첫번째는 width
                                    style += "width:" + tokens[0] + "px;";
                                }
                                if (tokens.Length > 1)
                                {
                                    imgNode.Attributes["height"] = tokens[1];//첫번째는 height.
                                    int height = int.Parse(tokens[1]);
                                    if (height > _currentRowHeight)
                                    {
                                        _currentRowHeight = height;
                                    }
                                    style += "height:" + tokens[1] + "px;";
                                }
                                if (style.Length > 0)
                                {
                                    imgNode.Attributes["style"] = style;
                                }

                            }
                            imgNode.Attributes["id"] = txtToReplace;//url..

                            //imgNode.Children.Add(new HtmlNode("br"));//내부에 br넣음.
                            newText += imgNode.OuterText;
                            ret = true;
                        }
                    }

                    else if (temp.ToUpper().IndexOf("%RESULT") == 0)
                    {
                        if (txtToReplace == null)
                        {
                            newText += "[" + temp + "]";//그대로 추가. 변경안함.
                        }
                        else
                        {
                            int opbeg = temp.IndexOf(":");
                            string option = "";
                            if (opbeg >= 0) option = temp.Substring(opbeg + 1);
                            newText += GetResultNodeTextByOption(txtToReplace, option);
                            if (ret != true) ret = true;
                        }
                    }
                    else if (temp.ToUpper().IndexOf("%TOTAL_RESULT") == 0)
                    {
                        int opbeg = temp.IndexOf(":");
                        string passText = "PASS";
                        string failText = "FAIL";
                        string condition = "";
                        if (opbeg >= 0)
                        {
                            temp = temp.Substring(opbeg + 1).Trim();
                            int txtBeg = temp.IndexOf(",");
                            if (txtBeg > 0)
                            {
                                condition = temp.Substring(0, txtBeg);
                                String passFail = temp.Substring(txtBeg + 1);
                                txtBeg = passFail.IndexOf('/');
                                if (txtBeg < 0)
                                {
                                    throw new Exception(@"condition form of [%TOTAL_RESULT] should be 
[%TOTAL_RESULT:ARG1{ == | <= | >= | < | > | != }ARG2,PASS_TEXT/FAIL_TEXT]
or
[%TOTAL_RESULT:,PASS_TEXT/FAIL_TEXT]
NOTE: YOU CANNOT USE MULTIPLE CONDITION IN THIS VERSION...

ex>[%TOTAL_RESULT:%NUM_OF_TABLE_RESULT==%NUM_OF_PASS_RESULT,PASS/FAIL]
ex>[%TOTAL_RESULT:%NUM_OF_TABLE_RESULT>%NUM_OF_FAIL_RESULT,성공/실패]
ex>[%TOTAL_RESULT:%NUM_OF_PASS_RESULT>0,성공/실패]

");
                                }
                                else
                                {
                                    passText = passFail.Substring(0, txtBeg);
                                    failText = passFail.Substring(txtBeg + 1);
                                }
                            }
                            else
                            {
                                condition = temp;
                            }
                        }
                        else
                        {
                            condition = "";
                        }

                        newText += GetTotalResultByCondition(condition, passText, failText);
                        if (ret != true) ret = true;

                    }
                    else if (temp.ToUpper().Equals("%PAGE_NUM"))
                    {
                        newText += _pageNum;//page가 추가될때마다 +1
                        if (ret != true) ret = null;
                    }
                    else if (temp.ToUpper().Trim().IndexOf("%INDEX")==0)
                    {
                        string indexCmd = temp.ToUpper().Trim();
                        int braBeg = temp.IndexOf('(', 5);
                        if (braBeg > 0)
                        {
                            int braEnd = temp.IndexOf(')', braBeg + 1);
                            if (braEnd < 0)
                            {
                                throw new Exception("wrong format! use %INDEX(n) or just use %INDEX");
                            }
                            string numStr = temp.Substring(braBeg + 1, braEnd - braBeg - 1);
                            if(int.TryParse(numStr.Trim(), out _addIndex)==false){
                                throw new Exception("wrong format! use %INDEX(n) or just use %INDEX");
                            }
                            
                        }
                        newText += _addIndex;//page가 추가될때마다 +1
                        if (ret != true) ret = null;
                    }
                    else if (temp.ToUpper().Equals("%NUM_OF_TABLE_RESULT"))
                    {
                        newText += GetNumOfTableResult();//page가 추가될때마다 +1
                        if (ret != true) ret = null;
                    }

                    else if (temp.ToUpper().Equals("%NUM_OF_PASS_RESULT"))
                    {
                        newText += GetNumOfPassResult();//page가 추가될때마다 +1
                        if (ret != true) ret = null;
                    }
                    else if (temp.ToUpper().Equals("%NUM_OF_FAIL_RESULT"))
                    {
                        newText += GetNumOfFailResult();//page가 추가될때마다 +1
                        if (ret != true) ret = null;
                    }
                    else if (temp.ToUpper().IndexOf("%TEST_DATE") == 0)
                    {
                        int optBeg = temp.IndexOf(':');
                        if (optBeg < 0)
                        {
                            newText += _testDate.ToString("yyyy-MM-dd HH:mm:ss");

                        }
                        else
                        {
                            string format = temp.Substring(optBeg + 1).Trim();
                            newText += _testDate.ToString(format);
                        }
                        if (ret != true) ret = null;

                    }
                    else if (temp.ToUpper().IndexOf("%CONTENT") == 0)
                    {
                        if (txtToReplace == null)
                        {
                            newText += "[" + temp + "]";//그대로 추가. 변경안함.
                        }
                        else
                        {
                            newText += txtToReplace;
                            if (ret != true) ret = true;
                        }
                    }
                    else
                    {
                        throw new Exception("the option [" + temp + "] doesn't support in this version...");
                    }

                }
                else if (temp.IndexOf("@") == 0)//기능
                {
                    //end = cellTxt.IndexOf(']');

                    string prop = temp.Substring(1);
                    int aliasBeg = prop.IndexOf(':');
                    if (aliasBeg >= 0)
                    {
                        prop = prop.Substring(0, aliasBeg).Trim();
                    }
                    if (_userProperties.ContainsKey(prop))
                    {
                        newText += _userProperties[prop];
                        if (ret != true) ret = null;
                    }
                    
                }
                else
                {
                    newText += "[" + temp + "]";//그대로 추가. 변경안함.

                    if (ret != true) ret = false;
                }

                beg = cellTxt.IndexOf('[', end + 1);
                if (beg >= 0)
                {
                    newText += cellTxt.Substring(end + 1, beg - end - 1);
                }
                else
                {
                    newText += cellTxt.Substring(end + 1);
                }

            }
            return ret;

        }

        public static string GetOnlyFileName(string fileName, bool withExt=true)
        {
            fileName = fileName.Replace("/", "\\");
            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            if (withExt)
            {
                return fileName;
            }
            else return fileName.Substring(0, fileName.LastIndexOf('.'));//ext뺀 순수 파일명.

        }
        public static string GetWinDrive()
        {
            String winDir = Environment.GetEnvironmentVariable("Windir").Replace("/", "\\");
            String winDrive = winDir.Substring(0, winDir.IndexOf('\\'));
            return winDrive;
        }

        public static string GetTempPath()
        {
            return Environment.GetEnvironmentVariable("Temp");
            
        }

        public int GetNumOfFailResult()
        {
            int count = 0;
            foreach (String result in _tableResults)
            {
                if (GetResultByOptions(result, "") == false) count++;
            }
            return count;
        }

        public int GetNumOfPassResult()
        {
            int count = 0;
            foreach (String result in _tableResults)
            {
                if (GetResultByOptions(result, "")) count++;
            }
            return count;
        }

        public int GetNumOfTableResult()
        {
            return _tableResults.Count;
        }

        public enum modes { name, eq, etc };
        public class CondToken
        {
            public modes Mode;
            public String Token;
            public CondToken(modes mode, String token)
            {
                Mode = mode;
                Token = token;
            }
        }
        public string GetTotalResultByCondition(string condition, String passText, String failText)
        {
            if (condition.Length == 0)
            {
                if (GetNumOfPassResult() == GetNumOfTableResult()) return passText;
                else return failText;
            }
            else
            {
                condition = condition.Trim();
                string etc = "";
                string name = "";
                string eq = "";

                modes mode = modes.etc;
                modes oldmode = modes.etc;
                List<CondToken> tokens = new List<CondToken>();

                for (int i = 0; i < condition.Length; i++)
                {
                    if (condition[i] == '%')
                    {
                        mode = modes.name;
                    }
                    else if (condition[i] == '=' || condition[i] == '<' || condition[i] == '>')
                    {
                        mode = modes.eq;
                    }
                    else if (condition[i] == ' ' || condition[i] == '\t' || condition[i] == '\n' || condition[i] == '\r')//mode가 eq였다가 eq도 아니고 cond도 아닌 다른 것이 올 때..
                    {
                        continue;//공백은 무시.
                    }
                    else if (mode == modes.eq)//mode가 eq였다가 eq도 아니고 cond도 아니고 공백도 아닌 다른 것이 올 때..
                    {
                        mode = modes.etc;
                    }

                    if (mode == modes.etc)
                    {
                        etc += condition[i];
                    }
                    else if (mode == modes.name)
                    {
                        name += condition[i];
                    }
                    else if (mode == modes.eq)
                    {
                        eq += condition[i];
                    }

                    if (i != 0 && mode != oldmode)
                    {
                        if (oldmode == modes.name)
                        {
                            tokens.Add(new CondToken(oldmode, name));
                            name = "";
                        }
                        else if (oldmode == modes.etc)
                        {
                            tokens.Add(new CondToken(oldmode, etc));
                            etc = "";
                        }
                        else if (oldmode == modes.eq)
                        {
                            tokens.Add(new CondToken(oldmode, eq));
                            eq = "";
                        }
                    }

                    oldmode = mode;
                }
                if (oldmode == modes.name)
                {
                    tokens.Add(new CondToken(oldmode, name));
                    name = "";
                }
                else if (oldmode == modes.etc)
                {
                    tokens.Add(new CondToken(oldmode, etc));
                    etc = "";
                }
                else if (oldmode == modes.eq)
                {
                    tokens.Add(new CondToken(oldmode, eq));
                    eq = "";
                }

                if (tokens.Count != 3)
                {
                    throw new Exception(@"condition form of [%TOTAL_RESULT] should be 
[%TOTAL_RESULT:ARG1{ == | <= | >= | < | > | != }ARG2,PASS_TEXT/FAIL_TEXT]
or
[%TOTAL_RESULT:,PASS_TEXT/FAIL_TEXT]
NOTE: YOU CANNOT USE MULTIPLE CONDITION IN THIS VERSION...

ex>[%TOTAL_RESULT:%NUM_OF_TABLE_RESULT==%NUM_OF_PASS_RESULT,PASS/FAIL]
ex>[%TOTAL_RESULT:%NUM_OF_TABLE_RESULT>%NUM_OF_FAIL_RESULT,성공/실패]
ex>[%TOTAL_RESULT:%NUM_OF_PASS_RESULT>0,성공/실패]

");
                }

                foreach (CondToken token in tokens)
                {
                    if (token.Mode == modes.name)
                    {
                        HtmlNode testNode = new HtmlNode("text");
                        testNode.InnerText = "[" + token.Token + "]";
                        ReplaceFunctions(testNode);//예약어를 모두 바꿈.
                        token.Token = testNode.InnerText;
                    }
                }
                bool result = false;
                try
                {
                    double d1 = double.Parse(tokens[0].Token);
                    double d2 = double.Parse(tokens[2].Token);

                    switch (tokens[1].Token)
                    {
                        case "==":
                            result = d1 == d2;
                            break;
                        case "!=":
                            result = d1 != d2;
                            break;
                        case "<=":
                            result = d1 <= d2;
                            break;
                        case ">=":
                            result = d1 >= d2;
                            break;
                        case "<":
                            result = d1 < d2;
                            break;
                        case ">":
                            result = d1 > d2;
                            break;
                        default:
                            throw new Exception("wrong equation!");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("wrong condition! - " + condition + "\n" + ex.Message);
                }

                if (result) return "<span class='PASS'>" + passText + "</span>";
                else return "<span class='FAIL'>" + failText + "</span>";
            }
        }

        public string GetResultNodeTextByOption(string txtToReplace, string option)
        {
            bool result = GetResultByOptions(txtToReplace, option);
            HtmlNode node = new HtmlNode("span");
            node.InnerText = txtToReplace;
            node.Attributes["class"] = (result == true) ? "PASS" : "FAIL";

            return node.OuterText;
        }

        public bool GetResultByOptions(string txtToReplace, string option)
        {
            List<String> options;
            if (option.Length > 0) options = option.Split(',').ToList();
            else options = new List<string>();

            List<String> newOpt = new List<String>();
            foreach (String opt in options)
            {
                String upperOpt = opt.ToUpper();

                if (newOpt.Contains(upperOpt) == false) newOpt.Add(upperOpt);
            }

            options = newOpt;

            if (options.Contains("PASS") == false) options.Add("PASS");
            if (options.Contains("OK") == false) options.Add("OK");
            if (options.Contains("성공") == false) options.Add("성공");
            if (options.Contains("정상") == false) options.Add("정상");

            bool result = false;

            String value = txtToReplace.ToUpper().Trim();

            foreach (String opt in options)
            {
                if (opt.IndexOf("%COND") == 0)
                {
                    int sizeBeg = opt.IndexOf('(');
                    int sizeEnd = opt.IndexOf(')');
                    String sizeToken;
                    if (sizeBeg >= 0)
                    {
                        sizeToken = opt.Substring(sizeBeg + 1, sizeEnd - sizeBeg).Trim();
                    }
                    else
                    {
                        sizeToken = "";
                    }

                    if (sizeToken.IndexOf('.') == 0)//소숫점 다음부터 크기 비교..
                    {
                        int limitSize;
                        if (sizeToken.Length == 1)
                        {
                            limitSize = 0;//소숫점 시작되면 끝.
                        }
                        else if (int.TryParse(sizeToken.Substring(1), out limitSize) == false)
                        {
                            throw new Exception(@"format of %COND is wrong! 
EX>%COND(.3) <-- COMPARE UNTIL 3rd position after point(ONLY NUMBER).
EX>%COND(5) <-- COMPARE 5 positions of whole string.
EX>%COND(.) <-- FINISH COMPARING WHEN MEET POINT OR END ON END OF NUMBER(ONLY NUMBER)");
                        }


                        string newCond = _cond.Trim('0');//앞뒤의 0을 삭제.
                        string newVal = value.Trim('0');

                        int valPtPos = newVal.IndexOf('.');//.의 위치를 가져옴.
                        int condPtPos = newCond.IndexOf('.');
                        if (valPtPos == newVal.Length - 1)
                        {
                            valPtPos = -1;
                            newVal = newVal.Substring(0, newVal.Length - 1);//뒤의 .을 빼냄.
                        }
                        if (condPtPos == newCond.Length - 1)
                        {
                            condPtPos = -1;
                            newCond = newCond.Substring(0, newCond.Length - 1);//뒤의 .을 빼냄.
                        }

                        if (valPtPos != condPtPos)
                        {
                            continue;//이 조건과는 맞지 않음.
                        }

                        if (valPtPos >= 0 && newVal.Length < valPtPos + limitSize + 1)
                        {
                            int sizeToBe = valPtPos + limitSize + 1;
                            for (int c = newVal.Length; c <= sizeToBe; c++)
                            {
                                newVal += '0';//단위에 맞게 0을 붙임.
                            }
                        }

                        if (condPtPos >= 0 && newCond.Length < condPtPos + limitSize + 1)
                        {
                            int sizeToBe = condPtPos + limitSize + 1;
                            for (int c = newCond.Length; c <= sizeToBe; c++)
                            {
                                newCond += '0';//단위에 맞게 0을 붙임.
                            }
                        }

                        if (newCond.Equals(newVal)) result = true;
                    }
                    else if (sizeToken.Length == 0)
                    {
                        if (_cond.Equals(value))
                        {
                            result = true;
                        }

                    }
                    else
                    {
                        int limitSize;
                        if (int.TryParse(sizeToken, out limitSize) == false)
                        {
                            throw new Exception(@"format of %COND is wrong! 
EX>%COND(.3) <-- COMPARE UNTIL 3rd position after point(ONLY NUMBER).
EX>%COND(5) <-- COMPARE 5 positions of whole string.
EX>%COND(.) <-- FINISH COMPARING WHEN MEET POINT OR END ON END OF NUMBER(ONLY NUMBER)");
                        }

                        if (_cond.Length >= limitSize || value.Length >= limitSize)//size가 적으면 아예 비교불가능.
                        {
                            bool tempResult = true;
                            for (int c = 0; c < limitSize; c++)
                            {
                                if (_cond[c] != value[c])
                                {
                                    tempResult = false;
                                    break;//다른게 하나라도 있을 경우 실패..
                                }
                            }
                            if (tempResult) result = tempResult;
                        }
                        else
                        {
                            throw new Exception("limit size of condition is " + limitSize + " but condition(" + _cond + ") size is " + _cond.Length + "and value(" + value + ") size is" + value.Length);
                        }

                    }

                }
                else if (opt.Equals(value))
                {
                    result = true;
                    break;
                }
            }
            return result;

        }


        internal void EndTable()
        {

            if (_currentTableHolderBase != null)
            {

                if (_currentTableHolder!=null && _currentTableHolder.Table != null && _currentTableHolder.Table.CellRows.Count == 0)
                {
                    _currentContent.Children.Remove(_currentTableHolder);
                }
                else
                {
                    TableHolder tempHolder = _currentTableHolderBase.Clone() as TableHolder;
                    tempHolder.RemoveTopOfTable();
                    tempHolder.Children.Remove(tempHolder.Table);

                    int height = tempHolder.GetTagHeight(DEFAULT_LINE_HEIGHT);
                    //int count = _currentTableHolderBase.GetLineCountAfterTable();
                    //int count = GetCount(lineText.InnerText, new String[] { "<br", "<p" });
                    AddPageHeight(height);
                
                }
                
            }
            _currentTableHolderBase = null;
            _currentTableHolder = null;
        }

        internal bool Init()
        {
            
            if (_fbasicForm.Length == 0)//argument로 미리 지정하지 않았다.
            {
                _fbasicForm = _currentPath + "\\Config\\" + "Report_form_basic.html";
            }
            if (_fcoverPage.Length == 0)//argument로 미리 지정하지 않았다.
            {
                _fcoverPage = _currentPath + "\\Config\\" + "cover_page.html";
                if (File.Exists(_fcoverPage) == false)
                {
                    _fcoverPage = "";
                }
            }

            if (_fProperties.Length == 0) 
                _fProperties = _currentPath + "\\Config\\Properties.txt";

            GetUserProperties(_fProperties);

            PropertyGetter propGet = new PropertyGetter(_currentPath);
            _tables = GetTables();//report_forms 디렉토리에서 모든 html파일을 가져와서 읽어온다. (또는 forms=form1.html,form2.html,...)
            List<HtmlNode> nodesToAskProps = new List<HtmlNode>();
            foreach (TableHolder holder in _tables.Values)
            {
                nodesToAskProps.Add(holder);
            }
           
            _basicHtml = GetBodyFromBasicReportForm(_fbasicForm);
            
            bool ret=false;
            if (_basicHtml != null)
            {
                _basicPageBase = _basicHtml.GetChildNodeByName("body").Clone();
                nodesToAskProps.Add(_basicHtml);
                ret = true;
            }

            if (_fcoverPage.Length != 0)
            {
                HtmlHolder coverHtml = GetBodyFromBasicReportForm(_fcoverPage);

                if (coverHtml != null)
                {
                    _coverPageBase = coverHtml.GetChildNodeByName("body").Clone();
                    nodesToAskProps.Add(_coverPageBase);
                    ret = true;
                }
            }
            

            propGet.GetProperties(nodesToAskProps, _userProperties);

            GetFilesToParse();
            GetExternalExePaths();
            GetPathToSave();

            return ret;
        }

        bool _removeAfterMakeReport = false;
        public Dictionary<String, String> ExternalExePaths = new Dictionary<string, string>();
        public void GetExternalExePaths()
        {
            ExternalExePaths.Clear();
            if (File.Exists(_currentPath + "\\config\\external_exe_paths.txt"))
            {
                String pathToRead = _currentPath + "\\config\\external_exe_paths.txt";

                String[] files = File.ReadAllText(pathToRead).Split('\n');
                foreach (String line in files)
                {
                    int beg = line.IndexOf(':');
                    if (beg > 0)
                    {
                        string name = line.Substring(0,beg).Trim();
                        string filePath = line.Substring(beg + 1).Trim();
                        if (filePath.Length > 0)
                        {
                            if (File.Exists(filePath) == false)
                            {
                                throw new Exception("File Path of [" + name + "] in config/external_exe_paths.txt is wrong! :" + filePath + "\n if you won't use it, set empty after :");
                            }
                            else
                            {
                                ExternalExePaths[name.ToLower()] = filePath;
                            }
                        }
                    }
                    
                }
            }
                
            
        }
        
        public void GetFilesToParse()
        {
            _removeAfterMakeReport = false;
            
            if (_fFilesToParse.Count == 0)
            {
                if (File.Exists(_currentPath + "\\config\\files_to_parse.txt"))
                {
                    String pathToRead = _currentPath + "\\config\\files_to_parse.txt";

                    String[] files = File.ReadAllText(pathToRead).Split('\n');
                    SetFilesToParse(files);
                }
                if (_fFilesToParse.Count == 0)
                {
                    throw new Exception(@"you need files to parse!


");
                }
            }
        }

        private void SetFilesToParse(string[] files)
        {
            foreach (String file in files)
            {

                string fileName = file.Trim();
                if (fileName.IndexOf("//") == 0) continue;//주석.

                if (fileName.IndexOf("%") == 0)
                {
                    if (fileName.Equals("%REMOVE_THIS_FILES")) //명령..
                    {
                        _removeAfterMakeReport = true;

                    }
                }
                else
                {
                   
                    int encodingBeg = fileName.IndexOf('[');
                    if (encodingBeg < 0)
                    {
                        string fullPath = GetRelativePath(fileName, _currentPath);
                        _fFilesToParse.Add(fullPath);
                        _fEncodingsToParse.Add(fullPath, Encoding.Unicode);
                    }
                    else
                    {
                        String fullPath = GetRelativePath(fileName.Substring(0,encodingBeg).Trim(), _currentPath);
                        _fFilesToParse.Add(fullPath);
                        
                        String encoding = GetContent(fileName.Substring(encodingBeg), "[", "]");
                        _fEncodingsToParse.Add(fullPath, GetEncodingByName(encoding));//encoding을 저장함.

                    }

                    

                    //_fFilesToParse.Add(GetRelativePath(fileName, _currentPath));
                }
            }
           
        }

        public Encoding GetEncodingByName(string encodingName)
        {
            string encString = encodingName.ToUpper().Trim();
            switch (encString)
            {
                case "MULTIBYTE":
                    return Encoding.Default;
                case "UTF-8":
                    return Encoding.UTF8;
                case "UTF-32":
                    return Encoding.UTF32;
                case "ASCII":
                    return Encoding.ASCII;
                case "EUC-KR":
                    return Encoding.GetEncoding("EUC-KR");
                case "UTF-16":
                case "UNICODE":
                default:
                    return Encoding.Unicode;
            }
        }


        public void GetPathToSave()
        {
            if (_pathToSave.Length == 0)
            {
                if (File.Exists(_currentPath + "\\config\\path_to_save.txt"))
                {
                    String pathToRead = _currentPath + "\\config\\path_to_save.txt";
                    _pathToSave = File.ReadAllText(pathToRead);

                }
                else
                {
                    throw new Exception(@"it doesn't exist config/path_to_save.txt or argument /dst=path_to_save.html.
the form of path to save is like:

name_%yyyy%-%MM%-%dd%_%HH%%mm%%ss%.html

or you can use user property's name like:

name_%@ID%
");
                }

            }

            PathToSave = _pathToSave;

            
        }





        public Dictionary<String, TableHolder> GetTables()
        {
            if (_fTableForms.Count == 0)
            {
                string[] files = Directory.GetFiles(_currentPath + "\\report_forms");
                foreach (String file in files)
                {
                    _fTableForms.Add(file);
                }
            }
            else
            {
                List<String> newList = new List<String>();
                foreach (String fileOrg in _fTableForms)
                {
                    newList.Add(GetRelativePath(fileOrg, _currentPath));
                }
                _fTableForms = newList;
            }

            Dictionary<String, TableHolder> tables = new Dictionary<String, TableHolder>();
            foreach (String file in _fTableForms)
            {
                String html = File.ReadAllText(file);
                HtmlHolder holder = new HtmlHolder();
                holder.GetContentFrom(html);
                HtmlNode body = holder.GetChildNodeByName("body");
                String bodyText = body.InnerText;
                HtmlNode tempHolderNode = null;
                String temp = "";
                String tableName = "";
                int tableBeg=0;
                int endId=-1;
                while(tableBeg>=0 && bodyText.Length>tableBeg){
                    tableBeg = bodyText.IndexOf("[#", endId + 1);
                    if (tableBeg >= 0)
                    {
                        if (tempHolderNode!=null)
                        {
                            temp += bodyText.Substring(endId + 1, tableBeg - endId - 1);//처음이라면 지금까지 나왔던 모든 태그 무시..
                            tempHolderNode.InnerText = temp;
                            temp = "";//초기화.
                            tables.Add(tableName, new TableHolder(tempHolderNode));
                            
                        }
                        
                        tempHolderNode = new HtmlNode("span");

                        endId = bodyText.IndexOf(']', tableBeg);
                        tableName = bodyText.Substring(tableBeg + 2, endId - tableBeg - 2).Trim().ToUpper();
                        
                    }
                    

                }
                if(tempHolderNode!=null){
                    temp += bodyText.Substring(endId+1);
                    tempHolderNode.InnerText = temp;
                    temp = "";//초기화.
                    tables.Add(tableName, new TableHolder(tempHolderNode));
                }
                
            }
            return tables;
        }

        public string GetRelativePath(string fileOrg, string currentPath)
        {
            String file = fileOrg.Replace("/", "\\");
            if (file.IndexOf('.') == 0)
            {
                String tempPath = currentPath;
                String[] pathTokens = file.Split("\\".ToCharArray());
                string newFile = "";
                for (int i = 0; i < pathTokens.Length; i++)
                {
                    if (pathTokens[i].Equals("."))
                    {
                        continue;
                    }
                    else if (pathTokens[i].Equals(".."))
                    {
                        tempPath = tempPath.Substring(0, tempPath.LastIndexOf("\\")); //이전 path로 이동.
                    }
                    else
                    {
                        if (newFile.Length != 0)
                        {
                            newFile += "\\";
                        }
                        newFile += pathTokens[i];
                    }
                }

                return tempPath + "\\" + newFile;
            }
            else if (file.IndexOf(":") >= 0)
            {
                return file;
            }
            else
            {
                return currentPath + "\\" + file;
            }
        }


        public void GetUserProperties(String propertiesFile)
        {

           
            if (File.Exists(propertiesFile))
            {
                //throw new Exception(_fProperties + " doesn't exist!");
                String[] lines = File.ReadAllLines(propertiesFile);
                _userProperties = GetUserProperties(lines, _userProperties);
            }
            
            
        }

        private Dictionary<string, string> GetUserProperties(string[] lines, Dictionary<String,String> currentProps)
        {
            Dictionary<string, string> dic = (currentProps==null)? new Dictionary<string, string>() : currentProps;
            
            foreach (String aLine in lines)
            {
                String line = aLine.Trim();
                if (line.Length == 0) continue;//빈줄은 무시.
                int beg = line.IndexOf(':');
                if (beg < 0)
                {
                    throw new Exception("wrong format of user property! - " + line);
                }
                string name = line.Substring(0, beg).Trim();
                string value = line.Substring(beg + 1).Trim();


                if (name.Length > 0 && value.Length > 0)
                {
                    if (dic.ContainsKey(name.ToUpper()) == false) dic[name.ToUpper()] = value;//기존에 있는 것 중복안함.
                }
                
                

            }
            return dic;
        }



        public HtmlHolder GetBodyFromBasicReportForm(String basicFile)
        {
            HtmlHolder holder = new HtmlHolder();

            if (File.Exists(basicFile) == false)
            {
                throw new Exception(basicFile + " doesn't exist!");
            }

            String xml = File.ReadAllText(basicFile);


            holder.GetContentFrom(xml);


            return holder;
        }


        public bool SaveReport()
        {
            File.WriteAllText(PathToSave, _basicHtml.OuterText);

            return true;
        }




        public void SetArgs(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (String argFrom in args)
                {
                    String arg = argFrom.Replace("%s", " ");//%s는 공백으로 치환한다.
                    if (arg.Length > 0)
                    {
                        String[] tokens = arg.Split("=".ToCharArray());
                        switch (tokens[0].ToLower().Trim())
                        {
                            case "/current_path":
                                _currentPath = tokens[1].ToLower().Trim().Replace("/","\\");
                                
                                break;
                            case "/properties":
                                string[] props = tokens[1].ToLower().Trim().Split(',');
                                
                                _userProperties = GetUserProperties(props, _userProperties);

                                break;
                            case "/property_file":
                                {
                                    string propFileTxt = tokens[1].Trim();
                                    int beg = propFileTxt.IndexOf('[');
                                    Encoding encoding = Encoding.Unicode;
                                    if (beg>=0)
                                    {
                                        string encodingTxt = GetContent(propFileTxt, "[", "]");
                                        encoding = GetEncodingByName(encodingTxt);
                                        propFileTxt = propFileTxt.Substring(0, beg).Trim();
                                    }
                                    propFileTxt = GetRelativePath(propFileTxt, _currentPath);
                                    if(File.Exists(propFileTxt))
                                    {
                                        String[] lines = File.ReadAllLines(propFileTxt, encoding);
                                        _userProperties = GetUserProperties(lines, _userProperties);
                                    }
                                    break;
                                }
                                
                            case "/src":
                            case "/files_to_parse":
                                {
                                    String[] files = tokens[1].Split(",".ToCharArray());
                                    SetFilesToParse(files);
                                }
                                break;
                            case "/dst":
                            case "/path_to_save":
                                PathToSave = tokens[1].Trim();
                                break;
                            case "/basic_form":
                                _fbasicForm = tokens[1].Trim();
                                break;
                            case "/cover_page":
                                _fcoverPage = tokens[1].Trim();
                                break;
                            case "/forms":
                                String[] forms = tokens[1].Split(",".ToCharArray());
                                foreach (String form in forms)
                                {
                                    _fTableForms.Add(form.Trim());
                                }
                                break;
                            case "/make_pdf":
                                _makePdf = true;
                                
                                break;
                            case "/print_now":
                                _printNow = true;
                                
                                break;

                        }
                    }
                }
            }
        }



        public bool MakeReport()//List<String> _fFilesToParse,Dictionary<String,Encoding> _fEncodingsToParse)
        {
            //_fFilesToParse, _fEncodingsToParse

            _basicHtml.GetChildNodeByName("body").Clear();//.Children.Clear();//.InnerText = "";//내부 모두 지움.
            //_coverHtml.GetChildNodeByName("body").Clear();
            _imgIndex = 0;
            _pageNum = 1;
            _testDate = DateTime.Now;
            _tableResults.Clear();
            _chartList.Clear();
            int tblCnt = 0;


            _currentTableHolderBase = null;//원본은 변하지 않아야 한다.
            _currentTableHolder = null;
            _currentPage = null;

           
            CopyExternalFolderTo(_dirToSaveHtml);

            _bodyContents.Clear();

            _currentRowHeight = DEFAULT_CELL_HEIGHT;
            foreach (String file in _fFilesToParse)
            {
                Encoding encoding = _fEncodingsToParse[file];
                StreamReader reader = new StreamReader(File.OpenRead(file),encoding);
                if (reader != null)
                {

                    String aLine;
                    int lineIndex = 0;
                    while ((aLine =  reader.ReadLine()) != null)
                    {
                        lineIndex++;
                        aLine = aLine.Trim();
                        string addErr = "  file:"+file+"\nline:"+lineIndex;
                        try
                        {



                            if (aLine.Length == 0) continue;

                            if (aLine.IndexOf("@@") != 0 && _currentPage==null)
                            {
                                SetCurrentPageAsBasicPage();
                            }
                            else if (aLine.IndexOf("@@") == 0 )
                            {
                                string baseName = aLine.Substring(2).ToUpper();

                                if (baseName.Equals("COVER") || baseName.Equals("COVERPAGE") || baseName.Equals("COVER_PAGE"))
                                {
                                    if (_fcoverPage.Length == 0)//cover page file이 없으면...
                                    {
                                        SetCurrentPageAsBasicPage();//basic으로 대체..
                                    }
                                    else
                                    {
                                        SetCurrentPageAsCoverPage(true);
                                    }
                                    
                                }
                                else if (baseName.Equals("COVERS") || baseName.Equals("COVERPAGES") || baseName.Equals("COVER_PAGES"))
                                {
                                    if (_fcoverPage.Length == 0)//cover page file이 없으면...
                                    {
                                        SetCurrentPageAsBasicPage();//basic으로 대체..
                                    }
                                    else
                                    {
                                        SetCurrentPageAsCoverPage(false);
                                    }

                                }
                                else
                                {
                                    SetCurrentPageAsBasicPage();
                                }
                            }
                            
                            if (aLine.IndexOf("//") == 0)
                            {
                                //주석. pass..

                                HtmlNode comment = new HtmlNode("comment");//실제로는 넣지 않을것임.
                                comment.InnerText = aLine.Substring(2).Trim();

                                _currentContent.Children.Add(comment);

                                continue;
                            }
                            else if (aLine.IndexOf(">>") == 0 || aLine.IndexOf("%TEXT") == 0)
                            {
                                #region %TEXT OR >>

                                string lineText = (aLine.IndexOf("%TEXT") == 0) ? aLine.Substring(5).Trim() : aLine.Substring(2).Trim();

                                HtmlNode test = new HtmlNode("text");//실제로는 넣지 않을것임.
                                test.InnerText = lineText + "<br/>\n";


                                if (_isNewTableMaking)
                                {
                                    int tableIndex = _currentTableHolderBase.Children.IndexOf(_currentTableHolderBase.Table);

                                    if (_currentTableHolderBase.Table.Children.Count > 0)//테이블 내용이 채워졌으면,
                                    {
                                        if (test.Children.Count > 0)
                                        {
                                            foreach (HtmlNode testChild in test.Children)
                                            {
                                                _currentTableHolderBase.Children.Add(testChild.Clone());
                                            }
                                        }
                                        else
                                        {
                                            _currentTableHolderBase.Children.Add(test);
                                        }
                                    }
                                    else
                                    {
                                        if (test.Children.Count > 0)
                                        {
                                            foreach (HtmlNode testChild in test.Children)
                                            {
                                                _currentTableHolderBase.Children.Insert(tableIndex++, testChild.Clone());
                                            }
                                        }
                                        else
                                        {
                                            _currentTableHolderBase.Children.Insert(tableIndex, test);
                                        }
                                    }
                                }
                                else
                                {
                                    EndTable();

                                    int lineHeight = test.GetTagHeight(DEFAULT_LINE_HEIGHT);
                                    if (_nowHeight + lineHeight > _pageHeight) GoNextPage();


                                    if (test.Children.Count > 0)
                                    {
                                        foreach (HtmlNode testChild in test.Children)
                                        {
                                            _currentContent.Children.Add(testChild.Clone());
                                        }
                                    }
                                    else
                                    {
                                        _currentContent.Children.Add(test);
                                    }

                                    AddPageHeight(lineHeight);


                                    //_nowHeight += DEFAULT_LINE_HEIGHT * count;
                                }


                                #endregion

                            }
                            else if (aLine.IndexOf("#") == 0)
                            {
                                #region #table

                                if (_isNewTableMaking)//이전에 newTable을 만들고 있었다면...
                                {
                                    HtmlNode holderNode = new HtmlNode("span");
                                    string tableId = _currentTableHolderBase.Name;
                                    holderNode.InnerText = _currentTableHolderBase.InnerText;//다시 내부셋팅.
                                    TableHolder newTable = new TableHolder(holderNode);//다시 테이블을 갱신.
                                    _tables[tableId] = newTable;
                                    _currentTableHolderBase = null;
                                    _isNewTableMaking = false;
                                    _baseTableHolder = null;
                                }
                                _currentTableHolderBase = null;
                                _currentTableHolder = null;

                                int begAlias = aLine.IndexOf('=');
                                string tableKind;
                                string tableName;

                                _currentTitleRowIndex = 0;
                                _contentRowIndex = 0;
                                _addIndex = 0;
                                if (begAlias < 0)
                                {
                                    tableKind = aLine.Substring(1).Trim().ToUpper();
                                    tableName = "";
                                }
                                else
                                {
                                    tableKind = aLine.Substring(1, begAlias - 1).Trim().ToUpper();
                                    tableName = aLine.Substring(begAlias + 1).Trim();
                                }

                                if (_tables.ContainsKey(tableKind))
                                {

                                    _currentTableHolderBase = _tables[tableKind].Clone() as TableHolder;

                                    string[] tokens = tableName.Split(',');
                                    int i = 0;

                                    foreach (HtmlNode node in _currentTableHolderBase.TableTitles)
                                    {
                                        if (tokens.Length > i && tokens[i].Trim().Length > 0)
                                        {
                                            string newText;
                                            node.InnerText = node.InnerText.Replace("[%TABLE_TITLE]", tokens[i].Trim());
                                            //bool? replaced = TextReplaced(node.InnerText, tokens[i], out newText);
                                            //node.InnerText = (replaced != false) ? newText : tokens[i];
                                            i++;
                                        }
                                        else
                                        {
                                            node.InnerText = "";
                                            //do nothing..
                                        }


                                    }

                                    //_currentTableHolder.Attributes["id"] = tableKind + (tblCnt++);//unique한 이름을 만들어주기 위하여..


                                    //if (_isNewTableMaking == false)
                                    {//이전에 newTable을 만들고 있지 않았을 때만..
                                        _currentTableHolder = _currentTableHolderBase.Clone() as TableHolder;

                                        TableHolder tempTag = _currentTableHolder.Clone() as TableHolder;
                                        tempTag.RemoveContentCells();
                                        tempTag.RemoveBottomOfTable();
                                        tempTag.RemoveContentCells();
                                        if (tempTag.Table != null)
                                        {
                                            tempTag.Table.AddCellRow(_currentTableHolderBase.Table.CellRows[0].Clone());
                                        }
                                        IsCellReplaced(tempTag, "TEST");
                                        int lineHeight = tempTag.GetTagHeight(DEFAULT_LINE_HEIGHT);

                                        tempTag.Children.Remove(tempTag.Table);



                                        //tempHeight는 첫번째 데이터열이 들어갔을 때 페이지를 넘는지 판단하기 위해서 구한다.
                                        //첫번째 행이 들어갔을 때 넘는다면 타이틀을 추가하는 의미가 없으므로 다음 줄로 넘어가야 한다.

                                        _currentTableHolder.RemoveContentCells();

                                        if (_nowHeight + lineHeight > _pageHeight)//비교는 내용이 하나 들어갔을 때를 가정함.
                                        {
                                            lineHeight = tempTag.GetTagHeight(DEFAULT_LINE_HEIGHT);//실제 크기는 테이블 영역을 빼고 추가함.
                                            //TableHolder temp = _currentTableHolderBase;
                                            //_currentTableHolderBase = null;
                                            GoNextPage(false);
                                            AddPageHeight(lineHeight);
                                            //_currentTableHolderBase = temp;
                                            // _nowHeight = 0;
                                        }
                                        else
                                        {
                                            //tempTag.Children.Remove(tempTag.Table);
                                            //_nowHeight += tempTag.GetTagHeight(DEFAULT_LINE_HEIGHT);//.GetLinesInNode();

                                            lineHeight = tempTag.GetTagHeight(DEFAULT_LINE_HEIGHT); //실제 크기는 테이블 영역을 빼고 추가함.
                                            _currentContent.Children.Add(_currentTableHolder);
                                            AddPageHeight(lineHeight);
                                            //_nowHeight += tempTag.GetTagHeight(DEFAULT_LINE_HEIGHT);//.GetLinesInNode();

                                            //int count = _currentTableHolder.GetLineCountBeforeTable();
                                            //int count = GetCount(lineText.InnerText, new String[] { "<br", "<p" });
                                            //_nowHeight += DEFAULT_LINE_HEIGHT * count;
                                            //테이블도 제외해야 함. 나중에 추가될 것이므로.
                                            //_nowHeight += _currentTableHolder.GetTagHeight(DEFAULT_LINE_HEIGHT);//.GetLinesInNode();


                                        }

                                    }
                                }
                                else
                                {

                                    continue;//테이블이 없으면 그냥 넘어감.(표시안함)
                                }
                                #endregion

                            }
                            else if (aLine.IndexOf("%TITLE") == 0)
                            {
                                #region %TITLE
                                #region TITLE PARSING

                                int colon = aLine.IndexOf(':');
                                List<String> titles = new List<String>();
                                if (colon >= 0)
                                {
                                    string txt = aLine.Substring(colon + 1).Trim();
                                    int i = 0;
                                    string temp = "";
                                    int inBracket = 0;
                                    while (i < txt.Length)
                                    {
                                        if (inBracket == 0 && txt[i] == ',')
                                        {
                                            titles.Add(temp);
                                            temp = "";
                                            i++;
                                            continue;
                                        }
                                        else
                                        {
                                            if (txt[i] == '(')
                                            {
                                                inBracket++;
                                            }
                                            else if (txt[i] == ')')
                                            {
                                                if (inBracket == 0)
                                                {
                                                    throw new Exception("bracket'( )' is not opened but closed.");
                                                }
                                                inBracket--;
                                            }
                                            temp += txt[i];
                                        }
                                        i++;
                                    }
                                    if (temp.Length > 0)
                                    {
                                        titles.Add(temp);
                                        temp = "";
                                    }

                                }
                                else
                                {
                                    throw new Exception("wrong format for %TITLE:...");
                                }
                                #endregion

                                if (_isNewTableMaking)
                                {
                                    #region %TITLE - NEW TABLE MAKING

                                    HtmlNode tr;
                                    HtmlNode tdBase = null;
                                    if (_baseTableHolder != null && _baseTableHolder.Table.TitleRows.Count > 0)
                                    {
                                        int selectedRow = (_currentTableHolderBase.Table.TitleRows.Count) % _baseTableHolder.Table.TitleRows.Count;
                                        tr = _baseTableHolder.Table.TitleRows[selectedRow].Clone();
                                        tdBase = tr.Children[0];
                                        tdBase.InnerText = "";//내부 모두 지움. Attribute만 남김.
                                        tr.Clear();//td모두 지움.
                                    }
                                    else
                                    {
                                        tr = new HtmlNode("tr");
                                    }
                                    _currentTableHolderBase.Table.AddTitleRow(tr);

                                    foreach (String cell in titles)
                                    {
                                        HtmlNode td = (tdBase != null) ? tdBase.Clone() : new HtmlNode("td");
                                        if (tr.Children.Count == 0)
                                        {
                                            td.InnerText = "[%H]";
                                        }
                                        tr.Children.Add(td);
                                        int size = 1;
                                        String cellTxt = cell;
                                        int sizeBeg = cellTxt.IndexOf("%SIZE");

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%SIZE", ")");
                                            string funcStr = "%SIZE" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string sizeTxt = GetContent(funcStr, "(", ")");

                                            string[] sizes = sizeTxt.Split(',');//width,height
                                            if (sizes.Length > 1)
                                            {
                                                int width;
                                                int height;
                                                if (int.TryParse(sizes[0].Trim(), out width) == false)
                                                {
                                                    throw new Exception("the form of %SIZE(width,height) is wrong!\n Use %SIZE(width) or %SIZE(width,height)!\n ex> %SIZE(100,20)");
                                                }
                                                if (int.TryParse(sizes[1].Trim(), out height) == false)
                                                {
                                                    throw new Exception("the form of %SIZE(width,height) is wrong!\n Use %SIZE(width) or %SIZE(width,height)!\n ex> %SIZE(100,20)");
                                                }
                                                td.Attributes["width"] = "" + width;
                                                td.Attributes["height"] = "" + height;
                                            }
                                            else
                                            {
                                                int width;
                                                if (int.TryParse(sizes[0].Trim(), out width) == false)
                                                {
                                                    throw new Exception("the form of %SIZE(width,height) is wrong!\n Use %SIZE(width) or %SIZE(width,height)!\n ex> %SIZE(100,20)");
                                                }

                                                td.Attributes["width"] = "" + width;
                                            }

                                        }
                                        sizeBeg = cellTxt.IndexOf("%SPAN");
                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%SPAN", ")");
                                            string funcStr = "%SPAN" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string sizeTxt = GetContent(funcStr, "(", ")");

                                            string[] spans = sizeTxt.Split(',');//width,height
                                            if (spans.Length > 1)
                                            {
                                                int col = 1;
                                                int row = 1;
                                                if (spans[0].Trim().Length > 0 && int.TryParse(spans[0].Trim(), out col) == false)
                                                {
                                                    throw new Exception("the form of %SPAN(colspan,rowspan) is wrong!\n Use %SPAN(colspan) or %SPAN(colspan,rowspan)!\n ex> %SPAN(2)");
                                                }
                                                if (spans[1].Trim().Length > 0 && int.TryParse(spans[1].Trim(), out row) == false)
                                                {
                                                    throw new Exception("the form of %SPAN(colspan,rowspan) is wrong!\n Use %SPAN(colspan) or %SPAN(colspan,rowspan)!\n ex> %SPAN(2)");
                                                }
                                                if (col > 1)
                                                {
                                                    td.Attributes["colspan"] = "" + col;
                                                }
                                                if (row > 1)
                                                {
                                                    td.Attributes["rowspan"] = "" + row;
                                                }

                                            }
                                            else
                                            {
                                                int col;
                                                if (int.TryParse(spans[0].Trim(), out col) == false)
                                                {
                                                    throw new Exception("the form of %SPAN(colspan,rowspan) is wrong!\n Use %SPAN(colspan) or %SPAN(colspan,rowspan)!\n ex> %SPAN(2)");
                                                }

                                                td.Attributes["colspan"] = "" + col;
                                            }

                                        }

                                        sizeBeg = cellTxt.IndexOf("%ROW_STYLE");

                                        #region %ROW_STYLE

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%ROW_STYLE", ")");
                                            string funcStr = "%ROW_STYLE" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string row_style = GetContent(funcStr, "(", ")");

                                            tr.Attributes["style"] = "" + row_style;

                                        }
                                        #endregion

                                        sizeBeg = cellTxt.IndexOf("%CELL_STYLE");

                                        #region %CELL_STYLE

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%CELL_STYLE", ")");
                                            string funcStr = "%CELL_STYLE" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string cell_style = GetContent(funcStr, "(", ")");

                                            td.Attributes["style"] = "" + cell_style;

                                        }
                                        #endregion

                                        #region %STYLE (==%CELL_STYLE)

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%STYLE", ")");
                                            string funcStr = "%STYLE" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string cell_style = GetContent(funcStr, "(", ")");

                                            td.Attributes["style"] = "" + cell_style;

                                        }
                                        #endregion


                                        td.InnerText += cellTxt;

                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region TITLE - IN SELECTED TABLE

                                    for (int c = 0; c < titles.Count; c++)
                                    {
                                        string newText;
                                        bool? replaced = TextReplaced("[" + titles[c] + "]", null, out newText);
                                        if (replaced != false) titles[c] = newText;
                                    }//기능별 변경.

                                    _currentRowHeight = 0;// DEFAULT_CELL_HEIGHT;

                                    if (_currentTableHolder == null) continue;

                                    int i = 0;
                                    if (_currentTitleRowIndex >= _currentTableHolder.Table.TitleRows.Count) continue;//title이 너무 많으면 그냥 무시..
                                    HtmlNode row = _currentTableHolder.Table.TitleRows[_currentTitleRowIndex++];
                                    if (row.GetNodesByContent("[%BLANK]").Count == 0 && row.GetNodesByContent("[%RESULT]").Count == 0)
                                    {
                                        IsCellReplaced(row, null);//내용 바꿈.
                                        row = _currentTableHolder.Table.TitleRows[_currentTitleRowIndex++];
                                    }
                                    //foreach (HtmlNode row in _currentTableHolder.Table.TitleRows)
                                    {
                                        int lineHeight = -1;

                                        foreach (HtmlNode cell in row.Children)//td
                                        {
                                            if (cell.Attributes.ContainsKey("height"))
                                            {
                                                int cellHeight = int.Parse(cell.Attributes["height"]);
                                                if (cellHeight > lineHeight)
                                                {
                                                    lineHeight = cellHeight;
                                                }
                                            }
                                            string txtToReplace = (i < titles.Count) ? titles[i].Trim() : "";//내용이 있으면 내용으로 converting..
                                            if (IsCellReplaced(cell, txtToReplace) == true)
                                            {
                                                i++;
                                            }
                                        }
                                        //_nowHeight += (DEFAULT_CELL_HEIGHT < _currentRowHeight && lineHeight < _currentRowHeight) ? _currentRowHeight :
                                        //(lineHeight < 0) ? DEFAULT_CELL_HEIGHT : lineHeight;
                                        //테이블이 추가될 때나 page가 넘어갈 때 높이에 포함한다.
                                    }
                                    #endregion
                                }

                                #endregion
                            }
                            else if (aLine.IndexOf("%NEW_TABLE") == 0) //새로운 테이블을 만든다.
                            {
                                #region %NEW_TABLE
                                _isNewTableMaking = true;
                                int colon = aLine.IndexOf(':');
                                String[] funcs;
                                String tableStyle = "";
                                if (colon >= 0)
                                {

                                    funcs = aLine.Substring(colon + 1).Trim().Split(',');

                                    String tableId = "";
                                    for (int c = 0; c < funcs.Length; c++)
                                    {
                                        string func = funcs[c].Trim();
                                        if (func.ToUpper().IndexOf("BASE") == 0)
                                        {
                                            String tableName = GetContent(func, "(", ")").ToUpper();
                                            if (tableName.IndexOf('#') == 0) tableName = tableName.Substring(1);//#를 떼고 찾음.
                                            _baseTableHolder = _tables[tableName];

                                        }
                                        else if (func.ToUpper().IndexOf("ID") == 0)
                                        {
                                            tableId = GetContent(func, "(", ")").ToUpper();
                                        }
                                        else if (func.ToUpper().IndexOf("STYLE") == 0)
                                        {
                                            tableStyle = GetContent(func, "(", ")").ToUpper();
                                        }

                                    }//기능별 변경.
                                    if (tableId.Length == 0)
                                    {
                                        throw new Exception("wrong format for %NEW_TABLE:ID(TABLE_ID),[BASE(BASE_TABLE_ID).\n ex> %NEW_TABLE:ID(TEST_TABLE),BASE(4CELL_TABLE)\nex> %NEW_TABLE:ID(TEST_TABLE)");
                                    }
                                    else
                                    {
                                        if (_tables.ContainsKey(tableId))
                                        {
                                            throw new Exception(tableId + " already exists in Table list!");
                                        }
                                    }
                                    if (_baseTableHolder == null)
                                    {
                                        HtmlNode table = new HtmlNode("table");
                                        if (tableStyle.Length == 0)
                                        {
                                            tableStyle += "border-top-width: 1px;border-right-width:1px;border-left-width:1px;border-bottom-width:1px;border-top-style: solid;border-right-style: solid; border-bottom-style: solid;border-left-style: solid;";
                                        }
                                        tableStyle += "width:100%";
                                        StyleItemCollection styles = new StyleItemCollection(tableStyle);

                                        table.Attributes["style"] = styles.LineText;
                                        HtmlNode text1 = new HtmlNode("text");
                                        HtmlNode text2 = new HtmlNode("text");

                                        HtmlNode span = new HtmlNode(tableId);//tableId를 Name으로 태그만듬.
                                        span.Children.Add(text1);
                                        span.Children.Add(table);
                                        span.Children.Add(text2);
                                        _currentTableHolderBase = new TableHolder(span);
                                    }
                                    else
                                    {
                                        _currentTableHolderBase = _baseTableHolder.Clone() as TableHolder;
                                        _currentTableHolderBase.Clear();
                                        foreach (HtmlNode node in _baseTableHolder.Children)
                                        {
                                            if (node is HtmlTable)
                                            {
                                                HtmlTable table = node.Clone() as HtmlTable;
                                                table.Clear();
                                                _currentTableHolderBase.Children.Add(table);

                                            }
                                            else
                                            {
                                                _currentTableHolderBase.Children.Add(node.Clone());
                                            }

                                        }
                                        _currentTableHolderBase = new TableHolder(_currentTableHolderBase);
                                        _currentTableHolderBase.Name = tableId;
                                    }



                                }
                                else
                                {
                                    throw new Exception("wrong format for %NEW_TABLE:ID(TABLE_ID),[BASE(BASE_TABLE_ID).\n ex> %NEW_TABLE:ID(TEST_TABLE),BASE(4CELL_TABLE)\nex> %NEW_TABLE:ID(TEST_TABLE)");
                                }
                                #endregion

                            }
                            else if (aLine.IndexOf("%TABLE_RESULT") == 0 && _isNewTableMaking == false)
                            {
                                if (_currentTableHolderBase != null)
                                {
                                    //int count = _currentTableHolderBase.GetLineCountAfterTable();
                                    //int count = GetCount(lineText.InnerText, new String[] { "<br", "<p" });
                                    //_nowHeight += DEFAULT_LINE_HEIGHT * count;
                                    //AddPageHeight(_currentTableHolder.GetTagHeight(DEFAULT_LINE_HEIGHT));//.GetLinesInNode();
                                    EndTable();
                                    //_currentTableHolderBase = null;
                                    int colon = aLine.IndexOf(':');
                                    if (colon < 0) colon = aLine.IndexOf('=');
                                    if (colon >= 0)
                                    {
                                        _tableResults.Add(aLine.Substring(colon + 1).Trim()); //테이블이 보이지 않으면 결과도 저장되지 않아야 한다.
                                    }
                                }

                            }
                            else if (aLine.IndexOf("%NEXT_PAGE") == 0 && _isNewTableMaking == false)
                            {
                                GoNextPage();
                            }
                            else if (aLine.IndexOf("%CHART") == 0 && _isNewTableMaking == false)
                            {
                                _currentRowHeight = DEFAULT_CELL_HEIGHT;
                                int colon = aLine.IndexOf(':');

                                if (colon >= 0)
                                {
                                    ChartHolder holder = new ChartHolder(aLine.Substring(colon + 1).Trim());
                                    _chartList[holder.Name] = holder;
                                }
                                else
                                {

                                }
                            }
                            else if (aLine.IndexOf("%ROW") == 0 && _isNewTableMaking == false)
                            {
                                #region %ROW

                                int numBeg = aLine.IndexOf('(');
                                int numEnd = aLine.IndexOf(')', numBeg + 1);
                                bool wrongFormat = false;
                                int rowIndex = -1;
                                string content = "";
                                if (numBeg > 0 && numEnd > numBeg)
                                {
                                    if (int.TryParse(aLine.Substring(numBeg + 1, numEnd - numBeg - 1).Trim(), out rowIndex) == false)
                                    {
                                        wrongFormat = true;
                                    }
                                    int contBeg = aLine.IndexOf(":", numEnd + 1);
                                    if (contBeg > 0)
                                    {
                                        content = aLine.Substring(contBeg + 1).Trim();
                                    }
                                    else
                                    {
                                        wrongFormat = true;
                                    }
                                }
                                else
                                {
                                    wrongFormat = true;
                                }
                                if (wrongFormat || rowIndex < 0)
                                {
                                    throw new Exception(@"WRONG FORMAT!
%ROW(n):arg1,arg2,...,argk
ex>%ROW(1):내용1,내용2,내용3
n은 1부터 시작할 것.
");
                                }
                                if (rowIndex >= _currentTableHolderBase.Table.CellRows.Count) rowIndex = _currentTableHolderBase.Table.CellRows.Count - 1;//최대값으로 조정.
                                List<String> cells = new List<String>();

                                int i = 0;
                                string temp = "";
                                int inBracket = 0;
                                while (i < content.Length)
                                {
                                    if (inBracket == 0 && content[i] == ',')
                                    {
                                        cells.Add(temp);
                                        temp = "";
                                        i++;
                                        continue;
                                    }
                                    else
                                    {
                                        if (content[i] == '(')
                                        {
                                            inBracket++;
                                        }
                                        else if (content[i] == ')')
                                        {
                                            if (inBracket == 0)
                                            {
                                                throw new Exception("bracket'( )' is not opened but closed.");
                                            }
                                            inBracket--;
                                        }
                                        temp += content[i];
                                    }
                                    i++;
                                }
                                if (temp.Length > 0)
                                {
                                    cells.Add(temp);
                                    temp = "";
                                }


                                for (int c = 0; c < cells.Count; c++)
                                {
                                    string newText;
                                    bool? replaced = TextReplaced("[" + cells[c] + "]", null, out newText);
                                    if (replaced != false) cells[c] = newText;
                                }//기능별 변경.


                                if (_currentTableHolderBase == null) continue;



                                if (_currentTableHolderBase.Table != null)
                                {
                                    if (_currentTableHolder.Table.CellRows.Count == 0)
                                    {
                                        AddTitleHeight(_currentTableHolder.Table);
                                    }

                                    HtmlNode row = _currentTableHolderBase.Table.CellRows[rowIndex];

                                    HtmlNode newRow = row.Clone();
                                    _currentTableHolder.Table.AddCellRow(newRow);

                                    int lineHeight = -1;
                                    i = 0;
                                    foreach (HtmlNode cell in newRow.Children)//td
                                    {
                                        string txtToReplace = (i < cells.Count) ? cells[i].Trim() : "";//내용이 있으면 내용으로 converting..
                                        if (IsCellReplaced(cell, txtToReplace) == true)
                                        {
                                            i++;
                                        }
                                    }
                                    lineHeight = newRow.GetTagHeight(DEFAULT_CELL_HEIGHT);

                                    if (_nowHeight + lineHeight > _pageHeight) GoNextPage();

                                    AddPageHeight(lineHeight);

                                }
                                else
                                {
                                    for (int c = 0; c < cells.Count; c++)
                                    {
                                        IsCellReplaced(_currentTableHolder, cells[c]);
                                    }
                                    int lineHeight = _currentTableHolder.GetTagHeight(DEFAULT_LINE_HEIGHT);
                                    if (_nowHeight + lineHeight > _pageHeight) GoNextPage();

                                    AddPageHeight(lineHeight);

                                    //ReplaceFunctions(_currentTableHolderBase);



                                    // _nowHeight += (DEFAULT_LINE_HEIGHT < _currentRowHeight) ? _currentRowHeight : DEFAULT_LINE_HEIGHT;
                                }
                                #endregion



                            }
                            else //etc row..
                            {
                                #region ETC ROW

                                List<String> cells = new List<String>();
                                String content = aLine;
                                int i = 0;
                                string temp = "";
                                int inBracket = 0;
                                while (i < content.Length)
                                {
                                    if (inBracket == 0 && content[i] == ',')
                                    {
                                        cells.Add(temp);
                                        temp = "";
                                        i++;
                                        continue;
                                    }
                                    else
                                    {
                                        if (content[i] == '(')
                                        {
                                            inBracket++;
                                        }
                                        else if (content[i] == ')')
                                        {
                                            if (inBracket == 0)
                                            {
                                                throw new Exception("bracket'( )' is not opened but closed.");
                                            }
                                            inBracket--;
                                        }
                                        temp += content[i];
                                    }
                                    i++;
                                }
                                if (temp.Length > 0)
                                {
                                    cells.Add(temp);
                                    temp = "";
                                }


                                if (_isNewTableMaking)
                                {
                                    #region making new row

                                    HtmlNode tr;
                                    HtmlNode tdBase = null;
                                    if (_baseTableHolder != null && _baseTableHolder.Table.CellRows.Count > 0)
                                    {
                                        int selectedRow = (_currentTableHolderBase.Table.CellRows.Count) % _baseTableHolder.Table.CellRows.Count;
                                        tr = _baseTableHolder.Table.CellRows[selectedRow].Clone();
                                        tdBase = tr.Children[0].Clone();
                                        tdBase.InnerText = "";//내부 모두 지움. Attribute만 남김.
                                        tr.Clear();//td모두 지움.
                                    }
                                    else
                                    {
                                        tr = new HtmlNode("tr");
                                    }
                                    _currentTableHolderBase.Table.AddCellRow(tr);

                                    foreach (String cell in cells)
                                    {
                                        HtmlNode td = (tdBase != null) ? tdBase.Clone() : new HtmlNode("td");
                                        tr.Children.Add(td);
                                        int size = 1;
                                        String cellTxt = cell;
                                        int sizeBeg = cellTxt.IndexOf("%SIZE");
                                        #region %SIZE

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%SIZE", ")");
                                            string funcStr = "%SIZE" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string sizeTxt = GetContent(funcStr, "(", ")");

                                            string[] sizes = sizeTxt.Split(',');//width,height
                                            if (sizes.Length > 1)
                                            {
                                                int width;
                                                int height;
                                                if (int.TryParse(sizes[0].Trim(), out width) == false)
                                                {
                                                    throw new Exception("the form of %SIZE(width,height) is wrong!\n Use %SIZE(width) or %SIZE(width,height)!\n ex> %SIZE(100,20)");
                                                }
                                                if (int.TryParse(sizes[1].Trim(), out height) == false)
                                                {
                                                    throw new Exception("the form of %SIZE(width,height) is wrong!\n Use %SIZE(width) or %SIZE(width,height)!\n ex> %SIZE(100,20)");
                                                }
                                                td.Attributes["width"] = "" + width;
                                                td.Attributes["height"] = "" + height;
                                            }
                                            else
                                            {
                                                int width;
                                                if (int.TryParse(sizes[0].Trim(), out width) == false)
                                                {
                                                    throw new Exception("the form of %SIZE(width,height) is wrong!\n Use %SIZE(width) or %SIZE(width,height)!\n ex> %SIZE(100,20)");
                                                }

                                                td.Attributes["width"] = "" + width;
                                            }

                                        }
                                        #endregion
                                        sizeBeg = cellTxt.IndexOf("%SPAN");

                                        #region %SPAN

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%SPAN", ")");
                                            string funcStr = "%SPAN" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string sizeTxt = GetContent(funcStr, "(", ")");

                                            string[] spans = sizeTxt.Split(',');//width,height
                                            if (spans.Length > 1)
                                            {
                                                int col = 1;
                                                int row = 1;
                                                if (spans[0].Trim().Length > 0 && int.TryParse(spans[0].Trim(), out col) == false)
                                                {
                                                    throw new Exception("the form of %SPAN(colspan,rowspan) is wrong!\n Use %SPAN(colspan) or %SPAN(colspan,rowspan)!\n ex> %SPAN(2)");
                                                }
                                                if (spans[1].Trim().Length > 0 && int.TryParse(spans[1].Trim(), out row) == false)
                                                {
                                                    throw new Exception("the form of %SPAN(colspan,rowspan) is wrong!\n Use %SPAN(colspan) or %SPAN(colspan,rowspan)!\n ex> %SPAN(2)");
                                                }
                                                if (col > 1)
                                                {
                                                    td.Attributes["colspan"] = "" + col;
                                                }
                                                if (row > 1)
                                                {
                                                    td.Attributes["rowspan"] = "" + row;
                                                }

                                            }
                                            else
                                            {
                                                int span;
                                                if (int.TryParse(spans[0].Trim(), out span) == false)
                                                {
                                                    throw new Exception("the form of %SPAN(colspan,rowspan) is wrong!\n Use %SPAN(colspan) or %SPAN(colspan,rowspan)!\n ex> %SPAN(2)");
                                                }

                                                td.Attributes["colspan"] = "" + span;
                                            }



                                        }
                                        #endregion

                                        sizeBeg = cellTxt.IndexOf("%ROW_STYLE");

                                        #region %ROW_STYLE

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%ROW_STYLE", ")");
                                            string funcStr = "%ROW_STYLE" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string row_style = GetContent(funcStr, "(", ")");

                                            tr.Attributes["style"] = "" + row_style;

                                        }
                                        #endregion

                                        sizeBeg = cellTxt.IndexOf("%CELL_STYLE");

                                        #region %CELL_STYLE

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%CELL_STYLE", ")");
                                            string funcStr = "%CELL_STYLE" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string cell_style = GetContent(funcStr, "(", ")");

                                            td.Attributes["style"] = "" + cell_style;

                                        }
                                        #endregion

                                        #region %STYLE (==%CELL_STYLE)

                                        if (sizeBeg >= 0)
                                        {
                                            string sizeFunc = GetContent(cellTxt, "%STYLE", ")");
                                            string funcStr = "%STYLE" + sizeFunc + ")";
                                            cellTxt = cellTxt.Replace(funcStr, "");//size정보 지움.

                                            string cell_style = GetContent(funcStr, "(", ")");

                                            td.Attributes["style"] = "" + cell_style;

                                        }
                                        #endregion

                                        td.InnerText += cellTxt;

                                    }
                                    #endregion
                                }
                                else
                                {


                                    #region add a row in report
                                    for (int c = 0; c < cells.Count; c++)
                                    {
                                        string newText;
                                        bool? replaced = TextReplaced("[" + cells[c] + "]", null, out newText);
                                        if (replaced != false) cells[c] = newText;
                                    }//기능별 변경.


                                    if (_currentTableHolderBase == null) continue;
                                    //List<HtmlNode> contentRows = new List<HtmlNode>();


                                    if (_currentTableHolderBase.Table != null)
                                    {
                                        if (_currentTableHolder.Table.CellRows.Count == 0)
                                        {
                                            AddTitleHeight(_currentTableHolder.Table);
                                        }

                                        if (_currentTableHolderBase.Table.CellRows.Count > 0)
                                        {
                                            HtmlNode row = _currentTableHolderBase.Table.CellRows[_contentRowIndex % _currentTableHolderBase.Table.CellRows.Count].Clone();
                                            //HtmlNode newRow = row.Clone();
                                            //contentRows.Add(newRow);

                                            i = 0;
                                            foreach (HtmlNode cell in row.Children)//td
                                            {

                                                string txtToReplace = (i < cells.Count) ? cells[i].Trim() : "";//내용이 있으면 내용으로 converting..
                                                if (IsCellReplaced(cell, txtToReplace) == true)
                                                {
                                                    i++;
                                                }
                                            }

                                            int lineHeight = row.GetTagHeight(DEFAULT_CELL_HEIGHT);


                                            if ((_nowHeight + lineHeight) > _pageHeight)
                                            {
                                                GoNextPage();

                                            }


                                            _currentTableHolder.Table.AddCellRow(row);
                                            AddPageHeight(lineHeight);//다시..

                                            //_nowHeight += (DEFAULT_CELL_HEIGHT < _currentRowHeight && lineHeight < _currentRowHeight) ? _currentRowHeight :
                                            //(lineHeight < 0) ? DEFAULT_CELL_HEIGHT : lineHeight;
                                            //_nowHeight += (lineHeight < 0) ? DEFAULT_CELL_HEIGHT : lineHeight;
                                            //if (_currentTableHolderBase.Table.CellRows.Count > (_contentRowIndex + 1)) 
                                            _contentRowIndex++;//마지막에 다다를때까지 차례대로 증가한다.
                                            _addIndex++;
                                        }


                                        //foreach (HtmlNode row in contentRows)


                                    }
                                    else
                                    {
                                        for (i = 0; i < cells.Count; i++)
                                        {
                                            IsCellReplaced(_currentTableHolder, cells[i]);

                                            //_nowHeight += DEFAULT_LINE_HEIGHT * count;
                                        }
                                        int lineHeight = _currentTableHolder.GetTagHeight(DEFAULT_CELL_HEIGHT);
                                        AddPageHeight(lineHeight);//.GetLinesInNode();

                                        //ReplaceFunctions(_currentTableHolderBase);



                                        //_nowHeight += (DEFAULT_LINE_HEIGHT < _currentRowHeight) ? _currentRowHeight : DEFAULT_LINE_HEIGHT;
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            if (_nowHeight + DEFAULT_LINE_HEIGHT > _pageHeight) GoNextPage();

                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message + addErr, ex);
                        }
                    }

                    reader.Close();
                }
                if (_removeAfterMakeReport)
                {
                    File.Delete(file);
                }
            }
            
            
            SetPropertiesOnNode(_basicHtml);
            HtmlNode body = _basicHtml.GetChildNodeByName("body");
            foreach (HtmlNode page in _bodyContents)
            {
                SetPropertiesOnNode(page);
                foreach (HtmlNode child in page.Children)
                {
                    body.Children.Add(child);
                }
            }

            
            return true;
        }

        private void SetCurrentPageAsCoverPage(bool only1Cover=true)
        {

            _pageBase = _coverPageBase;
            _currentPage = _pageBase.Clone();//_pageBase는 basic form에서 내부 body부분이다.
            if (only1Cover) _pageBase = _basicPageBase;
            HtmlNode tableAround = _currentPage.GetChildNodeByName("table");

            String style = (tableAround.Attributes.ContainsKey("style")) ? tableAround.Attributes["style"] : "";

            tableAround.Attributes["style"] = style + "page-break-before:avoid;";

            _bodyContents.Add(_currentPage);//페이지를 추가한다.
            SetPropertiesOnNode(_currentPage);
            _currentContent = _currentPage.GetNodeByFunction("[%CONTENT");


            if (_currentContent == null)
            {
                throw new Exception("basic form doesn't have [%CONTENT:height] form...");
            }

            _pageHeight = GetHeightFromContentFunc(_currentContent);

            //_currentContent.Children.Clear();

            _currentContent.InnerText = "";

            _nowHeight = 0;
        }
        private void SetCurrentPageAsBasicPage()
        {


            _pageBase = _basicPageBase;
            _currentPage = _pageBase.Clone();//_pageBase는 basic form에서 내부 body부분이다.

            HtmlNode tableAround = _currentPage.GetChildNodeByName("table");

            String style = (tableAround.Attributes.ContainsKey("style")) ? tableAround.Attributes["style"] : "";

            tableAround.Attributes["style"] = style + "page-break-before:avoid;";

            _bodyContents.Add(_currentPage);//페이지를 추가한다.
            SetPropertiesOnNode(_currentPage);
            _currentContent = _currentPage.GetNodeByFunction("[%CONTENT");


            if (_currentContent == null)
            {
                throw new Exception("basic form doesn't have [%CONTENT:height] form...");
            }

            _pageHeight = GetHeightFromContentFunc(_currentContent);

            //_currentContent.Children.Clear();

            _currentContent.InnerText = "";

            _nowHeight = 0;

        }

        private void AddTitleHeight(HtmlTable htmlTable)
        {
            if (htmlTable != null)
            {
                int i = 0;
                //HtmlNode row = _currentTableHolder.Table.TitleRows[_currentTitleRowIndex];

                foreach (HtmlNode row in _currentTableHolder.Table.TitleRows)
                {
                    int lineHeight = row.GetTagHeight(DEFAULT_CELL_HEIGHT);

                    if (lineHeight > 0)
                    {
                        AddPageHeight( lineHeight);
                    }
                    //_nowHeight += (DEFAULT_CELL_HEIGHT < _currentRowHeight && lineHeight < _currentRowHeight) ? _currentRowHeight :
                    //(lineHeight < 0) ? DEFAULT_CELL_HEIGHT : lineHeight;
                    //테이블이 추가될 때나 page가 넘어갈 때 높이에 포함한다.
                }
            }
        }

        public static String GetContent(string fullTxt, string begTxt, string endTxt)
        {
            int next;
            return GetContent(fullTxt, begTxt, endTxt, out next);
        }

        public static String GetContent(string fullTxt, string begTxt, string endTxt, out int next)
        {
            int beg = fullTxt.IndexOf(begTxt);
            int end = fullTxt.IndexOf(endTxt, beg + begTxt.Length);

            next = end + endTxt.Length;
            try
            {
                return fullTxt.Substring(beg + begTxt.Length, end - beg - begTxt.Length - endTxt.Length + 1);
            }
            catch
            {
                throw;
            }
            
        }

        public static void CopyExternalFolderTo(String dirToSave)
        {

            String[] files = Directory.GetFiles(This._currentPath + "\\externals");
            if (Directory.Exists(dirToSave + "\\externals") == false)
            {
                Directory.CreateDirectory(dirToSave + "\\externals");
            }
            
            foreach (String fileToCopy in files)
            {
                String fileName = GetOnlyFileName(fileToCopy, true);
                String newFile = dirToSave + "\\externals\\" + fileName;
                
                File.Copy(fileToCopy, newFile, true);
                
                
            }
            
        }

        private void GoNextPage(bool cutTopOfNextTable=true)
        {
            _pageNum++;
            _nowHeight = 0;
            if (_currentTableHolderBase != null)
            {
                
                TableHolder newHolder = _currentTableHolderBase.Clone() as TableHolder;
                AddTitleHeight(_currentTableHolder.Table);

                for (int iRow = 0; iRow < _currentTableHolder.Table.TitleRows.Count; iRow++)
                {
                    HtmlNode srcRow = _currentTableHolder.Table.TitleRows[iRow];
                    HtmlNode dstRow = newHolder.Table.TitleRows[iRow];
                    for (int iCell = 0; iCell < srcRow.Children.Count; iCell++)//td
                    {
                        HtmlNode srcCell = srcRow.Children[iCell];
                        HtmlNode dstCell = dstRow.Children[iCell];
                        IsCellReplaced(dstCell, srcCell.InnerText);
                    }
                }
                _currentTableHolder.RemoveBottomOfTable();//기존 테이블 아랫단 삭제
                if(cutTopOfNextTable) newHolder.RemoveTopOfTable();//복사테이블 윗단 삭제.
                _currentTableHolder = newHolder;
                _currentTableHolder.RemoveContentCells();
            }
            
            _currentPage = _pageBase.Clone();//_pageBase는 basic form에서 내부 body부분이다.
            SetPropertiesOnNode(_currentPage);
            _bodyContents.Add(_currentPage);//페이지를 추가한다.

            HtmlNode tableAround = _currentPage.GetChildNodeByName("table");

            String style = (tableAround.Attributes.ContainsKey("style")) ? tableAround.Attributes["style"] : "";

            tableAround.Attributes["style"] = style + "page-break-before:always;";
           

            _currentContent = _currentPage.GetNodeByFunction("[%CONTENT");//한번 체크했으므로 다시체크할 필요 없음.

            _pageHeight = GetHeightFromContentFunc(_currentContent);
            _currentContent.InnerText = "";//.Children.Clear();
            if (_currentTableHolder != null)
            {
                _currentContent.Children.Add(_currentTableHolder);
            }
            

            
        }

        public void AddPageHeight(int height)
        {
            _nowHeight += height;
        }





       
    }
}
