using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GlobalReporter;
using System.IO;
using System.Reflection;
using Pechkin;
//using Pechkin.Synchronized;
using System.Drawing.Printing;
// using Common.Logging;

namespace WebkitReporter
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ReportMakerMain main = new ReportMakerMain();
            main.SetArgs(args);

            

            var dllDirectory = main.Functions._currentPath+"\\Webkit";
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH").Replace(";"+dllDirectory,"") + ";" + dllDirectory);

            //LoadAllDlls(Directory.GetCurrentDirectory()+"\\Webkit");
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (main.MakePdf)
            {
                bool ret = main.LoadReport();
                GlobalConfig config = new GlobalConfig();
                config.SetOutputDpi(1600);
                config.SetOutputFormat(GlobalConfig.OutputFormat.Pdf);
                config.SetPaperSize(PaperKind.A4);




                SimplePechkin pechkin = new SimplePechkin(config);

                byte[] pdf = (pechkin).Convert(main.HtmlPath);

                /*
                byte[] pdf = new Pechkin.Synchronized.SynchronizedPechkin(
    new Pechkin.GlobalConfig()).Convert(
        new Pechkin.ObjectConfig()
       .SetLoadImages(true)
       .SetPrintBackground(true)
       .SetScreenMediaType(true)
       .SetCreateExternalLinks(true), _main.Html);
                */
                String pdfFile = main.PdfPathToSave;
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

            }
            else
            {
                ReportPreview helper = new ReportPreview(main);


                Application.Run(helper);
            }
            
        }

        private static void LoadAllDlls(string p)
        {
            String[] files = Directory.GetFiles(p);
            foreach (String file in files)
            {
                int extBeg = file.LastIndexOf('.');
                if(extBeg>0){
                    string ext = file.Substring(extBeg + 1).ToLower();
                    if (ext.Equals("dll"))
                    {
                        Assembly.LoadFrom(file);
                    }
                }

            }
            String[] dirs = Directory.GetDirectories(p);
            foreach (String dir in dirs)
            {
                LoadAllDlls(dir);
            }
            
            
        }
    }
}
