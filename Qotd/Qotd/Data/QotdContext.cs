using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Qotd.Entities;
using System.ComponentModel.DataAnnotations;
using Qotd.PresentationObjects;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Objects;
using System.Data.Entity.Infrastructure;
using System.Configuration;
using System.IO;
using Qotd.Utils;

namespace Qotd.Data
{
    public class QotdContext : DbContext, IDataProvider
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<QuestionContainer> QuestionContainers { get; set; }

        public DbSet<Answer> Answers { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<UserLikeComment> UserLikeComments { get; set; }

        public DbSet<UserVoteAnswer> UserVoteAnswers { get; set; }

        public DbSet<UserVoteQuestion> UserVoteQuestions { get; set; }

        public DbSet<UserQuestionSide> UserQuestionSides { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Activity> Activities { get; set; }

        public DbSet<UserActivityLink> UserActivityLinks { get; set; }

        public DbSet<ScoreEntry> ScoreEntries { get; set; }

        public DbSet<UserFollow> UserFollows { get; set; }

        public DbSet<UserFollowAnswer> UserFollowAnswers { get; set; }

        public DbSet<UserFollowQuestion> UserFollowQuestions { get; set; }

        public DbSet<UserFollowTag> UserFollowTags { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<SiteStatistics> SiteStatistics { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<AnswerTag> AnswerTags { get; set; }

        public DbSet<QuestionTag> QuestionTags { get; set; }

        public DbSet<ActivityTag> ActivityTags { get; set; }

        public ObjectContext ObjectContext
        {
            get
            {
                return ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        protected static Lucene.Net.Index.IndexWriter IndexWriter;

        static QotdContext()
        {
            InitialiseLucene();
        }

        private static void InitialiseLucene()
        {
            string tindex = ConfigurationManager.AppSettings["LucenePath"];
            if (File.Exists(Path.Combine(tindex, "write.lock")))
            {
                File.Delete(Path.Combine(tindex, "write.lock"));
            }
            Lucene.Net.Store.Directory dir;
            try
            {
                dir = Lucene.Net.Store.FSDirectory.GetDirectory(tindex);
            }
            catch (Exception)
            {
                dir = Lucene.Net.Store.FSDirectory.GetDirectory(tindex, true);
            }

            Lucene.Net.Analysis.Analyzer analyser = new Lucene.Net.Analysis.Standard.StandardAnalyzer();

            Lucene.Net.Index.IndexWriter indexWriter = new Lucene.Net.Index.IndexWriter(dir, analyser);

            IndexWriter = indexWriter;
        }

        #region IDataProvider Members

        public QotdContext()
        {
            
        }

        public void UpdateUserRankings()
        {
            string sql = @"with cte as (
 select {0}, rank() over (order by {2} desc, {3} desc, {4} desc, {5} desc, {6} desc, {7} desc) as row_rank
  from {1})
update cte
  set {0} = row_rank";
            ObjectContext.ExecuteStoreCommand(string.Format(sql, "OverallRank", "Users", "Score", "NumAnswers", "NumQuestions", "NumAnswersVoted", "NumQuestionsVoted", "NumComments"));
            ObjectContext.ExecuteStoreCommand(string.Format(sql, "OverallRankThisPeriod", "Users", "ScoreThisPeriod", "NumAnswersThisPeriod", "NumQuestionsThisPeriod", "NumAnswersVotedThisPeriod", "NumQuestionsVotedThisPeriod", "NumCommentsThisPeriod"));
        }

        public User GetUserByFacebookId(string facebookId)
        {
            return Users.SingleOrDefault(u => u.FacebookId == facebookId);
        }

        public User GetUserByEmail(string email)
        {
            return Users.SingleOrDefault(u => u.Email == email);
        }

        public UserPO GetUserByUsername(string username)
        {
            var user = Users.SingleOrDefault(u => u.Username == username);
            if (user == null) return null;
            return new UserPO()
            {
                User = user,
                UnreadNotifications = Notifications.Count(n => n.UserId == user.Id && (!n.IsRead))
            };
        }

        public TagPO GetTagById(Guid tagId, Guid? currentUserId = null)
        {
            var tag = GetTags(Tags.Where(t => t.Id == tagId)).SingleOrDefault();
            if (tag != null && currentUserId.HasValue)
            {
                tag.IsFollowedByCurrent = UserFollowTags.Any(u => u.SourceUserId == currentUserId.Value && u.TagId == tagId);
            }
            return tag;
        }

        private TagPO[] GetTags(IQueryable<Tag> tags)
        {
            return tags.ToArray().Select(t => new TagPO() { Tag = t }).ToArray();
        }

        public UserPO GetUserById(Guid userId, Guid? currentUserId = null)
        {
            var user = GetUsers(Users.Where(u => u.Id == userId)).SingleOrDefault();
            if (user != null && currentUserId.HasValue)
            {
                user.IsFollowedByCurrent = UserFollows.Any(u => u.SourceUserId == currentUserId.Value && u.TargetUserId == user.User.Id);
                user.IsFollowingCurrent = UserFollows.Any(u => u.TargetUserId == currentUserId.Value && u.SourceUserId == user.User.Id);
            }
            return user;
        }

        private UserPO[] GetUsers(IQueryable<User> users)
        {
            return users.ToArray().Select(u => new UserPO() { User = u }).ToArray();
        }

        public Question GetTodaysQuestion()
        {
            return QuestionContainers.Select(q => q.TodaysQuestion).SingleOrDefault();
        }

        public bool HasUserPickedSide(Guid userId, Guid questionId)
        {
            return UserQuestionSides.Any(u => u.UserId == userId && u.QuestionId == questionId);
        }

        public QuestionPO[] GetQuestionsLatest(int skip, int take, out int count)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            var query = Questions.Where(q => q.DateFor == date);
            count = query.Count();
            return GetQuestions(query
                .OrderByDescending(q => q.CreatedOn)
                .Skip(skip).Take(take), null);
        }

        public QuestionPO[] GetQuestionsRated(int skip, int take, out int count)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            var query = Questions.Where(q => q.DateFor == date);
            count = query.Count();
            return GetQuestions(query
                .OrderByDescending(q => q.VotesTotal).ThenBy(q => q.denorm_User_OverallRankThisPeriod)
                .Skip(skip).Take(take), null);
        }

        public QuestionPO[] GetQuestionsLatest(Guid userId, int skip, int take, out int count)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            var query = Questions.Where(q => q.DateFor == date);
            count = query.Count();
            return GetQuestions(query
                .OrderByDescending(q => q.CreatedOn)
                .Skip(skip).Take(take), userId);
        }

        public QuestionPO[] GetQuestionsRated(Guid userId, int skip, int take, out int count)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            var query = Questions.Where(q => q.DateFor == date);
            count = query.Count();
            return GetQuestions(query
                .OrderByDescending(q => q.VotesTotal).ThenBy(q => q.denorm_User_OverallRankThisPeriod)
                .Skip(skip).Take(take), userId);
        }

        private QuestionPO[] GetQuestions(IQueryable<Question> questions, Guid? userId)
        {
            var qs = questions.ToArray();

            var qids = qs.Select(q => q.Id).ToArray();

            var uvqs = userId.HasValue ? UserVoteQuestions.Where(u => u.UserId == userId && qids.Contains(u.QuestionId)).ToArray()
                .ToDictionary(q => q.QuestionId, q => q) : null;

            var pqs = new QuestionPO[qs.Length];
            for (int i = 0; i < qs.Length; i++)
            {
                var q = qs[i];
                pqs[i] = new QuestionPO()
                {
                    Question = q,
                    HasUserVoted = uvqs != null ? (userId == q.UserId ? true : uvqs.ContainsKey(q.Id)) : true,
                    User = new UserPO()
                    {
                        User = new User()
                        {
                            Id = q.UserId,
                            ProfileImageUrl = q.denorm_User_ProfileImageUrl,
                            DisplayName = q.denorm_User_DisplayName
                        }
                    }
                };
            }
            return pqs;
        }

        private AnswerPO[] GetAnswers(IQueryable<Answer> answers, Guid? userId)
        {
            var ans = answers.ToArray();

            var anids = ans.Select(a => a.Id).ToArray();

            var uvas = userId.HasValue ? UserVoteAnswers.Where(u => u.UserId == userId && anids.Contains(u.AnswerId)).ToArray()
                .ToDictionary(a => a.AnswerId, a => a) : null;

            var cids = new List<Guid>();
            foreach (var a in ans)
            {
                if (a.Comment1Id.HasValue) cids.Add(a.Comment1Id.Value);
                if (a.Comment2Id.HasValue) cids.Add(a.Comment2Id.Value);
            }
            var cmts = GetComments(Comments.Where(c => cids.Contains(c.Id)), userId)
                .GroupBy(c => c.Comment.AnswerId).ToDictionary(a => a.Key, a => a.OrderBy(c => c.Comment.CreatedOn).ToArray());

            var pans = new AnswerPO[ans.Length];
            for (int i = 0; i < ans.Length; i++)
            {
                var a = ans[i];
                pans[i] = new AnswerPO()
                {
                    Answer = a,
                    HasUserVoted = uvas != null ? (userId == a.UserId ? true : uvas.ContainsKey(a.Id)) : true,
                    Comments = (cmts.ContainsKey(a.Id) ? cmts[a.Id] : null),
                        // denormalised
                    User = new UserPO()
                    {
                        User = new User()
                        {
                            Id = a.UserId,
                            DisplayName = a.denorm_User_DisplayName,
                            ProfileImageUrl = a.denorm_User_ProfileImageUrl,
                            OverallRank = a.denorm_User_OverallRank,
                            OverallRankThisPeriod = a.denorm_User_OverallRankThisPeriod
                        }
                    }
                };
            }
            return pans;
        }

        public TagPO[] GetTagsFollowed(Guid userId)
        {
            return UserFollowTags.Where(u => u.SourceUserId == userId)
                .Select(u => u.Tag)
                .ToArray()
                .Select(u => new TagPO()
                {
                    Tag = u
                }).ToArray();
        }

        public UserPO[] GetUsersFollowed(Guid userId)
        {
            return UserFollows.Where(u => u.SourceUserId == userId)
                .Select(u => u.TargetUser)
                .ToArray()
                .Select(u => new UserPO()
                {
                    User = u
                }).ToArray();
        }

        public QuestionPO[] GetQuestionsFollowed(Guid userId, FollowSource source, int skip, int take, out int count)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            var query = UserFollowQuestions.Where(u => u.SourceUserId == userId && u.Question.DateFor == date 
                && (u.FollowSourceValue & (byte)source) > 0)
                .Select(u => u.Question);
            count = query.Count();
            return GetQuestions(query
                .OrderByDescending(q => q.VotesTotal).ThenBy(q => q.denorm_User_OverallRankThisPeriod)
                .Skip(skip).Take(take), userId);
        }

