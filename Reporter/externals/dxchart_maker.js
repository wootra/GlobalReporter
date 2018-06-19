/*
 chart function만드는 법.
 function을 정의하되, 이름규칙을 draw[차트이름]Chart(차트id,데이터array) 로 한다.(주의!차트이름은 항상 대문자여야 한다.)
 예>function drawLINEchart(div_name,data_arr)
 
 */
var colors=["#660000","#006600","#000066","#666600", "#006666","#660066", "#dd6600", "#66dd00","#6600dd", "#dd0066","#00dd66"];
var back_colors = ["#66FFFF", "#FF66FF", "#FFFF66", "#6666FF", "#FF6666", "#66FF66", "#dd66FF", "#66ddFF", "#66FFdd", "#ddFF66", "#FFdd66"];
var getLegends = function (names, defcolors) {
    var legends = [];
    if(names.length!=null && typeof(names)!="string"){
        
        for(var i=0; i<names.length; i++){
            var obj = {
                text:names[i],
                color: defcolors[i % 10]
            };
            legends.push(obj);
        }
    }else{
        legends.push({
            text:names,
            color: defcolors[0]
        });
    }
    return legends;

}

var GetLinearChartDataSet = function (arrsrc) {
    var dataset =[];
    if(typeof(arrsrc)!="string" && arrsrc.length!=null && arrsrc[0].length!=null){
        if(arrsrc[0].length>1){
            
            for(var item=0; item<arrsrc.length; item++){
                var obj = {};
                obj["x"] = arrsrc[item][0];
                for(var i=1; i< arrsrc[item].length; i++){
                    obj["y"+i] = arrsrc[item][i];
                }   
                dataset.push(obj); 
            }
            
        }
    }else if(typeof(arrsrc)!="string" && arrsrc.length!=null){
        
        for(var i=0; i< arrsrc.length; i++){
            var obj = {};
            obj["x"] = i;
            obj["y1"] = arrsrc[i];
            dataset.push(obj);
        }   
        
    }

   
    return dataset;
}

var GetPieChartDataSet = function (names, arrsrc) {
    var dataset =[];
    if(typeof(arrsrc)!="string" && arrsrc.length!=null){
        
        for(var i=0; i< arrsrc.length; i++){
            var obj = {};
            obj["x"] = (names.length == 0) ? i : names[i];
            obj["y1"] = arrsrc[i];
            obj["color"] = back_colors[i%10];
            dataset.push(obj);
        }   
        
    }

   
    return dataset;
}

var getAreaChart = function (type, chart_div_name, names, arrsrc, defcolors) {

    var chart1 = new dhtmlXChart({
        view: type,
        container: chart_div_name,
        value: "#y1#",
        alpha:0.6,
        color:defcolors[0],
        xAxis:{
			//title:"Year",
			template:"#x#"
		},
		yAxis:{
			//title:"Sales per year"
	    },
        legend: {
            layout: "x",
            width: 75,
            align: "center",
            valign: "bottom",
            values: getLegends(names, defcolors),
            margin: 10
        }
    });

    return chart1;
}

var getScatterChart = function (type, chart_div_name, names, arrsrc, defcolors) {

    var chart1 = new dhtmlXChart({
        view: type,
        container: chart_div_name,
        xValue: "#x#",
        yValue: "#y#",
        tooltip: "#type#",
        item: {
            borderColor: "#border_color#",
            color: "#color#",
            type: 's'
        },
        yAxis: {
            title: "Value Y"
        },
        xAxis: {
            title: "Value X"
        },
        legend: {
            layout: "x",
            width: 75,
            align: "center",
            valign: "bottom",
            values: getLegends(names, defcolors),
            margin: 10
        }
    });

    return chart1;
}

var Get2DChartDataSet = function (names, arrsrc) {
    var depth = getDepth(arrsrc);
    var dataset = [];
    if (depth == 3) {//3중배열. ex> [ [ [ x,y ],[ x,y ] ],[ [ x,y ],[ x,y ] ] ]
        for (var i = 0; i < arrsrc.length; i++) {
            for (var j = 0; j < arrsrc[i].length; j++) {
                var obj = { type: names[i] };
                obj["x"] = arrsrc[i][j][0];
                obj["y"] = arrsrc[i][j][1];
                obj["color"] = colors[i];
                obj["border_color"] = "#dddddd";// colors[i];
                dataset.push(obj);
            }
        }
    } else if (depth == 2) {
        for (var j = 0; j < arrsrc.length; j++) {
            var obj = { type: "value" };
            obj["x"] = arrsrc[j][0];
            obj["y"] = arrsrc[j][1];
            dataset.push(obj);
        }
    }
    return dataset;
}

