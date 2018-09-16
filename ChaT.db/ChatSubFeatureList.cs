namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SubFeatureList")]
    public partial class ChatSubFeatureList
    {
        [Key]
        public int SubFeatureId { get; set; }

        [StringLength(1000)]
        [Display(Name = "Feature Name")]
        public string SubFeatureName { get; set; }

        [StringLength(4000)]
        [Display(Name ="Feature Description")]
        public string SubFeatureDesc { get; set; }

        [Display(Name = "Story Groomed")]
        public bool StoryGroomed { get; set; }

        [Display(Name = "Development Complete")]
        public bool DevelopmentComplete { get; set; }

        public int FeatureId { get; set; }

        public int? EffortEstimated { get; set; }

        public int? EffortActual { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; }
    }
}
