var DeFiHelper = {

    Web3Instant: function fn() {
        return new Web3(Web3.givenProvider)
    }(),

    ChainId: 56,
    ChainName: 'Binance Smart Chain',
    ChainTokenName: 'Binance Coin',
    CurrencyName: 'BNB',
    RpcUrls: '',
    CurrentAddress: '',
    BalanceInEth: 0.00,
    Decimals: 18,
    ChainExplorer: 'https://bscscan.com',
    ChainRpc: 'https://rpc.ankr.com/bsc',

    initialize: function () {

        if (!window.Web3Instant) {
            window.Web3Instant = new Web3(Web3.givenProvider);
        }

        if (!(ethereum && ethereum.isMetaMask)) {
            //show popup with link to metamask extension install
            //window.location.replace('https://metamask.io/');
        }

        if (!(ethereum && ethereum.isTrust)) {
            //window.location.replace('https://metamask.io/');
        }
    },

    CheckNetWork: async function fn() {
        try {
            let currentChain = await DeFiHelper.Web3Instant.eth.getChainId();

            if (currentChain === DeFiHelper.ChainId) {
                return true;
            }

            return false

        } catch (e) {
            be.error('DAPP Notification', e.message)
            return false
        }
    },

    AnyConnectedAccounts: async function fn() {
        try {
            var accounts = await DeFiHelper.Web3Instant.eth.getAccounts();

            return accounts && accounts.length > 0;
        } catch (e) {
            console.log('DAPP Notification', e.message);

            return false;
        }
    },

    AddBSCChain: async function fn() {
        window.ethereum.request({
            method: 'wallet_addEthereumChain',
            params: [{
                chainId: Web3.utils.numberToHex(DeFiHelper.ChainId),
                chainName: DeFiHelper.ChainName,
                nativeCurrency: {
                    name: DeFiHelper.ChainTokenName,
                    symbol: DeFiHelper.CurrencyName,
                    decimals: DeFiHelper.Decimals
                },
                rpcUrls: [RpcUrls],
                blockExplorerUrls: ['https://bscscan.com']
            }]
        }).catch((error) => {
            console.log(error)
        });
    },

    SwitchChain: async function fn() {

        if (ethereum.isTrust)
            return true

        try {

            await ethereum.request({
                method: 'wallet_switchEthereumChain',
                params: [{
                    chainId: Web3.utils.numberToHex(DeFiHelper.ChainId)
                }],
            });

            return true;
        } catch (switchError) {

            if (switchError.code === 4902) {
                try {
                    window.ethereum.request({
                        method: 'wallet_addEthereumChain',
                        params: [{
                            chainId: Web3.utils.numberToHex(DeFiHelper.ChainId),
                            chainName: DeFiHelper.ChainName,
                            nativeCurrency: {
                                name: DeFiHelper.ChainTokenName,
                                symbol: DeFiHelper.CurrencyName,
                                decimals: DeFiHelper.Decimals
                            },
                            rpcUrls: [DeFiHelper.ChainRpc],
                            blockExplorerUrls: [DeFiHelper.ChainExplorer]
                        }]
                    }).catch((error) => {
                        console.log(error)
                    });
                } catch (addError) {
                    return false
                }
            }

            return false
        }
    },

    RegisterAccountChangeEvents: function fn(accountChangeHandler) {
        ethereum.on('accountsChanged', accountChangeHandler);
    },

    RegisterNetworkChangeEvents: function fn(networkChangeHandler) {
        ethereum.on('chainChanged', networkChangeHandler);
    },

    HasInstalledMetaMask: function fn() {
        try {
            return window.ethereum && window.ethereum.isMetaMask
        } catch (e) {
            return false;
        }
    },

    HasInstalledTrust: function fn() {
        try {
            return window.ethereum && window.ethereum.isTrust
        } catch (e) {
            return false;
        }
    },

    DApp: function () {

        this.init = async function fn() {

            registerEvents()

            //if (!window.ethereum) {
            //    //ShowConnectButton()
            //    //$('#connect-wallet-modal').modal('show');
            //    //$('.dapp_add-asset').hide()
            //    return
            //}

            //if (window.ethereum.isTrust) {
            //    //$('.dapp_add-asset').hide()
            //}

            DeFiHelper.RegisterAccountChangeEvents(handleAccountsChanged)

            DeFiHelper.RegisterNetworkChangeEvents(handleNetworkChanged)

            var hasAccounts = await DeFiHelper.AnyConnectedAccounts()

            if (hasAccounts) {

                var isValidNet = await DeFiHelper.CheckNetWork();

                if (isValidNet) {

                    await handleRequestAccounts();

                    return;

                } else {
                    //HandleDisconnected();
                    await DeFiHelper.DAppDisconnect();
                }
            } else {
                await DeFiHelper.DAppDisconnect();
                //HandleDisconnected();
            }
        }

        function registerEvents() {

            $('#wallet-connect').on('click', async function (e) {

                e.preventDefault();
                $('#ic_modal_wallet_list').modal('show');

            });

            $('#wallet-connect-metamask').on('click', async function (e) {

                e.preventDefault();
                debugger;
                var d = be.isDevice();
                if (!DeFiHelper.HasInstalledMetaMask()) {
                    if (be.isDevice()) {
                        var url = 'https://metamask.app.link/dapp/coinbest247.com/exchangedefi';
                        window.location.replace(url);
                        return;
                    }
                    
                }

                if (DeFiHelper.HasInstalledMetaMask()) {

                    await ConnectMetaMask();
                    $('#ic_modal_wallet_list').modal('hide');
                } else {
                   
                    $('#ic_modal_wallet_list').modal('hide');
                    $("#linkProvider").attr("href", "https://metamask.io/download/")
                    $('#ic_modal_wallet_install').modal('show');
                    return;
                }
            });

            $('#wallet-connect-trust').on('click', async function (e) {
                e.preventDefault()

                if (!DeFiHelper.HasInstalledTrust()) {
                    if (be.isDevice()) {
                        //https://github.com/satoshilabs/slips/blob/master/slip-0044.md
                        var url = 'https://link.trustwallet.com/open_url?url=https://coinbest247.com/exchangedefi';
                        window.location.replace(url);

                        return;
                    }
                    
                }

                if (DeFiHelper.HasInstalledTrust()) {

                    await ConnectTrustWallet();

                    $('#ic_modal_wallet_list').modal('hide');
                } else {
                 
                    $('#ic_modal_wallet_list').modal('hide');
                    $("#linkProvider").attr("href", "https://trustwallet.com/download")
                    $('#ic_modal_wallet_install').modal('show');
                    return;
                }
            });

        }

         

        async function ConnectTrustWallet() {

            

            var isValidNet = await DeFiHelper.CheckNetWork();

            if (!isValidNet) {
                await DeFiHelper.SwitchChain();
            } else {
                await handleRequestAccounts();
            }
        }

        async function ConnectMetaMask() {

            if (!DeFiHelper.HasInstalledMetaMask()) {

                if (be.isDevice()) {

                    var url = 'https://metamask.app.link/dapp/coinbest247.com/exchangedefi';
                    window.location.replace(url);

                    return;
                }
            }

            var isValidNet = await DeFiHelper.CheckNetWork();

            if (!isValidNet) {
                await DeFiHelper.SwitchChain();
            } else {
                await handleRequestAccounts();
            }
        }

        async function CheckBalance(address) {

            try {

                DeFiHelper.Web3Instant.eth.getBalance(address).then((balanceInWei) => {

                    DeFiHelper.BalanceInEth = parseFloat(DeFiHelper.Web3Instant.utils.fromWei(balanceInWei));

                    $('.CurrentBalance').html(DeFiHelper.BalanceInEth);
                });
            } catch (error) {
                console.log(error);
            }
        }

        async function handleRequestAccounts() {
            try {

                var isValidNet = await DeFiHelper.CheckNetWork();

                if (!isValidNet) {
                    return;
                }
                debugger;

                var ids = window.ethereum.isMetaMask;

                var idds = window.ethereum.isTrust;

                let accounts = await DeFiHelper.Web3Instant.eth.requestAccounts();

                if (accounts && accounts.length > 0) {

                    this.CurrentAddress = accounts[0];

                    await DeFiHelper.DAppConnect(CurrentAddress);

                    await CheckBalance(CurrentAddress);

                    HandleConnected();

                    return accounts

                }
            } catch (e) {
                HandleDisconnected();
                await DeFiHelper.DAppDisconnect();
            }
        }

        async function handleAccountsChanged(accounts) {

            if (accounts.length === 0) {

                HandleDisconnected();

                await DeFiHelper.DAppDisconnect();

            } else if (accounts[0]) {

                var isValidNet = await DeFiHelper.CheckNetWork();

                if (isValidNet) {

                    CurrentAddress = accounts[0];

                    await CheckBalance(CurrentAddress);

                    await DeFiHelper.DAppConnect(CurrentAddress);

                    HandleConnected();

                    return;
                }

                HandleDisconnected();

                await DeFiHelper.DAppDisconnect();
            }
        }

        async function handleNetworkChanged(networkId) {
            if (networkId !== DeFiHelper.ChainId) {
                HandleDisconnected();
            }
        }

        function HandleConnected() {
            if (CurrentAddress == '') {
                return;
            }

            $('.walletAddress').html(CurrentAddress);
            $('#wallet-connect').hide();
            $('#btnExchangeDefiConfirm').show();

            LoadData();
        }

        function HandleDisconnected() {

            CurrentAddress = '';

            BalanceInEth = 0.00;

            $('.walletAddress').html('Not connected');
            $('#wallet-connect').show();
            $('#btnExchangeDefiConfirm').hide();
            $(".CurrentBalance").html('0.0000');

            LoadData();
        }

        function LoadData(isPageChanged) {

            $.ajax({
                type: 'GET',
                data: {
                    keyword: $('#txt-search-keyword').val(),
                    page: be.configs.pageIndex,
                    pageSize: be.configs.pageSize,
                    address: CurrentAddress
                },
                url: '/Admin/Exchange/GetAllPaging',
                dataType: 'json',
                beforeSend: function () {
                    //be.startLoading();
                },
                success: function (response) {

                    be.stopLoading();

                    var template = $('#table-template').html();
                    var render = "";

                    $.each(response.Results, function (i, item) {
                        render += Mustache.render(template, {
                            TransactionStateName: item.TransactionStateName,
                            BNBAmount: be.formatNumber(item.BNBAmount, 4),
                            DateCreated: be.dateTimeFormatJson(item.DateCreated),
                            USDAmount: be.formatNumber(item.USDAmount, 4),
                            BNBTransactionHash: item.BNBTransactionHash,
                            TokenTransactionHash: item.TokenTransactionHash,
                            TokenAmount: be.formatNumber(item.TokenAmount, 4),
                            AddressFrom: item.AddressFrom,
                            AddressTo: item.AddressTo
                        });
                    });

                    $('#tbl-content').html(render);

                    if (response.RowCount)
                        be.wrapPaging(response.RowCount, function () {
                            LoadData();
                        }, isPageChanged);
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        }
    },

    VerifyTransactionRequest: function fn(transactionHash, dappTxnHash) {

        let data = {
            TransactionHash: transactionHash,
            DappTxnHash: dappTxnHash
        }

        return DeFiHelper.PostAsync('/SaleDefi/VerifyTransactionRequest', data)
    },

    UpdateSaleError: function fn(transactionHex, errorCode) {
      
        var errorMsg = "Undefine error";

        if (errorCode !== undefined) {
            errorMsg = errorCode.toString()
        }
        let data = {
            TransactionHex: transactionHex,
            ErrorCode: errorMsg
        }

        return DeFiHelper.PostLegacyAsync('/SaleDefi/UpdateErrorMetaMask', data);
    },

    InitializeSaleTokenProgress: function fn(amount) {

        let data = {
            Address: DeFiHelper.CurrentAddress,
            IsDevice: DeFiHelper.isDevice,
            BNBAmount: amount
        }

        if (ethereum.isMetaMask) {
            data.WalletType = "Metamask"
        }

        if (ethereum.isTrust) {
            data.WalletType = "Trust"
        }

        return DeFiHelper
            .PostAsync('/SaleDefi/InitializeTransactionProgress/', data);
    },

    SendTransaction: async function fn(data) {

        return await ethereum.request({
            method: 'eth_sendTransaction',
            params: [
                data,
            ]
        });
    },

    ConfirmSaleTokenTransaction: async function fn(amount) {

        var isSwitchSucess = await DeFiHelper.SwitchChain()

        if (!isSwitchSucess) {
            return;
        }

        be.startLoading();

        DeFiHelper
            .InitializeSaleTokenProgress(amount)
            .then(async res => {

                if (!res.Success) {

                    be.stopLoading();

                    if (!res.Message) {
                        be.error('DAPP Notification', 'Can not process transaction.')
                        return;
                    }

                    be.error(res.Message)
                    return
                }

                var amount = Web3.utils.toWei(res.Data.Value.toString());

                var hexAmount = Web3.utils.numberToHex(amount);

                var gasPrice = Web3.utils.numberToHex(6000000000);

                let data = {
                    from: res.Data.From,
                    to: res.Data.To,
                    value: hexAmount,
                    gasPrice: gasPrice,
                    //gas: '0x2540be400',
                    data: res.Data.TransactionHex
                };

                let transactionHash = await DeFiHelper.SendTransaction(data)
                    .then(txh => txh)
                    .catch(error => {

                        if (window.ethereum.isMetaMask) {

                            DeFiHelper.UpdateSaleError(data.data, error.code);

                            if (error.code === 4001) {
                                be.error('DAPP Notification', 'Transaction was Rejected')
                            }
                            else {
                                be.error('DAPP Notification',
                                    'Something went wrong! Please contact administrator for support. Code: '
                                    + error.code)
                            }
                        }

                        if (window.ethereum.isTrust) {

                            DeFiHelper.UpdateSaleError(data.data, 4001);

                            be.error('DAPP Notification', 'Transaction was Rejected')
                        }

                        be.stopLoading();
                    })

                if (!transactionHash) {
                    return;
                }

                be.stopLoading();

                be.startLoading('<b>We are processing your transaction.</b>' +
                    '<b> Kindly wait for a moment ultil the process completed...</b>');

                DeFiHelper
                    .VerifyTransactionRequest(transactionHash, data.data)
                    .then(res => {

                        be.stopLoading();

                        if (!res.Success) {

                            be.error('DAPP Notification', res.Message)
                            return
                        }
                        be.success('DAPP Notification', res.Message, function () {
                            window.location.reload();
                        })
                    });
            })
            .catch(error => {

                //console.log(error);
                be.error('DAPP Notification', 'Something went wrong! please, contact administrator.')
                be.stopLoading();
            })
    },

    PostLegacyAsync: async function fn(url = '', data = {}) {
        $.ajax({
            type: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(data),
            url: url,
            dataType: 'JSON',
            beforeSend: function () { },
            success: function (response) {

                if (response.status === 401) {
                    be.error('DAPP Notification',
                        'Please, Disconnect and connect your wallet again!')
                }

                return response.json()
            },
            error: function (message) {
                be.error(`${message.responseText}`, 'error');

            },
        });
    },

    PostAsync: async function fn(url = '', data = {}) {

        return await fetch(url, {
            method: 'POST',
            dataType: 'json',
            mode: 'cors',
            headers: {
                'Content-Type': 'application/json',
                'ConnectedAddress': DeFiHelper.CurrentAddress,
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify(data),
        })
            .then(response => {
                if (response.status === 401) {
                    be.error('DAPP Notification', 'Please, Disconnect and connect your wallet again!')
                }
                return response.json()
            })
    },

    GetAsync: async function fn(url = '') {
        let accounts = await DeFiHelper.Web3Instant.eth.getAccounts();

        let address = accounts[0];

        return await fetch(url, {
            method: 'GET',
            dataType: 'JSON',
            headers: {
                'Content-Type': 'application/json',
                'ConnectedAddress': address,
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
        }).then(response => {
            if (response.status === 401) {
                be.error('DAPP Notification', 'Please, Disconnect and connect your wallet again!')
            }
            return response.json()
        })
    },

    DAppConnect: async function DAppConnect(address) {

        this.CurrentAddress = address;
        if (address) {
            $('.walletAddress').html(address);
            $('#wallet-connect').hide();
            $('#btnExchangeDefiConfirm').show();
        }
    },

    DAppDisconnect: async function fn() {
        $('.walletAddress').html('Not connected');
        $('#wallet-connect').show();
        $('#btnExchangeDefiConfirm').hide();
    },

    CheckoutAssert: async function (checkoutAmount, rateId) {

        var isValidNet = await DeFiHelper.CheckNetWork();

        if (isValidNet) {
            await DeFiHelper.ConfirmProcessingTransaction(checkoutAmount, rateId);
        }
    },

    SaleToken: async function fn(checkoutAmount) {

        var isValidNet = await DeFiHelper.CheckNetWork();

        if (isValidNet) {
            await DeFiHelper.ConfirmSaleTokenTransaction(checkoutAmount);
        }
    }
}


if (typeof window.ethereum !== 'undefined') {
    console.log('MetaMask is installed!');
}