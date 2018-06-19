using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using IOHandling;
using System.IO;
using wXmlHandlers;
using System.Text.RegularExpressions;
using System.Xml;

namespace GlobalReporter
{
    public class ReportMakerMain
    {


        FuncHandler _f = new FuncHandler();
        public String PdfReaderPath{
            get
            {
                if (_f.ExternalExePaths.ContainsKey("pdf_reader"))
                {
                    return _f.ExternalExePaths["pdf_reader"];
                }
                else
                {
                    return "";
                }
                
            }
        }
        public String PathToSave
        {
            get
            {
                return _f.PathToSave;
            }
        }
        public bool LoadReport()
        {

            

            bool ret = _f.Init();

                if (ret == true) ret = _f.MakeReport();
                else return false;

                if (ret == true) _f.SaveReport();
                else return false;

            return true;
            
        }
        public void SetArgs(string[] args)
        {
            _f.SetArgs(args);
        }

        public string Title {
            get
            {
                return _f._basicHtml.GetChildNodeByName("head").GetChildNodeByName("title").InnerText;
            }
        }

        public Uri HtmlPath {
            get
            {
                return new Uri("FILE:///"+_f.PathToSave);
            }
        }

        public String Html {
            get {
                return _f._basicHtml.OuterText;
            }
        }

        public string PdfPathToSave {
            get
            {
                return _f.PdfPathToSave;

            }
        }

        public FuncHandler Functions
        {
            get
            {
                return _f;
            }
        }

        public bool MakePdf
        {
            get
            {
                return _f._makePdf;
            }
        }

        public bool PrintNow
        {
            get
            {
                return _f._printNow;
            }
        }
    }
}
