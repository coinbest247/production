var InvestTradingController = function () {
    this.initialize = function () {

        registerEvents();
        registerControl();

        setInterval(() => loadBotTradingPercent(), chartInterval);

        loadData(true);
    }

    this.RegisterChartInterval = function (setting) {
        chartInterval = setting;
    }

    var infoBotTrade_losts;
    var infoBotTrade_win;
    var showSpeedClock = false;
    var chartInterval = 10000;

    this.loadSpeedClock = function () {
        setTimeout(() => initZones(), 1000);
    };

    function registerControl() {
        be.registerNumber();
    }

    var registerEvents = function () {

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });
    }

    var currentProfitAmount = 0.0;

    function randomNumber(min, max) {
        return Math.random() * (max - min) + min;
    }

    function loadBotTradingPercent() {
        $.ajax({
            type: 'GET',
            url: '/admin/InvestTrading/GetBotTradingPercent',
            dataType: 'JSON',
            beforeSend: function () {
            },
            success: function (response) {
                currentProfitAmount = parseFloat(response.Rate);

                if (response.Rate == 0) {
                    $("#infoBotTrade_lots").text("0%");
                    $("#infoBotTrade_profit").text("0%");

                } else if (response.Rate < 0) {
                    $("#infoBotTrade_lots").text(response.RateString);
                    $("#infoBotTrade_profit").text("0%");

                } else {
                    $("#infoBotTrade_lots").text("0%");
                    if (response.RateString == "1,00%") {
                        var prof = randomNumber(0.1, 1).toFixed(2);
                        currentProfitAmount = prof;
                        $("#infoBotTrade_profit").text(prof + "%");
                    } else {
                        $("#infoBotTrade_profit").text(response.RateString);
                    }
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
            }
        });
    }

    function initZones() {
        if (!showSpeedClock) {
            var opts = {
                angle: -0.25,
                radiusScale: 0.9,
                lineWidth: 0.2,
                pointer: {
                    length: 0.6,
                    strokeWidth: 0.05,
                    color: '#000000'
                },
                staticLabels: {
                    font: "10px sans-serif",
                    labels: [],
                    fractionDigits: 0
                },
                staticZones: [
                    { strokeStyle: "rgb(255,0,0)", min: 0, max: 500 },
                    { strokeStyle: "rgb(200,100,0)", min: 500, max: 1000 },
                    { strokeStyle: "rgb(150,150,0)", min: 1000, max: 1500 },
                    { strokeStyle: "rgb(100,200,0)", min: 1500, max: 2000 },
                    { strokeStyle: "rgb(0,255,0)", min: 2000, max: 2530 },
                    { strokeStyle: "rgb(80,255,80)", min: 2530, max: 3500 }
                ],
                limitMax: false,
                limitMin: false,
                highDpiSupport: true
            };

            infoBotTrade_losts = new Gauge(document.getElementById("infoBotTrade-losts"));
            infoBotTrade_losts.setOptions(opts);
            infoBotTrade_losts.minValue = 0;
            infoBotTrade_losts.maxValue = 3500;
            infoBotTrade_losts.set(1750);

            infoBotTrade_win = new Gauge(document.getElementById("infoBotTrade-win"));
            infoBotTrade_win.setOptions(opts);
            infoBotTrade_win.minValue = 0;
            infoBotTrade_win.maxValue = 3500;
            infoBotTrade_win.set(1750);
        }
    };

    function loadData(isPageChanged) {

        $.ajax({
            type: "GET",
            data: {
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: "/Admin/InvestTrading/GetAllBotProfitHistoryPaging",
            dataType: "json",
            success: function (response) {

                var template = $("#table-template").html();
                var render = "";

                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        Id: item.Id,
                        ProfitPercent: item.ProfitPercent,
                        StartPrice: be.formatCurrency(item.StartPrice),
                        StopPrice: be.formatCurrency(item.StopPrice),
                        ProfitAmount: be.formatCurrency(item.ProfitAmount),
                        TypeName: item.TypeName,
                        TypeColor: item.Type == 2 ? "text-success" : "text-danger",
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        Result: item.Result,
                        ResultColor: item.IsWin ? "text-success" : "text-danger",
                        Margin: be.formatCurrency(item.Margin)
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#tbl-content').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
            },
            error: function (status) {
                console.log(status);
                be.notify('Cannot loading data', 'error')
            }
        })
    }
}