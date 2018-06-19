/*
 chart function만드는 법.
 function을 정의하되, 이름규칙을 draw[차트이름]Chart(차트id,데이터array) 로 한다.(주의!차트이름은 항상 대문자여야 한다.)
 예>function drawLINEchart(div_name,data_arr)
 
 */

var GetPairLinearData=function(datasrc, data_index){
    var data = [];
    for(var i=0; i<datasrc.length; i++){
        var obj = 
        {
            "x": datasrc[i][0],
            "y": datasrc[i][data_index]
        }
        data.push(obj); 
    }
    return data;
}
var GetSingleLinearData=function(datasrc){
    var data = [];
    for(var i=0; i<datasrc.length; i++){
        var obj = 
        {
            "x": i,
            "y": datasrc[i]
        }
        data.push(obj); 
    }
    return data;
}
var GetLinearChartDataSet = function (arrsrc) {
    var dataset =[];
    if(arrsrc.length!=null & arrsrc[0].length!=null){
        if(arrsrc[0].length>1){
            for(i=1; i< arrsrc[0].length; i++){
                var obj=
                {
                    "className": ".main.l"+i,
                    "data": GetPairLinearData(arrsrc, i)
                }
                dataset.push(obj);
            }    
        }
    }else if(arrsrc.length!=null){
        var obj=
        {
            "className": ".pizza",
            "data":GetSingleLinearData(arrsrc, i)
        }
        dataset.push(obj);
    }
    
    

    var data = {
      "xScale": "ordinal",
      "yScale": "linear",
      "main": dataset
    };
   
    return data;
}


var drawBARChart = function (chart_div_name, arrsrc) {
   var chartData = GetLinearChartDataSet(arrsrc);
   var chart = new xChart('bar', chartData, '#'+chart_div_name);
}
var drawLINEChart = function (chart_div_name, arrsrc) {
     
    var chartData = GetLinearChartDataSet(arrsrc);
    var chart = new xChart('line-dotted', chartData, '#'+chart_div_name);
    
    return chart;
}

var GetPieChartData = function (arrsrc) {

    var dataset = [];
    var labels = [];
    var colors = ["#F38630", "#86F330", "#8630F3", "#F33086", "#30F386", "#3086F3"];
    for (i = 0; i < arrsrc.length; i++) {//다음부터는 데이터임.
        label.push(arrsrc[i]);
        var jdata = {
            value: arrsrc[i],
            color: colors[i%(colors.length)],
        }
        dataset.push(jdata);
    }
    return {
        labels: labels,
        datasets: dataset
    };
    //return dataset;
}

var drawPIEChart = function (chart_div_name, arrsrc) {
   
}

var runfunc = function (func_name, div_name, funcargs) {
    eval("draw" + func_name + "Chart(" + div_name + "," + funcargs + ");");
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