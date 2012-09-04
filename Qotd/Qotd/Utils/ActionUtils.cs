using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;

namespace Qotd.Utils
{
    [Serializable]
    public class ActionEntry
    {
        public virtual int Value { get; set; }

        public virtual ActivityType Type { get; set; }

        public virtual DateTime Date { get; set; }
    }

    public interface IActionEntryStats
    {
        int NumFollowing { get; set; }

        int NumFollowingThisPeriod { get; set; }

        int NumAnswers { get; set; }

        int NumAnswersThisPeriod { get; set; }

        int NumQuestions { get; set; }

        int NumQuestionsThisPeriod { get; set; }

        int NumAnswersWon { get; set; }

        int NumAnswersWonThisPeriod { get; set; }

        int NumAnswersSecond { get; set; }

        int NumAnswersSecondThisPeriod { get; set; }

        int NumAnswersThird { get; set; }

        int NumAnswersThirdThisPeriod { get; set; }

        int NumQuestionsWon { get; set; }

        int NumQuestionsWonThisPeriod { get; set; }

        ActionEntry[] ActionEntriesThisPeriod { get; set; }
    }

    public interface IActionEntryExtendedStats : IActionEntryStats
    {
        int ScoreThisPeriod { get; set; }

        int Score { get; set; }

        int TotalQuestionVotesThisPeriod { get; set; }

        int TotalQuestionVotes { get; set; }

        int TotalAnswerVotesThisPeriod { get; set; }

        int TotalAnswerVotes { get; set; }

        int NumQuestionsVotedThisPeriod { get; set; }

        int NumQuestionsVoted { get; set; }

        int NumAnswersVotedThisPeriod { get; set; }

        int NumAnswersVoted { get; set; }

        int NumFollowsThisPeriod { get; set; }

        int NumFollows { get; set; }

        int NumCommentsThisPeriod { get; set; }

        int NumComments { get; set; }

        double AnswerQualityScore { get; set; }

        int AnswerQualityRank { get; set; }

        double QuestionQualityScore { get; set; }

        int QuestionQualityRank { get; set; }

        int ActivityLevelScore { get; set; }

        int ActivityLevelRank { get; set; }

        int SociabilityScore { get; set; }

        int SociabilityRank { get; set; }
    }

    public static class ActionEntryExtensions
    {
        public static readonly TimeSpan PERIOD_LENGTH = new TimeSpan(28, 0, 0, 0);

        public const int ANSWER_WIN_MULT = 4;
        public const int ANSWER_SECOND_MULT = 2;
        public const int ANSWER_THIRD_MULT = 1;
        public const int QUESTION_WIN_MULT = 10;
        public const int ANSWER_ACT_MULT = 10;
        public const int QUESTION_ACT_MULT = 10;
        public const int FOLLOWING_MULT = 8;
        public const int FOLLOWS_MULT = 4;

