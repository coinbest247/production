var StakingController = function () {
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
                AmountPayment: amount,
                Unit: unit
            };

            be.confirm('Confirm to staking PI', 'Are you sure to staking amount ' + amount + ' ' + tokenCode, function () {
                $.ajax({
                    type: "POST",
                    url: "/Admin/Staking/ConfirmStaking",
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

                            be.success('Confirm to staking PI', response.Message, function () {
                                window.location.reload();
                            });

                        } else {
                            be.error('Confirm to staking PI', response.Message);
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
            url: '/Admin/Staking/GetBalance?unit=' + unit,
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

                $('.lblTotalToken').html('0.00 PI');

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

        $('.lblTotalToken').html(be.formatNumber(receiveAmount, 2) + " " + "PI");
    }
}