        public AnswerPO[] GetAnswersFollowed(Guid userId, Guid questionId, FollowSource source, int skip, int take, out int count)
        {
            var query = UserFollowAnswers
                .Where(u => u.SourceUserId == userId && u.Answer.QuestionId == questionId && (u.FollowSourceValue & (byte)source) > 0)
                .Select(u => u.Answer);
            count = query.Count();
            return GetAnswers(query
                .OrderByDescending(a => a.VotesTotal)
                .ThenBy(a => a.denorm_User_OverallRankThisPeriod)
                .Skip(skip)
                .Take(take), userId);
        }

        public AnswerPO[] GetAnswersLatest(Guid questionId, int skip, int take, out int count)
        {
            var query = Answers.Where(a => a.QuestionId == questionId);
            count = query.Count();
            return GetAnswers(query
                .OrderByDescending(a => a.CreatedOn).Skip(skip).Take(take), null);
        }

        public AnswerPO[] GetAnswersLatest(Guid userId, Guid questionId, int skip, int take, out int count)
        {
            var query = Answers.Where(a => a.QuestionId == questionId);
            count = query.Count();
            return GetAnswers(query
                .OrderByDescending(a => a.CreatedOn).Skip(skip).Take(take), userId);
        }

        public AnswerPO[] GetAnswersRated(Guid questionId, int skip, int take, out int count)
        {
            var query = Answers.Where(a => a.QuestionId == questionId);
            count = query.Count();
            return GetAnswers(query
                .OrderByDescending(a => a.VotesTotal).ThenBy(a => a.denorm_User_OverallRankThisPeriod).Skip(skip).Take(take), null);
        }

