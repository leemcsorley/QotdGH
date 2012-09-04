using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services;
using Qotd.Entities;
using Qotd.PresentationObjects;

namespace QotdMvc.Controllers
{
    public class HomeController : BaseController
    {
        private const int DEFAULT_TAKE = 20;
        private const int DEFAULT_TAKE_LEADERBOARD = 50;
        private const int DEFAULT_TAKE_NOTIFICATIONS = 20;
        private const int DEFAULT_TAKE_ACTIVITIES = 40;
        private const int DEFAULT_SEARCH_TAKE = 5;

        public ActionResult AddFollow(Guid userId)
        {
            var targetUser = DataProvider.GetUserById(userId);
            QotdService.FollowUser(
                new UserFollow()
                {
                    SourceUser = UserEntity,
                    TargetUser = targetUser.User,
                    LinksCreated = false
                });
            return RedirectToAction("User", "Home", new { userId = userId });
        }

        public ActionResult AddFollowTag(Guid tagId)
        {
            QotdService.FollowTag(
                new UserFollowTag()
                {
                    SourceUser = UserEntity,
                    Tag = DataProvider.GetTagById(tagId, null).Tag,
                    LinksCreated = false
                });
            return RedirectToAction("Tag", "Home", new { tagId = tagId });
        }

        public ActionResult Tag(Guid tagId)
        {
            ViewBag.Tag = DataProvider.GetTagById(tagId, UserEntity == null ? (Guid?)null : UserEntity.Id);
            ViewBag.HideQuestion = false;
            return View();
        }

