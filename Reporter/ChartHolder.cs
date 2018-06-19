using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalReporter
{
    public class ChartHolder
    {
        public String Type { get; set; }
        public String Name { get; set; }
        public String Data { get; set; }
        string errMsg = @"ERROR ON %CHART FUNCTION..
%CHART:type,name,[[label,y1],[x2,y2],[x3,y3],...]
ex>%CHART:pie,chart1,[0, 10], [1, 20], [3, 30], [4, 90], [5, 80]
the possible types are line,pie,bar
";
        public ChartHolder(String chartData)
        {
            int nameBeg = chartData.IndexOf(',');
            
            if(nameBeg<0)
            {
                throw new Exception(errMsg);
            }
            Type = chartData.Substring(0, nameBeg).ToUpper();

            int argBeg = chartData.IndexOf(',', nameBeg + 1);
            if (argBeg < 0)
            {
                throw new Exception(errMsg);
            }
            Name = chartData.Substring(nameBeg + 1, argBeg - nameBeg - 1);
            Data = chartData.Substring(argBeg + 1);
        }

        public string Text {
            get
            {
                return "runfunc(\"" + Type + "\",\"" + Name + "\",'" + Data + "');\n";
            }
        }
    }
}
