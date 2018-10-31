namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChatSession")]
    public partial class ChatSession
    {
        [Key]
        public int SessionId { get; set; }

        public DateTime SessionStart { get; set; }

        [StringLength(255)]
        public string SessionUd { get; set; }

        public bool isAuth { get; set; }

        public int? IntentBeforeAuth { get; set; }
        

    }
}
