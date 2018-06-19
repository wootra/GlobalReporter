using System;
using System.Text;
using System.Diagnostics;
using System.Timers;
namespace IOHandling
{
    public class RegistryHandler
    {
        /// <summary>
        /// start부터 시작하여 공백 아닌 부분을 찾는다.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private static int FindNotSpace(int start, String val)
        {
            int len = val.Length;
            int i = start;
            for (; i < len; i++)
            {
                if (val[i] == ' ' || val[i] == '\n' || val[i] == '\t' || val[i] == '\r')//if white space
                {
                    continue;
                }
                else
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// n번째 공백이 아닌 곳을 찾는다.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="nTh"></param>
        /// <returns></returns>
        private static int FindNthNotSpace(String val, int nTh)
        {
            int startIndex = 0;
            int notSpace = 0;
            int i = 0;
            for (; i < nTh; i++)
            {
                if (startIndex < 0) return -1;
                notSpace = FindNotSpace(startIndex, val);
                if (notSpace < 0) return -1;
                startIndex = val.IndexOf(' ', notSpace);
            }
            return notSpace;
        }

        /// <summary>
        /// 공백으로 검색하여 n번째 문자열을 찾는다.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="nTh"></param>
        /// <returns></returns>
        private static String FindNthString(String val, int nTh)
        {
            int startStr = FindNthNotSpace(val, nTh);
            if (startStr < 0) return "";
            int endStr = val.IndexOf(' ', startStr);
            if (endStr < 0) endStr = val.Length;
            return val.Substring(startStr, endStr - startStr);
        }

        private static String FindNthStringToEnd(String val, int nTh)
        {
            int startStr = FindNthNotSpace(val, nTh);
            if (startStr < 0) return "";
            
            return val.Substring(startStr);
        }

        public static String getRegValue(String keyPath, String keyName)
        {
            Process process = ProcessHandler.getProcessAfterStart("reg", "query " + keyPath);
            String outStr = ProcessHandler.readOutputAndClose(process);
            String[] lines = outStr.Split("\n".ToCharArray());
            for (int num = 0; num < lines.Length; num++)
            {
                String aLine = lines[num].Replace("\r", "");
                if (aLine.Length == 0) continue;
                String key = FindNthString(aLine, 1);
                String value = FindNthStringToEnd(aLine, 3);
                if (key.Equals(keyName)) return value;
            }
            return null;
        }

        public static Timer _timer;
        public static Process _process;
        static Boolean _isOk = true;
        public static Boolean addReg(String regPath, String regValueName, String value, Boolean isForced)
        {
            String arg = (regValueName == null || regValueName.Length == 0) ? "" : " /v " + regValueName;
            arg += (value == null || value.Length == 0) ? "" : " /d  " + value;
            arg += (isForced) ? " /f" : "";
            _timer = new Timer(1000);
            _timer.AutoReset = false;
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            Process process = ProcessHandler.getProcessBeforeStart("reg", "add " + regPath + arg);
            _process = process;

            _timer.Start();
            process.Start();
            ProcessHandler.readOutputAndClose(process);

            _timer.Close();
            return _isOk;
        }

        public static Boolean delReg(String regPath, String regValueName, Boolean isForced)
        {
            String arg = (regValueName == null || regValueName.Length == 0) ? "" : " /v " + regValueName;
            arg += (isForced) ? " /f /va" : "";
            _timer = new Timer(1000);
            _timer.AutoReset = false;
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            Process process = ProcessHandler.getProcessBeforeStart("reg", "delete " + regPath + arg);
            _process = process;

            _timer.Start();
            process.Start();
            ProcessHandler.readOutputAndClose(process);

            _timer.Close();
            return _isOk;
        }

        static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _isOk = false;
            _process.Kill();
        }
    }
}
