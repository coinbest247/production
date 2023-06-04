var ReportController = function () {
    this.initialize = function () {

        registerControls();
        registerEvents();
    }


    function registerControls() {
        $("#FromDate").flatpickr();
        $("#ToDate").flatpickr();
    }

    function registerEvents() {
        $('#btnSearch').on('click', function (e) {
            loadSummary();
        });
    }

    function loadSummary() {
        var fromDate = $("#FromDate").val();
        var toDate = $("#ToDate").val();
        $.ajax({
            type: 'GET',
            data: { FromDate: fromDate, ToDate: toDate },
            url: '/admin/Report/GetInvestBotTradeReport',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                $('.lblTotalInvest').html(response.TotalInvestUSDT);
                $('.lbTotalAffiliateInterest').html(response.TotalAffiliateProfit);
                $('.lbTotalInterest').html(response.TotalDailyProfit);
                $('.lbTotalProfit').html(response.TotalProfit);

                be.stopLoading();
            },
            error: function (message) {
                be.stopLoading();
                be.notify(`${message.responseText}`, 'error');
                
            }
        });
    }
}