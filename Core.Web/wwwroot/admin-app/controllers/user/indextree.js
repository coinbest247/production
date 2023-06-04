var CustomerTreeController = function () {
    this.initialize = function () {
        loadTreeview();
    }

    //function loadTreeview(reload) {
    //    $.ajax({
    //        url: '/Admin/User/GetTreeAll',
    //        dataType: 'json',
    //        beforeSend: function () {
    //            be.startLoading();
    //        },
    //        success: function (response) {
    //            be.stopLoading();

    //            if (reload) {
    //                $('div#jstree').jstree(true).settings.core.data = response;
    //                $('div#jstree').jstree(true).refresh();
    //            }
    //            else {
    //                fillData(response);
    //            }
    //        },
    //        error: function (message) {
    //            be.notify(`${message.responseText}`, 'error');
    //            be.stopLoading();
    //        }
    //    });
    //}

    function loadTreeview() {
        $("div#jstree").jstree({
            plugins: ["state", "types"],
            core: {
                themes: { responsive: false },
                "check_callback": true,
                'data': {
                    'url': function (node) {
                        return '/Admin/User/GetTreeNode'; // Demo API endpoint -- Replace this URL with your set endpoint
                    },
                    'data': function (node) {
                        return {
                            'parent': node.id
                        };
                    }
                }
            },
            types: {
                default: {
                    "icon": "fa fa-users text-primary"
                },
                file: {
                    "icon": "fa fa-user text-danger"
                }
            },
            state: { "key": "demo3" }
        }).bind("loaded.jstree", function (event, data) {
            $(this).jstree("open_all");
        })
    }
}