        public AnswerPO[] GetAnswersRated(Guid userId, Guid questionId, int skip, int take, out int count)
        {
            var query = Answers.Where(a => a.QuestionId == questionId);
            count = query.Count();
            return GetAnswers(query
                .OrderByDescending(a => a.VotesTotal).ThenBy(a => a.denorm_User_OverallRankThisPeriod).Skip(skip).Take(take), userId);
        }

        public Answer GetAnswerById(Guid answerId)
        {
            return Answers.SingleOrDefault(a => a.Id == answerId);
        }

        public AnswerPO GetAnswerById(Guid answerId, Guid userId)
        {
            return GetAnswers(Answers.Where(a => a.Id == answerId), userId).SingleOrDefault();
        }

        public QuestionPO GetQuestionById(Guid questionId, Guid userId)
        {
            return GetQuestions(Questions.Where(q => q.Id == questionId), userId).SingleOrDefault();
        }

        public CommentPO GetCommentById(Guid commentId, Guid userId)
        {
            return GetComments(Comments.Where(c => c.Id == commentId), userId).SingleOrDefault();
        }

        private ActivityPO[] GetHistory(IQueryable<Activity> activities)
        {
            var act = activities.ToArray();

            var qids = act.Where(a => a.QuestionId.HasValue)
                .Select(a => a.QuestionId.Value).ToArray();

            var aids = act.Where(a => a.AnswerId.HasValue)
                .Select(a => a.AnswerId.Value).ToArray();

            var qs = GetQuestions(Questions.Where(q => qids.Contains(q.Id)), null)
                .ToDictionary(q => q.Question.Id, q => q);

            var ans = GetAnswers(Answers.Where(a => aids.Contains(a.Id)), null)
                .ToDictionary(a => a.Answer.Id, a => a);

            var apos = new ActivityPO[act.Length];

            for (int i = 0; i < apos.Length; i++)
            {
                var a = act[i];
                apos[i] = new ActivityPO()
                {
                    Activity = a,
                    Answer = a.AnswerId.HasValue ? ans[a.AnswerId.Value] : null,
                    Question = a.QuestionId.HasValue ? qs[a.QuestionId.Value] : null
                };
            }
            return apos;
        }

