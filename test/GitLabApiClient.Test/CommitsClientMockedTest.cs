using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using GitLabApiClient.Internal.Http;
using GitLabApiClient.Internal.Http.Serialization;
using GitLabApiClient.Internal.Queries;
using GitLabApiClient.Test.TestUtilities;
using Xunit;

namespace GitLabApiClient.Test
{
    [ExcludeFromCodeCoverage]
    public class CommitsClientMockedTest
    {
        [Fact]
        public async Task GetCommitBySha()
        {
            string gitlabServer = "http://fake-gitlab.com/";
            string projectId = "id";
            string sha = "6104942438c14ec7bd21c6cd5bd995272b3faff6";
            string url = $"/projects/{projectId}/repository/commits/{sha}";

            var handler = A.Fake<MockHandler>(opt => opt.CallsBaseMethods());
            A.CallTo(() => handler.SendAsync(HttpMethod.Get, url))
                .ReturnsLazily(() => HttpResponseMessageProducer.Success(
                    $"{{\"id\": \"{sha}\", }}"));
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(gitlabServer) })
            {
                var gitlabHttpFacade = new GitLabHttpFacade(new RequestsJsonSerializer(), client);
                var commitsClient = new CommitsClient(gitlabHttpFacade, new CommitQueryBuilder(), new CommitRefsQueryBuilder(), new CommitStatusesQueryBuilder());

                var commitFromClient = await commitsClient.GetAsync(projectId, sha);
                Assert.Equivalent(sha, commitFromClient.Id);
            }
        }

        [Fact]
        public async Task GetCommitsByRefName()
        {
            string gitlabServer = "http://fake-gitlab.com/";
            string projectId = "id";
            string refName = "6104942438c14ec7bd21c6cd5bd995272b3faff6";
            string url = $"/projects/id/repository/commits?ref_name={refName}&per_page=100&page=1";

            var handler = A.Fake<MockHandler>(opt => opt.CallsBaseMethods());
            A.CallTo(() => handler.SendAsync(HttpMethod.Get, url))
                .ReturnsLazily(() => HttpResponseMessageProducer.Success(
                    $"[  {{ \"id\": \"id1\",}},\n  {{\"id\": \"id2\",}}]"));
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(gitlabServer) })
            {
                var gitlabHttpFacade = new GitLabHttpFacade(new RequestsJsonSerializer(), client);
                var commitsClient = new CommitsClient(gitlabHttpFacade, new CommitQueryBuilder(), new CommitRefsQueryBuilder(), new CommitStatusesQueryBuilder());

                var commitsFromClient = await commitsClient.GetAsync(projectId, o => o.RefName = refName);
                Assert.Equivalent("id1", commitsFromClient[0].Id);
                Assert.Equivalent("id2", commitsFromClient[1].Id);

            }

        }

        [Fact]
        public async Task GetDiffsForCommit()
        {
            string gitlabServer = "http://fake-gitlab.com/";
            string projectId = "id";
            string sha = "6104942438c14ec7bd21c6cd5bd995272b3faff6";
            string url = $"/projects/id/repository/commits/{sha}/diff?per_page=100&page=1";

            var handler = A.Fake<MockHandler>(opt => opt.CallsBaseMethods());
            A.CallTo(() => handler.SendAsync(HttpMethod.Get, url))
                .ReturnsLazily(() => HttpResponseMessageProducer.Success(
                    $"[  {{ \"diff\": \"diff1\", \"new_path\": \"new_path1\", \"old_path\": \"old_path1\", \"a_mode\": \"a_mode1\", \"b_mode\": \"b_mode1\", \"new_file\": \"true\", \"renamed_file\": \"false\", \"deleted_file\": \"false\" }},\n  {{\"diff\": \"diff2\", \"new_path\": \"new_path2\", \"old_path\": \"old_path2\", \"a_mode\": \"a_mode2\", \"b_mode\": \"b_mode2\", \"new_file\": \"false\", \"renamed_file\": \"true\", \"deleted_file\": \"true\"}}]"));
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(gitlabServer) })
            {
                var gitlabHttpFacade = new GitLabHttpFacade(new RequestsJsonSerializer(), client);
                var commitsClient = new CommitsClient(gitlabHttpFacade, new CommitQueryBuilder(), new CommitRefsQueryBuilder(), new CommitStatusesQueryBuilder());

                var diffsFromClient = await commitsClient.GetDiffsAsync(projectId, sha);
                Assert.Equivalent("diff1", diffsFromClient[0].DiffText);
                Assert.Equivalent("new_path1", diffsFromClient[0].NewPath);
                Assert.Equivalent("old_path1", diffsFromClient[0].OldPath);
                Assert.Equivalent("a_mode1", diffsFromClient[0].AMode);
                Assert.Equivalent("b_mode1", diffsFromClient[0].BMode);
                Assert.True(diffsFromClient[0].IsNewFile);
                Assert.False(diffsFromClient[0].IsRenamedFile);
                Assert.False(diffsFromClient[0].IsDeletedFile);

                Assert.Equivalent("diff2", diffsFromClient[1].DiffText);
                Assert.Equivalent("new_path2", diffsFromClient[1].NewPath);
                Assert.Equivalent("old_path2", diffsFromClient[1].OldPath);
                Assert.Equivalent("a_mode2", diffsFromClient[1].AMode);
                Assert.Equivalent("b_mode2", diffsFromClient[1].BMode);
                Assert.False(diffsFromClient[1].IsNewFile);
                Assert.True(diffsFromClient[1].IsRenamedFile);
                Assert.True(diffsFromClient[1].IsDeletedFile);

            }
        }

        [Fact]
        public async Task GetStatusesForCommit()
        {
            string gitlabServer = "http://fake-gitlab.com/";
            string projectId = "id";
            string sha = "6104942438c14ec7bd21c6cd5bd995272b3faff6";
            string Name = "name1";
            string url = $"/projects/id/repository/commits/{sha}/statuses?name={Name}&per_page=100&page=1";

            var handler = A.Fake<MockHandler>(opt => opt.CallsBaseMethods());
            A.CallTo(() => handler.SendAsync(HttpMethod.Get, url))
                .ReturnsLazily(() => HttpResponseMessageProducer.Success(
                    $"[  {{\"id\":1,\"sha\":\"{sha}\",\"ref \":\"\",\"status\":\"success\",\"name\":\"name1\",\"target_url\":\"target_url1\",\"description\":\"success\",\"created_at\":\"2020-04-08T11:57:49.136+05:30\",\"started_at\":\"2020-04-08T11:58:00.362+05:30\",\"finished_at\":\"2020-04-08T11:58:06.121+05:30\",\"allow_failure\":false,\"coverage\":null,\"author\":{{\"id\":1,\"name\":\"name\",\"username\":\"username\",\"state\":\"active\",\"avatar_url\":\"avatar_url1\",\"web_url\":\"web_url1\"}} }},{{\"id\":2,\"sha\":\"{sha}\",\"ref \":\"\",\"status\":\"success\",\"name\":\"name2\",\"target_url\":\"target_url2\",\"description\":\"success\",\"created_at\":\"2020-04-08T11:57:49.136+05:30\",\"started_at\":\"2020-04-08T11:58:00.362+05:30\",\"finished_at\":\"2020-04-08T11:58:06.121+05:30\",\"allow_failure\":false,\"coverage\":null,\"author\":{{\"id\":2,\"name\":\"name2\",\"username\":\"username2\",\"state\":\"activ2\",\"avatar_url2\":\"avatar_url2\",\"web_url\":\"web_url2\"}} }}]"));
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(gitlabServer) })
            {
                var gitlabHttpFacade = new GitLabHttpFacade(new RequestsJsonSerializer(), client);
                var commitsClient = new CommitsClient(gitlabHttpFacade, new CommitQueryBuilder(), new CommitRefsQueryBuilder(), new CommitStatusesQueryBuilder());

                var statusesFromClient = await commitsClient.GetStatusesAsync(projectId, sha, o => o.Name = Name);
                Assert.Equivalent("success", statusesFromClient[0].Status);
                Assert.Equivalent("name1", statusesFromClient[0].Name);
                Assert.Equivalent("target_url1", statusesFromClient[0].TargetUrl);
                Assert.Equivalent("1", statusesFromClient[0].Id);

                Assert.Equivalent("success", statusesFromClient[1].Status);
                Assert.Equivalent("name2", statusesFromClient[1].Name);
                Assert.Equivalent("target_url2", statusesFromClient[1].TargetUrl);
                Assert.Equivalent("2", statusesFromClient[1].Id);

            }
        }
    }
}
