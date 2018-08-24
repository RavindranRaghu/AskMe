using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.Dto
{
    public class AudioContent
    {
        public string contentType { get; set; }
        public string contentAsBase64String {get; set;}
        public string fileName { get; set; }
    }
}