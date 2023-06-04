var LeaderboardController = function () {
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
            loadData(true);
        });
    }


    function loadData(isPageChanged) {
        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/Admin/Staking/GetLeaderboardAllPaging',
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
                        TransactionHash:item.TransactionHash,
                        Sponsor: item.Sponsor,
                        DateCreated: be.dateFormatJson(item.DateCreated),
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
}