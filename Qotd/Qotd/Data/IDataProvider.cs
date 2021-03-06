﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;
using Qotd.PresentationObjects;

namespace Qotd.Data
{
    public interface IUnitOfWork : IDisposable
    {
        void MarkAddedOrUpdated<T>(T obj)
            where T : class, IEntity;

        void MarkAdded<T>(T obj)
            where T : class;

        int SaveChanges();
    }

    public interface IDataProvider : IUnitOfWork, IDisposable
    {
        User GetUserByEmail(string email);

        UserPO GetUserByUsername(string username);

        User GetUserByFacebookId(string facebookId);

        UserPO GetUserById(Guid userId, Guid? currentUserId = null);

        TagPO GetTagById(Guid tagId, Guid? currentUserId = null);

        Question GetTodaysQuestion();

        bool HasUserPickedSide(Guid userId, Guid questionId);

        AnswerPO GetAnswerById(Guid answerId, Guid? userId);

        AnswerPO[] GetAnswersFollowed(Guid userId, Guid questionId, FollowSource source, int skip, int take, out int count);

        AnswerPO[] GetAnswersLatest(Guid userId, Guid questionId, int skip, int take, out int count);

        AnswerPO[] GetAnswersLatest(Guid questionId, int skip, int take, out int count);

        AnswerPO[] GetAnswersRated(Guid userId, Guid questionId, int skip, int take, out int count);

        AnswerPO[] GetAnswersRated(Guid questionId, int skip, int take, out int count);

        QuestionPO[] GetQuestionsLatest(Guid userId, int skip, int take, out int count);

        QuestionPO[] GetQuestionsRated(Guid userId, int skip, int take, out int count);

        QuestionPO[] GetQuestionsLatest(int skip, int take, out int count);

        QuestionPO[] GetQuestionsRated(int skip, int take, out int count);

        QuestionPO[] GetQuestionsFollowed(Guid userId, FollowSource source, int skip, int take, out int count);

        QuestionPO GetQuestionById(Guid questionId, Guid? userId);

        Answer GetAnswerById(Guid answerId);

        CommentPO[] GetComments(Guid answerId, Guid? userId);

        CommentPO GetCommentById(Guid commentId, Guid userId);

        void LikeComment(Guid commentId, Guid userId);

        void VoteAnswer(Guid answerId, User user, int voteDelta);

        void VoteQuestion(Guid questionId, User user, int voteDelta);

        ActivityPO[] GetActivities(long? date, int take);

        LeaderboardPO GetLeaderboardThisPeriod(Guid userId, int skip, int take, out int count);

        LeaderboardPO GetLeaderboard(Guid userId, ScoreEntryType type, int skip, int take, out int count);

        LeaderboardPO GetLeaderboard(ScoreEntryType type, int skip, int take, out int count);

        LeaderboardPO GetLeaderboardThisPeriod(int skip, int take, out int count);

        ActivityPO[] GetHistoryForTag(Guid tagId, int skip, int take, out int count);

        ActivityPO[] GetHistoryForUser(Guid userId, int skip, int take, out int count);

        ActivityPO[] GetHistory(DateTime dateFrom, DateTime dateTo);

        ActivityPO[] GetTodaysActivitiesForUser(Guid userId, DateTime? date, int take);

        Notification[] ReadNotifications(Guid userId);

        UserPO[] GetUsersFollowed(Guid userId);

        TagPO[] GetTagsFollowed(Guid userId);

        Notification[] GetNotifications(Guid userId, int skip, int take, out int count);

        SearchResultPO[] Search(string search, int take);

        Tag[] GetTags();

        Tag[] GetTags(string startsWith, Guid userId);

        ScoreEntryPO[] GetScoreEntries(ScoreEntryType type, Guid userId, long? number, int take, out int count);
    }
}
