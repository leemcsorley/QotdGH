﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Qotd.Data;
using Qotd.Utils;

namespace Qotd.Entities
{
    public class User : EntityBase, IActionEntryExtendedStats
    {
        private ActionEntry[] _actionEntriesThisPeriod;

        public virtual int OverallRank { get; set; }

        public virtual int OverallRankThisPeriod { get; set; }

        public virtual DateTime JoinedOn { get; set; }

        public virtual string Username { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual string Email { get; set; }

        public virtual string Password { get; set; }

        public virtual string ProfileImageUrl { get; set; }

        public virtual string FacebookId { get; set; }

        public virtual int NumFollows { get; set; }

        public virtual int NumFollowing { get; set; }

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

        public virtual int NumFollowsThisPeriod { get; set; }

        public virtual int NumFollowingThisPeriod { get; set; }

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

        // ratings - all relative
        public double AnswerQualityScore { get; set; }

        public int AnswerQualityRank { get; set; }

        public double AnswerQualityStars { get; set; }

        public double QuestionQualityScore { get; set; }

        public int QuestionQualityRank { get; set; }

        public double QuestionQualityStars { get; set; }

        public int ActivityLevelScore { get; set; }

        public int ActivityLevelRank { get; set; }

        public double ActivityLevelStars { get; set; }

        public int SociabilityScore { get; set; }

        public int SociabilityRank { get; set; }

        public double SociabilityStars { get; set; }

        public int OverallRating { get; set; }

        public int OverallRatingRank { get; set; }

        public double OverallStars { get; set; }

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
