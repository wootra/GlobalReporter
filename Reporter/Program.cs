using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GlobalReporter
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(params String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ReportMakerMain main = new ReportMakerMain();
            ReportHelper helper = new ReportHelper(main);
            helper.SetArgs(args);
            Application.Run(helper);
        }
    }
}
