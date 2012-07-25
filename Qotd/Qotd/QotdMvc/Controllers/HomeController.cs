﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services;
using Qotd.Entities;

namespace QotdMvc.Controllers
{
    public class HomeController : BaseController
    {
        private const int DEFAULT_TAKE = 20;
        
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

        public ActionResult AnswersLatest(int skip = 0, int take = DEFAULT_TAKE)
        {
            if (UserEntity != null)
                ViewBag.Answers = DataProvider.GetAnswersLatest(UserEntity.Id, TodaysQuestion.Id, skip, take);
            else
                ViewBag.Answers = DataProvider.GetAnswersLatest(TodaysQuestion.Id, skip, take);
            ViewBag.Skip = take;
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
            ViewBag.Skip = take;
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
            ViewBag.Skip = take;
            ViewBag.Take = DEFAULT_TAKE;
            ViewBag.Action = "AnswersRated";
            return View("Answers");
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
            DataProvider.VoteAnswer(answerId, UserEntity.Id, 1);
            return View("Answer", DataProvider.GetAnswerById(answerId, UserEntity.Id));
        }

        [HttpGet]
        public ActionResult VoteDown(Guid answerId)
        {
            DataProvider.VoteAnswer(answerId, UserEntity.Id, -1);
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
