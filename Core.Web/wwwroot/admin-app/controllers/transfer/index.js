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
                    url: "/Admin/Transfer/GetSponsor",
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

        $('.btnMax').on('click', function (e) {

            var balance = parseFloat($('#txtBalance').html().replace(/,/g, ''));

            if (balance > 0) {

                $('#txtAmount').val(be.formatNumber(balance, 4));

                CalculateReceived();
            }
        });

        $("#txtAmount").focusout(function () {
            CalculateReceived();
        });

        function CalculateReceived() {
            var balance = parseFloat($('#txtBalance').html().replace(/,/g, ''));

            var amount = parseFloat($('#txtAmount').val().replace(/,/g, ''));

            var feeAmount = amount * (transferFee / 100);

            var receiveAmount = amount - feeAmount;

            if (amount > balance) {
                $(".lblErrorInsufficient").html("Insufficient account balance");
            }
            else {
                $(".lblErrorInsufficient").html("");
            }

            $('#txtFeeAmount').html(be.formatNumber(feeAmount, 4));

            $('#txtAmountReceive').html(be.formatNumber(receiveAmount, 4));
        }

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
            Balance: parseFloat($('#txtBalance').html().replace(/,/g, '')),
            Amount: parseFloat($('#txtAmount').val().replace(/,/g, '')),
            Sponsor: $('#txtSponsor').val(),
            Unit: unit
        };


        var isValid = true;

        if (!TransferVM.Sponsor) {
            isValid = be.notify('Sponsor is required', 'error');
        }

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

        var code = $('#be-hidden-2faCode').val();

        var url = '/Admin/Transfer/Transfer?authenticatorCode=' + code;

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

    
    var tokenCode = "";
    var balance = 0;
    var unit = 1;
    var minTransfer = 0;
    var transferFee = 0;

    function loadBalance() {

        tokenCode = $('#ddlWallet option:selected').text();

        unit = parseInt($('#ddlWallet').val());


        $.ajax({
            type: 'GET',
            data: {
            },
            url: '/Admin/transfer/GetBalance?unit=' + unit,
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                $(".lblErrorInsufficient").html("");
                $(".lblTokenCode").html(tokenCode);
                $('#txtBalance').html(be.formatNumber(response.Data.Balance, 4));

                balance = response.Data.Balance;
                minTransfer = response.Data.MinTransfer;
                transferFee = response.Data.TransferFee;
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}