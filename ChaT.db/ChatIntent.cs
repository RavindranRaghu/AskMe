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

        [StringLength(2300)]
        [Display(Name = "ChaT Bot Response")]
        public string Response { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; }
    }
}
