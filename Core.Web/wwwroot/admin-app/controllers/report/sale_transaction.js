var ReportController = function () {
    this.initialize = function () {
        //loadReportInfo();
        registerEvents();
        registerControl();
    }

    function registerControl() {
        $("#FromDate").flatpickr();
        $("#ToDate").flatpickr();
    }

    function registerEvents() {

        $("#btnSearch").on('click', function (e) {
            loadReportInfo();
        });

        //UploadFile();
    };

    function loadReportInfo() {
        var fromDate = $("#FromDate").val();
        var toDate = $("#ToDate").val();
        var username = $("#username").val();
        $.ajax({
            type: "GET",
            url: "/admin/Report/GetSaleReportTransactionSummary",
            data: { keyword: username, FromDate: fromDate, ToDate: toDate },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $(".TotalMember").html(response.TotalMember);
                $(".totalBNBDeposit").html(response.TotalBNBDeposit);
                $(".totalBNBWithdraw").html(response.TotalBNBWithdraw);
                $(".totalUSDTDeposit").html(response.TotalUSDTDeposit);
                $(".totalUSDTWithdraw").html(response.TotalUSDTWithdraw);
                $(".totalSaving").html(response.TotalSavingUSDT);
                $(".totalExchangeSaleUSDT").html(response.TotalExchangeSaleUSDT);

                be.stopLoading();
                
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
            }
        });
    };

    function UploadFile() {
        $('#upload-file').on('click', function () {
            var file = $('#input-file').prop('files')[0];
            var token = $('input[name="__RequestVerificationToken"]').val();

            var formData = new FormData();

            formData.append('FileUpload', file);
            formData.append('__RequestVerificationToken', token);

            var url = '/Uploadfile/UploadMemberBalanceFile';

            be.startLoading();

            fetch(url, {
                method: 'POST',
                dataType: 'json',
                contentType: false,
                processData: false,
                body: formData
            })
                .then(response => {

                    return response.blob();
                })
                .then(blob => {
                    var url = window.URL.createObjectURL(blob);
                    var a = document.createElement('a');
                    a.href = url;
                    a.download = "notFoundMembers.csv";
                    document.body.appendChild(a); // we need to append the element to the dom -> otherwise it will not work in firefox
                    a.click();
                    a.remove();

                    be.stopLoading();
                    be.notify(`Imported Successful`, 'success');
                    $('#input-file').val('');
                    location.reload();
                })
                .catch(error => {
                    be.stopLoading();
                    $('#input-file').val('')
                });
        });
    }
}