var getLinearChart = function(type, chart_div_name, names, arrsrc, defcolors){
    
    var chart1 = new dhtmlXChart({
        view: type,
        container: chart_div_name,
        value: "#y1#",
        item: {
            borderColor: "#1293f8",
            color: "#ffffff"
        },
        line: {
            color: defcolors[0],
            width: 3
        },
        tooltip: {
            template: "#y1#"
        },
        offset: 0,
        xAxis: {
            template: "#x#"
        },
        yAxis: {
            //start: 0,
            //step: 1,
            //end: 10,
            template: function(value) {
                return value % 5 ? "": value;
            }
        },
        padding: {
            left: 35,
            bottom: 50
        },
        origin: 0,
        legend: {
            layout: "x",
            width: 75,
            align: "center",
            valign: "bottom",
            values: getLegends(names, defcolors),
            margin: 10
        }
    });

    return chart1;
}

var addLineSeries = function(chart1, names, arrsrc){
    if(typeof(arrsrc)!="string" && arrsrc.length!=null && arrsrc[0].length!=null){
        if(arrsrc[0].length>2){//multiple lines
            for(var i=2; i<arrsrc[0].length; i++)
            chart1.addSeries({
                value: "#y"+i+"#",
                item: {
                    borderColor: "#66cc00",
                    color: "#ffffff"
                },
                line: {
                    color: colors[i-1],
                    width: 3
                },
                tooltip: {
                    template: "#y"+i+"#"
                }
            });
        }
    }
}

var getBarChart = function (type, chart_div_name, names, arrsrc, defcolors) {

    var chart1 = new dhtmlXChart({
        view: type,
        container: chart_div_name,
        value: "#y1#",

        color: defcolors[0],
        width: 30,
        tooltip: {
            template: "#y1#"
        },
        offset: 0,
        xAxis: {
           template: function (value) {
                return (typeof(value)=="object")? value.x : value.toString();
            }
        },
        label: names[0],
        yAxis: {
            //start: 0,
            //step: 1,
            //end: 10,
            template: function (value) {
                return (typeof(value)=="object")? value.x : value.toString();
            },
            title: "y"
        },
        padding: {
            left: 35,
            bottom: 50
        },
        origin: 0,
        legend: {
            layout: "x",
            width: 75,
            align: "center",
            valign: "bottom",
            values: getLegends(names, defcolors),
            margin: 10
        }
    });

    return chart1;
}

var addBarSeries = function (chart1, names, arrsrc) {
    if (typeof (arrsrc) != "string" && arrsrc.length != null && arrsrc[0].length != null) {
        if (arrsrc[0].length > 2) {//multiple lines
            for (var i = 2; i < arrsrc[0].length; i++)
                chart1.addSeries({
                    value: "#y" + i + "#",
                    color: back_colors[i - 1],
                    width: 30,
                    label:names[i-1],
                    tooltip: {
                        template: "#y" + i + "#"
                    }
                });
        }
    }
}

var addAreaSeries = function (chart1, names, arrsrc, defcolors) {
    if (typeof (arrsrc) != "string" && arrsrc.length != null && arrsrc[0].length != null) {
        if (arrsrc[0].length > 2) {//multiple lines
            for (var i = 2; i < arrsrc[0].length; i++)
                chart1.addSeries({
                    value: "#y" + i + "#",
                    color: defcolors[i - 1],
                    label: "#x#"
                });
        }
    }
}


var drawBARChart = function (chart_div_name, names, arrsrc) {
    var data = GetLinearChartDataSet(arrsrc);
    var chart1 = getBarChart("bar", chart_div_name, names, arrsrc, back_colors);
    addBarSeries(chart1, names, arrsrc);
    chart1.parse(data, "json");
    return chart1;
}


var drawBARHChart = function (chart_div_name, names, arrsrc) {
    var data = GetLinearChartDataSet(arrsrc);
    var chart1 = getBarChart("barH", chart_div_name, names, arrsrc, back_colors);
    addBarSeries(chart1, names, arrsrc);
    chart1.parse(data, "json");
    return chart1;
}


var drawSBARChart = function (chart_div_name, names, arrsrc) {
    var data = GetLinearChartDataSet(arrsrc);
    var chart1 = getBarChart("stackedBar", chart_div_name, names, arrsrc, back_colors);
    addBarSeries(chart1, names, arrsrc);
    chart1.parse(data, "json");
    return chart1;
}


var drawSBARHChart = function (chart_div_name, names, arrsrc) {
    var data = GetLinearChartDataSet(arrsrc);
    var chart1 = getBarChart("stackedBarH", chart_div_name, names, arrsrc, back_colors);
    addBarSeries(chart1, names, arrsrc);
    chart1.parse(data, "json");
    return chart1;
}