        public Notification[] GetNotifications(Guid userId, int skip, int take, out int count)
        {
            var query = Notifications.Where(n => n.UserId == userId);
            count = query.Count();
            return query
                .OrderByDescending(n => n.Date)
                .Skip(skip)
                .Take(take)
                .ToArray();
        }

        public Notification[] ReadNotifications(Guid userId)
        {
            var nots = Notifications.Where(n => (!n.IsRead) && n.UserId == userId)
                .OrderByDescending(n => n.Date)
                .ToArray();

            foreach (var n in nots)
                n.IsRead = true;
            SaveChanges();

            int max = 5;
            if (nots.Length < max)
            {
                nots = nots.Concat(
                    Notifications.Where(n => n.IsRead && n.UserId == userId)
                    .OrderByDescending(n => n.Date)
                    .Take(max - nots.Length)
                    .ToArray()).ToArray();
            }

            return nots;
        }

        public ActivityPO[] GetHistory(DateTime dateFrom, DateTime dateTo)
        {
            var history = GetHistory(
                Activities.Where(a => (a.ActivityTypeValue == (byte)ActivityType.QuestionWin ||
                                       a.ActivityTypeValue == (byte)ActivityType.AnswerWin ||
                                       a.ActivityTypeValue == (byte)ActivityType.AnswerSecond ||
                                       a.ActivityTypeValue == (byte)ActivityType.AnswerThird) &&
                                       a.Date >= dateFrom && a.Date < dateTo)
               .OrderByDescending(a => a.Date));
            for (int i = 0; i < history.Length; i++)
            {
                if (history[i].Activity.AnswerId.HasValue) history[i].Activity.ActivityType = ActivityType.PostAnswer;
                if (history[i].Activity.QuestionId.HasValue) history[i].Activity.ActivityType = ActivityType.PostQuestion;
            }
            return history;
        }

        public ActivityPO[] GetTodaysActivitiesForUser(Guid userId, DateTime? date, int take)
        {
            if (date == null)
                date = DateTime.Now;
            DateTime startDate = DateTime.Now.Date;
            var query = Activities.Where(a => a.Date <= date && a.Date >= startDate && a.SourceUserId == userId);
            return GetHistory(
                query
                .OrderByDescending(a => a.Date)
                .Take(take));
        }

        public ActivityPO[] GetHistoryForUser(Guid userId, int skip, int take, out int count)
        {
            var query = Activities.Where(a => (a.ActivityTypeValue == (byte)ActivityType.PostAnswer ||
                                      a.ActivityTypeValue == (byte)ActivityType.PostQuestion) &&
                                      a.SourceUserId == userId);
            count = query.Count();
            return GetHistory(
                query
                .OrderByDescending(a => a.Date)
                .Skip(skip)
                .Take(take));

        }