        public static void AddAction<T>(this T stats, ActivityType type, object obj = null)
            where T : IActionEntryStats
        {
            IActionEntryExtendedStats estats = null;
            if (stats is IActionEntryExtendedStats)
                estats = (IActionEntryExtendedStats)stats;
            // potentially optimise
            DateTime cutoff = Qotd.Utils.Config.Now.Date - PERIOD_LENGTH;
            var actions = stats.ActionEntriesThisPeriod == null ? new List<ActionEntry>() : stats.ActionEntriesThisPeriod.Where(a => a.Date > cutoff)
                .ToList();

            var existing = actions.SingleOrDefault(a => a.Date == Qotd.Utils.Config.Now.Date && a.Type == type);
            if (existing != null)
                existing.Value++;
            else
                actions.Add(new ActionEntry() { Date = Qotd.Utils.Config.Now.Date, Value = 1, Type = type });

            var score = actions.SingleOrDefault(a => a.Date == Qotd.Utils.Config.Now.Date && a.Type == ActivityType.ReceiveScore);
            if (score == null)
            {
                score = new ActionEntry() { Date = Qotd.Utils.Config.Now.Date, Type = ActivityType.ReceiveScore };
                actions.Add(score);
            }

            int scoreDelta = 0;
            switch (type)
            {
                case ActivityType.PostAnswer:
                    stats.NumAnswers++;
                    break;
                case ActivityType.PostComment:
                    if (estats != null) estats.NumComments++;
                    break;
                case ActivityType.AnswerWin:
                    stats.NumAnswersWon++;
                    scoreDelta += ((Answer)obj).VotesTotal * ANSWER_WIN_MULT;
                    break;
                case ActivityType.AnswerSecond:
                    stats.NumAnswersSecond++;
                    scoreDelta += ((Answer)obj).VotesTotal * ANSWER_SECOND_MULT;
                    break;
                case ActivityType.AnswerThird:
                    stats.NumAnswersThird++;
                    scoreDelta += ((Answer)obj).VotesTotal * ANSWER_THIRD_MULT;
                    break;
                case ActivityType.PostQuestion:
                    stats.NumQuestions++;
                    break;
                case ActivityType.QuestionWin:
                    stats.NumQuestionsWon++;
                    scoreDelta += ((Question)obj).VotesTotal * QUESTION_WIN_MULT;
                    break;
                case ActivityType.ReceiveVoteUpAnswer:
                    if (estats != null) estats.TotalAnswerVotes++;
                    scoreDelta++;
                    break;
                case ActivityType.ReceiveVoteUpQuestion:
                    if (estats != null) estats.TotalQuestionVotes++;
                    scoreDelta++;
                    break;
                case ActivityType.ReceiveVoteDownAnswer:
                    if (estats != null) estats.TotalAnswerVotes--;
                    scoreDelta--;
                    break;
                case ActivityType.ReceiveVoteDownQuestion:
                    if (estats != null) estats.TotalQuestionVotes--;
                    scoreDelta--;
                    break;
                case ActivityType.VoteAnswer:
                    if (estats != null) estats.NumAnswersVoted++;
                    break;
                case ActivityType.VoteQuestion:
                    if (estats != null) estats.NumQuestionsVoted++;
                    break;
                case ActivityType.FollowUser:
                    if (estats != null) estats.NumFollows++;
                    break;
                case ActivityType.ReceiveFollow:
                    stats.NumFollowing++;
                    break;
            }
            score.Value += scoreDelta;
            if (estats != null) estats.Score += scoreDelta;
            stats.ActionEntriesThisPeriod = actions.ToArray();

            if (estats != null) estats.ScoreThisPeriod = actions.Where(a => a.Type == ActivityType.ReceiveScore).Sum(a => (int?)a.Value).GetValueOrDefault();
            if (estats != null) estats.NumFollowsThisPeriod = actions.Where(a => a.Type == ActivityType.FollowUser).Sum(a => (int?)a.Value).GetValueOrDefault();
            stats.NumFollowingThisPeriod = actions.Where(a => a.Type == ActivityType.ReceiveFollow).Sum(a => (int?)a.Value).GetValueOrDefault();
            stats.NumAnswersWonThisPeriod = actions.Where(a => a.Type == ActivityType.AnswerWin).Sum(a => (int?)a.Value).GetValueOrDefault();
            stats.NumAnswersSecondThisPeriod = actions.Where(a => a.Type == ActivityType.AnswerSecond).Sum(a => (int?)a.Value).GetValueOrDefault();
            stats.NumAnswersThirdThisPeriod = actions.Where(a => a.Type == ActivityType.AnswerThird).Sum(a => (int?)a.Value).GetValueOrDefault();
            stats.NumQuestionsWonThisPeriod = actions.Where(a => a.Type == ActivityType.QuestionWin).Sum(a => (int?)a.Value).GetValueOrDefault();
            stats.NumAnswersThisPeriod = actions.Where(a => a.Type == ActivityType.PostAnswer).Sum(a => (int?)a.Value).GetValueOrDefault();
            if (estats != null) estats.NumAnswersVotedThisPeriod = actions.Where(a => a.Type == ActivityType.VoteAnswer).Sum(a => (int?)a.Value).GetValueOrDefault();
            if (estats != null) estats.NumCommentsThisPeriod = actions.Where(a => a.Type == ActivityType.PostComment).Sum(a => (int?)a.Value).GetValueOrDefault();
            stats.NumQuestionsThisPeriod = actions.Where(a => a.Type == ActivityType.PostQuestion).Sum(a => (int?)a.Value).GetValueOrDefault();
            if (estats != null) estats.NumQuestionsVotedThisPeriod = actions.Where(a => a.Type == ActivityType.VoteQuestion).Sum(a => (int?)a.Value).GetValueOrDefault();
            if (estats != null) estats.TotalAnswerVotesThisPeriod = actions.Where(a => a.Type == ActivityType.ReceiveVoteUpAnswer).Sum(a => (int?)a.Value).GetValueOrDefault()
                    - actions.Where(a => a.Type == ActivityType.ReceiveVoteDownAnswer).Sum(a => (int?)a.Value).GetValueOrDefault();
            if (estats != null) estats.TotalQuestionVotesThisPeriod = actions.Where(a => a.Type == ActivityType.ReceiveVoteUpQuestion).Sum(a => (int?)a.Value).GetValueOrDefault()
                    - actions.Where(a => a.Type == ActivityType.ReceiveVoteDownQuestion).Sum(a => (int?)a.Value).GetValueOrDefault();

            // relative ratings
            if (estats != null)
            {
                // answer quality
                if (estats.NumAnswersThisPeriod > 0)
                {
                    estats.AnswerQualityScore = (estats.TotalAnswerVotesThisPeriod +
                        ANSWER_WIN_MULT * estats.NumAnswersWonThisPeriod +
                        ANSWER_SECOND_MULT * estats.NumAnswersSecondThisPeriod +
                        ANSWER_THIRD_MULT * estats.NumAnswersThisPeriod);
                }
                else estats.AnswerQualityScore = 0;
                // question quality
                if (estats.NumQuestionsThisPeriod > 0)
                {
                    estats.QuestionQualityScore = (estats.TotalQuestionVotesThisPeriod +
                        QUESTION_WIN_MULT * estats.NumQuestionsWonThisPeriod);
                }
                // activity
                estats.ActivityLevelScore = estats.NumAnswersThisPeriod * ANSWER_ACT_MULT +
                    estats.NumQuestionsThisPeriod * QUESTION_ACT_MULT +
                    estats.NumAnswersVotedThisPeriod +
                    estats.NumQuestionsVotedThisPeriod +
                    estats.NumComments;
                // sociability
                estats.SociabilityScore = estats.NumFollowingThisPeriod * FOLLOWING_MULT + estats.NumFollowsThisPeriod * FOLLOWS_MULT
                    + estats.NumComments;

            }
        }
    }
}
