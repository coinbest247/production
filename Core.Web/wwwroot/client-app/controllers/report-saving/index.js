var ReportController = function () {
    this.initialize = function () {

        registerControls();
        registerEvents();

        loadTab1();
        loadTab2();
        loadTab3();
    }


    function registerControls() {
    }

    function registerEvents() {
        $('.StakeTablinks3').on('click', function (e) {

            $('.StakeTablinks3').removeClass("active");

            refLevel = $(this).attr('data-id');

            $(this).addClass("active");

            loadTab2(true);
        });

        $('body').on('click', '.l_detail', function (e) {
            loadRef(e, this);
        });
    }

    var refLevel = 0;
    var f1Id = '';

    function loadRef(e, element) {

        e.preventDefault();

        f1Id = $(element).data('id');

        console.log(f1Id);

        loadTab4();
    }

    function loadTab1(isPageChanged) {

        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;

        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize
            },
            url: '/Report/GetAllSavingDirectPaging',
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
                        ProfitAmount: item.ProfitAmount,
                        SavingAmountUSD: item.SavingAmountUSD,
                        SavingAmount: item.SavingAmount,
                        Email: item.Email,
                        CreatedOn: be.dateTimeFormatJson(item.CreatedOn),
                        TokenCode: item.TokenCode
                    });
                });

                $('#tbl-1-content').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPagingCommission(response.RowCount, function () {
                        loadTab1();
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

        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize,
                levelId: refLevel
            },
            url: '/Report/GetAllSavingReferralPaging',
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
                        Amount: item.Amount,
                        Rate: item.Rate,
                        Email: item.Email,
                        CreatedOn: be.dateTimeFormatJson(item.CreatedOn)
                    });
                });

                $('#tbl-2-content').html(render);

                be.stopLoading();

                be.wrapPaging(response.RowCount, function () {
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

        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize,
                levelId: refLevel
            },
            url: '/Report/GetAllSavingLeadershipPaging',
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
                        Email: item.Email,
                        Saving: item.Saving,
                        SavingAffiliatet: item.SavingAffiliatet,
                        Total: item.Total,
                        Id: item.AppUserId
                    });
                });

                $('#tbl-3-content').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPagingLeadershipSale(response.RowCount, function () {
                        loadTab3();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadTab4(isPageChanged) {

        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;

        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize,
                appUserId: f1Id
            },
            url: '/Report/GetAllLeadershipReferralPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-4-template').html();

                var render = "";
                 
                if (response.RowCount > 0) {
                    $('#network-commission').attr("style", "display:block");
                    $('#network-not-found').attr("style", "display:none");
                } else {
                    $('#network-commission').attr("style", "display:none");
                    $('#network-not-found').attr("style", "display:block");
                }

                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        Email: item.Email,
                        Saving: item.Saving,
                        SavingAffiliatet: item.SavingAffiliatet,
                        Total: item.Total,
                        Id: item.AppUserId
                    });
                });

                $('#tbl-4-content').html(render);

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}