        public ActivityPO[] GetHistoryForTag(Guid tagId, int skip, int take, out int count)
        {
            var query = ActivityTags.Where(t => t.TagId == tagId)
                .Select(t => t.Activity)
                .Where(a => (a.ActivityTypeValue == (byte)ActivityType.PostAnswer ||
                                      a.ActivityTypeValue == (byte)ActivityType.PostQuestion));
            count = query.Count();
            return GetHistory(
                query
                .OrderByDescending(a => a.Date)
                .Skip(skip)
                .Take(take));
        }

        private CommentPO[] GetComments(IQueryable<Comment> comments, Guid? userId)
        {
            var cmts = comments.ToArray();

            var cids = cmts.Select(c => c.Id).ToArray();

            var ulcs = userId.HasValue ? new HashSet<Guid>(UserLikeComments.Where(c => cids.Contains(c.CommentId) && c.UserId == userId)
                .Select(c => c.CommentId)
                .ToArray()) : null;

            var toReturn = new CommentPO[cmts.Length];
            for (int i = 0; i < toReturn.Length; i++)
            {
                var c = cmts[i];
                toReturn[i] = new CommentPO()
                {
                    Comment = c,
                    HasUserLiked = userId.HasValue ? ulcs.Contains(c.Id) : false,
                    User = new UserPO()
                    {
                        User = new User()
                        {
                            DisplayName = c.denorm_User_DisplayName,
                            ProfileImageUrl = c.denorm_User_ProfileImageUrl,
                            Id = c.UserId
                        }
                    }
                };
            }
            return toReturn;
        }

        public CommentPO[] GetComments(Guid answerId, Guid userId)
        {
            return GetComments(Comments.Where(c => c.AnswerId == answerId).OrderBy(c => c.CreatedOn), userId);
        }

        public void LikeComment(Guid commentId, Guid userId)
        {
            if (!UserLikeComments.Any(u => u.UserId == userId && u.CommentId == commentId))
            {
                UserLikeComment ulc = new UserLikeComment()
                {
                    UserId = userId,
                    CommentId = commentId
                };
                UserLikeComments.Add(ulc);
                Comment cmt = Comments.SingleOrDefault(c => c.Id == commentId);
                cmt.NumLikes++;
                MarkAddedOrUpdated(cmt);
                SaveChanges();
            }
        }

        public LeaderboardPO GetLeaderboardThisPeriod(Guid userId, int skip, int take, out int count)
        {
            count = 0;
            var us = Users.Where(u => u.OverallRankThisPeriod > 0).OrderBy(u => u.OverallRankThisPeriod).Skip(skip).Take(take).ToArray();

            LeaderboardRowPO[] top = us.Select(UsertoLeaderboardThisPeriodFunc).ToArray();
            LeaderboardRowPO[] aroundUser = null;
            if (!us.Any(u => u.Id == userId))
            {
                int ur = Users.Where(u => u.Id == userId).Select(u => u.OverallRankThisPeriod).Single();

                aroundUser = Users.Where(u => u.OverallRankThisPeriod > (ur - 3) && u.OverallRankThisPeriod < (ur + 3))
                    .ToArray()
                    .Select(UsertoLeaderboardThisPeriodFunc).ToArray();
            }
            return new LeaderboardPO() { Top = top, AroundUser = aroundUser };
        }

        private Func<User, LeaderboardRowPO> UserToLeaderboardFunc =
            u => new LeaderboardRowPO()
            {
                User = u,
                Rank = u.OverallRank,
                Score = u.Score,
                A1 = u.NumAnswersWon,
                A2 = u.NumAnswersSecond,
                A3 = u.NumAnswersThird,
                Ac = u.NumAnswers,
                Qc = u.NumQuestions,
                Q1 = u.NumQuestionsWon,
                Qv = u.TotalQuestionVotes,
                Av = u.TotalAnswerVotes
            };
        private Func<User, LeaderboardRowPO> UsertoLeaderboardThisPeriodFunc =
            u => new LeaderboardRowPO()
            {
                User = u,
                Rank = u.OverallRankThisPeriod,
                Score = u.ScoreThisPeriod,
                A1 = u.NumAnswersWonThisPeriod,
                A2 = u.NumAnswersSecondThisPeriod,
                A3 = u.NumAnswersThirdThisPeriod,
                Ac = u.NumAnswersThisPeriod,
                Qc = u.NumQuestionsThisPeriod,
                Q1 = u.NumQuestionsWonThisPeriod,
                Qv = u.TotalQuestionVotesThisPeriod,
                Av = u.TotalAnswerVotesThisPeriod
            };

