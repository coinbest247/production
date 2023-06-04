var ExchangeController = function () {
    this.initialize = function () {

        loadTokenPrice();

        registerEvents();

        registerControl();
    }

    function registerControl() {

        $("#btnExchangeDefiConfirm").on('click', async function (e) {

            var amount = parseFloat($('#txtAmount').val());
            
            if (amount < minPayment) {
                $('.lblErrorInsufficient').html("Minimum payment is " + minPayment + " BNB");
                return;
            } else {
                $('.lblErrorInsufficient').html("");
            }

            if (DeFiHelper.BalanceInEth < amount) {
                $('.lblErrorInsufficient').html("Balance is not enough to payment " + minPayment + " BNB");
                return;
            } else {
                $('.lblErrorInsufficient').html("");
            }

            if (amount > 0) {
                await DeFiHelper.SaleToken(amount);
            }
        });
    }

    function registerEvents() {

        be.registerNumber();

        $('.btnMax').on('click', function (e) {

            var raw = parseFloat(DeFiHelper.BalanceInEth);

            if (raw > 0) {

                raw -= 0.0002;

                $('#txtAmount').val(be.formatNumber(raw, 4));

                CalculateInterest();
            }
        });

        $('#txtAmount').keyup(function (e) {

            var amount = parseFloat($(this).val());

            if (isNaN(amount)) {
                amount = 0;
            }

           CalculateInterest();
        });
    }

    function CalculateInterest() {
        
        var txtAmount = parseFloat($("#txtAmount").val());

        var totalUSD = bnbPrice * txtAmount;

        $(".lblTotalUSD").text("$" + be.formatCurrency(totalUSD));

        var totalToken = totalUSD / tokenPrice;

        $(".lblTotalToken").text(be.formatCurrency(totalToken) + " B247");
    }

    var tokenPrice = 0;

    var bnbPrice = 0;

    var minPayment = 0;

    function loadTokenPrice() {

        $.ajax({
            type: 'GET',
            data: {
            },
            url: '/Admin/Exchange/GetTokenPrice',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                tokenPrice = response.Data.TokenPrice;
                bnbPrice = response.Data.CoinPrice;
                minPayment = response.Data.MinPayment;

                $('#tokenPrice').val(response.Data.TokenPrice);

                $('.tokenPrice').html(response.Data.TokenPrice);

                $('.minBuy').html(response.Data.MinPayment);

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}