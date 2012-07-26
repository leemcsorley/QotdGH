using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Qotd.Entities
{
    [Serializable]
    public class ActionEntry
    {
        public int Num { get; set; }

        public ActivityType Type { get; set; }

        public DateTime Date { get; set; }
    }

    public static class UserExtensions
    {
        private static readonly TimeSpan PERIOD_LENGTH = new TimeSpan(28, 0, 0, 0);

        public static void AddAction(this User user, ActivityType type)
        {
            DateTime cutoff = DateTime.Now.Date - PERIOD_LENGTH;
            var actions = user.ActionEntriesThisPeriod.Where(a => a.Date > cutoff)
                .ToList();

            var existing = actions.SingleOrDefault(a => a.Date == DateTime.Now.Date && a.Type == type);
            if (existing != null)
                existing.Num++;
            else
                actions.Add(new ActionEntry() { Date = DateTime.Now.Date, Num = 1, Type = type });
            user.ActionEntriesThisPeriod = actions.ToArray();

            switch (type)
            {
                case ActivityType.PostAnswer:
                    user.NumAnswers++;
                    break;
                case ActivityType.PostComment:
                    user.NumComments++;
                    break;
            }

            user.NumAnswersThisPeriod = actions.Where(a => a.Type == ActivityType.PostAnswer).Sum(a => (int?)a.Num).GetValueOrDefault();
        }
    }

    public class User : EntityBase
    {
        private ActionEntry[] _actionEntriesThisPeriod;

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

        public virtual int NumAnswersThisPeriod { get; set; }

        public virtual int NumQuestionsThisPeriod { get; set; }

        public virtual int NumCommentsThisPeriod { get; set; }

        public virtual int NumAnswersVotedThisPeriod { get; set; }

        public virtual int NumQuestionsVotedThisPeriod { get; set; }

        public virtual int TotalAnswerVotesThisPeriod { get; set; }

        public virtual int TotalQuestionVotesThisPeriod { get; set; }

        public int Score
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public int ScoreThisPeriod
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

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
                    ms.Read(ActionEntriesThisPeriod_Data, 0, (int)ms.Length);
                }
            }
        }

        public virtual byte[] ActionEntriesThisPeriod_Data { get; set; }
    }
}
