using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Qotd.Data;
using Qotd.Entities;
using Qotd.Utils;

namespace QotdMvc.Service
{
    public class QotdService : IQotdService
    {
        private IDataProvider _dataProvider;

        public QotdService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public IDataProvider DataProvider
        {
            get { return _dataProvider; }
        }

        public void FollowUser(UserFollow userFollow)
        {
            // create the activity
            Activity activity = new Activity()
            {
                ActivityType = ActivityType.FollowUser,
                SourceUser = userFollow.SourceUser,
                TargetUser = userFollow.TargetUser,
                Date = DateTime.Now,
                Text = "",
                VisibleWithoutLink = true,
            };
            Denormaliser.Denormalise(activity);
            userFollow.SourceUser.AddAction(ActivityType.FollowUser);
            userFollow.TargetUser.AddAction(ActivityType.ReceiveFollow);

            DataProvider.MarkAddedOrUpdated(userFollow.SourceUser);
            DataProvider.MarkAddedOrUpdated(userFollow.TargetUser);
            DataProvider.MarkAdded(userFollow);
            DataProvider.SaveChanges();
        }

        public void SaveNewQuestion(Question question)
        {
            Denormaliser.Denormalise(question);

            // create the activity
            Activity activity = new Activity()
            {
                ActivityType = ActivityType.PostQuestion,
                SourceUser = question.User,
                Date = question.CreatedOn,
                Text = question.MainText.Crop(128),
                VisibleWithoutLink = true,
                Question = question
            };
            Denormaliser.Denormalise(activity);
            question.User.AddAction(ActivityType.PostQuestion);

            DataProvider.MarkAddedOrUpdated(question.User);
            DataProvider.MarkAddedOrUpdated(activity);
            DataProvider.MarkAddedOrUpdated(question);
            DataProvider.SaveChanges();
        }

        public void SaveNewAnswer(Answer answer)
        {
            Denormaliser.Denormalise(answer);

            // create the activity
            Activity activity = new Activity()
            {
                ActivityType = ActivityType.PostAnswer,
                SourceUser = answer.User,
                Date = answer.CreatedOn,
                Text = answer.Title.Crop(128),
                VisibleWithoutLink = true,
                Answer = answer
            };
            Denormaliser.Denormalise(activity);
            answer.User.AddAction(ActivityType.PostAnswer);

            DataProvider.MarkAddedOrUpdated(answer.User);
            DataProvider.MarkAddedOrUpdated(activity);
            DataProvider.MarkAddedOrUpdated(answer);
            DataProvider.SaveChanges();
        }

        public void SaveNewComment(Comment comment)
        {
            Denormaliser.Denormalise(comment);

            Answer answer = DataProvider.GetAnswerById(comment.AnswerId);
            answer.NumComments++;
            // add the comment to the answer
            if (!answer.Comment1Id.HasValue)
            {
                answer.Comment1 = comment;
            }
            else if (!answer.Comment2Id.HasValue)
            {
                answer.Comment2 = comment;
            }
            else
            {
                answer.Comment1 = answer.Comment2;
                answer.Comment2 = comment;
            }
            // create the activity
            Activity activity = new Activity()
            {
                Date = comment.CreatedOn,
                SourceUser = comment.User,
                Text = comment.Content.Crop(128),
                Comment = comment,
                ActivityType = ActivityType.PostComment,
                VisibleWithoutLink = true
            };
            Denormaliser.Denormalise(activity);
            comment.User.AddAction(ActivityType.PostComment);
            DataProvider.MarkAddedOrUpdated(comment.User);
            DataProvider.MarkAddedOrUpdated(activity);
            DataProvider.MarkAddedOrUpdated(comment);
            DataProvider.MarkAddedOrUpdated(answer);
            DataProvider.SaveChanges();
        }

        public void SaveNewUser(User user)
        {
            // create activity
            Activity activity = new Activity()
            {
                ActivityType = ActivityType.Join,
                SourceUser = user,
                Date = DateTime.Now,
                LinksCreated = false,
                NotificationsCreated = false,
                VisibleWithoutLink = true
            };
            Denormaliser.Denormalise(activity);

            DataProvider.MarkAddedOrUpdated(user);
            DataProvider.MarkAddedOrUpdated(user);
            DataProvider.SaveChanges();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_dataProvider != null)
            {
            }
        }

        #endregion
    }
}