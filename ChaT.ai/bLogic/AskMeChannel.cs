using AskMe.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChaT.db;

namespace ChaT.ai.bLogic
{
    public class AskMeChannel
    {
        private string Message = string.Empty;
        private ChatDatabaseModel db;
        public AskMeChannel(string message)
        {
            this.Message = message.ToLower();
            db = new ChatDatabaseModel();
        }

        public KeyValuePair<string, string> ChatResponse()
        {
            string responseMessage = "Sorry, I did not understand";
            string intentMessage = "NoIntentMatched";
            TFIDF getVocab = new TFIDF();

            if (Message.Contains("hello") || Message.Contains("good morning") || Message.Contains("how are you"))
            {
                return new KeyValuePair<string, string>("greet", "Hello");
            }
            else if (Message.Contains("good bye") || Message.Contains("take care") || Message.Contains("cya"))
            {
                return new KeyValuePair<string, string>("goodbye", "Thanks. Have a good one");
            }

            List<string> vocabList = getVocab.GetVocabulary(Message);
            List<ChatIntent> intentList = (from intention in db.ChatIntent
                                           where intention.ChatIntentId > 2
                                           select intention).ToList();

            List<string> intentNameList = intentList.Select(x => x.IntentName).ToList();

            foreach (string vocab in vocabList)
            {
                var hasIntent = intentList.Where(x => vocab.Contains(x.IntentName) || x.IntentName.Contains(vocab));
                if (hasIntent.Any())
                {
                    List<string> withIntentList = hasIntent.Select(x => x.IntentName).ToList();
                    foreach (string intent in withIntentList)
                    {
                        responseMessage = intent + " has been processed";
                        intentMessage = intent;
                        return new KeyValuePair<string, string>(intentMessage, responseMessage);
                    }
                }
            }

            LevenshteinDistance dist = new LevenshteinDistance();
            foreach (string vocab in vocabList)
            {
                foreach (string intent in intentNameList)
                {
                    if (dist.Compute(vocab, intent) < 4)
                    {
                        responseMessage = intent + " has been processed";
                        intentMessage = intent;
                        return new KeyValuePair<string, string>(intentMessage, responseMessage);
                    }
                }
            }

            SimilarityCalculator similarityCalculator = new SimilarityCalculator();
            List<ChatIntentQuestion> questionList = db.ChatIntentQuestion.ToList();
            Dictionary<string, int> questions = questionList.Select(t => new { t.QuestionDesc, t.ChatIntentId }).ToList().ToDictionary(x => x.QuestionDesc, y => y.ChatIntentId);
            KeyValuePair<string, int> questionHighestMatch = new KeyValuePair<string, int>();
            double compareHigh = 0;
            foreach (KeyValuePair<string, int> question in questions)
            {
                double compare = similarityCalculator.CompareString(Message, question.Key, 1);
                if (compareHigh < compare)
                {
                    compareHigh = compare;
                    questionHighestMatch = question;
                }
            }

            if (compareHigh > 0)
            {
                string intent = db.ChatIntent.Where(x => x.ChatIntentId == questionHighestMatch.Value).Select(y => y.IntentName).FirstOrDefault();
                responseMessage = intent + " has been processed";
                intentMessage = intent;
                return new KeyValuePair<string, string>(intentMessage, responseMessage);
            }

            return new KeyValuePair<string, string>(intentMessage, responseMessage);
        }
    }
}