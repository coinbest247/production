﻿var AffiliateController = function () {
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
            url: '/Admin/Staking/GetAffiliateAllPaging',
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
                        Remarks: item.Remarks,
                        Amount: be.formatNumber(item.Amount, 4),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        AppUserName: item.AppUserName,
                        Sponsor: item.Sponsor
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