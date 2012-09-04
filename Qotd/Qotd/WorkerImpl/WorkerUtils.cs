using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Data;
using Qotd.Entities;
using Qotd.Utils;
using System.Transactions;
using System.Data.SqlClient;
using System.Threading;

namespace Qotd.WorkerImpl
{
    public static class WorkerUtils
    {
        public static void ProcessAll()
        {
            while (true)
            {
                Thread.Sleep(500);
                using (QotdContext db = new QotdContext())
                {
                    db.CreateActivitiesAndNotifications();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.UpdateUserRankings();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.CreateUserFollowLinksForNewFollows();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.CreateUserFollowLinksForNewContent();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.ProcessTags();
                }
                using (QotdContext db = new QotdContext())
                {
                    db.AggregateNotifications();
                }
            }
        }

        public static void ProcessTags(this QotdContext db)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TimeSpan(1, 0, 0)))
            {
                var alltags = db.Tags.ToArray().ToDictionary(t => t.Value.ToLower(), t => t);
                var activities = db.Activities.Where(a => !a.Answer.TagsProcessed)
                    .Select(a => new { a.Id, a.AnswerId }).ToArray()
                    .GroupBy(a => a.AnswerId)
                    .ToDictionary(a => a.Key, a => a.ToArray());
                    // answers
                foreach (var ans in db.Answers.Where(a => !a.TagsProcessed)
                    .Select(a => new { a.Id, a.TagEntries_Data }).ToArray())
                {
                    var tes = ans.TagEntries_Data.Deserialise<TagEntry[]>();
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
                        tag.AddAction(ActivityType.PostAnswer, ans);
                        AnswerTag at = new AnswerTag()
                        {
                            AnswerId = ans.Id,
                            TagId = tag.Id
                        };
                        if (activities.ContainsKey(ans.Id))
                        {
                            foreach (var activity in activities[ans.Id])
                                db.MarkAdded(new ActivityTag()
                                {
                                    ActivityId = activity.Id,
                                    TagId = tag.Id
                                });
                            db.MarkAdded(at);
                        }
                    }
                    byte[] data = null;
                    tes.Serialise(d => data = d);
                    db.ObjectContext.ExecuteStoreCommand("update Answers set TagsProcessed = 1, TagEntries_Data = {1} where Id = {0}", ans.Id, data);
                }
                    // questions
                activities = db.Activities.Where(a => !a.Question.TagsProcessed)
                    .Select(a => new { a.Id, AnswerId = a.QuestionId }).ToArray()
                    .GroupBy(a => a.AnswerId)
                    .ToDictionary(a => a.Key, a => a.ToArray());
                foreach (var ans in db.Questions.Where(a => !a.TagsProcessed)
                    .Select(a => new { a.Id, a.TagEntries_Data }).ToArray())
                {
                    var tes = ans.TagEntries_Data.Deserialise<TagEntry[]>();
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
                        tag.AddAction(ActivityType.PostQuestion, ans);
                        QuestionTag at = new QuestionTag()
                        {
                            QuestionId = ans.Id,
                            TagId = tag.Id
                        };
                        if (activities.ContainsKey(ans.Id))
                        {
                            foreach (var activity in activities[ans.Id])
                                db.MarkAdded(new ActivityTag()
                                {
                                    ActivityId = activity.Id,
                                    TagId = tag.Id
                                });
                            db.MarkAdded(at);
                        }
                    }
                    byte[] data = null;
                    tes.Serialise(d => data = d);
                    db.ObjectContext.ExecuteStoreCommand("update Questions set TagsProcessed = 1, TagEntries_Data = {1} where Id = {0}", ans.Id, data);
                }
                db.SaveChanges();
                scope.Complete();
            }
        }
        
        public static void UpdateSiteStatistics(this QotdContext db)
        {
            var admin = db.Admins.Single();
            SiteStatistics stats = new SiteStatistics()
            {
                Date = Qotd.Utils.Config.Now,
                //MaxNumAnswers = db.Users.Max(u => u.NumAnswers),
                //MaxNumAnswersSecond = db.Users.Max(u => u.NumAnswersSecond),
                //MaxNumAnswersThird = db.Users.Max(u => u.NumAnswersThird),
                //MaxNumAnswersSecondThisPeriod = db.Users.Max(u => u.NumAnswersSecondThisPeriod),

            };
            admin.LatestSiteStatistics = stats;
            db.MarkAddedOrUpdated(stats);
            db.MarkAddedOrUpdated(admin);
            db.SaveChanges();
        }

        public static void PickWinningQuestion(this QotdContext db, DateTime? date = null)
        {
            if (date == null) date = Qotd.Utils.Config.Now.Date;
            var qs = db.Questions.Where(q => q.DateFor == date).ToArray();

            int count = 0;
            foreach (Question question in qs.OrderByDescending(q => q.VotesTotal).ThenBy(q => q.denorm_User_OverallRankThisPeriod))
            {
                if (count == 0)
                {
                    question.WinningQuestion = true;
                    question.User.AddAction(ActivityType.QuestionWin, question);
                    foreach (var te in question.TagEntries)
                    {
                        if (te.Approved)
                        {
                            var tag = db.Tags.Single(t => t.Id == te.TagId);
                            tag.AddAction(ActivityType.QuestionWin, question);
                        }
                    }

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
            if (date == null) date = Qotd.Utils.Config.Now.Date;
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
                        foreach (var te in answer.TagEntries)
                        {
                            if (te.Approved)
                            {
                                var tag = db.Tags.Single(t => t.Id == te.TagId);
                                tag.AddAction(ActivityType.AnswerWin, answer);
                            }
                        }
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
                        foreach (var te in answer.TagEntries)
                        {
                            if (te.Approved)
                            {
                                var tag = db.Tags.Single(t => t.Id == te.TagId);
                                tag.AddAction(ActivityType.AnswerSecond, answer);
                            }
                        }
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
                        foreach (var te in answer.TagEntries)
                        {
                            if (te.Approved)
                            {
                                var tag = db.Tags.Single(t => t.Id == te.TagId);
                                tag.AddAction(ActivityType.AnswerThird, answer);
                            }
                        }
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
            if (date == null) date = Qotd.Utils.Config.Now.Date;
            var qc = db.QuestionContainers.Single();
            var wq = db.Questions.Where(q => q.DateFor == date && q.WinningQuestion == true).Single();
            qc.TodaysQuestion = wq;
            db.SaveChanges();
        }

        private static void CreateLinksForNewUserFollow(QotdContext db, UserFollow userFollow, DateTime date)
        {
            var qst = db.Questions.Where(q => q.CreatedOn >= date && q.LinksCreated && q.UserId == userFollow.TargetUserId);
            var ans = db.Answers.Where(q => q.CreatedOn >= date && q.LinksCreated && q.UserId == userFollow.TargetUserId);
            foreach (var qid in qst.Select(q => q.Id).ToArray())
            {
                var ufq = db.UserFollowQuestions.SingleOrDefault(u => u.QuestionId == qid && u.SourceUserId == userFollow.SourceUserId);
                if (ufq == null)
                {
                    ufq = new UserFollowQuestion() { QuestionId = qid, SourceUserId = userFollow.SourceUserId };
                    db.MarkAdded(ufq);
                }
                ufq.Source = ufq.Source | FollowSource.UserFollow;
            }
            foreach (var aid in ans.Select(q => q.Id).ToArray())
            {
                var ufa = db.UserFollowAnswers.SingleOrDefault(u => u.AnswerId == aid && u.SourceUserId == userFollow.SourceUserId);
                if (ufa == null)
                {
                    ufa = new UserFollowAnswer() { AnswerId = aid, SourceUserId = userFollow.SourceUserId };
                    db.MarkAdded(ufa);
                }
                ufa.Source = ufa.Source | FollowSource.UserFollow;
            }
            db.ObjectContext.ExecuteStoreCommand("update UserFollows set LinksCreated = 1 where SourceUserId = {0} and TargetUserId = {1}", userFollow.SourceUserId, userFollow.TargetUserId);
        }

        private static void CreateLinksForNewUserFollowTag(QotdContext db, UserFollowTag userFollow, DateTime date)
        {
            var qst = db.QuestionTags.Where(q => q.Question.CreatedOn >= date && q.Question.LinksCreated && q.Question.TagsProcessed && q.TagId == userFollow.TagId)
                .Select(q => q.QuestionId)
                .Distinct();
            var ans = db.AnswerTags.Where(q => q.Answer.CreatedOn >= date && q.Answer.LinksCreated && q.Answer.TagsProcessed && q.TagId == userFollow.TagId)
                .Select(q => q.AnswerId)
                .Distinct();
            foreach (var qid in qst.ToArray())
            {
                var ufq = db.UserFollowQuestions.SingleOrDefault(u => u.QuestionId == qid && u.SourceUserId == userFollow.SourceUserId);
                if (ufq == null)
                {
                    ufq = new UserFollowQuestion() { QuestionId = qid, SourceUserId = userFollow.SourceUserId };
                    db.MarkAdded(ufq);
                }
                ufq.Source = ufq.Source | FollowSource.TagFollow;
            }
            foreach (var aid in ans.ToArray())
            {
                var ufa = db.UserFollowAnswers.SingleOrDefault(u => u.AnswerId == aid && u.SourceUserId == userFollow.SourceUserId);
                if (ufa == null)
                {
                    ufa = new UserFollowAnswer() { AnswerId = aid, SourceUserId = userFollow.SourceUserId };
                    db.MarkAdded(ufa);
                }
                ufa.Source = ufa.Source | FollowSource.TagFollow;
            }
            db.ObjectContext.ExecuteStoreCommand("update UserFollowTags set LinksCreated = 1 where SourceUserId = {0} and TagId = {1}", userFollow.SourceUserId, userFollow.TagId);
        }

        public static void CreateUserFollowLinksForNewFollows(this QotdContext db)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                DateTime date = Qotd.Utils.Config.Now.AddDays(-7);
                foreach (var uf in db.UserFollows.Where(u => !u.LinksCreated).ToArray())
                    CreateLinksForNewUserFollow(db, uf, date);
                foreach (var uf in db.UserFollowTags.Where(u => !u.LinksCreated).ToArray())
                    CreateLinksForNewUserFollowTag(db, uf, date);
                db.SaveChanges();
                scope.Complete();
            }
        }

        public static void CreateUserFollowLinksForNewContent(this QotdContext db)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                Dictionary<Tuple<Guid, Guid>, UserFollowQuestion> qcreated = new Dictionary<Tuple<Guid, Guid>, UserFollowQuestion>();
                var qids = db.QuestionTags.Where(q => !q.Question.LinksCreated)
                    .Select(q => new { q.QuestionId, q.TagId, q.Question.UserId })
                    .GroupBy(q => new { q.QuestionId, q.UserId })
                    .ToArray();
                foreach (var aid in db.Questions.Where(a => !a.LinksCreated)
                    .Select(a => new { a.Id, a.UserId }).ToArray())
                {
                    foreach (var uf in db.UserFollows.Where(u => u.TargetUserId == aid.UserId).ToArray())
                    {
                        var ufa = new UserFollowQuestion()
                        {
                            Source = FollowSource.UserFollow,
                            QuestionId = aid.Id,
                            SourceUserId = uf.SourceUserId
                        };
                        db.MarkAdded(ufa);
                        qcreated.Add(new Tuple<Guid, Guid>(aid.Id, uf.SourceUserId), ufa);
                    }
                    db.ObjectContext.ExecuteStoreCommand("update Questions set LinksCreated = 1 where Id = {0}", aid.Id);
                }
                foreach (var qid in qids)
                {
                    foreach (var qt in qid)
                    {
                        foreach (var uft in db.UserFollowTags.Where(u => u.TagId == qt.TagId).ToArray())
                        {
                            UserFollowQuestion ufq;
                            if (!qcreated.TryGetValue(new Tuple<Guid, Guid>(qid.Key.QuestionId, uft.SourceUserId), out ufq))
                            {
                                ufq = new UserFollowQuestion()
                                {
                                    SourceUserId = uft.SourceUserId,
                                    QuestionId = qid.Key.QuestionId
                                };
                                db.MarkAdded(ufq);
                                qcreated[new Tuple<Guid, Guid>(qid.Key.QuestionId, uft.SourceUserId)] = ufq;
                            }
                            ufq.Source = ufq.Source | FollowSource.TagFollow;
                        }
                    }
                    db.ObjectContext.ExecuteStoreCommand("update Questions set LinksCreated = 1 where Id = {0}", qid.Key.QuestionId);
                }
                
                Dictionary<Tuple<Guid, Guid>, UserFollowAnswer> acreated = new Dictionary<Tuple<Guid, Guid>, UserFollowAnswer>();
                var aids = db.AnswerTags.Where(q => !q.Answer.LinksCreated)
                    .Select(q => new { q.AnswerId, q.TagId, q.Answer.UserId })
                    .GroupBy(q => new { q.AnswerId, q.UserId })
                    .ToArray();
                foreach (var aid in db.Answers.Where(a => !a.LinksCreated)
                    .Select(a => new { a.Id, a.UserId }).ToArray())
                {
                    foreach (var uf in db.UserFollows.Where(u => u.TargetUserId == aid.UserId).ToArray())
                    {
                        var ufa = new UserFollowAnswer()
                        {
                            Source = FollowSource.UserFollow,
                            AnswerId = aid.Id,
                            SourceUserId = uf.SourceUserId
                        };
                        db.MarkAdded(ufa);
                        acreated.Add(new Tuple<Guid, Guid>(aid.Id, uf.SourceUserId), ufa);
                    }
                    db.ObjectContext.ExecuteStoreCommand("update Answers set LinksCreated = 1 where Id = {0}", aid.Id);
                }
                foreach (var aid in aids)
                {
                    foreach (var at in aid)
                    {
                        foreach (var uft in db.UserFollowTags.Where(u => u.TagId == at.TagId).ToArray())
                        {
                            UserFollowAnswer ufa;
                            if (!acreated.TryGetValue(new Tuple<Guid, Guid>(aid.Key.AnswerId, uft.SourceUserId), out ufa))
                            {
                                ufa = new UserFollowAnswer()
                                {
                                    SourceUserId = uft.SourceUserId,
                                    AnswerId = aid.Key.AnswerId
                                };
                                db.MarkAdded(ufa);
                                acreated[new Tuple<Guid, Guid>(aid.Key.AnswerId, uft.SourceUserId)] = ufa;
                            }
                            ufa.Source = ufa.Source | FollowSource.TagFollow;
                        }
                    }
                    db.ObjectContext.ExecuteStoreCommand("update Answers set LinksCreated = 1 where Id = {0}", aid.Key.AnswerId);
                }
                db.SaveChanges();
                scope.Complete();
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
