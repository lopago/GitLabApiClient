using System.Threading.Tasks;
using Xunit;
using static GitLabApiClient.Test.Utilities.GitLabApiHelper;

namespace GitLabApiClient.Test
{
    [Trait("Category", "LinuxIntegration")]
    [Collection("GitLabContainerFixture")]
    public class FilesClientTest
    {
        private readonly FilesClient _sut = new FilesClient(GetFacade());

        [Fact]
        public async Task GetFile()
        {
            var file = await _sut.GetAsync(TestProjectId, "README.md");
            Assert.NotNull(file);
            Assert.Equal("base64", file.Encoding);
            Assert.Equal("master", file.Reference);
            Assert.Equal("README.md", file.Filename);
            Assert.Equal("README.md", file.FullPath);
            Assert.Equal("IyBUZXN0IHByb2plY3QKCkhlbGxvIHdvcmxkCg==", file.Content);
            Assert.Equal(28,file.Size);
            Assert.Equal("b6cb63af62daa14162368903ca4e42350cb1d855446febbdb22fb5c24f9aeedb", file.ContentSha256);
            Assert.Equal(40, file.BlobId.Length);
            Assert.Equal(40, file.CommitId.Length);
            Assert.Equal(40, file.LastCommitId.Length);
            Assert.Equal("# Test project\n\nHello world\n", file.ContentDecoded);
        }
    }
}
