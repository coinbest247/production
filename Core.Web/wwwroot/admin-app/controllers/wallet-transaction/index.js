var WalletTransactionController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {

        be.registerNumber();
    }

    var registerEvents = function () {

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

        $('#btnSaleReport').on('click', function () {
            FilterSaleReport();
        });
    }



    function loadData(isPageChanged) {

        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#Sale_Report_Customer_Search').val(),
                transactionId: $("#WalletTransactionType").val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/WalletTransaction/GetAllPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();
                var render = "";

                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        UserName: item.AppUserName,
                        TypeName: item.TypeName,
                        UnitName: item.UnitName,
                        Function: item.Status == 1 ? '<a data-id="' + item.Id + '" class="btn-approve btn-icon btn btn-light-dark btn-sm me-2 mt-3"><i class="fa fa-check text-success"></i></a>'
                            + '<a data-id="' + item.Id + '" class="btn-reject btn-icon btn btn-light-dark btn-sm mt-3"><i class="fa fa-minus-circle text-danger"></i></a>' : "",
                        StatusName: be.getTicketStatus(item.Status),
                        Sponsor: item.Sponsor,
                        AddressFrom: item.AddressFrom,
                        TransactionHash: item.TransactionHash,
                        AddressTo: item.AddressTo,
                        Amount: be.formatNumber(item.Amount, 4),
                        AmountReceive: be.formatNumber(item.AmountReceive, 4),
                        Fee: be.formatCurrency(item.Fee),
                        FeeAmount: be.formatNumber(item.FeeAmount, 4),
                        DateUpdated: be.dateTimeFormatJson(item.DateUpdated),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
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
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function FilterSaleReport() {

        loadData(true);

    }
}