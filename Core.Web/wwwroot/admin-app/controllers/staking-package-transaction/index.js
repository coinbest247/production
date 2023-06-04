var StakingPackageTransactionController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {
        $("#FromDate").flatpickr();
        $("#ToDate").flatpickr();
        $(".numberFormat").each(function () {
            var numberValue = parseFloat($(this).val().replace(/,/g, ''));
            if (!numberValue) {
                $(this).val(be.formatCurrency(0));
            }
            else {
                $(this).val(be.formatCurrency(numberValue));
            }
        });

    }

    function registerEvents() {
        $("#btnSearch").on('click', function (e) {
            loadData(false);
        });
        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadData(true);
            }
        });

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });

        $('.numberFormat').on("keypress", function (e) {
            var keyCode = e.which ? e.which : e.keyCode;
            var ret = ((keyCode >= 48 && keyCode <= 57) || keyCode == 46);
            if (ret)
                return true;
            else
                return false;
        });

        $(".numberFormat").focusout(function () {
            var numberValue = parseFloat($(this).val().replace(/,/g, ''));
            if (!numberValue) {
                $(this).val(be.formatCurrency(0));
            }
            else {
                $(this).val(be.formatCurrency(numberValue));
            }
        });
    }

    function loadData(isPageChanged) {
        var fromDate = $("#FromDate").val();
        var toDate = $("#ToDate").val();
        var timeLine = $("#TimeLineType").val();
        $.ajax({
            type: 'GET',
            data: {
                fromDate: fromDate,
                toDate: toDate,
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize,
                timeLine: timeLine
            },
            url: '/admin/transaction/GetStakingPackageAllPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                var template = $('#table-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                  
                    render += Mustache.render(template, {
                        AppUserName: item.AppUserName,
                        TypeName: item.TypeName,
                        PackageName: item.PackageName,
                        InterestRate: item.InterestRate,
                        ReceiveAmount: item.ReceiveAmount,
                        ReceiveLatest: be.dateTimeFormatJson(item.ReceiveLatest),
                        ReceiveTimes: item.ReceiveTimes,
                        StakingAmount: item.StakingAmount,
                        StakingTimes: item.StakingTimes,
                        TimeLineName: item.TimeLineName,
                        Function: item.IsGetedCommission == false ? '<a data-id="' + item.Id + '" class="btn-get-commission btn btn-light-dark btn-sm me-2 mt-3 fs-8">Get Commission</a>' : '<span class="text-success fs-7 fw-bolder">Received Today</span>',
                        Sponsor: item.Sponsor,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                    });
                });

                $('#tbl-content').html(render);

                //be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                //be.stopLoading();
            }
        });
    }
}