var drawAREAChart = function (chart_div_name, names, arrsrc) {
    var data = GetLinearChartDataSet(arrsrc);
    var chart1 = getAreaChart("area", chart_div_name, names, arrsrc, back_colors);
    addAreaSeries(chart1, names, arrsrc, back_colors);
    chart1.parse(data, "json");
    return chart1;
}

var drawSAREAChart = function (chart_div_name, names, arrsrc) {
    var data = GetLinearChartDataSet(arrsrc);
    var chart1 = getAreaChart("stackedArea", chart_div_name, names, arrsrc, back_colors);
    addAreaSeries(chart1, names, arrsrc, back_colors);
    chart1.parse(data, "json");
    return chart1;
}




var drawLINEChart = function (chart_div_name, names, arrsrc) {
    var data = GetLinearChartDataSet(arrsrc);
    var chart1 = getLinearChart("line", chart_div_name, names, arrsrc, colors);
    addLineSeries(chart1, names, arrsrc);
    chart1.parse(data, "json");
    return chart1;
}


var drawSPLINEChart = function (chart_div_name, names, arrsrc) {
    var data = GetLinearChartDataSet(arrsrc);
    var chart1 = getLinearChart("spline", chart_div_name, names, arrsrc, colors);
    addLineSeries(chart1, names, arrsrc);
    chart1.parse(data, "json");
    return chart1;
}

var getDepth = function (arrsrc) {
    if (typeof (arrsrc) == "object" && arrsrc.length != null) {
        return getDepth(arrsrc[0])+1;
    } else {
        return 0;
    }
}



var drawSCATTERChart = function (chart_div_name, names, arrsrc) {
    var data = Get2DChartDataSet(names, arrsrc);
    var chart1 = getScatterChart("scatter", chart_div_name, names, arrsrc, colors);
    addLineSeries(chart1, names, arrsrc);
    chart1.parse(data, "json");
    return chart1;
}

var GetPieChart = function (type, chart_div_name) {
    var chart = new dhtmlXChart({
        view: type,
        container: chart_div_name,
        value: "#y1#",
        color: "#color#",
        legend: {
            width: 65,
            align: "right",
            valign: "top",
            marker: {
                type: "round",
                width: 15
            },
            template: "#x#"
        },
        pieInnerText: "<b>#y1#</b>"
    });
    return chart;
}

var GetRadarChart = function (type, chart_div_name) {
    var chart = new dhtmlXChart({
        view: type,
        container: chart_div_name,
        value: "#y1#",
        tooltip: "#y1#",
        alpha: 0.2,
        line: {
            color: "#3399ff",
            width: 1
        },
        xAxis: {
            template: "#x#"
        },
        yAxis: {
            lineShape: 'arc'
        },
        fill: true,
        color: "#3399ff",
        disableItems: false
    });
    return chart;
}

var drawPIEChart = function (chart_div_name, names, arrsrc) {
    var pieData = GetPieChartDataSet(names, arrsrc);
    var pieChart = GetPieChart("pie", chart_div_name);
    pieChart.parse(pieData, "json");
}
var draw3DPIEChart = function (chart_div_name, names, arrsrc) {
    var pieData = GetPieChartDataSet(names, arrsrc);
    var pieChart = GetPieChart("pie3D", chart_div_name);
    pieChart.parse(pieData, "json");
}

var drawDONUTChart = function (chart_div_name, names, arrsrc) {
    var pieData = GetPieChartDataSet(names, arrsrc);
    var pieChart = GetPieChart("donut", chart_div_name);
    pieChart.parse(pieData, "json");
}


var drawRADARChart = function (chart_div_name, names, arrsrc) {
    var pieData = GetPieChartDataSet(names, arrsrc);
    var pieChart = GetRadarChart("radar", chart_div_name);
    pieChart.parse(pieData, "json");
}

