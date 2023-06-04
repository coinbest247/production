var InvestPackage = function () {
    this.initialize = function () {
        registerEvents();
        registerControl();
        loadBalance();
    }

    function registerControl() {
    }

    function registerEvents() {

        be.registerNumber();

        $('body').on('change', '#ddlWallet', function (e) {
            loadBalance();
        });
        
        $('.btnMax').on('click', function (e) {

            $('#txtAmount').val(be.formatNumber(balance, 4));
        });

        $('#txtAmount').keyup(function (e) {

            var amount = parseFloat($(this).val().replace(/,/g, ''));

            if (isNaN(amount)) {
                amount = 0;
            }

            if (amount > balance) {
                $(this).val(be.formatNumber(balance, 4));
            }
        });

        $("#btnConfirm").on('click', async function (e) {

            e.preventDefault();

            if (!isAllowPayment) {
                be.notify(errorMsg, 'warning');
                return;
            }

            
            var tAmount = parseFloat($('#txtAmount').val().replace(/,/g, ''));

            if (!tAmount) {
                be.notify('Invalid investment package', 'warning');
                return;
            }

            var postData = {
                Unit: unit,
                Amount: tAmount
            };

            be.confirm('Confirm to payment invest ', 'Are you sure to payment ' + tAmount + ' ' + tokenCode +' to buy invest package', function () {
                $.ajax({
                    type: "POST",
                    url: "/Admin/InvestPackage/ConfirmPaymentInvest",
                    headers: {
                        "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                    },
                    data: JSON.stringify(postData),
                    dataType: "JSON",
                    contentType: "application/json",
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        if (response.Success) {
                            be.success('Payment invest', response.Message, function () {
                                window.location.reload();
                            });

                        } else {
                            be.error('Payment invest', response.Message);
                        }

                        be.stopLoading();
                    },
                    error: function (message) {
                        be.notify(`${message.responseText}`, 'error');
                        be.stopLoading();
                    }
                });
            });
        });
    }

    var tokenCode = "";
    var balance = 0;
    var unit = 1;
    var isAllowPayment = false;
    var errorMsg = "";

    function loadBalance() {

        tokenCode = $('#ddlWallet option:selected').text();

        unit = parseInt($('#ddlWallet').val());

        $.ajax({
            type: 'GET',
            data: {
            },
            url: '/Admin/InvestPackage/GetBalance?unit=' + unit,
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                $('#txtAmount').val(be.formatNumber(0, 4));

                $(".lblErrorInsufficient").html("");

                $(".lblTokenCode").html(tokenCode);

                $('#txtBalance').html(be.formatNumber(response.Balance, 4));

                isAllowPayment = response.IsAllowPayment;

                errorMsg = response.ErrorMsg;

                balance = response.Balance;

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

}