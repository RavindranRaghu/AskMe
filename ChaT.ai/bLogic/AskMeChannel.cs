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
        private AskMeCommon common;
        private AskMezPossibleMatch zPossibleMatch;
        private AskMeSynonymMatch synonymMatch;
        private AskMeContentManager contentManager = new AskMeContentManager();
        KeyValuePair<int, string> finalResponse = new KeyValuePair<int, string>();
        public AskMeChannel(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
            hiBye = new AskMeHiBye(Message, Node);
            suggestionMatch = new AskMeSuggestionMatch(Message, Node);
            common = new AskMeCommon(Message, Node);
            zPossibleMatch = new AskMezPossibleMatch(Message, Node);
            synonymMatch = new AskMeSynonymMatch(Message, Node);
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

            if (finalResponse.Value == contentManager.NoIntentMatchedResponse)
            {
                common.LogFailureResponse();
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
            KeyValuePair<int, bool> fullMatch = suggestionMatch.FullSuggestionMatch(intentList);
            if (fullMatch.Value)
            {
                responseMessage = intentList.Where(x => x.ChatIntentId == fullMatch.Key).Select(x => x.Response).FirstOrDefault();
                return new KeyValuePair<int, string>(fullMatch.Key, responseMessage);
            }

            KeyValuePair<int, bool> partialMatch = suggestionMatch.PartialSuggestionMatch(intentList);
            if (partialMatch.Value)
            {
                responseMessage = intentList.Where(x => x.ChatIntentId == partialMatch.Key).Select(x => x.Response).FirstOrDefault();
                return new KeyValuePair<int, string>(partialMatch.Key, responseMessage);
            }
            #endregion


            List<string> vocabList = getVocab.GetVocabulary(Message);
            if (vocabList.Count == 0)
                return new KeyValuePair<int, string>(Node, responseMessage);

            #region 3.TFIDF Match Process
            SimilarityCalculator similarityCalculator = new SimilarityCalculator();
            List<ChatIntentQuestion> questionList = db.ChatIntentQuestion.ToList();
            Dictionary<int, double> scoreDict = new Dictionary<int, double>();
            foreach (ChatIntentQuestion question in questionList)
            {
                double compare = similarityCalculator.CompareString(Message, question.QuestionDesc, 1);
                KeyValuePair<int, double> score = new KeyValuePair<int, double>(question.ChatIntentId, compare);
                if (scoreDict.ContainsKey(score.Key))
                {
                    if (scoreDict[score.Key] < compare)
                    {
                        scoreDict[score.Key] = compare;
                    }
                }
                else
                {
                    scoreDict.Add(score.Key, score.Value);
                }
            }

            if (scoreDict.Where(x => x.Value > 0.45).Any())
            {
                int maxScoreChatIntentId = scoreDict.OrderByDescending(x => x.Value).Select(y => y.Key).FirstOrDefault();
                string response = db.ChatIntent.Where(x => x.ChatIntentId == maxScoreChatIntentId).Select(y => y.Response).FirstOrDefault();
                responseMessage = response;
                Node = maxScoreChatIntentId;
                return new KeyValuePair<int, string>(Node, responseMessage);
            }
            else if (scoreDict.Where(x => x.Value >= 0.23).Any())
            {
                List<int> possibeMatch = scoreDict.OrderByDescending(x => x.Value).Where(x => x.Value >= 0.23).Select(y => y.Key).ToList();
                responseMessage = contentManager.IntentPossibleMatchedResponse;
                foreach (int match in possibeMatch)
                {
                    responseMessage = responseMessage + "<br>";
                    string suggestion = db.ChatIntent.Where(x => x.ChatIntentId == match).Select(y => y.IntentDescription).FirstOrDefault();
                    responseMessage = responseMessage + suggestion;
                }
                responseMessage = responseMessage + "<br>" + contentManager.IntentSuggestionResponse;
                return new KeyValuePair<int, string>(Node, responseMessage);
            }
            #endregion

            #region 4.Probable Match Process
            KeyValuePair<string, bool> probableMatchCorrect = zPossibleMatch.ProbableMatchCorrectSpelling(vocabList);
            if (probableMatchCorrect.Value)
            {
                common.LogFailureResponse();
                responseMessage = probableMatchCorrect.Key;
                return new KeyValuePair<int, string>(Node, responseMessage);
            }

            KeyValuePair<string, bool> probableMatchTypo = zPossibleMatch.ProbableMatchTypoError(vocabList);
            if (probableMatchTypo.Value)
            {
                common.LogFailureResponse();
                responseMessage = probableMatchTypo.Key;
                return new KeyValuePair<int, string>(Node, responseMessage);
            }
            #endregion

            #region 4.Synonym Match Process
            KeyValuePair<string, bool> synMatch = synonymMatch.SynonymMatch(vocabList);
            if (synMatch.Value)
            {
                common.LogFailureResponse();
                responseMessage = synMatch.Key;
                return new KeyValuePair<int, string>(Node, responseMessage);
            }
            #endregion

            return new KeyValuePair<int, string>(Node, responseMessage);
        }


        private string GetEntityforIntent(int chatIntentId, List<string> vocabList)
        {
            string entity = string.Empty;
            List<ChatEntity> entityList = new List<ChatEntity>(); // db.ChatEntity.Where(z => z.ChatIntentId == chatIntentId.ToString()).ToList();

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