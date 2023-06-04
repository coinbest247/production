var ExchangeController = function () {
    this.initialize = function () {
        loadDataExchange();
        registerEvents();
        registerControl();
    }

    var bnbBalance = 0;


    function registerControl() {
    }

    function registerEvents() {

        be.registerNumber();

        $('.btnMax').on('click', function (e) {
            if (balance > 0) {

                $('#txtAmount').val(be.formatNumber(balance, 4));

                CalculateReceived();
            }
        });

        $("#btnExchangeDefiConfirm").on('click', async function (e) {
            var amount = parseFloat($('#txtAmount').val());

            if (amount < minPayment) {
                $('.lblErrorInsufficient').html("Minimum payment is " + minPayment + " BNB");
            } else {
                $('.lblErrorInsufficient').html("");
            }

            if (amount > 0) {
                await DeFiHelper.SaleToken(amount);
            }
        });


        $('#txtAmount').keyup(function (e) {

            var amount = parseFloat($(this).val().replace(/,/g, ''));

            if (isNaN(amount)) {
                amount = 0;
            }

            if (amount > balance) {
                $(this).val(be.formatNumber(balance, 4));
            }

            CalculateReceived();
        });

        $("#btnConfirm").on('click', async function (e) {

            e.preventDefault();

            var amount = parseFloat($('#txtAmount').val().replace(/,/g, ''));

            switch (unit) {
                case 2: 
                case 3:
                    if (amount < minPayment) {
                        be.notify('Min payment = ' +minPayment +' USDT', 'warning');
                        return;
                    }

                    break;
                case 5:
                    if (amount < minPayment) {
                        be.notify('Min payment = ' + minPayment + ' BNB', 'warning');
                        return;
                    }
                    break;
                default:
            }

            

            var postData = {
                Amount: amount,
                Unit: unit
            };

            be.confirm('Confirm to buy token', 'Are you sure to buy amount ' + amount + ' ' + tokenCode, function () {
                $.ajax({
                    type: "POST",
                    url: "/Admin/Exchange/ConfirmExchangeToken",
                    headers: {
                        "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                    },
                    data: JSON.stringify(postData),
                    dataType: "json",
                    contentType: "application/json",
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        if (response.Success) {
                            be.success('Payment', response.Message, function () {
                                window.location.reload();
                            });

                        } else {
                            be.error('Payment', response.Message);
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

        $('body').on('change', '#ddlWallet', function (e) {
            loadDataExchange();
        });
    }

    var tokenCode = '';
    var unit = 2;
    var balance = 0;
    var tokenPrice = 0;
    var bnbPrice = 0;
    var minPayment = 0;
    function loadDataExchange() {

        tokenCode = $('#ddlWallet option:selected').text();
        unit = parseInt($('#ddlWallet').val());
        $.ajax({
            type: 'GET',
            data: {
            },
            url: '/Admin/Exchange/GetBalance?unit=' + unit,
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                balance = response.Data.Balance;
                tokenPrice = response.Data.TokenPrice;
                bnbPrice = response.Data.BalanceInUSDT;

                $('.CurrentBalance').html(be.formatNumber(response.Data.Balance, 4));

                $('#tokenPrice').val(response.Data.TokenPrice);

                $('.tokenPrice').html(response.Data.TokenPrice);

                $('.lblTokenCode').html(tokenCode);

                $('.lblTotalUSD').val(response.Data.BalanceInUSDT);

                $('.min-buy').html(response.Data.MinPayment + ' ' + tokenCode);

                minPayment = response.Data.MinPayment;

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function CalculateReceived() {
        
        var amount = parseFloat($('#txtAmount').val().replace(/,/g, ''));
        var receiveAmount = 0;
        
        if (tokenCode == "BNB") {
            totalInUSDT = amount * bnbPrice;
            receiveAmount = totalInUSDT / tokenPrice;
            $('.lblTotalUSD').html(be.formatNumber(totalInUSDT, 4)  + "$");
        } else {
            $('.lblTotalUSD').html(be.formatNumber(amount, 4) + "$");
            receiveAmount = amount / tokenPrice;
        }
        

        if (amount > balance) {
            $(".lblErrorInsufficient").html("Insufficient account balance");
        }
        else {
            $(".lblErrorInsufficient").html("");
        }

        $('.lblTotalToken').html(be.formatNumber(receiveAmount, 2) + " " + $('#tokenCode').val());
    }
}