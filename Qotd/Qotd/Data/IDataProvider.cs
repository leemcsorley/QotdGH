using System;
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

        int SaveChanges();
    }

    public interface IDataProvider : IUnitOfWork, IDisposable
    {
        User GetUserByEmail(string email);

        User GetUserByUsername(string username);

        User GetUserByFacebookId(string facebookId);

        Question GetTodaysQuestion();

        bool HasUserPickedSide(Guid userId, Guid questionId);

        AnswerPO GetAnswerById(Guid answerId, Guid userId);

        AnswerPO[] GetAnswersLatest(Guid userId, Guid questionId, int skip, int take);

        AnswerPO[] GetAnswersLatest(Guid questionId, int skip, int take);

        AnswerPO[] GetAnswersRated(Guid userId, Guid questionId, int skip, int take);

        AnswerPO[] GetAnswersRated(Guid questionId, int skip, int take);

        QuestionPO[] GetQuestionsLatest(Guid userId, int skip, int take);

        QuestionPO[] GetQuestionsRated(Guid userId, int skip, int take);

        QuestionPO[] GetQuestionsLatest(int skip, int take);

        QuestionPO[] GetQuestionsRated(int skip, int take);

        Answer GetAnswerById(Guid answerId);

        CommentPO[] GetComments(Guid answerId, Guid userId);

        CommentPO GetCommentById(Guid commentId, Guid userId);

        void LikeComment(Guid commentId, Guid userId);

        void VoteAnswer(Guid answerId, User user, int voteDelta);

        void VoteQuestion(Guid questionId, User user, int voteDelta);

        ActivityPO[] GetActivities(DateTime? date, int take);

        LeaderboardPO GetLeaderboardThisPeriod(Guid userId, int skip, int take);

        LeaderboardPO GetLeaderboard(Guid userId, int skip, int take);

        LeaderboardPO GetLeaderboard(int skip, int take);

        LeaderboardPO GetLeaderboardThisPeriod(int skip, int take);
    }
}
