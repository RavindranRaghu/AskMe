using ChaT.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.Dto
{
    public class IntentOperation
    {
        public ChatIntentDto intent { get; set; }
        public string Operation { get; set; }
    }

    public class ChatIntentDto
    {
        public int ChatIntentId { get; set; }

        public string IntentName { get; set; }

        public string IntentDescription { get; set; }

        public int ParentId { get; set; }

        public string ParentName { get; set; }

        public string Response { get; set; }

        public bool NeedAuth { get; set; }

        public bool IsRedirect { get; set; }

        public int? RedirectIntent { get; set; }

        public string RedirectIntentName { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}