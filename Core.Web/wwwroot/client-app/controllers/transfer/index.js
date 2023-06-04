var TransferController = function () {
    this.initialize = function () {
        loadBalance();
        registerEvents();
        registerControl();
    }

    function registerControl() {

    }

    function registerEvents() {

        be.registerNumber();

        $("#txtSponsor").focusout(function () {

            var sponsor = $(this).val();

            if (sponsor) {

                $.ajax({
                    type: "GET",
                    url: "/Transfer/GetSponsor",
                    dataType: "json",
                    contentType: "application/json",
                    data: { sponsor: sponsor },
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        be.stopLoading();

                        if (response.Success) {
                            $(".lblDisplayEmail").html('<span class="form-label mt-2 text-success lblDisplayEmail">Transfer to user email: ' + response.Message + '</span>');
                        }
                        else {
                            $(".lblDisplayEmail").html('<span class="form-label mt-2 text-danger lblDisplayEmail">' + response.Message + '</span>');
                        }
                    },
                    error: function (message) {
                        be.notify(`${message.responseText}`, 'error');
                        be.stopLoading();
                    }
                });

            }
        });

        $("#txtAmount").focusout(function () {

            var balance = parseFloat($('#txtBalance').val().replace(/,/g, ''));

            var amount = parseFloat($(this).val().replace(/,/g, ''));

            var feeAmount = amount * (transferFee / 100);

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

        $('body').on('click', '#btnConfirmTransfer', function (e) {
            e.preventDefault();

            if (checkEnabled2FA()) {

                var isValid = validateTransfer();
                if (!isValid)
                    return;

                be.verifyCodeAndPassword(confirmTransfer);
            }
        });

        $('body').on('change', '#ddlWallet', function (e) {
            loadBalance();
        });
    }

    function checkEnabled2FA() {
        var isEnabled2FA = $("#Enabled2FA").val();
        if (isEnabled2FA) {
            return true;
        }
        else {
            be.error("Two-factor authentication (2FA)", "Your account has not enabled 2FA, please go to the profile page to enable.");
            return false;
        }
    }

    var TransferVM = null;

    function validateTransfer() {

        TransferVM = {
            Balance: parseFloat($('#txtBalance').val().replace(/,/g, '')),
            Amount: parseFloat($('#txtAmount').val().replace(/,/g, '')),
            TokenCode: tokenCode
        };


        var isValid = true;

        if (TransferVM.Amount <= 0) {
            isValid = be.notify('Amount is required', 'error');
        }
        else {
            if (TransferVM.Amount < minTransfer) {
                isValid = be.notify('Minimum transfer ' + minTransfer + ' ' + tokenCode, 'error');
            }
        }

        if (TransferVM.Amount > TransferVM.Balance) {
            isValid = be.notify('Insufficient account balance', 'error');
        }

        return isValid;
    }

    function confirmTransfer() {

        TransferVM.Password = $('#be-hidden-password').val();
        TransferVM.Sponsor = $('#txtSponsor').val();

        var code = $('#be-hidden-2faCode').val();

        var url = '/Transfer/Transfer?authenticatorCode=' + code;

        $.ajax({
            type: "POST",
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: url,
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(TransferVM),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                be.stopLoading();

                if (response.Success) {
                    be.success('Transfer', response.Message, function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Transfer', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    var minTransfer = 0;
    var transferFee = 0.0;
    var tokenCode = "";

    function loadBalance() {

        tokenCode = $('#ddlWallet').val();

        $.ajax({
            type: 'GET',
            data: {
            },
            url: '/Transfer/GetBalance?tokenCode=' + tokenCode,
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                $('#txtFeeAmount').val(be.formatNumber(0, 4));
                $('#txtAmountReceive').val(be.formatNumber(0, 4));
                $('#txtAmount').val(be.formatNumber(0, 4));

                $('#txtBalance').val(be.formatNumber(response.Balance, 4));
                $('.lblMinTransfer').text(response.MinTransfer);

                $('.lblTokenCode').text(tokenCode);

                 
                minTransfer = response.MinTransfer;

                balance = response.Balance;

                transferFee = response.TransferFee;

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}