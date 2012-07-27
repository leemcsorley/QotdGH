using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Data;
using Qotd.Entities;

namespace Qotd.WorkerImpl
{
    public static class WorkerUtils
    {
        public static void PickWinningQuestion(this QotdContext db, DateTime? date = null)
        {
            if (date == null) date = DateTime.Now.Date;
            var qs = db.Questions.Where(q => q.DateFor == date).ToArray();

            int count = 0;
            foreach (Question question in qs.OrderByDescending(q => q.VotesTotal).ThenBy(q => q.denorm_User_OverallRankThisPeriod))
            {
                if (count == 0)
                {
                    question.WinningQuestion = true;
                    question.User.AddAction(ActivityType.QuestionWin, question);
                }
                else
                {
                    question.WinningQuestion = false;
                }
                count++;
            }
            db.SaveChanges();
        }

        public static void PickWinningAnswers(this QotdContext db, DateTime? date = null)
        {
            if (date == null) date = DateTime.Now.Date;
            var qs = db.Questions.Where(q => q.DateFor == date && q.WinningQuestion.HasValue && q.WinningQuestion.Value).Single();

            int count = 0;
            foreach (Answer answer in db.Answers.Where(a => a.QuestionId == qs.Id)
                .OrderByDescending(a => a.VotesTotal).ThenBy(a => a.denorm_User_OverallRankThisPeriod).Take(3))
            {
                switch (count)
                {
                    case 0:
                        answer.User.AddAction(ActivityType.AnswerWin, answer);
                        break;
                    case 1:
                        answer.User.AddAction(ActivityType.AnswerSecond, answer);
                        break;
                    case 2:
                        answer.User.AddAction(ActivityType.AnswerThird, answer);
                        break;
                }
                count++;
            }
            db.SaveChanges();
        }

        public static void TransitionToWinningQuestion(this QotdContext db, DateTime? date = null)
        {
            if (date == null) date = DateTime.Now.Date;
            var qc = db.QuestionContainers.Single();
            var wq = db.Questions.Where(q => q.DateFor == date && q.WinningQuestion == true).Single();
            qc.TodaysQuestion = wq;
            db.SaveChanges();
        }

        public static void CreateActivitiesAndNotifications(this QotdContext db)
        {
            // create links
            
            // TODO

            // create notifications
            var activities = db.Activities.Where(a => !a.NotificationsCreated).ToArray();

            Notification not;
            Guid[] userIds;
            foreach (var act in activities)
            {
                switch (act.ActivityType)
                {
                    case ActivityType.Join:
                        break;
                    case ActivityType.LikeComment:
                        // create notification for only the author of the comment
                        not = new Notification()
                        {
                            Date = act.Date,
                            IsRead = false,
                            SourceUserId = act.SourceUserId,
                            UserId = act.Comment.UserId,
                            ActivityType = act.ActivityType
                        };
                        act.NotificationsCreated = true;
                        db.MarkAddedOrUpdated(act);
                        db.MarkAddedOrUpdated(not);
                        db.SaveChanges();
                        break;
                    case ActivityType.PostComment:
                        Guid aid = act.Comment.AnswerId;
                        // create notifications for all the users in the comment stream and the author of the answer
                        userIds = db.Comments.Where(c => c.AnswerId == aid && c.UserId != act.SourceUserId)
                            .Select(c => c.UserId)
                            .Concat(db.Answers.Where(a => a.Id == aid && a.UserId != act.SourceUserId)
                                        .Select(a => a.UserId))
                            .Distinct()
                            .ToArray();
                        foreach (var uid in userIds)
                        {
                            not = new Notification()
                            {
                                Date = act.Date,
                                IsRead = false,
                                SourceUserId = act.SourceUserId,
                                UserId = uid,
                                ActivityType = act.ActivityType
                            };
                            db.MarkAddedOrUpdated(not);
                        }
                        act.NotificationsCreated = true;
                        db.MarkAddedOrUpdated(act);
                        db.SaveChanges();
                        break;
                    case ActivityType.PickSide:
                        break;
                    case ActivityType.PostAnswer:
                        break;
                    case ActivityType.PostQuestion:
                        break;
                    case ActivityType.VoteAnswer:
                        // create notification only for author of answer
                        not = new Notification()
                        {
                            Date = act.Date,
                            IsRead = false,
                            SourceUserId = act.SourceUserId,
                            UserId = act.Answer.UserId,
                            ActivityType = act.ActivityType
                        };
                        act.NotificationsCreated = true;
                        db.MarkAddedOrUpdated(act);
                        db.MarkAddedOrUpdated(not);
                        db.SaveChanges();
                        break;
                    case ActivityType.VoteQuestion:
                        // create notification only for author of question
                        not = new Notification()
                        {
                            Date = act.Date,
                            IsRead = false,
                            SourceUserId = act.SourceUserId,
                            UserId = act.Question.UserId,
                            ActivityType = act.ActivityType
                        };
                        act.NotificationsCreated = true;
                        db.MarkAddedOrUpdated(act);
                        db.MarkAddedOrUpdated(not);
                        db.SaveChanges();
                        break;
                }
            }
        }
    }
}
