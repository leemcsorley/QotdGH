using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Qotd.Data;

namespace Qotd.Entities
{
    [Serializable]
    public class ActionEntry
    {
        public virtual int Value { get; set; }

        public virtual ActivityType Type { get; set; }

        public virtual DateTime Date { get; set; }
    }

    public static class UserExtensions
    {
        private static readonly TimeSpan PERIOD_LENGTH = new TimeSpan(28, 0, 0, 0);

        private const int ANSWER_WIN_MULT = 4;
        private const int ANSWER_SECOND_MULT = 2;
        private const int ANSWER_THIRD_MULT = 1;
        private const int QUESTION_WIN_MULT = 10;

        public static void AddAction(this User user, ActivityType type, object obj = null)
        {
            // potentially optimise
            DateTime cutoff = DateTime.Now.Date - PERIOD_LENGTH;
            var actions = user.ActionEntriesThisPeriod == null ? new List<ActionEntry>() : user.ActionEntriesThisPeriod.Where(a => a.Date > cutoff)
                .ToList();

            var existing = actions.SingleOrDefault(a => a.Date == DateTime.Now.Date && a.Type == type);
            if (existing != null)
                existing.Value++;
            else
                actions.Add(new ActionEntry() { Date = DateTime.Now.Date, Value = 1, Type = type });

            var score = actions.SingleOrDefault(a => a.Date == DateTime.Now.Date && a.Type == ActivityType.ReceiveScore);
            if (score == null)
            {
                score = new ActionEntry() { Date = DateTime.Now.Date, Type = ActivityType.ReceiveScore };
                actions.Add(score);
            }

            int scoreDelta = 0;
            switch (type)
            {
                case ActivityType.PostAnswer:
                    user.NumAnswers++;
                    break;
                case ActivityType.PostComment:
                    user.NumComments++;
                    break;
                case ActivityType.AnswerWin:
                    user.NumAnswersWon++;
                    scoreDelta += ((Answer)obj).VotesTotal * ANSWER_WIN_MULT;
                    break;
                case ActivityType.AnswerSecond:
                    user.NumAnswersSecond++;
                    scoreDelta += ((Answer)obj).VotesTotal * ANSWER_SECOND_MULT;
                    break;
                case ActivityType.AnswerThird:
                    user.NumAnswersThird++;
                    scoreDelta += ((Answer)obj).VotesTotal * ANSWER_THIRD_MULT;
                    break;
                case ActivityType.PostQuestion:
                    user.NumQuestions++;
                    break;
                case ActivityType.QuestionWin:
                    user.NumQuestionsWon++;
                    scoreDelta += ((Question)obj).VotesTotal * QUESTION_WIN_MULT;
                    break;
                case ActivityType.ReceiveVoteUpAnswer:
                    user.TotalAnswerVotes++;
                    scoreDelta++;
                    break;
                case ActivityType.ReceiveVoteUpQuestion:
                    user.TotalQuestionVotes++;
                    scoreDelta++;
                    break;
                case ActivityType.ReceiveVoteDownAnswer:
                    user.TotalAnswerVotes--;
                    scoreDelta--;
                    break;
                case ActivityType.ReceiveVoteDownQuestion:
                    user.TotalQuestionVotes--;
                    scoreDelta--;
                    break;
                case ActivityType.VoteAnswer:
                    user.NumAnswersVoted++;
                    break;
                case ActivityType.VoteQuestion:
                    user.NumQuestionsVoted++;
                    break;
            }
            score.Value += scoreDelta;
            user.Score += scoreDelta;
            user.ActionEntriesThisPeriod = actions.ToArray();

            user.ScoreThisPeriod = actions.Where(a => a.Type == ActivityType.ReceiveScore).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumAnswersWonThisPeriod = actions.Where(a => a.Type == ActivityType.AnswerWin).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumAnswersSecondThisPeriod = actions.Where(a => a.Type == ActivityType.AnswerSecond).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumAnswersThirdThisPeriod = actions.Where(a => a.Type == ActivityType.AnswerThird).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumQuestionsWonThisPeriod = actions.Where(a => a.Type == ActivityType.QuestionWin).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumAnswersThisPeriod = actions.Where(a => a.Type == ActivityType.PostAnswer).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumAnswersVotedThisPeriod = actions.Where(a => a.Type == ActivityType.VoteAnswer).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumCommentsThisPeriod = actions.Where(a => a.Type == ActivityType.PostComment).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumQuestionsThisPeriod = actions.Where(a => a.Type == ActivityType.PostQuestion).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.NumQuestionsVotedThisPeriod = actions.Where(a => a.Type == ActivityType.VoteQuestion).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.TotalAnswerVotesThisPeriod = actions.Where(a => a.Type == ActivityType.ReceiveVoteUpAnswer).Sum(a => (int?)a.Value).GetValueOrDefault()
                - actions.Where(a => a.Type == ActivityType.ReceiveVoteDownAnswer).Sum(a => (int?)a.Value).GetValueOrDefault();
            user.TotalQuestionVotesThisPeriod = actions.Where(a => a.Type == ActivityType.ReceiveVoteUpQuestion).Sum(a => (int?)a.Value).GetValueOrDefault()
                - actions.Where(a => a.Type == ActivityType.ReceiveVoteDownQuestion).Sum(a => (int?)a.Value).GetValueOrDefault();
        }
    }

