var NFTMarketController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {
        be.registerNumber();
    }

    function registerEvents() {

    }

    function loadData(isPageChanged) {
        $.ajax({
            type: 'GET',
            url: '/Admin/NFTMarketplace/GetNFTItems',
            dataType: 'json',
            beforeSend: function () {
            },
            success: function (response) {

                var cupTemplate = $('#cup-template').html();

                var cupRender = "";

                $.each(response.CupItems, function (i, item) {
                    cupRender += Mustache.render(cupTemplate, {
                        Id: item.Id,
                        ImageUrl: item.ImageUrl.replace('~', ''),
                        Name: item.Name,
                        HashRate: be.formatNumber(item.HashRate),
                        VolumeOfMining: be.formatNumber((60 * 60 * item.HashRate) / 36000000 ),
                        MaxOut: be.formatNumber(item.MaxOut),
                        TimeToUse: be.formatNumber(item.TimeToUse),
                        Price: be.formatNumber(item.Price),
                        Code: item.Code
                    });
                });

                $('#cup-content').html(cupRender);

                var machineTemplate = $('#machine-template').html();

                var machineRender = "";

                $.each(response.MachineItems, function (i, item) {
                    machineRender += Mustache.render(machineTemplate, {
                        Id: item.Id,
                        ImageUrl: item.ImageUrl.replace('~', ''),
                        Name: item.Name,
                        HashRate: be.formatNumber(item.HashRate),
                        TimeToUse: be.formatNumber(item.TimeToUse),
                        Price: be.formatNumber(item.Price),
                        Code: item.Code
                    });
                });

                $('#machine-content').html(machineRender);


                var shopItemTemplate = $('#shop-template').html();

                var shopRender = "";

                $.each(response.ShopItems, function (i, item) {
                    shopRender += Mustache.render(shopItemTemplate, {
                        Id: item.Id,
                        ImageUrl: item.ImageUrl.replace('~', ''),
                        Name: item.Name,
                        HashRate: be.formatNumber(item.HashRate),
                        TimeToUse: be.formatNumber(item.TimeToUse),
                        Price: be.formatNumber(item.Price),
                        Code: item.Code
                    });
                });

                $('#shop-content').html(shopRender);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
            }
        });
    }
}