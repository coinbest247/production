var WalletTransferController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {

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



        $('#btnCreate').on('click', function (e) {
            Create();
        });

        $('#btnRecall').on('click', function (e) {
            Recall();
        });
    }

    function Create() {

        var data = {
            TotalWallet: parseInt($('#txtTotalWallet').val()),
            AmountFrom: parseInt($('#txtAmountFrom').val()),
            AmountTo: parseInt($('#txtAmountTo').val()),
        };

        var isValid = true;

        if (data.TotalWallet <= 0) {
            isValid = be.notify('Holders is required.', 'error');
        }

        if (data.AmountFrom <= 0) {
            isValid = be.notify('Amount From is required.', 'error');
        }

        if (data.AmountTo <= data.AmountFrom) {
            isValid = be.notify('Amount To is required.', 'error');
        }

        if (isValid) {
            $.ajax({
                type: "POST",
                headers: {
                    "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                url: "/Admin/WalletTransfer/Create",
                dataType: "json",
                data: { modelJson: JSON.stringify(data) },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    if (response.Success) {
                        be.success('Create', response.Message, function () {
                            window.location.reload();
                        });
                    }
                    else {
                        be.error('Create', response.Message);
                    }

                    be.stopLoading();
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        }
    }

    function Recall() {
        var data = {
            TotalWallet: parseInt($('#txtTotalWallet').val())
        };

        var isValid = true;

        if (data.TotalWallet <= 0) {
            isValid = be.notify('wallet is required.', 'error');
        }

        if (isValid) {
            $.ajax({
                type: "POST",
                headers: {
                    "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                url: "/Admin/WalletTransfer/Recall",
                dataType: "json",
                data: { modelJson: JSON.stringify(data) },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    if (response.Success) {
                        be.success('Recall', response.Message, function () {
                            window.location.reload();
                        });
                    }
                    else {
                        be.error('Recall', response.Message);
                    }

                    be.stopLoading();
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
            url: '/admin/WalletTransfer/GetAllPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                $("#TotalAmountTransferred").html(be.formatCurrency(response.OtherResult));

                var template = $('#table-template').html();
                var render = "";

                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        Id: item.Id,
                        PublishKey: item.PublishKey,
                        TransferHash: item.TransferHash,
                        RecallHash: item.RecallHash,
                        IsRecall: be.getStatus(item.IsRecall),
                        DateCreated: be.dateFormatJson(item.DateCreated),
                        DateModified: be.dateFormatJson(item.DateModified),
                        Amount: be.formatCurrency(item.Amount)
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