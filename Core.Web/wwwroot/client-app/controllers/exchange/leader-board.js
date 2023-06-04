var LeaderBoardController = function () {
    this.initialize = function () {
        loadSaleReport();

        loadData();

        setTimeout(loadData, 10000);

        registerEvents();
        registerControl();
    }

    function registerControl() {
        be.registerNumber();
    }

    function registerEvents() {
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
    }

    function loadSaleReport() {
        $.ajax({
            type: 'GET',
            url: '/Exchange/GetSaleReport',
            dataType: 'json',
            beforeSend: function () {
            },
            success: function (response) {
                $('.SeedRoundSupplied').html(be.formatCurrency(response.SeedRoundSupplied));
                $('.SeedRoundSuppliedPercent').html(response.SeedRoundSuppliedPercent);
                $('.AngelRoundSupplied').html(be.formatCurrency(response.AngelRoundSupplied));
                $('.AngelRoundSuppliedPercent').html(response.AngelRoundSuppliedPercent);
                $('.PrivateRoundSupplied').html(be.formatCurrency(response.PrivateRoundSupplied));
                $('.PrivateRoundSuppliedPercent').html(response.PrivateRoundSuppliedPercent);
                $('.PublishRoundSupplied').html(be.formatCurrency(response.PublishRoundSupplied));
                $('.PublishRoundSuppliedPercent').html(response.PublishRoundSuppliedPercent);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
            }
        });
    }

    function loadData(isPageChanged) {

        be.configs.pageSize = 20;

        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/Exchange/GetAllLeaderBoardPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                var template = $('#table-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {

                    //var bnbTransactionHashShort = item.BNBTransactionHash.substring(0, 10)
                    //    + "....." + item.BNBTransactionHash.substring(item.BNBTransactionHash.length - 10);

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        BNBAmount: be.formatNumber(item.AmountFrom, 4),
                        Amount: be.formatCurrency(item.Amount),
                        TypeName: item.TypeRound,
                        DateCreated: be.dateFormatJson(item.CreatedOn),
                        Sponsor: item.Sponsor
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

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