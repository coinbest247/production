var ReportController = function () {
    this.initialize = function () {

        registerControls();
        registerEvents();

        loadHistory();
        loadSaleSummary();
    }


    function registerControls() {
    }

    function registerEvents() {
        $('.ProjectV2_tablinks').on('click', function (e) {

            refLevel = $(this).attr('data-id');
            loadHistory();
            loadSaleSummary();
        });
    }

    var refLevel = 0;

    function loadHistory(isPageChanged) {

        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;

        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize,
                refLevel: refLevel
            },
            url: '/Report/GetAllSaleHistoriesPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();

                var render = "";

                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        TypeRound: item.TypeRound,
                        ActionType: item.ActionType,
                        Amount: item.Amount,
                        AmountFrom: item.AmountFrom,
                        Email: item.Email,
                        CreatedOn: be.dateFormatJson(item.CreatedOn),
                        TokenFrom: item.TokenFrom
                    });
                });

                $('#tbl-content').html(render);

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadSaleSummary() {
        $.ajax({
            type: 'GET',
            data: {
                refLevel: refLevel
            },
            url: '/Report/GetAllSaleReferralSummary',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                $('.lbRefTotalSale').html(response.TotalSale);
                $('.lbRefTotalBNB').html(response.TotalSalebByBNB);
                $('.lbRefTotalBNBAff').html(response.TotalSalebByBNBAffiliate);
                $('.lbRefTotalSwap').html(response.TotalSwap);
                $('.lbRefTotalSwapAff').html(response.TotalSwapAffiliate);

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}