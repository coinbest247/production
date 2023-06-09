﻿using Core.Application.ViewModels.System;
using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Application.ViewModels.Valuesshare
{
    public class NetworkViewModel
    {
        public NetworkViewModel()
        {
            Members = new List<AppUserViewModel>();
        }

        public bool EmailConfirmed { set; get; }
        public string Email { set; get; }
        public string Member { set; get; }
        public string Sponsor { get; set; }
        public string ReferalLink { get; set; }
        public string ReferralAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalMember { get; set; }

        public int TotalF1 { get; set; }
        public int TotalF2 { get; set; }
        public int TotalF3 { get; set; }
        public int TotalF4 { get; set; }
        public int TotalF5 { get; set; }

        public List<AppUserViewModel> Members { get; set; }
    }
}
