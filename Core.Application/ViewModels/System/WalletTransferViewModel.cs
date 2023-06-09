﻿using Core.Application.ViewModels.Blog;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.Valuesshare;
using Core.Data.Enums;
using System;
using System.Collections.Generic;

namespace Core.Application.ViewModels.System
{
    public class WalletTransferViewModel
    {
        public WalletTransferViewModel()
        {
        }

        public int Id { get; set; }
        public string PrivateKey { get; set; }
        public string PublishKey { get; set; }
        public string TransferHash { get; set; }
        public string RecallHash { get; set; }
        public decimal Amount { get; set; }
        public bool IsRecall { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
