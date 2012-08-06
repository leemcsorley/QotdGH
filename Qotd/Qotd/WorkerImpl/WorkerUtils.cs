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
        public static void ProcessTags(this QotdContext db)
        {
            var alltags = db.Tags.ToArray().ToDictionary(t => t.Value.ToLower(), t => t);
            foreach (var ans in db.Answers.Where(a => !a.TagsProcessed).ToArray())
            {
                var tes = ans.TagEntries;
                foreach (var te in tes)
                {
                    Tag tag;
                    if (alltags.ContainsKey(te.Value.ToLower()))
                    {
                        tag = alltags[te.Value.ToLower()];
                        te.Approved = tag.Approved;
                    }
                    else
                    {
                        tag = new Tag()
                        {
                            Approved = false,
                            Value = te.Value
                        };
                        db.MarkAdded(tag);
                    }
                    AnswerTag at = new AnswerTag()
                    {
                        Answer = ans,
                        Tag = tag
                    };
                    db.MarkAdded(at);
                }
                ans.TagEntries = tes;
                ans.TagsProcessed = true;
                db.SaveChanges();
            }
            foreach (var qst in db.Questions.Where(q => !q.TagsProcessed).ToArray())
            {
                var tes = qst.TagEntries;
                Tag tag;
                foreach (var te in tes)
                {
                    if (alltags.ContainsKey(te.Value.ToLower()))
                    {
                        tag = alltags[te.Value.ToLower()];
                        te.Approved = tag.Approved;
                    }
                    else
                    {
                        tag = new Tag()
                        {
                            Approved = false,
                            Value = te.Value
                        };
                        db.MarkAdded(tag);
                    }
                    QuestionTag qt = new QuestionTag()
                    {
                        Question = qst,
                        Tag = tag
                    };
                    db.MarkAdded(qt);
                }
                qst.TagEntries = tes;
                qst.TagsProcessed = true;
                db.SaveChanges();
            }
        }
        
        public static void UpdateSiteStatistics(this QotdContext db)
        {
            var admin = db.Admins.Single();
            SiteStatistics stats = new SiteStatistics()
            {
                Date = DateTime.Now,
                MaxNumAnswers = db.Users.Max(u => u.NumAnswers),
                MaxNumAnswersSecond = db.Users.Max(u => u.NumAnswersSecond),
                MaxNumAnswersThird = db.Users.Max(u => u.NumAnswersThird),
                MaxNumAnswersSecondThisPeriod = db.Users.Max(u => u.NumAnswersSecondThisPeriod),

            };
            admin.LatestSiteStatistics = stats;
            db.MarkAddedOrUpdated(stats);
            db.MarkAddedOrUpdated(admin);
            db.SaveChanges();
        }

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

                    // create activity
                    Activity activity = new Activity()
                    {
                        ActivityType = ActivityType.QuestionWin,
                        Date = date.Value,
                        SourceUserId = question.UserId,
                        Question = question,
                        Text = "",
                        VisibleWithoutLink = true
                    };
                    db.MarkAddedOrUpdated(activity);
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
                Activity activity;
                switch (count)
                {
                    case 0:
                        answer.IsFirst = true;
                        answer.AnswerRank = 1;
                        answer.User.AddAction(ActivityType.AnswerWin, answer);
                        activity = new Activity()
                        {
                            ActivityType = ActivityType.AnswerWin,
                            Date = date.Value,
                            SourceUserId = answer.UserId,
                            Answer = answer,
                            Text = "",
                            VisibleWithoutLink = true
                        };
                        db.MarkAddedOrUpdated(answer);
                        db.MarkAddedOrUpdated(activity);
                        break;
                    case 1:
                        answer.IsSecond = true;
                        answer.AnswerRank = 2;
                        answer.User.AddAction(ActivityType.AnswerSecond, answer);
                        activity = new Activity()
                        {
                            ActivityType = ActivityType.AnswerSecond,
                            Date = date.Value,
                            SourceUserId = answer.UserId,
                            Answer = answer,
                            Text = "",
                            VisibleWithoutLink = true
                        };
                        db.MarkAddedOrUpdated(answer);
                        db.MarkAddedOrUpdated(activity);
                        break;
                    case 2:
                        answer.IsThird = true;
                        answer.AnswerRank = 3;
                        answer.User.AddAction(ActivityType.AnswerThird, answer);
                        activity = new Activity()
                        {
                            ActivityType = ActivityType.AnswerThird,
                            Date = date.Value,
                            SourceUserId = answer.UserId,
                            Answer = answer,
                            Text = "",
                            VisibleWithoutLink = true
                        };
                        db.MarkAddedOrUpdated(answer);
                        db.MarkAddedOrUpdated(activity);
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

        public static void CreateUserFollowLinks(this QotdContext db)
        {
            DateTime date = DateTime.Now.AddDays(-7);
            // for new user follows
            foreach (var uf in db.UserFollows.Where(u => !u.LinksCreated).ToArray())
            {
                foreach (var qid in db.Questions.Where(q => q.CreatedOn >= date && q.LinksCreated && q.UserId == uf.TargetUserId).Select(q => q.Id).ToArray())
                {
                    UserFollowQuestion ufq = db.UserFollowQuestions.SingleOrDefault(u => u.SourceUserId == uf.SourceUserId && u.QuestionId == qid);
                    if (ufq == null)
                    {
                        ufq = new UserFollowQuestion()
                        {
                            QuestionId = qid,
                            SourceUserId = uf.SourceUserId
                        };
                        db.MarkAdded(ufq);
                    }
                    ufq.Source = ufq.Source | FollowSource.UserFollow;
                }
                foreach (var aid in db.Answers.Where(q => q.CreatedOn >= date && q.LinksCreated && q.UserId == uf.TargetUserId).Select(q => q.Id).ToArray())
                {
                    UserFollowAnswer ufa = db.UserFollowAnswers.SingleOrDefault(u => u.SourceUserId == uf.SourceUserId && u.AnswerId == aid);
                    if (ufa == null)
                    {
                        ufa = new UserFollowAnswer()
                        {
                            AnswerId = aid,
                            SourceUserId = uf.SourceUserId
                        };
                        db.MarkAdded(ufa);
                    }
                    ufa.Source = ufa.Source | FollowSource.UserFollow;
                }
                uf.LinksCreated = true;
                db.SaveChanges();
            }
            // for new tag follows
            foreach (var uft in db.UserFollowTags.Where(u => !u.LinksCreated).ToArray())
            {
                foreach (var qid in db.QuestionTags.Where(q => q.TagId == uft.TagId && q.Question.CreatedOn >= date && q.Question.LinksCreated).Select(q => q.QuestionId).ToArray())
                {
                    UserFollowQuestion ufq = db.UserFollowQuestions.SingleOrDefault(u => u.SourceUserId == uft.SourceUserId && u.QuestionId == qid);
                    if (ufq == null)
                    {
                        ufq = new UserFollowQuestion()
                        {
                            QuestionId = qid,
                            SourceUserId = uft.SourceUserId
                        };
                        db.MarkAdded(ufq);
                    }
                    ufq.Source = ufq.Source | FollowSource.TagFollow;
                }
                foreach (var aid in db.AnswerTags.Where(a => a.TagId == uft.TagId && a.Answer.CreatedOn >= date && a.Answer.LinksCreated).Select(a => a.AnswerId).ToArray())
                {
                    UserFollowAnswer ufa = db.UserFollowAnswers.SingleOrDefault(u => u.SourceUserId == uft.SourceUserId && u.AnswerId == aid);
                    if (ufa == null)
                    {
                        ufa = new UserFollowAnswer()
                        {
                            AnswerId = aid,
                            SourceUserId = uft.SourceUserId
                        };
                        db.MarkAdded(ufa);
                    }
                    ufa.Source = ufa.Source | FollowSource.TagFollow;
                }
            }
            // for new questions
            foreach (var question in db.Questions.Where(q => (!q.LinksCreated)).ToArray())
            {
                // users
                foreach (var uf in db.UserFollows.Where(u => u.TargetUserId == question.UserId).ToArray())
                {
                    UserFollowQuestion ufq = db.UserFollowQuestions.SingleOrDefault(u => u.SourceUserId == uf.SourceUserId && u.QuestionId == question.Id);
                    if (ufq == null)
                    {
                        ufq = new UserFollowQuestion()
                        {
                            QuestionId = question.Id,
                            SourceUserId = uf.SourceUserId
                        };
                        db.MarkAdded(ufq);
                    }
                    ufq.Source = ufq.Source | FollowSource.UserFollow;
                }
                // tags
                foreach (var te in question.TagEntries.Where(t => t.Approved))
                    foreach (var uft in db.UserFollowTags.Where(u => u.TagId == te.TagId))
                    {
                        UserFollowQuestion ufq = db.UserFollowQuestions.SingleOrDefault(u => u.SourceUserId == uft.SourceUserId && u.QuestionId == question.Id);
                        if (ufq == null)
                        {
                            ufq = new UserFollowQuestion()
                            {
                                QuestionId = question.Id,
                                SourceUserId = uft.SourceUserId
                            };
                            db.MarkAdded(ufq);
                        }
                        ufq.Source = ufq.Source | FollowSource.TagFollow;
                    }
                question.LinksCreated = true;
                db.SaveChanges();
            }
            // for new answers
            foreach (var answer in db.Answers.Where(a => (!a.LinksCreated)).ToArray())
            {
                // users
                foreach (var uf in db.UserFollows.Where(u => u.TargetUserId == answer.UserId).ToArray())
                {
                    UserFollowAnswer ufa = new UserFollowAnswer()
                    {
                        AnswerId = answer.Id,
                        SourceUserId = uf.SourceUserId
                    };
                    db.MarkAdded(ufa);
                }
                // tags
                foreach (var te in answer.TagEntries.Where(t => t.Approved))
                    foreach (var uft in db.UserFollowTags.Where(u => u.TagId == te.TagId))
                    {
                        UserFollowAnswer ufq = db.UserFollowAnswers.SingleOrDefault(u => u.SourceUserId == uft.SourceUserId && u.AnswerId == answer.Id);
                        if (ufq == null)
                        {
                            ufq = new UserFollowAnswer()
                            {
                                AnswerId = answer.Id,
                                SourceUserId = uft.SourceUserId
                            };
                            db.MarkAdded(ufq);
                        }
                        ufq.Source = ufq.Source | FollowSource.TagFollow;
                    }
                answer.LinksCreated = true;
                db.SaveChanges();
            }
        }

        public static void AggregateNotifications(this QotdContext db)
        {
            foreach (var un in db.Notifications.Where(n => (!n.IsRead) && n.RelatedObjectId.HasValue)
                .OrderByDescending(n => n.Date)
                .GroupBy(n => new { n.UserId, n.ActivityTypeValue, n.RelatedObjectId.Value }).Where(g => g.Count() > 1).ToArray())
            {
                var ns = un.ToArray();
                ns[0].SourceUser2 = ns[1].SourceUser1;
                ns[0].SourceUser2ProfileImageUrl = ns[1].SourceUser1ProfileImageUrl;
                db.Notifications.Remove(ns[1]);
                if (ns.Length > 2)
                {
                    ns[0].SourceUser3 = ns[2].SourceUser1;
                    ns[0].SourceUser3ProfileImageUrl = ns[2].SourceUser1ProfileImageUrl;
                    db.Notifications.Remove(ns[2]);
                }
                if (ns.Length > 3)
                {
                    ns[0].OtherUserCount = ns.Length - 3;
                    for (int i = 3; i < ns.Length; i++)
                        db.Notifications.Remove(ns[i]);
                }
                db.SaveChanges();
            }
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
                            SourceUser1 = act.denorm_SourceUser_DisplayName,
                            SourceUser1ProfileImageUrl = act.denorm_SourceUser_ProfileImageUrl,
                            UserId = act.Comment.UserId,
                            ActivityType = act.ActivityType,
                            CommentId = act.CommentId,
                            AnswerId = act.AnswerId,
                            Text = act.Text,
                            RelatedObjectId = act.RelatedObjectId
                        };
                        act.NotificationsCreated = true;
                        db.MarkAddedOrUpdated(act);
                        db.MarkAddedOrUpdated(not);
                        db.SaveChanges();
                        break;
                    case ActivityType.PostComment:
                        Guid aid = act.AnswerId.Value;
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
                                SourceUser1 = act.denorm_SourceUser_DisplayName,
                                SourceUser1ProfileImageUrl = act.denorm_SourceUser_ProfileImageUrl,
                                UserId = uid,
                                ActivityType = act.ActivityType,
                                CommentId = act.CommentId,
                                AnswerId = act.AnswerId,
                                Text = act.Text,
                                Text2 = act.Text2,
                                RelatedObjectId = act.RelatedObjectId
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
                            SourceUser1 = act.denorm_SourceUser_DisplayName,
                            SourceUser1ProfileImageUrl = act.denorm_SourceUser_ProfileImageUrl,
                            UserId = act.Answer.UserId,
                            ActivityType = act.ActivityType,
                            AnswerId = act.Answer.Id,
                            Text = act.Text,
                            Text2 = act.Text2,
                            RelatedObjectId = act.RelatedObjectId
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
                            SourceUser1 = act.denorm_SourceUser_DisplayName,
                            SourceUser1ProfileImageUrl = act.denorm_SourceUser_ProfileImageUrl,
                            UserId = act.Question.UserId,
                            ActivityType = act.ActivityType,
                            QuestionId = act.Question.Id,
                            Text = act.Text,
                            Text2 = act.Text2,
                            RelatedObjectId = act.RelatedObjectId
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
