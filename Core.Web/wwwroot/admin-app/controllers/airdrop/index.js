var AirdropController = function () {
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

        $('body').on('click', '#btnConfirmAirdrop', function (e) {
            e.preventDefault();
            confirmAirdrop();
        });
    }

    function confirmAirdrop() {

        var AirdropVM = {
            UserTelegramChannel: $('#txtUserTelegramChannel').val(),
            UserTelegramCommunity: $('#txtUserTelegramCommunity').val(),
            UserFacebook: $('#txtUserFacebook').val()
        };

        var isValid = true;

        if (!AirdropVM.UserTelegramChannel
            && !AirdropVM.UserTelegramCommunity
            && !AirdropVM.UserFacebook) {

            isValid = be.notify('Missions cannot be left blank', 'error');
        }

        if (isValid) {

            $.ajax({
                type: "POST",
                headers: {
                    "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                url: "/Admin/Airdrop/ConfirmAirdrop",
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(AirdropVM),
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.stopLoading();

                    if (response.Success) {
                        be.success('Airdrop', response.Message, function () {
                            window.location.reload();
                        });
                    }
                    else {
                        be.error('Airdrop', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });

        }
    }

    function loadData(isPageChanged) {
        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/Admin/Airdrop/GetAllPaging',
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