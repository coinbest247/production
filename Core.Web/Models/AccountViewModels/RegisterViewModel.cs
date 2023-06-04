﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        public string Sponsor { get; set; }

        public string BNBBEP20PublishKey { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public bool IsAcceptToc { get; set; }

        public bool IsAcceptUpdated { get; set; }   
    }
}
