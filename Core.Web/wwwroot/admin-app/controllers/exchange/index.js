var ExchangeController = function () {
    this.initialize = function () {
        loadDataExchange();
        registerEvents();
        registerControl();
    }

    function registerControl() {
    }

    function registerEvents() {

        be.registerNumber();

        $('.btnMax').on('click', function (e) {

            $('#txtAmount').val(be.formatNumber(balance, 4));

            CalculateReceived();
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
                        be.notify('Min payment = ' + minPayment + ' USDT', 'warning');
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
                    url: "/Admin/Exchange/ConfirmBuyToken",
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
    var coinPrice = 0;
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
                minPayment = response.Data.MinPayment;
                coinPrice = response.Data.CoinPrice;

                $('.CurrentBalance').html(be.formatNumber(balance, 4));

                $('#tokenPrice').val(tokenPrice);

                $('.tokenPrice').html(tokenPrice);

                $('.lblTokenCode').html(tokenCode);

                $('.minBuy').html(minPayment + ' ' + tokenCode);

                $('#txtAmount').val(be.formatNumber(0, 4))

                $('.lblTotalUSD').html('$0.00');

                $('.lblTotalToken').html('0.00 B247');

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

        var totalUSD = amount * coinPrice;

        $('.lblTotalUSD').html("$" + be.formatNumber(totalUSD, 2));

        var receiveAmount = amount * coinPrice / tokenPrice;

        $('.lblTotalToken').html(be.formatNumber(receiveAmount, 2) + " " + $('#tokenCode').val());
    }
}