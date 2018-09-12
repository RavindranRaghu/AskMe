using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

    public class AskDtoWithIntentList
    {
        public List<AskDto> askList { get; set; }
        public List<SelectListItem> intents { get; set; }
    }
}