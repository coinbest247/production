﻿using Core.Data.Entities;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public abstract class BaseService
    {
        protected readonly UserManager<AppUser> _userManager;

        protected BaseService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        protected virtual async Task<TResult> VerifyAuthenticatorCodeBeforeExcute<TResult>(AppUser user, string code, Func<Task<TResult>> func)
        {
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);

            if (!isValid) return default(TResult);

            return await func.Invoke();
        }
    }
}

