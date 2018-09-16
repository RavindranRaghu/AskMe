using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.bLogic
{
    public class AskMeContentManager
    {
        public string GreetResponse {
            get { return "Hello, How may i help you";}
        }

        public string GoodbyeResponse
        {
            get{ return "Thanks , Have a good one";}
        }

        public string NoIntentMatchedResponse
        {
            get{ return "Sorry, I did not understand";}
        }

        public string IntentPossibleMatchedResponse
        {
            get { return "Sorry, not sure I understand, Did You Mean"; }
        }


        public string IntentSuggestionResponse
        {
            get { return "(or) enter one of the Suggestions"; }
        }

        
    }
}