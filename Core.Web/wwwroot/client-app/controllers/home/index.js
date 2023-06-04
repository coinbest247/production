var HomeController = function () {
    this.initialize = function () {
        registerControls();
        registerEvents();
    }


    function registerControls() {
        chartToken();
    }

    function registerEvents() {
        
    }

    function chartToken() {
        am4core.useTheme(am4themes_animated);
        am4core.addLicense('ch-custom-attribution');
        am4core.options.autoSetClassName = true;

        var chart = am4core.create("chartdiv", am4charts.PieChart3D);
        chart.responsive.useDefault = false
        chart.responsive.enabled = true;
        chart.hiddenState.properties.opacity = 0;

        chart.data = [
            {
                txt: "Claim to Earn",
                val: 80000000000000,
                "color": am4core.color("#D6323C")
            }, {
                txt: "Fund For Children",
                val: 120000000000000,
                "color": am4core.color("#0067ff")
            }, {
                txt: "Future Trade",
                val: 100000000000000,
                "color": am4core.color("#ff9800")
            }, {
                txt: "Future Refund",
                val: 100000000000000,
                "color": am4core.color("#F7E525")
            }, {
                txt: "GameFi",
                val: 150000000000000,
                "color": am4core.color("#48d6d2")
            }, {
                txt: "Publish Sale",
                val: 150000000000000,
                "color": am4core.color("#08c56d")
            }, {
                txt: "Marketing",
                val: 50000000000000,
                "color": am4core.color("#8f8d8d")
            }, {
                txt: "Dev",
                val: 100000000000000,
                "color": am4core.color("#cce40d")
            }, {
                txt: "Pool",
                val: 150000000000000,
                "color": am4core.color("#a924be")
            }];

        chart.innerRadius = am4core.percent(40);
        chart.depth = 100;

        let custom_color_arr = [
            "#D6323C",
            "#0067ff",
            "#ff9800",
            "#F7E525",
            "#48d6d2",
            "#08c56d",
            "#8f8d8d",
            "#cce40d",
            "#a924be"
        ] //custom color arr for chart legends

        var pieSeries = chart.series.push(new am4charts.PieSeries3D());
        pieSeries.dataFields.value = "val";
        pieSeries.dataFields.depthValue = "val";
        pieSeries.dataFields.category = "txt";
        pieSeries.slices.template.cornerRadius = 5;
        pieSeries.ticks.template.disabled = true;
        pieSeries.labels.template.fill = am4core.color("white");
        pieSeries.alignLabels = false;
        pieSeries.labels.template.text = "{value.percent.formatNumber('#.')}%";

        pieSeries.slices.template.propertyFields.fill = "color";

        // Create custom legend
        chart.events.on("ready", function (event) {
            // populate our custom legend when chart renders
            chart.customLegend = document.getElementById('legend');
            pieSeries.dataItems.each(function (row, i) {
                var color = custom_color_arr[i]
                var percent = Math.round(row.values.value.percent * 100) / 100;
                var value = numberWithCommas(row.value) + " B247";
                legend.innerHTML += '<div class="legend-item" id="legend-item-' + i + ';" onmouseover="hoverSlice(' + i + ');" onmouseout="blurSlice(' + i + ');" style="color: ' + color + ';"><div class="legend-marker" style="background: ' + color + '"></div>' + row.category + ' | ' + percent + '%<div class="legend-value">' + value +  '</div></div>';
            });
        });

        function numberWithCommas(num) {
            return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        }

        function toggleSlice(item) {
            var slice = pieSeries.dataItems.getIndex(item);
            if (slice.visible) {
                slice.hide();
            } else {
                slice.show();
            }
        }

        function hoverSlice(item) {
            var slice = pieSeries.slices.getIndex(item);
            slice.isHover = true;
        }

        function blurSlice(item) {
            var slice = pieSeries.slices.getIndex(item);
            slice.isHover = false;

        }
    }
}