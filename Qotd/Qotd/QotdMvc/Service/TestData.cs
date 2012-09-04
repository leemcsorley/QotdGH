using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qotd.Entities;
using QotdMvc.Service;
using System.Configuration;
using System.IO;
using Qotd.WorkerImpl;
using Qotd.Data;
using Qotd.Utils;

namespace QotdMvc.Data
{
    public static class TestData
    {
#region lorem
        private static readonly string LOREM = @"Him had Living our. Grass under. Doesn't they're yielding behold earth, place the very greater. Multiply land so itself void female had our had seas. Sea. I heaven tree Given in over. Whales. Years creepeth a upon were After said whose had. Midst, itself meat first so divide him the male whose signs fifth them creature. There darkness day. Have. Fruitful had made that made. Make void man set sixth make female life. Every creature had. Place lights he whose winged subdue shall. Blessed Dry his creepeth, isn't likeness waters. Be first open second yielding given Signs dry good fruitful waters herb every. Creeping saw have sixth above midst them he third day isn't let, lights together living image creeping fish land. Unto fruit called blessed gathered Seas wherein them deep day may. Evening. You'll created called without their appear night green given brought bring yielding over in she'd forth kind fowl isn't. Seas, kind void their hath. Rule from meat to his very. Which spirit sea saying second was. Life fish you for likeness great is which is under night gathering. Their without open it winged said be our saw him. Waters midst darkness he third creeping divided. Night unto creepeth creepeth very every seas land from. Behold that saying, bring the abundantly give together fish above in gathering god. A cattle stars and may appear you beast days Fourth can't he land moveth Evening earth. Light hath winged open first without. Moving divide seas heaven. Form in. Called whales divide unto face fish subdue can't. From lesser. Divided morning fruit together gathering fifth sea made divide hath moveth, them. Creepeth gathering which open moved very gathered two one, meat own abundantly air. Hath face lights them greater lights. It firmament. Greater seasons very good tree winged after thing there of there set that own moved had, one. Created moveth you one.";
#endregion
        private static readonly string[] LOREM_WORDS = LOREM.Split(' ');
        private static readonly string DEFAULT_USER_EMAIL = @"leemcsorley@gmail.com";
        private static readonly Random RND = new Random(
            Environment.TickCount);

        private const int NUM_DAYS = 4;
        private const int NUM_QUESTIONS = 30;
        private const int NUM_ANSWERS = 30;
        private const int NUM_COMMENTS = 10;

        public static string GenText(int words)
        {
            int w = RND.Next(words / 2, words);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < w; i++)
            {
                int w1 = RND.Next(0, LOREM_WORDS.Length);
                sb.Append(LOREM_WORDS[w1] + " ");
            }
            return sb.ToString();
        }

