var ReportController = function () {
    this.initialize = function () {

        registerControls();
        registerEvents();

    }


    function registerControls() {
        $('#txt-startdate').datepicker({
            autoclose: true
        });

       
    }

    function registerEvents() {
        

        $('#btnSearch').on('click', async function (e) {
            await loadTab1();
        });

    }

   

    async function loadTab1() {

        $.ajax({
            type: 'GET',
            data: {
                startDate: $('#txt-startdate').val()
            },
            url: '/Traffic/GetTraffics',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();

                var render = "";

                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        clientCountryName: item.countryName,
                        requests: item.requests
                        
                    });
                });

                $('#tbl-content').html(render);

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}