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
using GlobalReporter;
using Pechkin;
//using Pechkin.Synchronized;
using System.Drawing.Printing;
using Common.Logging;
using System.Threading;

namespace WebkitReporter
{
    public partial class ReportPreview : Form
    {
        ReportMakerMain _main;
        
        public ReportPreview(ReportMakerMain main)
        {
            SetPath(); 
            m_closeOnVisible = false;
            InitializeComponent();
            _main = main;
            
           // ReportView.BindingContextChanged += new EventHandler(ReportView_BindingContextChanged);
            //this.Load += new EventHandler(ReportHelper_Load);
        }


        private void SetPath()
        {

        }

        private void webKitBrowser1_Load(object sender, EventArgs e)
        {
            try
            {
                bool ret = _main.LoadReport();
                if (ret)
                {
                    ShowReport();
                   
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try
                {
                    //Dispose(true);
                    //Close();
                    m_closeOnVisible = true;
                }
                catch (Exception ex2)
                {

                }
                
            }
        }

        void ReportView_VisibleChanged(object sender, System.EventArgs e)
        {
            if (m_closeOnVisible) Close();
        }

        internal void SetArgs(string[] args)
        {
            _main.SetArgs(args);

        }

        void PrintReport()
        {



            ReportView.ShowPageSetupDialog();
            ReportView.ShowPrintDialog();

            /*
            bool ret = ReadSrcData();
            if (ret == true)
            {
                ReportView.ShowPageSetupDialog();
                ReportView.ShowPrintDialog();
            }
            else
            {
                
                if (_isLoaded)
                {
                    Close();
                }
                else
                {
                    _closeOnStart = true;
                }
            }
             */
        }

        private bool ShowReport()
        {

            ReportView.Navigate("file:///" + _main.PathToSave.Replace("\\", "/"));
            //ReportView.Navigated += new WebBrowserNavigatedEventHandler(ReportView_Navigated);
            ReportView.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(ReportView_DocumentCompleted);      
            //File.Delete(newPath);
            //Directory.Delete(newDir, true);
            return true;
        }

        void ReportView_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_main.PrintNow)
            {
                
                PrintReport();
                //Thread.Sleep(5000);
                //Close();
            }
        }

        void ReportView_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

        }


        private void B_Print_Click(object sender, EventArgs e)
        {
            ReportView.PageSettings.Margins = new Margins(0, 0, 0, 0);
            //ReportView.PageSettings.PrinterSettings. = new Margins(0, 0, 0, 0);
            ReportView.ShowPageSetupDialog();
            ReportView.ShowPrintDialog();
        }

       
        private void B_Pdf_Click(object sender, EventArgs e)
        {
            
            GlobalConfig config = new GlobalConfig();
            config.SetOutputDpi(1600);
            config.SetOutputFormat(GlobalConfig.OutputFormat.Pdf);
            config.SetPaperSize(PaperKind.A4);
            
            
          
            
            SimplePechkin pechkin = new SimplePechkin(config);
            
            byte[] pdf = (pechkin).Convert(_main.HtmlPath);
            
            /*
            byte[] pdf = new Pechkin.Synchronized.SynchronizedPechkin(
new Pechkin.GlobalConfig()).Convert(
    new Pechkin.ObjectConfig()
   .SetLoadImages(true)
   .SetPrintBackground(true)
   .SetScreenMediaType(true)
   .SetCreateExternalLinks(true), _main.Html);
            */
            String pdfFile = _main.PdfPathToSave;
            if (File.Exists(pdfFile))
            {
                File.Delete(pdfFile);//원래 있으면 지우고..
                System.Threading.Thread.Sleep(3000);//3초 기다림.
            }
            try
            {
                using (FileStream file = System.IO.File.Create(pdfFile))
                {
                    file.Write(pdf, 0, pdf.Length);
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (C_OpenPdf.Checked)
            {
                String readerPath = (_main.PdfReaderPath.Length > 0)? _main.PdfReaderPath: FindAcrobatReader();

                if (readerPath.Length>0)
                {
                    IOHandling.ProcessHandler.getProcessAfterStart(readerPath, "\""+pdfFile+"\"");
                }
                else
                {
                    throw new Exception("Acrobat Reader가 깔려있지 않거나 external_exe_paths.txt에 PDF_READER항목을 비워두었습니다.");
                }
            }
            
        }

        private string FindAcrobatReader()
        {
              
            string path = RegistryHandler.getRegValue("HKEY_CLASSES_ROOT\\Software\\Adobe\\Acrobat\\Exe","(기본값)");
            path = path.Trim('"');
            return path; 
            /*
            String winDir = Environment.GetEnvironmentVariable("Windir").Replace("/","\\");
            String winDrive = winDir.Substring(0, winDir.IndexOf('\\'));
            string adobePath = winDrive + @"\Program Files (x86)\Adobe\";
            String[] adobeDirs = Directory.GetDirectories(adobePath);
            double maxVer = -1;
            string readerDir=null;
            foreach (String dir in adobeDirs)
            {
                string nowPath = dir.Substring(adobePath.Length);
                if (nowPath.Trim().ToLower().IndexOf("reader") == 0)//acrobat reader버전들 중..
                {
                    string verStr = nowPath.Trim().Substring(6).Trim();//reader를 빼고.
                    
                    string[] vertokens = verStr.Split('.');
                    if (vertokens.Length < 2) vertokens = new String[] { verStr, "0" };//강제로 .0을 넣어줌.
                    double ver;
                    if (double.TryParse(vertokens[0] + "." + vertokens[1], out ver)==false)
                    {
                        ver = 0;   
                    }
                    if (ver > maxVer)
                    {
                        maxVer = ver;
                        readerDir = dir;
                    }
                }
            }
            if (maxVer < 0) {
                throw new Exception("Acrobat Reader is NOT installed!");
            }
            String[] files = Directory.GetFiles(readerDir+"\\Reader");
            string readerExe = "";
            foreach (String file in files)
            {
                int extBeg = file.LastIndexOf('.');
                if (extBeg < 0) continue;
                string ext = file.Substring(extBeg).ToLower();
                if (ext.Equals(".exe"))
                {
                    string fileName = file.Substring(file.LastIndexOf('\\')+1).ToLower();
                    if (fileName.Equals("acrord32.exe"))
                    {
                        readerExe = file;
                        break;
                    }
                    else if (fileName.Equals("acrobatreader32.exe"))
                    {
                        readerExe = file;
                        break;
                    }
                    else if (fileName.Equals("AcroRd32.exe"))
                    {
                        readerExe = file;
                        break;
                    }

                }
            }
            return readerExe;
             */
        }


        public bool m_closeOnVisible { get; set; }
    }
}


