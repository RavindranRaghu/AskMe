namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChatIntent")]
    public partial class ChatIntent
    {
        [Key]
        public int ChatIntentId { get; set; }

        [StringLength(255)]
        public string IntentName { get; set; }

        [StringLength(1000)]
        [Display(Name ="Alias Name")]
        public string IntentDescription { get; set; }

        public int ParentId { get; set; }

        //public bool? NeedEntity { get; set; }

        [StringLength(2300)]
        [Display(Name = "ChaT Bot Response")]
        public string Response { get; set; }

        [Display(Name = "Need Authorization")]
        public bool NeedAuth { get; set; }

        [Display(Name = "Is Redirect Flow")]
        public bool IsRedirect { get; set; }

        [Display(Name = "Redirect Intent")]
        public int? RedirectIntent { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; }
    }
}
