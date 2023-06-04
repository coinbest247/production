var AirdropManageController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {

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

        $('body').on('click', '.btn-approve', function (e) {
            approveAirdrop(e, this);
        });

        $('body').on('click', '.btn-reject', function (e) {
            rejectAirdrop(e, this);
        });
    }

    function approveAirdrop(e, element) {
        e.preventDefault();

        be.confirm('Approve Airdrop', 'Are you sure to Approve?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/AirdropManage/Approve",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {
                        be.success('Approve', response.Message, function () {
                            loadData();
                        });
                    }
                    else {
                        be.error('Approve', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function rejectAirdrop(e, element) {
        e.preventDefault();

        be.confirm('Reject Airdrop', 'Are you sure to Reject?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/AirdropManage/Reject",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {
                        be.success('Reject', response.Message, function () {
                            loadData();
                        });
                    }
                    else {
                        be.error('Reject', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
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
            url: '/Admin/AirdropManage/GetAllPaging',
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
                        StatusName: be.getTicketStatus(item.Status),
                        Sponsor: item.Sponsor,
                        UserTelegramCommunity: item.UserTelegramCommunity,
                        UserTelegramChannel: item.UserTelegramChannel,
                        UserFacebook: item.UserFacebook,
                        DateUpdated: be.dateTimeFormatJson(item.DateUpdated),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        Function: item.Status == 1 ? '<a data-id="' + item.Id + '" class="btn-approve btn-icon btn btn-light-dark btn-sm me-2 mt-3"><i class="fa fa-check text-success"></i></a>'
                            + '<a data-id="' + item.Id + '" class="btn-reject btn-icon btn btn-light-dark btn-sm mt-3"><i class="fa fa-minus-circle text-danger"></i></a>' : "",
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
}