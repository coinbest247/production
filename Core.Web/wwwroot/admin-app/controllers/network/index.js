var NetworkController = function () {
    this.initialize = function () {
        loadTreeview();
        loadNetworkInfo();
        registerEvents();
    }
    var refIndex = 1;
    function registerEvents() {

        $('.txt-search-keyword-1,.txt-search-keyword-2,.txt-search-keyword-3').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                be.configs.pageSize = $("#ddl-show-page").val();
                be.configs.pageIndex = 1;
                loadData(true);
            }
        });
        $('.nav-link-network').click(function () {
            var indexNumber = $(this).attr('data-id');
            if (refIndex != indexNumber) {
                be.configs.pageSize = $("#ddl-show-page").val();
                be.configs.pageIndex = 1;
                refIndex = indexNumber;
                loadData(true);
            }
        });
        $("#ddl-show-page").on('change', function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });

        document.getElementById("btnCopyReferlink").addEventListener("click", function () {
            copyToClipboard(document.getElementById("txtReferlink"));
        });
    };

    function copyToClipboard(elem) {
        // create hidden text element, if it doesn't already exist
        var targetId = "_hiddenCopyText_";
        var isInput = elem.tagName === "INPUT" || elem.tagName === "TEXTAREA";
        var origSelectionStart, origSelectionEnd;
        if (isInput) {
            // can just use the original source element for the selection and copy
            target = elem;
            origSelectionStart = elem.selectionStart;
            origSelectionEnd = elem.selectionEnd;
        } else {
            // must use a temporary form element for the selection and copy
            target = document.getElementById(targetId);
            if (!target) {
                var target = document.createElement("textarea");
                target.style.position = "absolute";
                target.style.left = "-9999px";
                target.style.top = "0";
                target.id = targetId;
                document.body.appendChild(target);
            }
            target.textContent = elem.textContent;
        }
        // select the content
        var currentFocus = document.activeElement;
        target.focus();
        target.setSelectionRange(0, target.value.length);

        // copy the selection
        var succeed;
        try {
            succeed = document.execCommand("copy");
        } catch (e) {
            succeed = false;
        }
        // restore original focus
        if (currentFocus && typeof currentFocus.focus === "function") {
            currentFocus.focus();
        }

        if (isInput) {
            // restore prior selection
            elem.setSelectionRange(origSelectionStart, origSelectionEnd);
        } else {
            // clear temporary content
            target.textContent = "";
        }

        be.notify('Copy to clipboard is successful', 'success');

        return succeed;
    }

    function loadTreeview() {
        $("div#jstree").jstree({
            plugins: ["state", "types"],
            core: {
                themes: { responsive: false },
                "check_callback": true,
                'data': {
                    'url': function (node) {
                        return '/Admin/Network/GetMemberTreeNode'; // Demo API endpoint -- Replace this URL with your set endpoint
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


    function loadNetworkInfo() {
        $.ajax({
            type: 'GET',
            url: '/Admin/Network/GetNetworkSummaryInfo',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#TotalNetwork').text(response.TotalNetwork);
                $('#TotalInvest').text(response.TotalInvest);
                $('#TotalInvestAffiliate').text(response.TotalInvestAffiliate);
                $('#TotalBuyInsurance').text(response.TotalNetwork);
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}