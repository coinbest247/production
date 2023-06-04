var ReportController = function () {
    this.initialize = function () {

        registerControls();
        registerEvents();

        //loadSummary();

    }


    function registerControls() {
        $('#txt-startdate').datepicker({
            autoclose: true,
            format: 'mm/dd/yyyy'
        });

        $('#txt-enddate').datepicker({
            autoclose: true,
            format: 'mm/dd/yyyy'
        });
    }

    function registerEvents() {
        //$('.StakeTablinks3').on('click', function (e) {

        //    $('.StakeTablinks3').removeClass("active");

        //    refLevel = $(this).attr('data-id');

        //    $(this).addClass("active");

        //    loadTab2();
        //});

        $('#btnSearch').on('click', function (e) {
            loadSummary();
        });

        $('#btnSearch').on('click', function (e) {
            loadSummary();
        });

        $('.wallet-inout').on('click', function (e) {
            var attr = $(this).attr("attr-id");
            var arr = attr.split('-');
            loadTab1(true, arr[0], arr[1]);
            document.getElementById("OpenProject").click();
        });

        $('.exchange-inout').on('click', function (e) {
            loadTab2(true);
            document.getElementById("OpenProject2").click();
        });

        $('.saving-inout').on('click', function (e) {
            
            loadTab3(true);
            document.getElementById("OpenProject3").click();
        });


        $('body').on('click', '.l_detail', function (e) {
            loadRef(e, this);
        });
    }

    var refLevel = 0;
    var f1Id = '';

    function loadRef(e, element) {

        e.preventDefault();

        f1Id =  $(element).data('id');

    }

    function loadTab1(isPageChanged,txnTypeId,unit) {

        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;
        var query = $('#txt-query').val();
        var startDate = $('#txt-startdate').val();
        var endDate = $('#txt-enddate').val();

        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize,
                transactionType: txnTypeId ,// Deposit
                query: query,
                startdate: startDate,
                endDate: endDate,
                unit: unit
            },
            url: '/Report/GetAllLeadershipTransactionsPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-1-template').html();

                var render = "";

                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        Email : item.Email,
                        Amount: item.Amount,
                        AmountUSDT: item.AmountUSDT,
                        Unit: item.Unit,
                        Type:item.Type,
                        CreatedOn: be.dateTimeFormatJson(item.CreatedOn)
                    });
                });

                //$('.lb-total-deposit').html(response.TotalUSDT + ' USD');

                $('#tbl-1-content').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPagingCommission(response.RowCount, function () {
                        loadTab1(false, txnTypeId, unit);
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadTab2(isPageChanged) {

        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;
        var query = $('#txt-query').val();
        var startDate = $('#txt-startdate').val();
        var endDate = $('#txt-enddate').val();
        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize,
                query: query,
                startdate: startDate,
                endDate: endDate
            },
            url: '/Report/GetAllLeadershipSaleHistoriesPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-2-template').html();

                var render = "";

                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        Email: item.Email,
                        Amount: item.Amount,
                        AmountUSDT: item.AmountUSDT,
                        Unit: item.Unit,
                        TypeName: item.TypeName,
                        CreatedOn: be.dateTimeFormatJson(item.DateCreated)
                    });
                });

                $('#tbl-2-content').html(render);

                //$('.lb-total-withdraw').html(response.TotalUSDT + ' USD');

                be.stopLoading();


                if (response.RowCount)
                    be.wrapPagingLeadershipSale(response.RowCount, function () {
                        loadTab2();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadTab3(isPageChanged) {

        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;
        var query = $('#txt-query').val();
        var startDate = $('#txt-startdate').val();
        var endDate = $('#txt-enddate').val();
        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize,
                query: query,
                startdate: startDate,
                endDate: endDate
            },
            url: '/Report/GetAllLeadershipSavingHistoriesPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-3-template').html();

                var render = "";
                
                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        SavingAmount: item.SavingAmount,
                        SavingAmountUSD: item.SavingAmountUSD,
                        Email: item.Email,
                        TokenCode: item.TokenCode,
                        Rate: item.Rate,
                        ProfitAmount: item.ProfitAmount,
                        CreatedOn: be.dateTimeFormatJson(item.CreatedOn)
                    });
                });

                $('#tbl-3-content').html(render);

                //$('.lb-total-txn').html(response.TotalUSDT + ' USD');

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadTab3();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadSummary() {
        var query = $('#txt-query').val();
        var startDate = $('#txt-startdate').val();
        var endDate = $('#txt-enddate').val();

        $.ajax({
            type: 'GET',
            data: {
                query: query,
                startdate: startDate,
                endDate: endDate
            },
            url: '/Report/GetAllLeadershipSummaryTransaction',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {


                $('.lb-total-deposit-bnb').html(response.TotalBNBDeposit + ' BNB');
                $('.lb-total-withdraw-bnb').html(response.TotalBNBWithdraw + ' BNB');
                $('.lb-total-deposit-usdt').html(response.TotalUSDTDeposit + ' USDT');
                $('.lb-total-withdraw-usdt').html(response.TotalUSDTWithdraw + ' USDT');
                $('.lb-total-transfer-usdt').html(response.TotalTransferUSDT + ' USDT');
                $('.lb-total-received-transfer-usdt').html(response.TotalReceivedTransferUSDT + ' USDT');
                $('.lb-total-exchange').html(response.TotalExchangeSaleUSDT + ' USDT');
                $('.lb-total-saving').html(response.TotalSavingUSDT + ' USDT');
                $('.lb-total-transfer-bnb').html(response.TotalTransferBNB + ' BNB');
                $('.lb-total-received-transfer-bnb').html(response.TotalReceivedTransferBNB + ' BNB');
                be.stopLoading();

            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}