var runfunc = function (func_name, div_name, funcargs) {

    if (document.getElementById(div_name) != null) {

        var func = eval("draw" + func_name + "Chart;");
        if (func != null) {
            eval("draw" + func_name + "Chart('" + div_name + "'," + funcargs + ");");
        }
    }
}

 /*

var getBarOpt = function () {
    var opt = {

        //Boolean - If we show the scale above the chart data			
        scaleOverlay: false,

        //Boolean - If we want to override with a hard coded scale
        scaleOverride: false,

        //** Required if scaleOverride is true **
        //Number - The number of steps in a hard coded scale
        scaleSteps: null,
        //Number - The value jump in the hard coded scale
        scaleStepWidth: null,
        //Number - The scale starting value
        scaleStartValue: null,

        //String - Colour of the scale line	
        scaleLineColor: "rgba(0,0,0,.1)",

        //Number - Pixel width of the scale line	
        scaleLineWidth: 1,

        //Boolean - Whether to show labels on the scale	
        scaleShowLabels: true,

        //Interpolated JS string - can access value
        scaleLabel: "<%=value%>",

        //String - Scale label font declaration for the scale label
        scaleFontFamily: "'Arial'",

        //Number - Scale label font size in pixels	
        scaleFontSize: 12,

        //String - Scale label font weight style	
        scaleFontStyle: "normal",

        //String - Scale label font colour	
        scaleFontColor: "#666",

        ///Boolean - Whether grid lines are shown across the chart
        scaleShowGridLines: true,

        //String - Colour of the grid lines
        scaleGridLineColor: "rgba(0,0,0,.05)",

        //Number - Width of the grid lines
        scaleGridLineWidth: 1,

        //Boolean - If there is a stroke on each bar	
        barShowStroke: true,

        //Number - Pixel width of the bar stroke	
        barStrokeWidth: 2,

        //Number - Spacing between each of the X value sets
        barValueSpacing: 5,

        //Number - Spacing between data sets within X values
        barDatasetSpacing: 1,

        //Boolean - Whether to animate the chart
        animation: false,

        //Number - Number of animation steps
        animationSteps: 1,

        //String - Animation easing effect
        animationEasing: "easeOutQuart",

        //Function - Fires when the animation is complete
        onAnimationComplete: null

    };
    return opt;
}
var getLineOpt = function () {
    var opt = {

        //Boolean - If we show the scale above the chart data			
        scaleOverlay: false,

        //Boolean - If we want to override with a hard coded scale
        scaleOverride: false,

        //** Required if scaleOverride is true **
        //Number - The number of steps in a hard coded scale
        scaleSteps: null,
        //Number - The value jump in the hard coded scale
        scaleStepWidth: null,
        //Number - The scale starting value
        scaleStartValue: null,

        //String - Colour of the scale line	
        scaleLineColor: "rgba(0,0,0,.1)",

        //Number - Pixel width of the scale line	
        scaleLineWidth: 1,

        //Boolean - Whether to show labels on the scale	
        scaleShowLabels: true,

        //Interpolated JS string - can access value
        scaleLabel: "<%=value%>",

        //String - Scale label font declaration for the scale label
        scaleFontFamily: "'Arial'",

        //Number - Scale label font size in pixels	
        scaleFontSize: 12,

        //String - Scale label font weight style	
        scaleFontStyle: "normal",

        //String - Scale label font colour	
        scaleFontColor: "#666",

        ///Boolean - Whether grid lines are shown across the chart
        scaleShowGridLines: true,

        //String - Colour of the grid lines
        scaleGridLineColor: "rgba(0,0,0,.05)",

        //Number - Width of the grid lines
        scaleGridLineWidth: 1,

        //Boolean - Whether the line is curved between points
        bezierCurve: true,

        //Boolean - Whether to show a dot for each point
        pointDot: true,

        //Number - Radius of each point dot in pixels
        pointDotRadius: 3,

        //Number - Pixel width of point dot stroke
        pointDotStrokeWidth: 1,

        //Boolean - Whether to show a stroke for datasets
        datasetStroke: true,

        //Number - Pixel width of dataset stroke
        datasetStrokeWidth: 2,

        //Boolean - Whether to fill the dataset with a colour
        datasetFill: true,

        //Boolean - Whether to animate the chart
        animation: false,

        //Number - Number of animation steps
        animationSteps: 60,

        //String - Animation easing effect
        animationEasing: "easeOutQuart",

        //Function - Fires when the animation is complete
        onAnimationComplete: null

    }
    return opt;
}
var getPieOpt = function(){
    var opt = {
        //Boolean - Whether we should show a stroke on each segment
        segmentShowStroke: true,

        //String - The colour of each segment stroke
        segmentStrokeColor: "#fff",

        //Number - The width of each segment stroke
        segmentStrokeWidth: 2,

        //Boolean - Whether we should animate the chart	
        animation: false,

        //Number - Amount of animation steps
        animationSteps: 1,

        //String - Animation easing effect
        animationEasing: "easeOutBounce",

        //Boolean - Whether we animate the rotation of the Pie
        animateRotate: true,

        //Boolean - Whether we animate scaling the Pie from the centre
        animateScale: false,

        //Function - Will fire on animation completion.
        onAnimationComplete: null
    }
    return opt;
}
*/