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

namespace GlobalReporter
{
    public partial class ReportHelper : Form
    {
        ReportMakerMain _main;
            
        public ReportHelper(ReportMakerMain main)
        {
            InitializeComponent();
            _main = main;
            this.Load += new EventHandler(ReportHelper_Load);
        }

        
        void ReportHelper_Load(object sender, EventArgs e)
        {
            try
            {
                bool ret = _main.LoadReport();
                if (ret)
                {
                    if (ret == true) ShowReport();
                    else Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
            
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


            ReportView.Navigate("file://" + _main.PathToSave.Replace("\\", "/"));
            return true;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            ReportView.ShowPageSetupDialog();
            ReportView.ShowPrintDialog();
        }


      
    }

}