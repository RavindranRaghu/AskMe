using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.Dto
{
    public class AskDto
    {
        public int QuestionId { get; set; }

        public string QuestionDesc { get; set; }

        public int ChatIntentId { get; set; }

        public string ChatIntentName { get; set; }

        public DateTime UpdatedDate { get; set; }

    }
}