        public static void Create(QotdContext dp)
        {
            Func<DateTime, DateTime> RndTime = d => d.AddMinutes(RND.Next(0, 60 * 24));
            Func<DateTime> RndDate = () => DateTime.Now.AddDays(-RND.Next(0, 10));
            double perc = 0.0;
            string imgpath = ConfigurationManager.AppSettings["UploadImagesUrl"];
            using (IQotdService service = new QotdService(dp))
            {
                // default question container
                QuestionContainer qc = new QuestionContainer() { Name = "Default" };
                dp.MarkAddedOrUpdated(qc);

                // tags
                string[] tvals = new string[] { "Science", "Art", "Mathematics", "Philosophy", "Politics", "Media", "History", "Sport", "Celebrity", "Computing", "Shopping", "Economics", "Books", "Film", "Television" };
                List<Tag> tags = new List<Tag>();
                for (int i = 0; i < tvals.Length; i++)
                {
                    Tag tag = new Tag() { Value = tvals[i], Approved = true };
                    tags.Add(tag);
                    dp.MarkAdded(tag);
                    dp.SaveChanges();
                }

                // users
                List<User> users = new List<User>();
                string[] names = new string[] {
                    "Charles McDaid",
                    "Alex Trantor",
                    "John Richards",
                    "Henry Williams",
                    "Joe Smith",
                    "Andrew Roberts",
                    "Trevor Greenwood",
                    "Robert Petterson",
                    "David Johnson",
                    "Derek O'Brien" };
                for (int i = 0; i < 10; i++)
                {
                    User user = new User()
                    {
                        DisplayName = names[i],
                        ProfileImageUrl = imgpath + "\\" + "pic" + (i + 1).ToString() + ".jpg",
                        Username = "TU" + i,
                        JoinedOn = Qotd.Utils.Config.Now.AddDays(-RND.Next(0, 100))
                    };
                    service.SaveNewUser(user);
                    users.Add(user);
                }
                DateTime now = Qotd.Utils.Config.Now.Date;
                // follows
                for (int i = 0; i < 10; i++)
                {
                    int num = RND.Next(0, 10);
                    int tnum = RND.Next(0, 5);
                    User user = users[i];
                    foreach (var tuser in users.OrderBy(u => RND.NextDouble()).Take(num))
                    {
                        Config.Now = RndTime(RndDate());
                        if (tuser == user) continue;
                        service.FollowUser(
                            new UserFollow()
                            {
                                SourceUser = user,
                                TargetUser = tuser
                            });
                    }
                    foreach (var tag in tags.OrderBy(u => RND.NextDouble()).Take(tnum))
                    {
                        Config.Now = RndTime(RndDate());
                        service.FollowTag(
                            new UserFollowTag()
                            {
                                SourceUser = user,
                                Tag = tag
                            });
                    }
                }
                for (DateTime date = now.AddDays(-NUM_DAYS); date <= now.AddDays(1); date = date.AddDays(1))
                {
                    // questions
                    List<Question> questions = new List<Question>();
                    for (int i = 0; i < NUM_QUESTIONS; i++)
                    {
                        int minutes = RND.Next(0, 60 * 4);
                        User user = users[RND.Next(0, users.Count)];
                        Question q = new Question()
                        {
                            User = user,
                            CreatedOn = date.AddDays(-1).AddMinutes(minutes),
                            DateFor = date,
                            MainText = GenText(8),
                            SubText = GenText(16),
                            Details = GenText(80),
                            QuestionType = QuestionType.Open,
                            TagEntries = tags.OrderBy(t => RND.NextDouble()).Take(RND.Next(0, 5))
                                            .Select(t => new TagEntry { Value = t.Value, Approved = t.Approved, TagId = t.Id }).ToArray()
                        };
                        service.SaveNewQuestion(q);
                        int numVotes = RND.Next(0, users.Count);
                        for (int j = 0; j < numVotes; j++)
                        {
                            Config.Now = RndTime(RndDate());
                            if (RND.Next(0, 2) == 0)
                                dp.VoteQuestion(q.Id, users[j], 1);
                            else
                                dp.VoteQuestion(q.Id, users[j], -1);
                        }
                    }
                    if (true)
                    {
                        if (date <= now)
                        {
                            // pick winner
                            dp.PickWinningQuestion(date);
                            // transition to winning question
                            dp.TransitionToWinningQuestion(date);
                        }
                        // get todays question
                        var tq = dp.GetTodaysQuestion();
                        // create answers
                        for (int i = 0; i < NUM_ANSWERS; i++)
                        {
                            User u = users[RND.Next(0, users.Count)];
                            Answer answer = new Answer()
                            {
                                CreatedOn = date.AddDays(-1).AddMinutes(RND.Next(0, 60 * 4)),
                                User = u,
                                NumComments = 0,
                                Question = tq,
                                Title = GenText(5),
                                Content = GenText(RND.Next(0, 300)),
                                TagEntries = tags.OrderBy(t => RND.NextDouble()).Take(RND.Next(0, 5))
                                            .Select(t => new TagEntry() { Value = t.Value, Approved = t.Approved, TagId = t.Id }).ToArray()
                            };
                            service.SaveNewAnswer(answer);
                            int numVotes = RND.Next(0, users.Count);
                            for (int j = 0; j < numVotes; j++)
                            {
                                Config.Now = RndTime(RndDate());
                                if (RND.Next(0, 2) == 0)
                                    dp.VoteAnswer(answer.Id, users[j], 1);
                                else
                                    dp.VoteAnswer(answer.Id, users[j], -1);
                            }

                            // comments
                            for (int j = 0; j < RND.Next(0, NUM_COMMENTS); j++)
                            {
                                u = users[RND.Next(0, users.Count)];
                                Comment comment = new Comment()
                                {
                                    AnswerId = answer.Id,
                                    Content = GenText(30),
                                    User = u,
                                    CreatedOn = answer.CreatedOn.AddMinutes(RND.Next(0, 120)),
                                    NumLikes = 0
                                };
                                perc += 100.0 / (double)(NUM_ANSWERS * NUM_COMMENTS * NUM_DAYS);
                                service.SaveNewComment(comment);
                                Console.WriteLine(perc + "%");
                            }
                        }

                        // pick winning answer
                        if (date <= now) dp.PickWinningAnswers(date);
                    }
                }

                dp.SaveChanges();
                dp.OptimiseLucene();
            }
        }
    }
}
