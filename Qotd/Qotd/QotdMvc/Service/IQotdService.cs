using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qotd.Entities;

namespace QotdMvc.Service
{
    public interface IQotdService : IDisposable
    {
        void SaveNewUser(User user);

        void SaveNewComment(Comment comment);

        void SaveNewAnswer(Answer answer);

        void SaveNewQuestion(Question question);

        void FollowUser(UserFollow userFollow);
    }
}