        public LeaderboardPO GetLeaderboard(Guid userId, int skip, int take, out int count)
        {
            count = 0;
            var us = Users.Where(u => u.OverallRank > 0).OrderBy(u => u.OverallRank).Skip(skip).Take(take).ToArray();

            LeaderboardRowPO[] top = us.Select(UserToLeaderboardFunc).ToArray();
            LeaderboardRowPO[] aroundUser = null;
            if (!us.Any(u => u.Id == userId))
            {
                int ur = Users.Where(u => u.Id == userId).Select(u => u.OverallRank).Single();

                aroundUser = Users.Where(u => u.OverallRank > (ur - 3) && u.OverallRank < (ur + 3))
                    .ToArray()
                    .Select(UserToLeaderboardFunc).ToArray();
            }
            return new LeaderboardPO() { Top = top, AroundUser = aroundUser };
        }

        public LeaderboardPO GetLeaderboard(int skip, int take, out int count)
        {
            count = 0;
            var us = Users.Where(u => u.OverallRank > 0).OrderBy(u => u.OverallRank).Skip(skip).Take(take).ToArray();

            return new LeaderboardPO() { Top = us.Select(UserToLeaderboardFunc).ToArray() };
        }

        public LeaderboardPO GetLeaderboardThisPeriod(int skip, int take, out int count)
        {
            count = 0;
            var us = Users.Where(u => u.OverallRankThisPeriod > 0).OrderBy(u => u.OverallRankThisPeriod).Skip(skip).Take(take).ToArray();

            return new LeaderboardPO()
            {
                Top = us.Select(UsertoLeaderboardThisPeriodFunc).ToArray()
            };
        }


        public void VoteAnswer(Guid answerId, User user, int voteDelta)
        {
            // TODO - create activity
            if (!UserVoteAnswers.Any(u => u.AnswerId == answerId && u.UserId == user.Id))
            {
                UserVoteAnswer uva = new UserVoteAnswer()
                {
                    UserId = user.Id,
                    VoteDelta = voteDelta,
                    AnswerId = answerId
                };
                UserVoteAnswers.Add(uva);
                Answer ans = Answers.Single(a => a.Id == answerId);
                ans.VotesTotal += voteDelta;
                if (voteDelta < 0) ans.VotesDown -= voteDelta;
                else ans.VotesUp += voteDelta;
                user.AddAction(ActivityType.VoteAnswer);
                User auser = ans.User;
                if (voteDelta == 1)
                {
                    auser.AddAction(ActivityType.ReceiveVoteUpAnswer);
                }
                else if (voteDelta == -1)
                {
                    auser.AddAction(ActivityType.ReceiveVoteDownAnswer);
                }
                else throw new System.NotImplementedException();

                // create activity
                Activity activity = new Activity()
                {
                    ActivityType = ActivityType.VoteAnswer,
                    Answer = ans,
                    SourceUserId = user.Id,
                    denorm_SourceUser_DisplayName = user.DisplayName,
                    denorm_SourceUser_ProfileImageUrl = user.ProfileImageUrl,
                    Date = DateTime.Now,
                    LinksCreated = false,
                    VisibleWithoutLink = true,
                    NotificationsCreated = false,
                    TargetUserId = ans.UserId,
                    Text = ans.Title
                };

                MarkAddedOrUpdated(activity);
                MarkAddedOrUpdated(ans);
                MarkAddedOrUpdated(user);
                MarkAddedOrUpdated(auser);
                SaveChanges();
            }
        }

