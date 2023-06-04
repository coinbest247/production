var configsController = function () {

    this.initialize = function () {      
        loadData();        
        registerEvents();      
    }
    function registerEvents() {
        $('#btnCreate').off('click').on('click', function () {
            console.log("btn creat running");         
            $('#kt_modal_Add_Edit_Configs').modal('show');
        });

        $('body').on('click', '.btn-edit', function (e) {       
            var that = $(this).data('id');
            loadDetails(that);
        });

        $('.numberFormat').on("keypress", function (e) {
            var keyCode = e.which ? e.which : e.keyCode;
            console.log(keyCode);
            var ret = ((keyCode >= 48 && keyCode <= 57) || keyCode == 46 || keyCode == 45);
            if (ret)
                return true;
            else
                return false;
        });

        $('body').on('click', '.btn-delete', function (e) {
            //e.preventDefault();
            var that = $(this).data('id');
           
            be.confirm('Delete configs','Are you sure to delete?', function () {
                deleteConfigs(that);
            });
        });

        $('#btnSave').on('click', function (e) {
            saveConfigs(e);
        });

    }

    function loadData(isPageChanged) {
        var template = $("#table-template").html();
        var render = "";
        $.ajax({
            type: "GET",
            data: {
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
        
            url: "/admin/InvestTradingConfig/GetAllPaging",
            dataType: "json",
            success: function (response) {
                
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        Id: item.Id,
                        Name: item.Name,
                        Value: item.Value,
                        Description: item.Description,

                    });
                    
                });
                $('#lblTotalRecords').text(response.RowCount);
                $("#tbl-content").html(render);           
            },
            error: function (status) {
                console.log(status);
                be.notify('Cannot loading data', 'error')
            }
        })
    }

    function loadDetails(that) {
        $.ajax({
            type: "GET",
            url: "/Admin/InvestTradingConfig/GetById",
            data: { id: that },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                var data = response;
                $('#hidId').val(data.Id);
                $('#txtName').val(data.Name);                   
                $('#txtDes').val(data.Description);                 
                $('#numValue').val(data.Value);  
                $('#kt_modal_Add_Edit_Configs').modal('show');
                be.stopLoading();
            },
            error: function (status) {
                be.notify('Có lỗi xảy ra', 'error');
                be.stopLoading();
            }
        });
    }

    function deleteConfigs(that) {
        $.ajax({
            type: "POST",
            url: "/Admin/game/Delete",
            data: { id: that },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                be.notify('Delete successful', 'success');
                be.stopLoading();
                loadData(true);
            },
            error: function (status) {
                be.notify('Has an error in delete progress', 'error');
                be.stopLoading();
            }
        });
    }

    function saveConfigs(e) {         
            e.preventDefault();
        var id = $('#hidId').val();
            var value = $('#numValue').val();

            $.ajax({
                type: "POST",
                url: "/Admin/InvestTradingConfig/SaveConfig",
                data: {
                    Id: id,
                    Value: value,                     
                },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.notify('Update config successful', 'success');
                    $('#kt_modal_Add_Edit_Configs').modal('hide');
                    //resetFormMaintainance();
                    be.stopLoading();
                    loadData(true);
                },
                error: function () {
                    be.notify('Has an error in save product progress', 'error');
                    be.stopLoading();
                }      
        })
    }
}

