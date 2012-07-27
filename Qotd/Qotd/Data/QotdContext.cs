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

        public ObjectContext ObjectContext
        {
            get
            {
                return ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        #region IDataProvider Members

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

        public User GetUserByUsername(string username)
        {
            return Users.SingleOrDefault(u => u.Username == username);
        }

        public Question GetTodaysQuestion()
        {
            return QuestionContainers.Select(q => q.TodaysQuestion).SingleOrDefault();
        }

        public bool HasUserPickedSide(Guid userId, Guid questionId)
        {
            return UserQuestionSides.Any(u => u.UserId == userId && u.QuestionId == questionId);
        }

        public QuestionPO[] GetQuestionsLatest(int skip, int take)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            return GetQuestions(Questions.Where(q => q.DateFor == date)
                .OrderByDescending(q => q.CreatedOn)
                .Skip(skip).Take(take), null);
        }

        public QuestionPO[] GetQuestionsRated(int skip, int take)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            return GetQuestions(Questions.Where(q => q.DateFor == date)
                .OrderByDescending(q => q.VotesTotal).ThenBy(q => q.denorm_User_OverallRankThisPeriod)
                .Skip(skip).Take(take), null);
        }

        public QuestionPO[] GetQuestionsLatest(Guid userId, int skip, int take)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            return GetQuestions(Questions.Where(q => q.DateFor == date)
                .OrderByDescending(q => q.CreatedOn)
                .Skip(skip).Take(take), userId);
        }

        public QuestionPO[] GetQuestionsRated(Guid userId, int skip, int take)
        {
            DateTime date = DateTime.Now.AddDays(1).Date;
            return GetQuestions(Questions.Where(q => q.DateFor == date)
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
                pqs[i] = new QuestionPO()
                {
                    Question = qs[i],
                    HasUserVoted = uvqs != null ? uvqs.ContainsKey(qs[i].Id) : false,
                    UserDisplayName = qs[i].denorm_User_DisplayName,
                    UserProfileImageUrl = qs[i].denorm_User_ProfileImageUrl
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
                pans[i] = new AnswerPO()
                {
                    Answer = ans[i],
                    HasUserVoted = uvas != null ? uvas.ContainsKey(ans[i].Id) : false,
                    Comments = (cmts.ContainsKey(ans[i].Id) ? cmts[ans[i].Id] : null),
                        // denormalised
                    UserDisplayName = ans[i].denorm_User_DisplayName,
                    UserProfileImageUrl = ans[i].denorm_User_ProfileImageUrl
                };
            }
            return pans;
        }

        public AnswerPO[] GetAnswersLatest(Guid questionId, int skip, int take)
        {
            return GetAnswers(Answers.Where(a => a.QuestionId == questionId)
                .OrderByDescending(a => a.CreatedOn).Skip(skip).Take(take), null);
        }

        public AnswerPO[] GetAnswersLatest(Guid userId, Guid questionId, int skip, int take)
        {
            return GetAnswers(Answers.Where(a => a.QuestionId == questionId)
                .OrderByDescending(a => a.CreatedOn).Skip(skip).Take(take), userId);
        }

        public AnswerPO[] GetAnswersRated(Guid questionId, int skip, int take)
        {
            return GetAnswers(Answers.Where(a => a.QuestionId == questionId)
                .OrderByDescending(a => a.VotesTotal).ThenBy(a => a.denorm_User_OverallRankThisPeriod).Skip(skip).Take(take), null);
        }

        public AnswerPO[] GetAnswersRated(Guid userId, Guid questionId, int skip, int take)
        {
            return GetAnswers(Answers.Where(a => a.QuestionId == questionId)
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

        public CommentPO GetCommentById(Guid commentId, Guid userId)
        {
            return GetComments(Comments.Where(c => c.Id == commentId), userId).SingleOrDefault();
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
                toReturn[i] = new CommentPO()
                {
                    Comment = cmts[i],
                    HasUserLiked = userId.HasValue ? ulcs.Contains(cmts[i].Id) : false,
                    UserDisplayName = cmts[i].denorm_User_DisplayName,
                    UserProfileImageUrl = cmts[i].denorm_User_ProfileImageUrl
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

        public LeaderboardPO GetLeaderboardThisPeriod(Guid userId, int skip, int take)
        {
            var us = Users.OrderBy(u => u.OverallRankThisPeriod).Skip(skip).Take(take).ToArray();

            LeaderboardRowPO[] top = us.Select(u => new LeaderboardRowPO() { User = u }).ToArray();
            LeaderboardRowPO[] aroundUser = null;
            if (!us.Any(u => u.Id == userId))
            {
                int ur = Users.Where(u => u.Id == userId).Select(u => u.OverallRankThisPeriod).Single();

                aroundUser = Users.Where(u => u.OverallRankThisPeriod > (ur - 3) && u.OverallRankThisPeriod < (ur + 3))
                    .ToArray()
                    .Select(u => new LeaderboardRowPO() { User = u, Rank = u.OverallRankThisPeriod, Score = u.ScoreThisPeriod,
                                                          A1 = u.NumAnswersWonThisPeriod,
                                                          A2 = u.NumAnswersSecondThisPeriod,
                                                          A3 = u.NumAnswersThirdThisPeriod,
                                                          Ac = u.NumAnswersThisPeriod,
                                                          Qc = u.NumQuestionsThisPeriod,
                                                          Q1 = u.NumQuestionsWonThisPeriod,
                                                          Qv = u.TotalQuestionVotesThisPeriod,
                                                          Av = u.TotalAnswerVotesThisPeriod
                    }).ToArray();
            }
            return new LeaderboardPO() { Top = top, AroundUser = aroundUser };
        }

        public LeaderboardPO GetLeaderboard(Guid userId, int skip, int take)
        {
            var us = Users.OrderBy(u => u.OverallRank).Skip(skip).Take(take).ToArray();

            LeaderboardRowPO[] top = us.Select(u => new LeaderboardRowPO() { User = u }).ToArray();
            LeaderboardRowPO[] aroundUser = null;
            if (!us.Any(u => u.Id == userId))
            {
                int ur = Users.Where(u => u.Id == userId).Select(u => u.OverallRank).Single();

                aroundUser = Users.Where(u => u.OverallRank > (ur - 3) && u.OverallRank < (ur + 3))
                    .ToArray()
                    .Select(u => new LeaderboardRowPO() { User = u, Rank = u.OverallRank, Score = u.Score,
                                                          A1 = u.NumAnswersWon,
                                                          A2 = u.NumAnswersSecond,
                                                          A3 = u.NumAnswersThird,
                                                          Ac = u.NumAnswers,
                                                          Qc = u.NumQuestions,
                                                          Q1 = u.NumQuestionsWon,
                                                          Qv = u.TotalQuestionVotes,
                                                          Av = u.TotalAnswerVotes
                    }).ToArray();
            }
            return new LeaderboardPO() { Top = top, AroundUser = aroundUser };
        }

        public LeaderboardPO GetLeaderboard(int skip, int take)
        {
            var us = Users.OrderBy(u => u.OverallRank).Skip(skip).Take(take).ToArray();

            return new LeaderboardPO() { Top = us.Select(u => new LeaderboardRowPO() { User = u, Rank = u.OverallRank, Score = u.Score,
                A1 = u.NumAnswersWon,
                A2 = u.NumAnswersSecond,
                A3 = u.NumAnswersThird,
                Ac = u.NumAnswers,
                Qc = u.NumQuestions,
                Q1 = u.NumQuestionsWon,
                Qv = u.TotalQuestionVotes,
                Av = u.TotalAnswerVotes
            }).ToArray() };
        }

        public LeaderboardPO GetLeaderboardThisPeriod(int skip, int take)
        {
            var us = Users.OrderBy(u => u.OverallRankThisPeriod).Skip(skip).Take(take).ToArray();

            return new LeaderboardPO() { Top = us.Select(u => new LeaderboardRowPO() { User = u, Rank = u.OverallRankThisPeriod, Score = u.ScoreThisPeriod,
                                                                                       A1 = u.NumAnswersWonThisPeriod,
                                                                                       A2 = u.NumAnswersSecondThisPeriod,
                                                                                       A3 = u.NumAnswersThirdThisPeriod,
                                                                                       Ac = u.NumAnswersThisPeriod,
                                                                                       Qc = u.NumQuestionsThisPeriod,
                                                                                       Q1 = u.NumQuestionsWonThisPeriod,
                                                                                       Qv = u.TotalQuestionVotesThisPeriod,
                                                                                       Av = u.TotalAnswerVotesThisPeriod
            }).ToArray()
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
                }).ToArray();
        }

        #endregion

        #region IUnitOfWork Members

        public void MarkAddedOrUpdated<T>(T obj) where T : class, IEntity
        {
            if (obj.Id == default(Guid))
                Set<T>().Add(obj);
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

            modelBuilder.Entity<User>().Ignore(u => u.ActionEntriesThisPeriod);

            modelBuilder.Entity<UserLikeComment>().HasKey(t => new { t.UserId, t.CommentId });
            modelBuilder.Entity<UserVoteAnswer>().HasKey(t => new { t.UserId, t.AnswerId });
            modelBuilder.Entity<UserVoteQuestion>().HasKey(t => new { t.UserId, t.QuestionId });
            modelBuilder.Entity<UserQuestionSide>().HasKey(t => new { t.UserId, t.QuestionId });
            modelBuilder.Entity<UserActivityLink>().HasKey(t => new { t.UserId, t.ActivityId });

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
        }
    }
}
