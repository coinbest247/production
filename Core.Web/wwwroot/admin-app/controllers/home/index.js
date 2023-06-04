var HomeController = function () {
    this.initialize = function () {

        registerEvents();

        $(".running timer").hide();

        $(".running .labels").hide();

        loadBotTradeData(true);

        //GetTokenPriceData();

        loadClaimDailyStatus();
    }

    function registerEvents() {
        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadBotTradeData(true);
        });

        $('#btnAirdrop').on('click', function (e) {
            e.preventDefault();
            validateAirdropInfo();
        });

        $('#btnClaim').on('click', function (e) {
            e.preventDefault();
            ClaimDaily();
        });

    };

    function loadClaimDailyStatus() {
        $.ajax({
            type: 'GET',
            url: '/Admin/Wallet/GetClaimDailyStatus',
            dataType: 'json',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                console.log(response);
                if (response.IsClaimed) {
                    $('#btnClaim').hide();
                    $("#claimCountdown").show(); $(".running timer").show();
                    $(".running .labels").show();
                    $(".jumbotron").attr("data-date", response.NextClaim);
                } else {
                    $('#btnClaim').show();
                    $("#claimCountdown").hide();
                    $(".running timer").hide();
                    $(".running .labels").hide();
                }

                //be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        }).done(function (data) {
            debugger;
            $("#js-countdown").html("");
            var s = document.createElement("script");
            s.type = "text/javascript";
            s.src = "/admin-app/controllers/home/multi-countdown.js";
            $("#js-countdown").append(s);
        });
    }

    function ClaimDaily() {
        debugger;
        be.confirm('Claim daily reward', 'Are you sure to claim today reward ' + '?', function () {
            $.ajax({
                type: "GET",
                url: "/Admin/Wallet/ClaimDailyReward",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                dataType: "JSON",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    if (response.Success) {

                        be.success('Claim', 'Claim daily reward is success', function () {
                            window.location.reload();
                        });

                    } else {
                        be.error('Claim', response.Message);
                    }

                    be.stopLoading();
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function validateAirdropInfo() {
        var data = {
            Password: $('#txtPassword').val(),
            Email: $('#txtEmail').val(),
            Sponsor: $('#txtSponsor').val(),
        };

        var isValid = true;

        if (!data.Sponsor) {
            isValid = be.notify('Sponsor is required!!!', 'error');
        }

        if (!data.Password) {
            isValid = be.notify('Password is required!!!', 'error');
        }

        if (data.Password != data.ConfirmPassword) {
            isValid = be.notify('Password did not match!!!', 'error');
        }

        if (!data.Email) {
            isValid = be.notify('Email is required!!!', 'error');
        }
        else {
            data.Email = String(data.Email).toLowerCase();
            if (!validateEmail(data.Email)) {
                isValid = be.notify('Email incorrect format!!!', 'error');
            }
        }

        if (isValid) {

            $.ajax({
                type: 'POST',
                headers: {
                    "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                data: { registerVm: data, "g-recaptcha-response": captcharesponse },
                url: '/Admin/Account/Register',
                dataType: 'json',
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    if (response.Success) {
                        be.success('Register is success', response.Message, function () {
                            window.location.href = '/login';
                        });
                    }
                    else {
                        be.error('Register is failed', response.Message);
                    }

                    be.stopLoading();
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                },
            });
        }
    }

    function loadCurrentLucky() {
        $.ajax({
            type: 'GET',
            url: '/Admin/Exchange/GetCurrentLucky',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                $('.PreviousLucky').html(response.PreviousLucky);
                $('.SaleAmount').html(be.formatNumber(response.SaleAmount, 4));
                $('.RewardAmount').html(be.formatNumber(response.RewardAmount, 4));
                $('.TotalJoin').html(response.TotalJoin);
                $('.StartedEvent').html(be.dateFormatJson(response.StartedEvent));
                $('.FinishedEvent').html(be.dateFormatJson(response.FinishedEvent));

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadSaleReport() {
        $.ajax({
            type: 'GET',
            url: '/Admin/Exchange/GetSaleReport',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                //be.stopLoading();

                $('.SeedRoundSupplied').html(be.formatCurrency(response.SeedRoundSupplied));
                $('.SeedRoundRemaining').html(be.formatCurrency(response.SeedRoundRemaining));
                $('.SeedRoundSuppliedPercent').html(response.SeedRoundSuppliedPercent);

                $('.AngelRoundSupplied').html(be.formatCurrency(response.AngelRoundSupplied));
                $('.AngelRoundRemaining').html(be.formatCurrency(response.AngelRoundRemaining));
                $('.AngelRoundSuppliedPercent').html(response.AngelRoundSuppliedPercent);

                $('.PrivateRoundSupplied').html(be.formatCurrency(response.PrivateRoundSupplied));
                $('.PrivateRoundRemaining').html(be.formatCurrency(response.PrivateRoundRemaining));
                $('.PrivateRoundSuppliedPercent').html(response.PrivateRoundSuppliedPercent);

                $('.PublishRoundSupplied').html(be.formatCurrency(response.PublishRoundSupplied));
                $('.PublishRoundRemaining').html(be.formatCurrency(response.PublishRoundRemaining));
                $('.PublishRoundSuppliedPercent').html(response.PublishRoundSuppliedPercent);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                //be.stopLoading();
            }
        });
    }

    function loadStakingData(top) {
        $.ajax({
            type: 'GET',
            data: {
                top: top
            },
            url: '/Admin/Staking/GetLeaderboard',
            dataType: 'json',
            beforeSend: function () {
            },
            success: function (response) {

                var template = $('#table-template-staking').html();
                var render = "";

                $.each(response, function (i, item) {
                    render += Mustache.render(template, {
                        AppUserName: item.AppUserName,
                        TypeName: item.TypeName,
                        PackageName: item.PackageName,
                        PackageImage: be.getImgPackage(item.Package),
                        InterestRate: be.formatNumber(item.InterestRate, 2),
                        ReceiveAmount: be.formatNumber(item.ReceiveAmount, 4),
                        ReceiveLatest: be.dateTimeFormatJson(item.ReceiveLatest),
                        ReceiveTimes: item.ReceiveTimes,
                        StakingAmount: be.formatNumber(item.StakingAmount, 2),
                        StakingTimes: item.StakingTimes,
                        TimeLineName: item.TimeLineName,
                        PublishKey: item.PublishKey,
                        TransactionHash: item.TransactionHash,
                        Sponsor: item.Sponsor,
                        DateCreated: be.dateFormatJson(item.DateCreated),
                    });
                });

                $('#tbl-content-staking').html(render);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
            }
        });
    }

    function tradingviewWidget() {
        var data = {
            'symbols': [
                {
                    'description': 'BTC/USDT',
                    'proName': 'BINANCE:BTCUSDT'
                },
                {
                    'description': 'ETH/USDT',
                    'proName': 'BINANCE:ETHUSDT'
                },
                {
                    'description': 'BNB/USDT',
                    'proName': 'BINANCE:BNBUSDT'
                },
                {
                    'description': 'TRX/USDT',
                    'proName': 'BINANCE:TRXUSDT'
                },
                {
                    'description': 'DOGE/USDT',
                    'proName': 'DOGE'
                }
            ],
            'showSymbolLogo': true,
            'colorTheme': 'dark',
            'isTransparent': true,
            'displayMode': 'regular',
            'locale': 'en',
            'container_id': 'tradingview_0d214'
        };
        const script = document.createElement('script');
        script.src = 'https://s3.tradingview.com/external-embedding/embed-widget-ticker-tape.js'
        script.async = true;
        script.innerHTML = JSON.stringify(data);

        document.getElementById("ticker-tape").appendChild(script);
    }

    function loadBotTradeData(isPageChanged) {

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

                $('#lblTotalRecords').text(response.RowCount);

                $("#tbl-content").html(render);

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadBotTradeData();
                    }, isPageChanged);
            },
            error: function (status) {
                console.log(status);
                be.notify('Cannot loading data', 'error')
            }
        })
    }


    var priceArr = [];
    var categoryArr = [];

    function GetTokenPriceData() {
        $.ajax({
            type: "GET",
            data: {
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },

            url: "/Admin/Report/GetAllTokenPrice",
            dataType: "json",
            success: function (response) {
                priceArr = response.Prices;
                categoryArr = response.Dates;
                initChartsWidget1();
                $(".CurrentPrice").html("$" + be.formatNumber(response.CurrentPrice, 4))
            },
            error: function (status) {
                be.notify('Cannot loading data', 'error')
            }
        })
    }

    function initChartsWidget1() {
        var element = document.getElementById("kt_charts_widget_token_price_chart");

        if (!element) {
            return;
        }

        var chart = {
            self: null,
            rendered: false
        };

        var initChart = function () {
            var height = parseInt(KTUtil.css(element, 'height'));
            var labelColor = KTUtil.getCssVariableValue('--kt-gray-500');
            var borderColor = KTUtil.getCssVariableValue('--kt-gray-200');
            var baseColor = KTUtil.getCssVariableValue('--kt-success');
            var secondaryColor = KTUtil.getCssVariableValue('--kt-gray-900');

            var options = {
                series: [{
                    name: 'Price',
                    data: priceArr,

                }],

                chart: {
                    fontFamily: 'inherit',
                    type: 'bar',
                    height: height,
                    toolbar: {
                        show: false
                    }
                },
                plotOptions: {
                    bar: {
                        horizontal: false,
                        columnWidth: ['30%'],
                        borderRadius: 4
                    },
                },
                legend: {
                    show: false
                },
                dataLabels: {
                    enabled: true,
                    formatter: function (value, context) {
                        return '$' + value;
                    },
                    style: {
                        fontWeight: "bold",
                        colors: ["black"]
                    }
                },
                stroke: {
                    show: true,
                    width: 2,
                    colors: ['transparent']
                },
                xaxis: {
                    categories: categoryArr,
                    axisBorder: {
                        show: false,
                    },
                    axisTicks: {
                        show: false
                    },
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    }

                },
                yaxis: {
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    }
                },
                fill: {
                    opacity: 1
                },
                states: {
                    normal: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    hover: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    active: {
                        allowMultipleDataPointsSelection: false,
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    }
                },
                tooltip: {
                    style: {
                        fontSize: '12px'
                    },
                    y: {
                        formatter: function (val) {
                            return "$" + val + ""
                        }
                    }
                },
                colors: [baseColor, secondaryColor],
                grid: {
                    borderColor: borderColor,
                    strokeDashArray: 4,
                    yaxis: {
                        lines: {
                            show: true
                        }
                    }
                }
            };

            chart.self = new ApexCharts(element, options);
            chart.self.render();
            chart.rendered = true;
        }

        // Init chart
        initChart();

        // Update chart on theme mode change
        KTThemeMode.on("kt.thememode.change", function () {
            if (chart.rendered) {
                chart.self.destroy();
            }

            initChart();
        });
    }
}
