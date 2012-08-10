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
        private static readonly string LOREM = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas libero tellus, sodales eget hendrerit vel, pretium ultricies est. Vivamus magna diam, cursus non pulvinar non, euismod in turpis. Praesent facilisis ultrices dignissim. Donec condimentum porttitor iaculis. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam dui quam, volutpat sit amet adipiscing nec, rhoncus a purus. Aenean purus odio, fringilla non pellentesque sed, consequat sit amet orci.

Praesent sit amet imperdiet turpis. Mauris aliquet lacinia libero rhoncus mattis. In hac habitasse platea dictumst. Fusce interdum, eros a egestas dignissim, tortor nisi convallis neque, sed lobortis elit ligula in sem. Morbi vitae quam nisl. Etiam bibendum, ante et volutpat imperdiet, dui augue rhoncus mi, lacinia euismod leo sapien eu nisl. Nulla ornare facilisis nisl, a blandit magna semper vitae. Etiam semper consectetur arcu ut hendrerit. Nunc mollis velit vitae sem molestie non volutpat ligula fringilla.

Etiam lacus ipsum, posuere eu rutrum at, dignissim vitae nisl. Phasellus euismod fermentum metus, vel luctus nulla ultrices a. Aliquam vel dui in quam lacinia pulvinar ut et enim. Nunc velit massa, ultrices et sagittis quis, viverra dignissim risus. Quisque sed tellus non purus bibendum dapibus. Sed auctor odio non mi vehicula non molestie metus aliquet. Nulla facilisi. Sed vel justo vitae mauris dictum suscipit at sodales nunc. Etiam eget urna lectus. Nunc tempor elit ut sem pellentesque in condimentum sapien malesuada. Curabitur fringilla venenatis lacus commodo porta. Aenean ut rhoncus erat. Quisque rhoncus blandit metus volutpat dapibus. Donec id eros vitae dolor dignissim venenatis rhoncus ac dolor. Morbi varius ultrices augue in faucibus. Donec lacus felis, luctus vitae egestas at, auctor sit amet nunc.

Nunc consequat turpis vulputate risus commodo pulvinar. Quisque ut eros dui, non pharetra libero. Etiam interdum gravida nibh sed ornare. Nunc sed interdum nunc. Pellentesque eleifend ligula ac lacus suscipit id pulvinar magna lacinia. Maecenas lorem neque, auctor non venenatis eget, facilisis ut dolor. Quisque vel dui nulla, mattis blandit purus. Nulla mattis, ligula eget scelerisque suscipit, tortor felis faucibus nunc, eu accumsan tortor ante et magna. Nam eu neque quam, non tincidunt augue. Nulla libero elit, blandit sit amet cursus vel, placerat in eros. Vivamus pulvinar leo a metus scelerisque eget molestie erat viverra. Nam ac eros nibh, quis malesuada elit. Nunc id leo non diam ornare mollis.

Nunc enim justo, scelerisque in adipiscing non, ornare et nisl. Nam sodales dapibus nunc, vel accumsan lacus porttitor sed. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras ultricies, mauris euismod viverra tristique, orci dui luctus urna, sed blandit quam nunc eu velit. Mauris eu enim id metus imperdiet ornare sed at dui. Praesent consequat vestibulum turpis vitae ultricies. Mauris lobortis lacus quis augue sodales vehicula. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus nec quam sodales turpis mollis vulputate id venenatis eros. Suspendisse in dolor sem. Integer in aliquet velit.";
#endregion
        private static readonly string[] LOREM_WORDS = LOREM.Split(' ');
        private static readonly string DEFAULT_USER_EMAIL = @"leemcsorley@gmail.com";
        private static readonly Random RND = new Random(
            Environment.TickCount);

        private const int NUM_DAYS = 4;
        private const int NUM_QUESTIONS = 30;
        private const int NUM_ANSWERS = 30;
        private const int NUM_COMMENTS = 10;

        private static string GenText(int words)
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
                        int minutes = RND.Next(0, 60 * 24);
                        User user = users[RND.Next(0, users.Count)];
                        Question q = new Question()
                        {
                            User = user,
                            CreatedOn = date.AddMinutes(-minutes),
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
                                CreatedOn = date.AddMinutes(-RND.Next(0, 60 * 24)),
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
            }
        }
    }
}
