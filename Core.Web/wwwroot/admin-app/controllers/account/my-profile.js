var MyProfileController = function () {
    this.initialize = function (TwoAuthIssuer) {
        registerEvents();
        registerControl(TwoAuthIssuer);
    }
    
    function registerControl(TwoAuthIssuer) {
        var code = $('#txtAuthenticatorCode').val();
        var email = $('#email').val();
         
        var encodedEmail = encodeURIComponent(email);

        var url = `otpauth://totp/${TwoAuthIssuer}:${encodedEmail}?secret=${code}&issuer=${TwoAuthIssuer}&digits=6`

        jQuery('#qrcodeAuthenticatorCode').qrcode({
            text: url,
            width: 300,
            height: 300,
        });
    }

    var registerEvents = function () {

        $('#btnMyProfile').on('click', function (e) {
            e.preventDefault();
            validateMyProfileInfo();
        });

        $('#btnVerifyCode').on('click', function (e) {
            e.preventDefault();
            verifyAuthenticatorCode();
        });

        $('#btnDisabled2FA').on('click', function (e) {
            e.preventDefault();
            disable2FA();
        });

        $('#btnChangePassword').on('click', function (e) {
            e.preventDefault();
            changePassword();
        });
    }

    function validateResetPassword(data) {
        var isValid = true;

        if (!data.ConfirmPassword) {
            isValid = be.notify('Confirm password is required', 'error');
        }

        if (!data.OldPassword) {
            isValid = be.notify('Old password is required', 'error');
        }

        if (!data.Password) {
            isValid = be.notify('New password is required', 'error');
        }

        if (data.Password !== data.ConfirmPassword) {
            isValid = be.notify('New password and confirmation password do not match', 'error');
        }

        return isValid;
    }

    function changePassword() {
        let data = {
            OldPassword: $('#txtOld').val(),
            Password: $('#txtNew').val(),
            ConfirmPassword: $('#txtConfirm').val(),
        }

        if (validateResetPassword(data)) {
            $.ajax({
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                type: 'POST',
                url: '/Admin/Account/ResetPasswordManual',
                dataType: 'json',
                data: JSON.stringify(data),
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    if (response.Success) {
                        be.success('Change Password', "Change password is successful", function () {
                            //window.location.reload();
                        });
                    }
                    else {
                        be.error('Change Password', response.Message);
                    }

                    be.stopLoading();
                },
                error: function (message) {

                    console.log(message.responseText);

                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                },
            });
        }
    }

    function disable2FA() {
        var confirmedPassword = $('#confirmedPassword2FA').val();
        var data = { Password: confirmedPassword }

        $.ajax({
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            type: 'POST',
            url: '/Authentication/Disable2FA',
            dataType: 'json',
            data: JSON.stringify(data),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                
                if (response.Success) {
                    be.success('Disable 2FA', "Your account has disabled 2FA", function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Disable 2FA', response.Message);
                }

                be.stopLoading();
            },
            error: function (message) {

                console.log(message.responseText);

                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            },
        });
    }

    function verifyAuthenticatorCode() {
        var verifyCode = $('#verifyCode').val();

        if (!verifyCode) return;

        $.ajax({
            type: 'GET',
            url: '/Authentication/EnableAuthentication/' + verifyCode,
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                if (response.Success) {
                    be.success('Verify code', "Your account has enabled 2FA", function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Verify code', response.Message);
                }

                be.stopLoading();
            },
            error: function (message) {

                console.log(message.responseText);

                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            },
        });
    }
}