﻿using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("MenuItems")]
    public class MenuItem : DomainEntity<int>, ISwitchable, IDateTracking
    {
        [Required]
        [MaxLength(256)]
        public string Name { set; get; }

        [Required]
        public int MenuGroupId { get; set; }

        [Required]
        [StringLength(128)]
        public string FunctionId { get; set; }

        [Required]
        [StringLength(250)]
        public string URL { set; get; }

        public int? ParentId { set; get; }

        public string IconCss { get; set; }
        public int SortOrder { set; get; }
        public Status Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        [ForeignKey("MenuGroupId")]
        public virtual MenuGroup MenuGroup { set; get; }

        [ForeignKey("FunctionId")]
        public virtual Function Function { set; get; }
    }
}
