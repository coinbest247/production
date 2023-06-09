﻿using Core.Application.ViewModels.System;
using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Application.ViewModels.Blog
{
    public class MenuGroupViewModel
    {
        public MenuGroupViewModel()
        {
            MenuItems = new List<MenuItemViewModel>();
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { set; get; }

        public Status Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        public string RoleName { get; set; }

        public AppRoleViewModel AppRole { set; get; }
        public List<MenuItemViewModel> MenuItems { set; get; }
    }
}
