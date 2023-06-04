var WalletController = function () {
    this.initialize = function (publishKeyRaw, tokenKey, tokenCode) {
        publishKey = publishKeyRaw;

        this.tokenCodeName = tokenCode;

        loadWallets(tokenKey, tokenCode);

        registerEvents();
        registerControl();
    }

    var publishKey = "";
    var feeWithdraw = "";
    var tokenCode = "";
    var minWithdraw = 1;
    var unit = 1;
    var tokenCodeName = '';

    function registerControl() {
        be.registerNumber();

        jQuery('#qrcodePublishKey').qrcode({
            text: publishKey
        });
    }

    var registerEvents = function () {
        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                
            }
        });
        $('#btnSearch').on('click', function () {
            loadData(true);
        });

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });

        $('body').on('click', '.btnDeposit', function (e) {
            e.preventDefault();

            var tokenCode = $(this).data('tokencode');
            $(".tokenCode").html(tokenCode);

            var minDeposit = $(this).data('mindeposit');
            $(".minDeposit").html(minDeposit);

            $("#txtPublishKey").val(publishKey)

            showModalDeposit()
        });

        $('body').on('click', '#btnCopyPublishKey', function (e) {
            e.preventDefault();
            copyPublishKey();
        });

        $("#txtAmount").focusout(function () {

            var balance = parseFloat($('.balance').val().replace(/,/g, ''));

            var amount = parseFloat($(this).val().replace(/,/g, ''));

            var feeAmount = amount * (feeWithdraw / 100);

            var receiveAmount = amount - feeAmount;

            if (amount > balance) {
                $(".lblErrorInsufficient").html("Insufficient account balance");
            }
            else {
                $(".lblErrorInsufficient").html("");
            }

            $('#txtFeeAmount').val(be.formatNumber(feeAmount, 4));

            $('#txtAmountReceive').val(be.formatNumber(receiveAmount, 4));
        });

        $('body').on('click', '.btnWithdraw', function (e) {
            e.preventDefault();
            if (checkEnabled2FA()) {

                tokenCode = $(this).data('tokencode');
                $(".tokenCode").html(tokenCode);

                minWithdraw = $(this).data('minwithdraw');
                $(".minWithdraw").html(minWithdraw);

                var balance = $(this).data('balance');
                $(".balance").val(balance);

                feeWithdraw = parseFloat($(this).data('fee'));

                unit = parseInt($(this).data('unit'));

                resetModalWithdraw();

                showModalWithdraw()
            }
        });

        $('body').on('click', '#btnConfirmWithdraw', function (e) {
            e.preventDefault();

            var isValid = validateWithdraw();
            if (!isValid)
                return;

            hideModalWithdraw();

            be.verifyCodeAndPassword(confirmWithdraw);
        });

        $('.btnMax').on('click', function (e) {

            if (transferBalance > 0) {

                $('#txtTransferAmount').val(be.formatNumber(transferBalance, 4));
            }
        });




        $('#txtTransferAmount').keyup(function (e) {

            var amount = parseFloat($(this).val().replace(/,/g, ''));

            if (isNaN(amount)) {
                amount = 0;
            }

            if (amount > transferBalance) {
                $(this).val(be.formatNumber(transferBalance, 4));
            }

        });

        $('body').on('click', '#btnConfirmFuture', function (e) {
            e.preventDefault();
            confirmTransferFuture();
        });

        $('body').on('click', '.btnTransfer', function (e) {
            e.preventDefault();
            tokenCode = $(this).data('tokencode');
            unit = $(this).data('unit');
            $(".tokenCode").html(tokenCode);
            $('.lblTokenCode').html(tokenCode);

            loadTokenWallet(unit);

            showModalTransferFuture();
        });

        $('body').on('click', '.btnTransferMain', function (e) {
            e.preventDefault();
            tokenCode = $(this).data('tokencode');
            unit = $(this).data('unit');
            $(".tokenCode").html(tokenCode);
            $('.lblTokenCode').html(tokenCode);

            loadTokenWalletFuture(unit);

            showModalTransferMain();
        });

        $('.btnMaxMain').on('click', function (e) {

            if (transferBalance > 0) {

                $('#txtTransferMainAmount').val(be.formatNumber(transferBalance, 4));
            }
        });

        $('body').on('click', '#btnConfirmMain', function (e) {
            e.preventDefault();
            confirmTransferMain();
        });


        $('body').on('click', '.btnSwap', function (e) {
            e.preventDefault();
            tokenCode = $(this).data('tokencode');
            unit = $(this).data('unit');
            $(".tokenCode").html(tokenCode);
            $('.lblTokenCode').html(tokenCode);

            loadTokenWalletSwap(unit);

            showModalSwapMain();
        });

        $('.btnMaxSwap').on('click', function (e) {

            if (transferBalance > 0) {

                $('#txtSwapAmount').val(be.formatNumber(transferBalance, 4));
            }
        });

        $('body').on('click', '#btnConfirmSwap', function (e) {
            e.preventDefault();
            confirmSwapMain();
        });
    }

    var transferBalance = 0;

    var withdrawVM = null;

    function validateWithdraw() {

        withdrawVM = {
            Balance: parseFloat($('.balance').val().replace(/,/g, '')),
            Amount: parseFloat($('#txtAmount').val().replace(/,/g, '')),
            ReceiveAddress: $("#txtReceiveAddress").val(),
            Unit: unit
        };

        var isValid = true;

        if (withdrawVM.Amount <= 0) {
            isValid = be.notify('Amount is required', 'error');
        }
        else {
            if (withdrawVM.Amount < minWithdraw) {
                isValid = be.notify('Minimum withdraw ' + minWithdraw + ' ' + tokenCode, 'error');
            }
        }

        if (withdrawVM.Amount > withdrawVM.Balance) {
            isValid = be.notify('Insufficient account balance', 'error');
        }

        if (!withdrawVM.ReceiveAddress) {
            isValid = be.notify('Receive Address is required', 'error');
        }

        return isValid;
    }

    function confirmTransferFuture() {
        debugger;
        var trasnferFutureAmount = $('#txtTransferAmount').val();
        var amount = parseFloat(trasnferFutureAmount.replace(/,/g, ''));
        var data = {
            Amount: amount,
            Unit: unit
        }

        $.ajax({
            type: "POST",
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: '/admin/wallet/transferFuture',
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(data),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                be.stopLoading();

                if (response.Success) {
                    be.success('Transfer From Main To Future', response.Message, function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Transfer From Main To Future', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function confirmTransferMain() {
        var trasnferMainAmount = $('#txtTransferMainAmount').val();
        var amount = parseFloat(trasnferMainAmount.replace(/,/g, ''));
        var data = {
            Amount: amount,
            Unit: unit
        }

        $.ajax({
            type: "POST",
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: '/admin/wallet/TransferMain',
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(data),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                be.stopLoading();

                if (response.Success) {
                    be.success('Transfer From Future To Main', response.Message, function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Transfer From Future To Main', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function confirmSwapMain() {
        var swapAmount = $('#txtSwapAmount').val();
        var amount = parseFloat(swapAmount.replace(/,/g, ''));
        var data = {
            Amount: amount,
            Unit: unit
        }

        $.ajax({
            type: "POST",
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: '/admin/wallet/Swap',
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(data),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                be.stopLoading();

                if (response.Success) {
                    be.success('Swap to USDT', response.Message, function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Swap to USDT', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function confirmWithdraw() {


        withdrawVM.Password = $('#be-hidden-password').val()

        var code = $('#be-hidden-2faCode').val();

        var url = '/Admin/Wallet/Withdraw?authenticatorCode=' + code;

        $.ajax({
            type: "POST",
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: url,
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(withdrawVM),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                be.stopLoading();

                if (response.Success) {
                    be.success('Withdraw', response.Message, function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Withdraw', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function showModalDeposit() {
        $("#modal_deposit").modal("show");
    }

    function copyPublishKey() {
        var copyText = $("#txtPublishKey");
        copyText.select();
        document.execCommand("copy");
        be.notify('Copy to clipboard is successful', 'success');
    }

    function resetModalWithdraw() {
        $('#txtAmount').val(be.formatNumber(0, 4));
        $('#txtFeeAmount').val(be.formatNumber(0, 4));
        $('#txtAmountReceive').val(be.formatNumber(0, 4));
        $('#txtReceiveAddress').val('');
    }

    function showModalTransferFuture() {
        $("#modal_transfer_tofuture").modal("show");
        
    }

    function showModalTransferMain() {
        $("#modal_transfer_tomain").modal("show");

    }

    function showModalSwapMain() {
        $("#modal_swap").modal("show");

    }

    function hideModalTransferFuture() {
        $("#modal_transfer_tofuture").modal("hide");

    }

    function showModalWithdraw() {
        $("#modal_withdraw").modal("show");
    }
    function hideModalWithdraw() {
        $("#modal_withdraw").modal("hide");
    }

    function checkEnabled2FA() {
        var isEnabled2FA = $("#Enabled2FA").val() == "True";
        if (isEnabled2FA) {
            return true;
        }
        else {
            be.error("Two-factor authentication (2FA)", "Your account has not enabled 2FA, please go to the profile page to enable.");
            return false;
        }
    }

    function loadWallets(tokenKey,tokenCode) {
        $.ajax({
            type: 'POST',
            url: '/Admin/Wallet/GetWallets',
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            dataType: 'json',
            data: {
                token: tokenKey
            },
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                be.stopLoading();

                if (response.Success) {

                    $('#' + tokenCode+'Amount').html(be.formatNumber(response.Data.BCAmount, 4));
                    $('.'+ tokenCode + 'Amount').html(be.formatNumber(response.Data.BCAmount, 4));

                    $('.btnWithdraw').data('balance', be.formatNumber(response.Data.BCAmount, 4))

                    $('#USDTAmount').html(be.formatNumber(response.Data.USDTAmount, 4));
                    $('.USDTAmount').html(be.formatNumber(response.Data.USDTAmount, 4));
                    $('.btnWithdrawUSDT').data('balance', be.formatNumber(response.Data.USDTAmount, 4));

                    $('#BUSDAmount').html(be.formatNumber(response.Data.BUSDAmount, 4));
                    $('.BUSDAmount').html(be.formatNumber(response.Data.BUSDAmount, 4));
                    $('.btnWithdrawBUSD').data('balance', be.formatNumber(response.Data.BUSDAmount, 4));

                    $('#SHIBClaimAmount').html(be.formatNumber(response.Data.SHIBClaimAmount, 4));
                    $('.SHIBClaimAmount').html(be.formatNumber(response.Data.SHIBClaimAmount, 4));

                    $('#BNBAmount').html(be.formatNumber(response.Data.BNBAmount, 4));
                    $('.BNBAmount').html(be.formatNumber(response.Data.BNBAmount, 4));
                    $('.btnWithdrawBNB').data('balance', be.formatNumber(response.Data.BNBAmount, 4));

                    $('#BOTTRADEAmount').html(be.formatNumber(response.Data.BotTradeAmount, 4));
                    $('.BOTTRADEAmount').html(be.formatNumber(response.Data.BotTradeAmount, 4));

                    $('#BCClaimAmount').html(be.formatNumber(response.Data.BCClaimAmount, 4));
                    $('.BCClaimAmount').html(be.formatNumber(response.Data.BCClaimAmount, 4));

                    $('#BCFutureAmount').html(be.formatNumber(response.Data.BCFutureAmount, 4));
                    $('.BCFutureAmount').html(be.formatNumber(response.Data.BCFutureAmount, 4));

                    $('#USDTFutureAmount').html(be.formatNumber(response.Data.USDTFutureAmount, 4));
                    $('.USDTFutureAmount').html(be.formatNumber(response.Data.USDTFutureAmount, 4));

                    $('#PiNetworkAmount').html(be.formatNumber(response.Data.PiNetworkAmount, 4));
                    $('.PiNetworkAmount').html(be.formatNumber(response.Data.PiNetworkAmount, 4));

                    $('#PiNetworkFutureAmount').html(be.formatNumber(response.Data.PiNetworkFutureAmount, 4));
                    $('.PiNetworkFutureAmount').html(be.formatNumber(response.Data.PiNetworkFutureAmount, 4));

                    loadData();
                }
                else {
                    be.notify(response.Message, 'error');
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
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
            url: '/Admin/Wallet/GetAllTicketPaging',
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

    function loadTokenWallet(unitInput) {
        $.ajax({
            type: 'GET',
            data: {
                unit: unitInput
            },
            url: '/Admin/Wallet/GetTokenWallet',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                transferBalance = response.Balance;

                $(".CurrentBalance").html(be.formatNumber(transferBalance, 4));
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadTokenWalletFuture(unitInput) {
        $.ajax({
            type: 'GET',
            data: {
                unit: unitInput
            },
            url: '/Admin/Wallet/GetTokenWalletFuture',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                transferBalance = response.Balance;

                $(".CurrentBalance").html(be.formatNumber(transferBalance, 4));
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadTokenWalletSwap(unitInput) {
        $.ajax({
            type: 'GET',
            data: {
                unit: unitInput
            },
            url: '/Admin/Wallet/GetTokenWalletSwap',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                transferBalance = response.Balance;

                $(".CurrentBalance").html(be.formatNumber(transferBalance, 4));
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}