    public class User : EntityBase
    {
        private ActionEntry[] _actionEntriesThisPeriod;

        public virtual int OverallRank { get; set; }

        public virtual int OverallRankThisPeriod { get; set; }

        public virtual string Username { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual string Email { get; set; }

        public virtual string Password { get; set; }

        public virtual string ProfileImageUrl { get; set; }

        public virtual string FacebookId { get; set; }

        public virtual int NumComments { get; set; }

        public virtual int NumAnswers { get; set; }

        public virtual int NumQuestions { get; set; }

        public virtual int NumAnswersVoted { get; set; }

        public virtual int NumQuestionsVoted { get; set; }

        public virtual int NumAnswersWon { get; set; }

        public virtual int NumQuestionsWon { get; set; }

        public virtual int NumAnswersSecond { get; set; }

        public virtual int NumAnswersThird { get; set; }

        public virtual int TotalAnswerVotes { get; set; }

        public virtual int TotalQuestionVotes { get; set; }

        public virtual int NumQuestionsWonThisPeriod { get; set; }

        public virtual int NumAnswersWonThisPeriod { get; set; }

        public virtual int NumAnswersSecondThisPeriod { get; set; }

        public virtual int NumAnswersThirdThisPeriod { get; set; }

        public virtual int NumAnswersThisPeriod { get; set; }

        public virtual int NumQuestionsThisPeriod { get; set; }

        public virtual int NumCommentsThisPeriod { get; set; }

        public virtual int NumAnswersVotedThisPeriod { get; set; }

        public virtual int NumQuestionsVotedThisPeriod { get; set; }

        public virtual int TotalAnswerVotesThisPeriod { get; set; }

        public virtual int TotalQuestionVotesThisPeriod { get; set; }

        public int Score { get; set; }

        public int ScoreThisPeriod { get; set; }

        // complex serialized properties
        public virtual ActionEntry[] ActionEntriesThisPeriod
        {
            get
            {
                if (_actionEntriesThisPeriod == null && ActionEntriesThisPeriod_Data != null)
                {
                    using (MemoryStream ms = new MemoryStream(ActionEntriesThisPeriod_Data))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        _actionEntriesThisPeriod = (ActionEntry[])bf.Deserialize(ms);
                    }
                }
                return _actionEntriesThisPeriod;
            }
            set
            {
                _actionEntriesThisPeriod = value;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, _actionEntriesThisPeriod);
                    ActionEntriesThisPeriod_Data = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(ActionEntriesThisPeriod_Data, 0, (int)ms.Length);
                }
            }
        }

        public virtual byte[] ActionEntriesThisPeriod_Data { get; set; }
    }
}
