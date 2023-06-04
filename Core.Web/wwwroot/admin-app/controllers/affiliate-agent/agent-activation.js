var AgentActivationController = function () {
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

        $("#btnConfirm").on('click', async function (e) {

            e.preventDefault();

            if (!isAllowPayment) {
                be.notify(errorMsg, 'warning');
                return;
            }

            var postData = {
                unit: unit
            };

            be.confirm('Agent Activation', 'Are you sure to payment agent activation', function () {
                $.ajax({
                    type: "POST",
                    url: "/Admin/AffiliateAgent/ConfirmAgentActivation?unit=" + unit,
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
                            be.success('Agent Activation', response.Message, function () {
                                window.location.reload();
                            });

                        } else {
                            be.error('Agent Activation', response.Message);
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
            url: '/Admin/AffiliateAgent/GetBalance?unit=' + unit,
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
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