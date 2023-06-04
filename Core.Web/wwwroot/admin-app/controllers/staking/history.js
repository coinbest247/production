var HistoryController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {
        be.registerNumber();
    }

    var registerEvents = function () {

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });

        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadData(true);
            }
        });

        $('#btnSearch').on('click', function () {
            Search();
        });

        $('body').on('click', '.btn-reject', function (e) {
            cancelStaking(e, this);
        });
    }
    function Search() {

        loadData(true);

    }

    function loadData(isPageChanged) {
        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/Admin/Staking/GetAllPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                be.stopLoading();

                var template = $('#table-template').html();
                var render = "";

                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        AppUserName: item.AppUserName,
                        TypeName: item.TypeName,
                        InterestRate: be.formatNumber(item.InterestRate, 2),
                        ReceiveAmount: be.formatNumber(item.ReceiveAmount, 4),
                        ReceiveLatest: item.ReceiveLatest != null? be.dateFormatJson(item.ReceiveLatest):"N/A",
                        ReceiveTimes: item.ReceiveTimes,
                        StakingAmount: be.formatNumber(item.StakingAmount, 4),
                        PaymentAmount: be.formatNumber(item.PaymentAmount, 4),
                        PaymentUnit: be.formatNumber(item.PaymentUnit, 4),
                        StakingTimes: item.StakingTimes,
                        Sponsor: item.Sponsor,
                        DateCreated: be.dateFormatJson(item.DateCreated),
                        CancelOn: item.CancelOn != null ? be.dateFormatJson(item.CancelOn) : "N/A",
                        CompletedOn: item.CompletedOn != null ? be.dateFormatJson(item.CompletedOn) : "N/A",
                        Function: item.Type == 1 ? '<a data-id="' + item.Id + '" class="btn-reject btn-icon btn btn-light-dark btn-sm mt-3"><i class="fa fa-minus-circle text-danger"></i></a>' : ''
                    });
                });

                $('#tbl-content').html(render);

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

    function cancelStaking(e, element) {
        e.preventDefault();

        be.confirm('Cancel staking', 'Are you sure to cancel staking?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/Staking/CancelStaking",
                data: { id: $(element).data('id') },
                dataType: "json",
                headers: {
                    "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {
                        be.success('Cancel Staking', response.Message, function () {
                            loadData();
                        });
                    }
                    else {
                        be.error('Cancel Staking', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }
}