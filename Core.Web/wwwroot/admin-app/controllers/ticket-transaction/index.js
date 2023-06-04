var TicketTransactionController = function () {
    this.initialize = function () {
        loadData();
        loadTodayData();
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

        $('body').on('click', '.btn-approve', function (e) {
            approveTicket(e, this);
        });

        $('body').on('click', '.btn-reject', function (e) {
            rejectTicket(e, this);
        });

        $('body').on('click', '.btn-lock', function (e) {
            lockTicket(e, this);
        });

        $('body').on('click', '.btn-back', function (e) {
            backTicket(e, this);
        });

        $('#btnSearch').on('click', function () {
            FilterSearch();
        });
    }

    function approveTicket(e, element) {
        e.preventDefault();

        be.confirm('Approve Ticket', 'Are you sure to Approve?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/TicketTransaction/ApproveTicket",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {
                        be.success('Approve Ticket', response.Message, function () {
                            loadData();
                            loadTodayData();
                        });
                    }
                    else {
                        be.error('Approve Ticket', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function rejectTicket(e, element) {
        e.preventDefault();

        be.confirm('Reject Ticket', 'Are you sure to Reject?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/TicketTransaction/RejectTicket",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {
                        be.success('Reject Ticket', response.Message, function () {
                            loadData();
                            loadTodayData();
                        });
                    }
                    else {
                        be.error('Reject Ticket', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function lockTicket(e, element) {
        e.preventDefault();

        be.confirm('Lock Ticket', 'Are you sure to Lock?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/TicketTransaction/LockTicket",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {
                        be.success('Lock Ticket', response.Message, function () {
                            loadData();
                            loadTodayData();
                        });
                    }
                    else {
                        be.error('Lock Ticket', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function backTicket(e, element) {
        e.preventDefault();

        be.confirm('Rollback Ticket', 'Are you sure to Rollback?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/TicketTransaction/RollbackTicket",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {
                        be.success('Rollback Ticket', response.Message, function () {
                            loadData();
                            loadTodayData();
                        });
                    }
                    else {
                        be.error('Rollback Ticket', response.Message);
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
                transactionStatus: $("#TicketTransactionStatus").val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/TicketTransaction/GetAllPaging',
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
                            + '<a data-id="' + item.Id + '" class="btn-reject btn-icon btn btn-light-dark btn-sm mt-3"><i class="fa fa-minus-circle text-danger"></i></a>'
                            + '<a data-id="' + item.Id + '" class="btn-lock btn btn-light-dark btn-sm mt-3"><i class="fa fa-lock text-dark"></i></a>' :
                            '<a data-id="' + item.Id + '" class="btn-back btn btn-light-dark btn-sm mt-3"><i class="fa fa-backward text-dark"></i></a>',
                        StatusName: be.getTicketStatus(item.Status),
                        Sponsor: item.Sponsor,
                        AddressFrom: item.AddressFrom,
                        AddressTo: item.AddressTo,
                        Amount: be.formatNumber(item.Amount, 4),
                        AmountReceive: be.formatNumber(item.AmountReceive, 4),
                        Fee: be.formatCurrency(item.Fee),
                        FeeAmount: be.formatNumber(item.FeeAmount, 4),
                        DateUpdated: be.dateTimeFormatJson(item.DateUpdated),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        TransactionHash: item.TransactionHash
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

    function loadTodayData(isPageChanged) {

        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                transactionStatus: $("#TicketTransactionStatus").val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/TicketTransaction/GetAllTodayPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {

                var template = $('#table-today-template').html();
                var render = "";

                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        UserName: item.AppUserName,
                        TypeName: item.TypeName,
                        UnitName: item.UnitName,
                        Function: item.Status == 1 ? '<a data-id="' + item.Id + '" class="btn-approve btn-icon btn btn-light-dark btn-sm me-2 mt-3"><i class="fa fa-check text-success"></i></a>'
                            + '<a data-id="' + item.Id + '" class="btn-reject btn-icon btn btn-light-dark btn-sm mt-3"><i class="fa fa-minus-circle text-danger"></i></a>'
                            + '<a data-id="' + item.Id + '" class="btn-lock btn btn-light-dark btn-sm mt-3"><i class="fa fa-lock text-dark"></i></a>' :
                            '<a data-id="' + item.Id + '" class="btn-back btn btn-light-dark btn-sm mt-3"><i class="fa fa-backward text-dark"></i></a>',
                        StatusName: be.getTicketStatus(item.Status),
                        Sponsor: item.Sponsor,
                        AddressFrom: item.AddressFrom,
                        AddressTo: item.AddressTo,
                        Amount: be.formatNumber(item.Amount, 4),
                        AmountReceive: be.formatNumber(item.AmountReceive, 4),
                        Fee: be.formatCurrency(item.Fee),
                        FeeAmount: be.formatNumber(item.FeeAmount, 4),
                        DateUpdated: be.dateTimeFormatJson(item.DateUpdated),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        TransactionHash: item.TransactionHash
                    });
                });

                $('#tbl-content-today').html(render);

                be.stopLoading();

            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function FilterSearch() {

        loadData(true);
    }
}