        public void VoteQuestion(Guid questionId, User user, int voteDelta)
        {
            // TODO - create activity
            if (!UserVoteQuestions.Any(u => u.QuestionId == questionId && u.UserId == user.Id))
            {
                UserVoteQuestion uva = new UserVoteQuestion()
                {
                    UserId = user.Id,
                    VoteDelta = voteDelta,
                    QuestionId = questionId
                };
                UserVoteQuestions.Add(uva);
                Question ans = Questions.Single(a => a.Id == questionId);
                ans.VotesTotal += voteDelta;
                if (voteDelta < 0) ans.VotesDown -= voteDelta;
                else ans.VotesUp += voteDelta;
                user.AddAction(ActivityType.VoteQuestion);
                User quser = ans.User;
                if (voteDelta == 1)
                {
                    quser.AddAction(ActivityType.ReceiveVoteUpQuestion);
                }
                else if (voteDelta == -1)
                {
                    quser.AddAction(ActivityType.ReceiveVoteDownQuestion);
                }
                else throw new System.NotImplementedException();

                // create activity
                Activity activity = new Activity()
                {
                    ActivityType = ActivityType.VoteQuestion,
                    Question = ans,
                    SourceUserId = user.Id,
                    denorm_SourceUser_DisplayName = user.DisplayName,
                    denorm_SourceUser_ProfileImageUrl = user.ProfileImageUrl,
                    Date = DateTime.Now,
                    LinksCreated = false,
                    VisibleWithoutLink = true,
                    NotificationsCreated = false,
                    TargetUserId = ans.UserId,
                    Text = ans.MainText
                };

                MarkAddedOrUpdated(activity);

                MarkAddedOrUpdated(ans);
                MarkAddedOrUpdated(user);
                MarkAddedOrUpdated(quser);
                SaveChanges();
            }
        }

        public ActivityPO[] GetActivities(DateTime? date, int take)
        {
            var activities = (IQueryable<Activity>)Activities;
            if (date.HasValue)
                activities = activities.Where(a => a.Date < date.Value);

            return activities.OrderByDescending(a => a.Date).Take(take).ToArray()
                .Select(a => new ActivityPO()
                {
                    Activity = a,
                    UserDisplayName = a.denorm_SourceUser_DisplayName,
                    UserProfileImageUrl = a.denorm_SourceUser_ProfileImageUrl
                }).OrderByDescending(a => a.Activity.Date).ToArray();
        }

        public SearchResultPO[] Search(string search, int skip, int take)
        {
            throw new System.NotImplementedException();
        }

        public Tag[] GetTags()
        {
            return Tags.ToArray();
        }

        public Tag[] GetTags(string startsWith)
        {
            return Tags.Where(t => t.Value.StartsWith(startsWith)).ToArray();
        }

        #endregion

        #region IUnitOfWork Members

        private enum AddedOrUpdated
        {
            Added, Updated
        }

        private static void AddSearchItem(Guid id, SearchItemType type, AddedOrUpdated state, params string[] text)
        {
            string value = String.Join(" ", text);
            if (state == AddedOrUpdated.Updated)
            {
                IndexWriter.DeleteDocuments(new Lucene.Net.Index.Term("Id", id.ToString()));
            }

            Lucene.Net.Documents.Field idField = new Lucene.Net.Documents.Field("Id", id.ToString(),
                Lucene.Net.Documents.Field.Store.YES,
                Lucene.Net.Documents.Field.Index.NOT_ANALYZED,
                Lucene.Net.Documents.Field.TermVector.YES);
            Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();
            doc.Add(idField);
            Lucene.Net.Documents.Field valueField = new Lucene.Net.Documents.Field("Value", value,
                Lucene.Net.Documents.Field.Store.YES,
                Lucene.Net.Documents.Field.Index.ANALYZED,
                Lucene.Net.Documents.Field.TermVector.YES);
            doc.Add(valueField);
            Lucene.Net.Documents.Field typeField = new Lucene.Net.Documents.Field("Type", type.ToString(),
                Lucene.Net.Documents.Field.Store.YES,
                Lucene.Net.Documents.Field.Index.NOT_ANALYZED,
                Lucene.Net.Documents.Field.TermVector.NO);
            doc.Add(typeField);
        }

        private static readonly Dictionary<Type, Action<object, AddedOrUpdated>> OnAddedOrUpdated
            = new Dictionary<Type, Action<object, AddedOrUpdated>>()
            {
                { typeof(Activity), (a, u) => 
                    {
                        Activity act = (Activity)a;
                        if (act.CommentId.HasValue)
                            act.RelatedObjectId = act.CommentId;
                        else if (act.AnswerId.HasValue)
                            act.RelatedObjectId = act.AnswerId;
                        else if (act.QuestionId.HasValue)
                            act.RelatedObjectId = act.QuestionId;
                    } 
                },
                { typeof(User), (o, s) =>
                    {
                        User user = (User)o;
                        AddSearchItem(user.Id, SearchItemType.User, s, user.DisplayName, user.Email);
                    }
                    },
                { typeof (Answer), (o, s) =>
                    {
                        Answer answer = (Answer)o;
                        AddSearchItem(answer.Id, SearchItemType.AnswerContent, s, answer.Title, answer.Content);
                        AddSearchItem(answer.Id, SearchItemType.AnswerTags, s, answer.TagEntries.GetTagString()); 
                    }
                    },
                { typeof(Question), (o, s) =>
                    {
                        Question question = (Question)o;
                        AddSearchItem(question.Id, SearchItemType.QuestionContent, s, question.MainText, question.SubText, question.Details);
                        AddSearchItem(question.Id, SearchItemType.QuestionTags, s, question.TagEntries.GetTagString());
                    }
                    }
            };

