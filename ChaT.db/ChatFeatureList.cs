namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FeatureList")]
    public partial class ChatFeatureList
    {
        [Key]
        public int FeatureId { get; set; }

        [StringLength(1000)]
        [Display(Name = "Feature Name")]
        public string FeatureName { get; set; }

        [StringLength(4000)]
        [Display(Name ="Feature Description")]
        public string FeatureDesc { get; set; }

        [Display(Name = "Story Groomed")]
        public bool StoryGroomed { get; set; }

        [Display(Name = "Development Complete")]
        public bool DevelopmentComplete { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; }
    }
}