        public ActionResult Tags(string q)
        {
            return Json(
                    DataProvider.GetTags(q)
                    .Select(t => new { value = t.Id, name = t.Value }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UserPopover(Guid userId)
        {
            return View(DataProvider.GetUserById(userId));
        }

        public ActionResult User(Guid userId)
        {
            ViewBag.TargetUser = DataProvider.GetUserById(userId, UserEntity == null ? (Guid?)null : UserEntity.Id);
            ViewBag.HideQuestion = false;
            return View();
        }

        public ActionResult HistoryForTag(Guid tagId, int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            var activities = DataProvider.GetHistoryForTag(tagId, skip, take, out count);
            ViewBag.Activities = activities;
            ViewBag.Action = "HistoryForTag";
            ViewBag.Skip = skip + take;
            ViewBag.Take = take;
            ViewBag.More = count > skip + activities.Length;
            return View("History");
        }

        public ActionResult TodaysActivitiesForUser(Guid userId, DateTime? skip = null, int take = DEFAULT_TAKE)
        {
            var activities = DataProvider.GetTodaysActivitiesForUser(userId, skip, take);
            ViewBag.Activities = activities;
            ViewBag.Action = "TodaysActivitiesForUser";
            ViewBag.Take = take;
            if (activities.Length > 0)
                ViewBag.Skip = activities[activities.Length - 1].Activity.Date;
            ViewBag.TargetUserId = userId;
            return View("ActivitiesFull");
        }

        public ActionResult HistoryForUser(Guid userId, int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            var activities = DataProvider.GetHistoryForUser(userId, skip, take, out count);
            ViewBag.Activities = activities;
            ViewBag.Action = "HistoryForUser";
            ViewBag.Skip = skip + take;
            ViewBag.Take = take;
            ViewBag.More = count > skip + activities.Length;
            return View("History");
        }

        public ActionResult History(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (dateFrom == null) dateFrom = Qotd.Utils.Config.Now.Date.AddDays(-DEFAULT_TAKE);
            if (dateTo == null) dateTo = Qotd.Utils.Config.Now;
            ViewBag.Activities = DataProvider.GetHistory(dateFrom.Value, dateTo.Value);
            ViewBag.Action = "History";
            ViewBag.Group = true;
            return View("History");
        }

        public ActionResult Notifications()
        {
            var not = DataProvider.ReadNotifications(UserEntity.Id);
            ViewBag.Notifications = not;
            return View();
        }

        public ActionResult AllNotifications(bool ajax = false, int skip = 0, int take = DEFAULT_TAKE_NOTIFICATIONS)
        {
            int count = 0;
            var nots = DataProvider.GetNotifications(UserEntity.Id, skip, take, out count);
            ViewBag.Notifications = nots; 
            ViewBag.Skip = skip + take;
            ViewBag.Take = take;
            ViewBag.Layout = ajax ? "" : "~/Views/Shared/_Layout.cshtml";
            ViewBag.More = count > skip + nots.Length;
            return View();
        }

        public ActionResult SearchItem(Guid id, SearchItemType type)
        {
            switch (type)
            {
                case SearchItemType.User:
                    return RedirectToAction("User", "Home", new { userId = id });
                case SearchItemType.AnswerContent:
                    return RedirectToAction("Answer", "Home", new { answerId = id, ajax = false, pageUrl = Url.Action("Index", "Home"), pageName = "Home" });
                case SearchItemType.AnswerTags:
                    return RedirectToAction("Answer", "Home", new { answerId = id, ajax = false, pageUrl = Url.Action("Index", "Home"), pageName = "Home" });
                case SearchItemType.QuestionTags:
                    return RedirectToAction("Question", "Home", new { questionId = id, ajax = false, pageUrl = Url.Action("Index", "Home"), pageName = "Home" });
                case SearchItemType.QuestionContent:
                    return RedirectToAction("Question", "Home", new { questionId = id, ajax = false, pageUrl = Url.Action("Index", "Home"), pageName = "Home" });
            }
            throw new System.NotImplementedException();
        }

        public ActionResult Search(string search, bool ajax = true)
        {
            if (ajax)
            {
                var results = DataProvider.Search(search, DEFAULT_SEARCH_TAKE);

                return View(results);
            }
            else throw new System.NotImplementedException();
        }

        //
        // GET: /Home/
        public ActionResult Index()
        {
            if (UserEntity != null)
                ViewBag.HasPickedSide = DataProvider.HasUserPickedSide(
                    UserEntity.Id, TodaysQuestion.Id);

            return View();
        }

        public ActionResult UserScores(Guid userId, ScoreEntryType type, long? number = null, int take = DEFAULT_TAKE_ACTIVITIES)
        {
            var user = DataProvider.GetUserById(userId);
            int count = 0;
            var entries = DataProvider.GetScoreEntries(type, userId, number, take, out count);
            ViewBag.ScoreEntries = entries;
            ViewBag.Type = type;
            ViewBag.TargetUserId = userId;
            ViewBag.Take = take;
            if (entries != null && entries.Length > 0)
                ViewBag.Number = entries[entries.Length - 1].Activity.Activity.ActivityNumber;
            switch (type)
            {
                case ScoreEntryType.AnswerVotes:
                    ViewBag.Points = user.User.AnswerQualityScore;
                    ViewBag.Rank = user.User.AnswerQualityRank;
                    ViewBag.Stars = user.User.AnswerQualityStars;
                    break;
                case ScoreEntryType.QuestionVotes:
                    ViewBag.Points = user.User.QuestionQualityScore;
                    ViewBag.Rank = user.User.QuestionQualityRank;
                    ViewBag.Stars = user.User.QuestionQualityStars;
                    break;
                case ScoreEntryType.ActivityLevel:
                    ViewBag.Points = user.User.ActivityLevelScore;
                    ViewBag.Rank = user.User.ActivityLevelRank;
                    ViewBag.Stars = user.User.ActivityLevelStars;
                    break;
                case ScoreEntryType.Sociability:
                    ViewBag.Points = user.User.SociabilityScore;
                    ViewBag.Rank = user.User.SociabilityRank;
                    ViewBag.Stars = user.User.SociabilityStars;
                    break;
                case ScoreEntryType.Overall:
                    ViewBag.Points = user.User.OverallRating;
                    ViewBag.Rank = user.User.OverallRatingRank;
                    ViewBag.Star = user.User.OverallStars;
                    break;
            }
            return View();
        }

        public ActionResult UserScoresTab(Guid userId)
        {
            ViewBag.TargetUserId = userId;
            ViewBag.TargetUser = DataProvider.GetUserById(userId);
            return View();
        }

        public ActionResult ReportForm()
        {
            return View();
        }

        public ActionResult ReportAnswer(Guid answerId)
        {
            DataProvider.MarkAdded(
                new ContentReport()
                {
                    CreatedOn = Qotd.Utils.Config.Now,
                    User = UserEntity,
                    AnswerId = answerId
                });
            DataProvider.SaveChanges();
            return View("ReportSuccess");
        }

        public ActionResult ReportQuestion(Guid questionId)
        {
            DataProvider.MarkAdded(
                new ContentReport()
                {
                    CreatedOn = Qotd.Utils.Config.Now,
                    User = UserEntity,
                    QuestionId = questionId
                });
            DataProvider.SaveChanges();
            return View("ReportSuccess");
        }

        public ActionResult Comments(Guid answerId)
        {
            ViewBag.Comments = DataProvider.GetComments(answerId, UserEntity == null ? (Guid?)null : UserEntity.Id);
            return View();
        }

        public ActionResult Activities(long? number = null, int take = DEFAULT_TAKE_ACTIVITIES)
        {
            var activities = DataProvider.GetActivities(number, take);
            ViewBag.Activities = activities;
            ViewBag.Date = null;
            if (activities.Length > 0)
                ViewBag.Number = activities[0].Activity.ActivityNumber;
            else
                ViewBag.Number = number;
            return View();
        }

        public ActionResult Answer(Guid answerId, bool ajax = true, string pageUrl = null, string pageName = null)
        {
            AnswerPO answer = DataProvider.GetAnswerById(answerId, UserEntity == null ? (Guid?) null : UserEntity.Id);
            if (ajax)
                return View("Answer", answer);
            else
            {
                ViewBag.Object = answer;
                ViewBag.Type = SingleType.Answer;
                ViewBag.PastPageUrl = pageUrl;
                ViewBag.PastPageName = pageName;
                return View("Single");
            }
        }

        public ActionResult Question(Guid questionId, bool ajax = true, string pageUrl = null, string pageName = null)
        {
            var question = DataProvider.GetQuestionById(questionId, UserEntity == null ? (Guid?)null : UserEntity.Id);
            if (ajax)
                return View("Question", question);
            else
            {
                ViewBag.Object = question;
                ViewBag.Type = SingleType.Question;
                ViewBag.PastPageUrl = pageUrl;
                ViewBag.PastPageName = pageName;
                return View("Single");
            }
        }

        public ActionResult QuestionsLatest(int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            QuestionPO[] questions;
            if (UserEntity != null)
                questions = DataProvider.GetQuestionsLatest(UserEntity.Id, skip, take, out count);
            else
                questions = DataProvider.GetQuestionsLatest(skip, take, out count);
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Questions = questions;
            ViewBag.Action = "QuestionsLatest";
            ViewBag.More = count > skip + questions.Length;
            return View("Questions");
        }

        public ActionResult QuestionsRated(int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            QuestionPO[] questions;
            if (UserEntity != null)
                questions = DataProvider.GetQuestionsRated(UserEntity.Id, skip, take, out count);
            else
                questions = DataProvider.GetQuestionsRated(skip, take, out count);
            ViewBag.Questions = questions;
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "QuestionsRated";
            ViewBag.More = count > skip + questions.Length;
            return View("Questions");
        }

        public ActionResult QuestionsTab(int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            QuestionPO[] questions;
            if (UserEntity != null)
                questions = DataProvider.GetQuestionsRated(UserEntity.Id, skip, take, out count);
            else
                questions = DataProvider.GetQuestionsRated(skip, take, out count);
            ViewBag.Questions = questions;
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "QuestionsRated";
            ViewBag.More = count > skip + questions.Length;
            return View("QuestionsTab");
        }

        public ActionResult QuestionsFollowed(int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            QuestionPO[] questions;
            questions = DataProvider.GetQuestionsFollowed(UserEntity.Id, FollowSource.TagFollow | FollowSource.UserFollow, skip, take, out count);
            ViewBag.Questions = questions;
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "QuestionsFollowed";
            ViewBag.More = count > skip + questions.Length;
            return View("Questions");
        }

        public ActionResult AnswersFollowed(int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            AnswerPO[] answers;
            answers = DataProvider.GetAnswersFollowed(UserEntity.Id, TodaysQuestion.Id, FollowSource.TagFollow | FollowSource.UserFollow, skip, take, out count);
            ViewBag.Answers = answers;
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "AnswersFollowed";
            ViewBag.More = count > skip + answers.Length;
            return View("Answers");
        }

        public ActionResult AnswersLatest(int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            AnswerPO[] answers;
            if (UserEntity != null)
                answers = DataProvider.GetAnswersLatest(UserEntity.Id, TodaysQuestion.Id, skip, take, out count);
            else
                answers = DataProvider.GetAnswersLatest(TodaysQuestion.Id, skip, take, out count);
            ViewBag.Answers = answers;
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "AnswersLatest";
            ViewBag.More = count > skip + answers.Length;
            return View("Answers");
        }

        public ActionResult AnswersTab(int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            AnswerPO[] answers;
            if (UserEntity != null)
                answers = DataProvider.GetAnswersRated(UserEntity.Id, TodaysQuestion.Id, skip, take, out count);
            else
                answers = DataProvider.GetAnswersRated(TodaysQuestion.Id, skip, take, out count);
            ViewBag.Answers = answers;
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "AnswersRated";
            ViewBag.More = count > skip + answers.Length;
            return View("AnswersTab");
        }

        public ActionResult AnswersRated(int skip = 0, int take = DEFAULT_TAKE)
        {
            int count;
            AnswerPO[] answers;
            if (UserEntity != null)
                answers = DataProvider.GetAnswersRated(UserEntity.Id, TodaysQuestion.Id, skip, take, out count);
            else
                answers = DataProvider.GetAnswersRated(TodaysQuestion.Id, skip, take, out count);
            ViewBag.Answers = answers;
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "AnswersRated";
            ViewBag.More = count > skip + answers.Length;
            return View("Answers");
        }

        public ActionResult LeaderboardTab()
        {
            return View();
        }

        public ActionResult Leaderboard(ScoreEntryType type)
        {

            return View();
        }

        public ActionResult LeaderboardOverall(int skip = 0, int take = DEFAULT_TAKE_LEADERBOARD)
        {
            //int count;
            //LeaderboardPO leaderboard;
            //if (UserEntity == null)
            //    leaderboard = DataProvider.GetLeaderboard(skip, DEFAULT_TAKE_LEADERBOARD, out count);
            //else
            //    leaderboard = DataProvider.GetLeaderboard(UserEntity.Id, skip, DEFAULT_TAKE_LEADERBOARD, out count);
            //return View("Leaderboard", leaderboard);
            throw new System.NotImplementedException();
        }

        public ActionResult LeaderboardThisPeriod(int skip = 0, int take = DEFAULT_TAKE_LEADERBOARD)
        {
            //int count;
            //LeaderboardPO leaderboard;
            //if (UserEntity == null)
            //    leaderboard = DataProvider.GetLeaderboardThisPeriod(skip, DEFAULT_TAKE_LEADERBOARD, out count);
            //else
            //    leaderboard = DataProvider.GetLeaderboardThisPeriod(UserEntity.Id, skip, DEFAULT_TAKE_LEADERBOARD, out count);
            //return View("Leaderboard", leaderboard);
            throw new System.NotImplementedException();
        }

        public ActionResult FollowList(Guid userId)
        {
            ViewBag.Users = DataProvider.GetUsersFollowed(userId);
            return View();
        }

        public ActionResult FollowListTags(Guid userId)
        {
            ViewBag.Tags = DataProvider.GetTagsFollowed(userId);
            return View();
        }

        [HttpGet]
        public ActionResult LikeComment(Guid commentId)
        {
            DataProvider.LikeComment(commentId, UserEntity.Id);
            return View("Comment", DataProvider.GetCommentById(commentId, UserEntity.Id));
        }

        [HttpGet]
        public ActionResult VoteUp(Guid answerId)
        {
            DataProvider.VoteAnswer(answerId, UserEntity, 1);
            return View("Answer", DataProvider.GetAnswerById(answerId, UserEntity.Id));
        }

        [HttpGet]
        public ActionResult VoteDown(Guid answerId)
        {
            DataProvider.VoteAnswer(answerId, UserEntity, -1);
            return View("Answer", DataProvider.GetAnswerById(answerId, UserEntity.Id));
        }

        [HttpGet]
        public ActionResult VoteUpQuestion(Guid questionId)
        {
            DataProvider.VoteQuestion(questionId, UserEntity, 1);
            return View("Question", DataProvider.GetQuestionById(questionId, UserEntity.Id));
        }

        [HttpGet]
        public ActionResult VoteDownQuestion(Guid questionId)
        {
            DataProvider.VoteQuestion(questionId, UserEntity, -1);
            return View("Question", DataProvider.GetQuestionById(questionId, UserEntity.Id));
        }

        [HttpGet, ValidateInput(false)]
        public ActionResult CommentSubmit(Guid answerId, string content)
        {
            try
            {
                DateTime date = Qotd.Utils.Config.Now;
                // create the comment
                Comment comment = new Comment()
                {
                    User = UserEntity,
                    AnswerId = answerId,
                    Content = content,
                    CreatedOn = date,
                    NumLikes = 0
                };
                // save
                QotdService.SaveNewComment(comment);
                return View("Answer", DataProvider.GetAnswerById(answerId, UserEntity.Id));
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult QuestionSubmit(string main, string sub, string details, string tags)
        {
            try
            {
                DateTime now = Qotd.Utils.Config.Now;

                // create the question
                Question question = new Question()
                {
                    CreatedOn = now,
                    DateFor = now.AddDays(1).Date,
                    MainText = main,
                    SubText = sub,
                    Details = details,
                    QuestionType = QuestionType.Open,
                    User = UserEntity,
                    TagEntries = tags.Split(',').Where(s => !String.IsNullOrWhiteSpace(s))
                        .Select(s => new TagEntry() { Value = s, Approved = false }).ToArray()
                };

                QotdService.SaveNewQuestion(question);

                return Json(new { questionId = question.Id });
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AnswerSubmit(string title, string content, string tags)
        {
            try
            {
                DateTime date = Qotd.Utils.Config.Now;

                // create the answer
                Answer answer = new Answer()
                {
                    CreatedOn = date,
                    Content = content,
                    Title = title,
                    Question = TodaysQuestion,
                    User = UserEntity,
                    DebateSide = 0,
                    Comment1Id = null,
                    Comment2Id = null,
                    TagEntries = tags.Split(',').Where(s => !String.IsNullOrWhiteSpace(s))
                        .Select(s => new TagEntry() { Value = s, Approved = false }).ToArray()
                };
                QotdService.SaveNewAnswer(answer);

                return Json(new { answerId = answer.Id });
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }
    }
}
