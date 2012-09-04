using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Qotd.Entities;
using System.Configuration;
using QotdMvc.Data;
using Qotd.PresentationObjects;

namespace QotdRealTimeTester
{
    public class Tester : IDisposable
    {
        int _seed;
        Thread _thread;

        const int MAX_SLEEP = 15000;
        const int MIN_SLEEP = 4000;

        static readonly string[] FIRST_NAMES = new string[]
        {
            "John", "Alex", "Joe", "Craig", "Mary", "Sally", "Jane"
        };

        static readonly string[] LAST_NAMES = new string[]
        {
            "Hunter", "Smith", "Russell", "McQueen", "McDonald", "Fletcher"
        };

        public Tester(int seed)
        {
            _seed = seed;
        }

        public void Start()
        {
            _thread = new Thread(new ThreadStart(Test));
            _thread.Start();
        }

        private void Test()
        {
            string imgpath = ConfigurationManager.AppSettings["UploadImagesUrl"];
            using (Qotd.Data.QotdContext dp = new Qotd.Data.QotdContext())
            using (QotdMvc.Service.IQotdService service = new QotdMvc.Service.QotdService(dp))
            {
                Question tq = dp.GetTodaysQuestion();
                var tags = dp.GetTags();
                Random rnd = new Random(_seed + Environment.TickCount);
                bool userCreated = false;
                User user = null;
                AnswerPO[] answers;
                QuestionPO[] questions;
                AnswerPO answerpo;
                QuestionPO questionpo;
                
                while (true)
                {
                    int rsleep = rnd.Next(MAX_SLEEP - MIN_SLEEP) + MIN_SLEEP;
                    Thread.Sleep(rsleep);

                    if (!userCreated)
                    {
                        user = new User()
                        {
                            DisplayName = FIRST_NAMES[rnd.Next(0, FIRST_NAMES.Length)] + " " + LAST_NAMES[rnd.Next(0, LAST_NAMES.Length)],
                            ProfileImageUrl = imgpath + "\\" + "pic" + (_seed).ToString() + ".jpg",
                            Username = "RTTU" + _seed,
                            JoinedOn = Qotd.Utils.Config.Now
                        };
                        service.SaveNewUser(user);
                        userCreated = true;
                    }
                    else
                    {
                        int action = rnd.Next(0, 6);
                        int count;
                        switch (action)
                        {
                            case 0:
                                // post question
                                Question q = new Question()
                                {
                                    User = user,
                                    CreatedOn = Qotd.Utils.Config.Now,
                                    DateFor = Qotd.Utils.Config.Now.Date,
                                    MainText = TestData.GenText(8),
                                    SubText = TestData.GenText(16),
                                    Details = TestData.GenText(80),
                                    QuestionType = QuestionType.Open,
                                    TagEntries = tags.OrderBy(t => rnd.NextDouble()).Take(rnd.Next(0, 5))
                                                    .Select(t => new TagEntry { Value = t.Value, Approved = t.Approved, TagId = t.Id }).ToArray()
                                };
                                service.SaveNewQuestion(q);
                                break;
                            case 1:
                                // post answer
                                Answer answer = new Answer()
                                {
                                    CreatedOn = Qotd.Utils.Config.Now,
                                    User = user,
                                    NumComments = 0,
                                    Question = tq,
                                    Title = TestData.GenText(5),
                                    Content = TestData.GenText(rnd.Next(0, 300)),
                                    TagEntries = tags.OrderBy(t => rnd.NextDouble()).Take(rnd.Next(0, 5))
                                                .Select(t => new TagEntry() { Value = t.Value, Approved = t.Approved, TagId = t.Id }).ToArray()
                                };
                                service.SaveNewAnswer(answer);
                                break;
                            case 2:
                                // post comment
                                
                                // first get random answer
                                answers = dp.GetAnswersLatest(tq.Id, 0, 1000, out count);
                                Comment comment = new Comment()
                                {
                                    AnswerId = answers[rnd.Next(0, answers.Length)].Answer.Id,
                                    Content = TestData.GenText(30),
                                    User = user,
                                    CreatedOn = Qotd.Utils.Config.Now,
                                    NumLikes = 0
                                };
                                service.SaveNewComment(comment);
                                break;
                            case 3:
                                // vote answer
                                answers = dp.GetAnswersLatest(user.Id, tq.Id, 0, 1000, out count);
                                answerpo = answers[rnd.Next(0, answers.Length)];
                                if (!answerpo.HasUserVoted)
                                    dp.VoteAnswer(answerpo.Answer.Id, user, rnd.Next(0, 2) == 0 ? 1 : -1);
                                break;
                            case 4:
                                // vote question
                                questions = dp.GetQuestionsLatest(user.Id, 0, 1000, out count);
                                questionpo = questions[rnd.Next(0, questions.Length)];
                                if (!questionpo.HasUserVoted)
                                    dp.VoteQuestion(questionpo.Question.Id, user, rnd.Next(0, 2) == 0 ? 1 : -1);
                                break;
                            case 5:
                                // follow tag
                                break;
                            case 6:
                                // follow user
                                break;
                        }
                    }
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_thread != null)
                _thread.Abort();
        }

        #endregion
    }
}