        public void MarkAddedOrUpdated<T>(T obj) where T : class, IEntity
        {
            if (obj.Id == default(Guid))
                Set<T>().Add(obj);
            if (OnAddedOrUpdated.ContainsKey(typeof(T)))
                OnAddedOrUpdated[typeof(T)](obj, obj.Id == default(Guid) ? AddedOrUpdated.Added : AddedOrUpdated.Updated);
        }

        public void MarkAdded<T>(T obj) where T : class
        {
            Set<T>().Add(obj);
            if (OnAddedOrUpdated.ContainsKey(typeof(T)))
                OnAddedOrUpdated[typeof(T)](obj, AddedOrUpdated.Added);
        }

        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Question>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Answer>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Comment>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Activity>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Notification>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<ScoreEntry>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<SiteStatistics>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Admin>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Tag>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<User>().Ignore(u => u.ActionEntriesThisPeriod);
            modelBuilder.Entity<Tag>().Ignore(u => u.ActionEntriesThisPeriod);

            modelBuilder.Entity<UserLikeComment>().HasKey(t => new { t.UserId, t.CommentId });
            modelBuilder.Entity<UserVoteAnswer>().HasKey(t => new { t.UserId, t.AnswerId });
            modelBuilder.Entity<UserVoteQuestion>().HasKey(t => new { t.UserId, t.QuestionId });
            modelBuilder.Entity<UserQuestionSide>().HasKey(t => new { t.UserId, t.QuestionId });
            modelBuilder.Entity<UserActivityLink>().HasKey(t => new { t.UserId, t.ActivityId });
            modelBuilder.Entity<UserFollow>().HasKey(t => new { t.SourceUserId, t.TargetUserId });
            modelBuilder.Entity<UserFollowAnswer>().HasKey(t => new { t.SourceUserId, t.AnswerId });
            modelBuilder.Entity<UserFollowQuestion>().HasKey(t => new { t.SourceUserId, t.QuestionId });
            modelBuilder.Entity<UserFollowTag>().HasKey(t => new { t.SourceUserId, t.TagId });
            modelBuilder.Entity<AnswerTag>().HasKey(t => new { t.AnswerId, t.TagId });
            modelBuilder.Entity<QuestionTag>().HasKey(t => new { t.QuestionId, t.TagId });
            modelBuilder.Entity<ActivityTag>().HasKey(t => new { t.ActivityId, t.TagId });

            modelBuilder.Entity<Comment>().HasRequired(t => t.Answer)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserVoteAnswer>().HasRequired(t => t.Answer)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserVoteQuestion>().HasRequired(t => t.User)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserActivityLink>().HasRequired(t => t.Activity)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<Activity>().HasOptional(t => t.Comment)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<Activity>().HasOptional(t => t.Answer)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<Activity>().HasOptional(t => t.Question)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserActivityLink>().HasRequired(t => t.User)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<Answer>().HasRequired(t => t.User)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserLikeComment>().HasRequired(t => t.Comment)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserQuestionSide>().HasRequired(t => t.Question)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserFollow>().HasRequired(t => t.SourceUser)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserFollow>().HasRequired(t => t.TargetUser)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserFollowAnswer>().HasRequired(t => t.SourceUser)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserFollowAnswer>().HasRequired(t => t.Answer)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserFollowQuestion>().HasRequired(t => t.SourceUser)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserFollowQuestion>().HasRequired(t => t.Question)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<UserFollowTag>().HasRequired(t => t.SourceUser)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<AnswerTag>().HasRequired(t => t.Answer)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<AnswerTag>().HasRequired(t => t.Tag)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<QuestionTag>().HasRequired(t => t.Question)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<QuestionTag>().HasRequired(t => t.Tag)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<ActivityTag>().HasRequired(t => t.Activity)
                .WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<ActivityTag>().HasRequired(t => t.Tag)
                .WithMany().WillCascadeOnDelete(false);
        }
    }
}
