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
        private int Node = 0;
        private ChatDatabaseModel db;
        private AskMeHiBye hiBye;
        private AskMeSuggestionMatch suggestionMatch;
        private AskMeContentManager contentManager = new AskMeContentManager();
        KeyValuePair<int, string> finalResponse = new KeyValuePair<int, string>();
        public AskMeChannel(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
            hiBye = new AskMeHiBye(Message, Node);
            suggestionMatch = new AskMeSuggestionMatch(Message, Node);
        }

        //ChaT Bot Reponse Main Entry
        public KeyValuePair<string, string> ChatResponse()
        {
            string responseMessage = contentManager.NoIntentMatchedResponse;
            string intentMessage = "NoIntentMatched";
            string entity = string.Empty;
            TFIDF getVocab = new TFIDF();
            Dictionary<string, string> reponseDict = new Dictionary<string, string>();

            if (hiBye.Greet())
            {
                return new KeyValuePair<string, string>("greet", contentManager.GreetResponse);
            }
            else if (hiBye.GoodBye())
            {
                return new KeyValuePair<string, string>("goodbye", contentManager.GoodbyeResponse);
            }

            List<string> vocabList = getVocab.GetVocabulary(Message);

            List<ChatIntent> intentList = (from intention in db.ChatIntent
                                           where intention.ChatIntentId > 2 && intention.ParentId == Node
                                           select intention).ToList();

            foreach (string vocab in vocabList)
            {
                var hasIntent = intentList.Where(x=>x.ParentId == Node).Where(x => vocab.Contains(x.IntentName) || x.IntentName.Contains(vocab));
                if (hasIntent.Any())
                {
                    Dictionary<int, string> intentDict = hasIntent.Select(t => new { t.ChatIntentId, t.IntentName}).ToList().ToDictionary(x => x.ChatIntentId, y => y.IntentName);
                    foreach (KeyValuePair<int, string> intent in intentDict)
                    {
                        entity = GetEntityforIntent(intent.Key, vocabList);
                        if (string.IsNullOrEmpty(entity))
                        {
                            responseMessage = intent.Value + " has been processed";
                        }
                        else
                        {
                            responseMessage = intent.Value + " has been processed on " + entity;
                        }
                        
                        intentMessage = intent.Value;
                        return new KeyValuePair<string, string>(intentMessage, responseMessage);
                    }
                }
            }

            Dictionary<int, string> intentNameDict = intentList.Select(t => new { t.ChatIntentId, t.IntentName }).ToList().ToDictionary(x => x.ChatIntentId, y => y.IntentName);
            LevenshteinDistance dist = new LevenshteinDistance();
            foreach (string vocab in vocabList)
            {
                foreach (KeyValuePair< int, string> intent in intentNameDict)
                {
                    if (dist.Compute(vocab, intent.Value) < 4)
                    {
                        entity = GetEntityforIntent(intent.Key, vocabList);
                        if (string.IsNullOrEmpty(entity))
                        {
                            responseMessage = intent.Value + " has been processed";
                        }
                        else
                        {
                            responseMessage = intent.Value + " has been processed on " + entity;
                        }
                        intentMessage = intent.Value;
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
                entity = GetEntityforIntent(questionHighestMatch.Value, vocabList);
                if (string.IsNullOrEmpty(entity))
                {
                    responseMessage = intent + " has been processed";
                }
                else
                {
                    responseMessage = intent + " has been processed on " + entity;
                }
                intentMessage = intent;
                return new KeyValuePair<string, string>(intentMessage, responseMessage);
            }

            return new KeyValuePair<string, string>(intentMessage, responseMessage);
        }

        //ChaT Initializer
        public KeyValuePair<int, string> ChatInitializer()
        {
            finalResponse = ChatResponseMain();
            if (finalResponse.Value == contentManager.NoIntentMatchedResponse)
            {
                var hasParentNode = db.ChatIntent.Where(x => x.ChatIntentId == Node && Node != 0);
                if (hasParentNode.Any())
                {
                    Node = hasParentNode.Select(y => y.ParentId).FirstOrDefault();
                    finalResponse = ChatInitializer();
                }
            }
            
            return finalResponse;
        }

        //ChaT Bot Reponse Main Entry
        public KeyValuePair<int, string> ChatResponseMain()
        {
            string responseMessage = contentManager.NoIntentMatchedResponse;
            TFIDF getVocab = new TFIDF();
            Dictionary<string, string> reponseDict = new Dictionary<string, string>();

            #region 1.CheckIntentGreetingOrGoodbye
            if (hiBye.Greet())
                return new KeyValuePair<int, string>(0, contentManager.GreetResponse);
            else if (hiBye.GoodBye())
                return new KeyValuePair<int, string>(0, contentManager.GoodbyeResponse);
            #endregion

            List<ChatIntent> intentList = (from intention in db.ChatIntent
                                           where intention.ChatIntentId > 2 && intention.ParentId == Node
                                           select intention).ToList();

            #region 2.CheckIntentFullMatchbySuggestion
            KeyValuePair<int, bool> fullMatch = suggestionMatch.FullMatch(intentList);
            if (fullMatch.Value)
            {
                responseMessage = intentList.Where(x => x.ChatIntentId == fullMatch.Key).Select(x => x.Response).FirstOrDefault();
                return new KeyValuePair<int, string>(fullMatch.Key, responseMessage);
            }

            KeyValuePair<int, bool> partialMatch = suggestionMatch.PartialMatch(intentList);
            if (partialMatch.Value)
            {
                responseMessage = intentList.Where(x => x.ChatIntentId == partialMatch.Key).Select(x => x.Response).FirstOrDefault();
                return new KeyValuePair<int, string>(partialMatch.Key, responseMessage);
            }
            #endregion


            List<string> vocabList = getVocab.GetVocabulary(Message);
            if (vocabList.Count == 0)
                return new KeyValuePair<int, string>(Node, responseMessage);

            //foreach (string vocab in vocabList)
            //{
            //    var hasIntent = intentList.Where(x => x.ParentId == Node).Where(x => vocab.Contains(x.IntentName) || x.IntentName.Contains(vocab));
            //    if (hasIntent.Any())
            //    {
            //        Dictionary<int, string> intentDict = hasIntent.Select(t => new { t.ChatIntentId, t.IntentName }).ToList().ToDictionary(x => x.ChatIntentId, y => y.IntentName);
            //        foreach (KeyValuePair<int, string> intent in intentDict)
            //        {
            //            KeyValuePair<int, string> childIntent = GetChildIntent(intent.Key, vocabList);
            //            if (string.IsNullOrEmpty(childIntent.Value))
            //            {
            //                responseMessage = intentList.Where(x => x.ChatIntentId == intent.Key).Select(y => y.Response).FirstOrDefault();
            //                Node = intent.Key;
            //            }
            //            else
            //            {
            //                responseMessage = childIntent.Value;
            //                Node = childIntent.Key;
            //            }
            //            return new KeyValuePair<int, string>(Node, responseMessage);
            //        }
            //    }
            //}
            //Dictionary<int, string> intentNameDict = intentList.Select(t => new { t.ChatIntentId, t.IntentName }).ToList().ToDictionary(x => x.ChatIntentId, y => y.IntentName);
            //LevenshteinDistance dist = new LevenshteinDistance();
            //foreach (string vocab in vocabList)
            //{
            //    foreach (KeyValuePair<int, string> intent in intentNameDict)
            //    {
            //        if (dist.Compute(vocab, intent.Value) < 4)
            //        {
            //            KeyValuePair<int, string> childIntent = GetChildIntent(intent.Key, vocabList);
            //            if (string.IsNullOrEmpty(childIntent.Value))
            //            {
            //                responseMessage = intentList.Where(x => x.ChatIntentId == intent.Key).Select(y => y.Response).FirstOrDefault();
            //                Node = intent.Key;
            //            }
            //            else
            //            {
            //                responseMessage = childIntent.Value;
            //                Node = childIntent.Key;
            //            }
            //            return new KeyValuePair<int, string>(Node, responseMessage);
            //        }
            //    }
            //}

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
                string response = db.ChatIntent.Where(x => x.ChatIntentId == questionHighestMatch.Value).Select(y => y.Response).FirstOrDefault();
                //KeyValuePair<int, string> childIntent = GetChildIntent(questionHighestMatch.Value, vocabList);
                //if (string.IsNullOrEmpty(childIntent.Value))
                //{
                //    responseMessage = response;
                //    Node = questionHighestMatch.Value;
                //}
                //else
                //{
                //    responseMessage = childIntent.Value;
                //    Node = childIntent.Key;
                //}
                responseMessage = response;
                Node = questionHighestMatch.Value;
                return new KeyValuePair<int, string>(Node, responseMessage);
            }

            return new KeyValuePair<int, string>(Node, responseMessage);
        }


        private KeyValuePair<int, string> GetChildIntent(int chatIntentId, List<string> vocabList)
        {
            string entity = string.Empty;
            List<ChatIntent> intentList = db.ChatIntent.Where(z => z.ParentId == chatIntentId).ToList();

            foreach (string vocab in vocabList)
            {
                var hasIntent = intentList.Where(x => vocab.Contains(x.IntentName) || x.IntentName.Contains(vocab));
                if (hasIntent.Any())
                {
                    Dictionary<int, string> intentNameDict = hasIntent.Select(t => new { t.ChatIntentId, t.Response }).ToList().ToDictionary(x => x.ChatIntentId, y => y.Response);
                    foreach (KeyValuePair<int, string> intentName in intentNameDict)
                    {
                        return intentName;
                    }
                }
            }

            List<string> intentNames = intentList.Select(x => x.IntentName).ToList();
            LevenshteinDistance dist = new LevenshteinDistance();
            foreach (string vocab in vocabList)
            {
                foreach (string intentName in intentNames)
                {
                    if (dist.Compute(vocab, intentName) < 4)
                    {
                        //entity = entityName;
                        //return entity;
                    }
                }
            }

            return new KeyValuePair<int, string>();
        }

        private string GetEntityforIntent(int chatIntentId, List<string> vocabList)
        {
            string entity = string.Empty;
            List<ChatEntity> entityList = db.ChatEntity.Where(z => z.ChatIntentId == chatIntentId.ToString()).ToList();

            foreach (string vocab in vocabList)
            {
                var hasEntity = entityList.Where(x => vocab.Contains(x.EntityName) || x.EntityName.Contains(vocab));
                if (hasEntity.Any())
                {
                    List<string> withEntityList = hasEntity.Select(x => x.EntityName).ToList();
                    foreach (string entityName in withEntityList)
                    {
                        entity = entityName;
                        return entity;
                    }
                }
            }

            List<string> entityNames = entityList.Select(x => x.EntityName).ToList();
            LevenshteinDistance dist = new LevenshteinDistance();
            foreach (string vocab in vocabList)
            {
                foreach (string entityName in entityNames)
                {
                    if (dist.Compute(vocab, entityName) < 4)
                    {
                        //entity = entityName;
                        //return entity;
                    }
                }
            }

            return entity;
        }
    }
}