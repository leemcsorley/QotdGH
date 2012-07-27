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
        
        //
        // GET: /Home/
        public ActionResult Index()
        {
            if (UserEntity != null)
                ViewBag.HasPickedSide = DataProvider.HasUserPickedSide(
                    UserEntity.Id, TodaysQuestion.Id);

            return View();
        }

        public ActionResult Comments(Guid answerId)
        {
            ViewBag.Comments = DataProvider.GetComments(answerId, UserEntity.Id);
            return View();
        }

        public ActionResult Activities()
        {
            ViewBag.Activities = DataProvider.GetActivities(null, DEFAULT_TAKE);
            return View();
        }

        public ActionResult Answer(Guid answerId)
        {
            return View("Answer", DataProvider.GetAnswerById(answerId, UserEntity.Id));
        }

        public ActionResult QuestionsLatest(int skip = 0, int take = DEFAULT_TAKE)
        {
            if (UserEntity != null)
                ViewBag.Questions = DataProvider.GetQuestionsLatest(UserEntity.Id, skip, take);
            else
                ViewBag.Questions = DataProvider.GetQuestionsLatest(skip, take);
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "QuestionsLatest";
            return View("Questions");
        }

        public ActionResult QuestionsRated(int skip = 0, int take = DEFAULT_TAKE)
        {
            if (UserEntity != null)
                ViewBag.Questions = DataProvider.GetQuestionsRated(UserEntity.Id, skip, take);
            else
                ViewBag.Questions = DataProvider.GetQuestionsRated(skip, take);
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "QuestionsRated";
            return View("Questions");
        }

        public ActionResult QuestionsTab(int skip = 0, int take = DEFAULT_TAKE)
        {
            if (UserEntity != null)
                ViewBag.Questions = DataProvider.GetQuestionsRated(UserEntity.Id, skip, take);
            else
                ViewBag.Questions = DataProvider.GetQuestionsRated(skip, take);
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "QuestionsRated";
            return View("QuestionsTab");
        }

        public ActionResult AnswersLatest(int skip = 0, int take = DEFAULT_TAKE)
        {
            if (UserEntity != null)
                ViewBag.Answers = DataProvider.GetAnswersLatest(UserEntity.Id, TodaysQuestion.Id, skip, take);
            else
                ViewBag.Answers = DataProvider.GetAnswersLatest(TodaysQuestion.Id, skip, take);
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "AnswersLatest";
            return View("Answers");
        }

        public ActionResult AnswersTab(int skip = 0, int take = DEFAULT_TAKE)
        {
            if (UserEntity != null)
                ViewBag.Answers = DataProvider.GetAnswersRated(UserEntity.Id, TodaysQuestion.Id, skip, take);
            else
                ViewBag.Answers = DataProvider.GetAnswersRated(TodaysQuestion.Id, skip, take);
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "AnswersRated";
            return View("AnswersTab");
        }

        public ActionResult AnswersRated(int skip = 0, int take = DEFAULT_TAKE)
        {
            if (UserEntity != null)
                ViewBag.Answers = DataProvider.GetAnswersRated(UserEntity.Id, TodaysQuestion.Id, skip, take);
            else
                ViewBag.Answers = DataProvider.GetAnswersRated(TodaysQuestion.Id, skip, take);
            ViewBag.Skip = skip + take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "AnswersRated";
            return View("Answers");
        }

        public ActionResult LeaderboardTab()
        {
            LeaderboardPO leaderboard;
            if (UserEntity == null)
                leaderboard = DataProvider.GetLeaderboardThisPeriod(0, DEFAULT_TAKE_LEADERBOARD);
            else
                leaderboard = DataProvider.GetLeaderboardThisPeriod(UserEntity.Id, 0, DEFAULT_TAKE_LEADERBOARD);
            ViewBag.Leaderboard = leaderboard;
            return View();
        }

        public ActionResult LeaderboardOverall(int skip = 0, int take = DEFAULT_TAKE_LEADERBOARD)
        {
            LeaderboardPO leaderboard;
            if (UserEntity == null)
                leaderboard = DataProvider.GetLeaderboard(skip, DEFAULT_TAKE_LEADERBOARD);
            else
                leaderboard = DataProvider.GetLeaderboard(UserEntity.Id, skip, DEFAULT_TAKE_LEADERBOARD);
            return View("Leaderboard", leaderboard);
        }

        public ActionResult LeaderboardThisPeriod(int skip = 0, int take = DEFAULT_TAKE_LEADERBOARD)
        {
            LeaderboardPO leaderboard;
            if (UserEntity == null)
                leaderboard = DataProvider.GetLeaderboardThisPeriod(skip, DEFAULT_TAKE_LEADERBOARD);
            else
                leaderboard = DataProvider.GetLeaderboardThisPeriod(UserEntity.Id, skip, DEFAULT_TAKE_LEADERBOARD);
            return View("Leaderboard", leaderboard);
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

        [HttpGet, ValidateInput(false)]
        public ActionResult CommentSubmit(Guid answerId, string content)
        {
            try
            {
                DateTime date = DateTime.Now;
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
        public ActionResult QuestionSubmit(string main, string sub, string details)
        {
            try
            {
                DateTime now = DateTime.Now;

                // create the question
                Question question = new Question()
                {
                    CreatedOn = now,
                    DateFor = now.AddDays(1).Date,
                    MainText = main,
                    SubText = sub,
                    Details = details,
                    QuestionType = QuestionType.Open,
                    User = UserEntity
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
        public ActionResult AnswerSubmit(string title, string content)
        {
            try
            {
                DateTime date = DateTime.Now;

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
                    Comment2Id = null
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
