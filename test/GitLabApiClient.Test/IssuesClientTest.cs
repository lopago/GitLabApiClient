using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitLabApiClient.Internal.Queries;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Issues.Requests;
using GitLabApiClient.Models.Issues.Responses;
using GitLabApiClient.Models.Notes.Requests;
using GitLabApiClient.Models.Notes.Responses;
using Xunit;
using static GitLabApiClient.Test.Utilities.GitLabApiHelper;

namespace GitLabApiClient.Test
{
    [Trait("Category", "LinuxIntegration")]
    [Collection("GitLabContainerFixture")]
    public class IssuesClientTest
    {
        private readonly IssuesClient _sut = new IssuesClient(
            GetFacade(), new IssuesQueryBuilder(), new ProjectIssueNotesQueryBuilder());

        [Fact]
        public async Task CreatedIssueCanBeUpdated()
        {
            //arrange
            var createdIssue = await _sut.CreateAsync(TestProjectTextId, new CreateIssueRequest("Title1")
            {
                Assignees = new List<int> { 1 },
                Confidential = true,
                Description = "Description1",
                Labels = new[] { "Label1" },
                MilestoneId = 2,
                DiscussionToResolveId = 3,
                MergeRequestIdToResolveDiscussions = 4
            });

            //act
            var updatedIssue = await _sut.UpdateAsync(TestProjectTextId, createdIssue.Iid, new UpdateIssueRequest()
            {
                Assignees = new List<int> { 11 },
                Confidential = false,
                Description = "Description11",
                Labels = new[] { "Label11" },
                Title = "Title11",
                MilestoneId = 22
            });

            //assert
            Assert.Equal(TestProjectTextId, updatedIssue.ProjectId);
            Assert.False(updatedIssue.Confidential);
            Assert.Equal("Description11", updatedIssue.Description);
            Assert.Equal(new[] { "Label11" }, updatedIssue.Labels);
            Assert.Equal("Title11", updatedIssue.Title);
        }

        [Fact]
        public async Task CreatedIssueCanBeClosed()
        {
            //arrange
            var createdIssue = await _sut.CreateAsync(TestProjectTextId, new CreateIssueRequest("Title1"));

            //act
            var updatedIssue = await _sut.UpdateAsync(TestProjectTextId, createdIssue.Iid, new UpdateIssueRequest()
            {
                State = UpdatedIssueState.Close
            });

            //assert
            Assert.Equal(IssueState.Closed, updatedIssue.State);
        }

        [Fact]
        public async Task CreatedIssueCanBeListedFromProject()
        {
            //arrange
            string title = Guid.NewGuid().ToString();
            await _sut.CreateAsync(TestProjectTextId, new CreateIssueRequest(title));

            //act
            var listedIssues = await _sut.GetAllAsync(projectId: TestProjectTextId, options: o => o.Filter = title);

            //assert
            var i = listedIssues.Single();
            Assert.Equal(TestProjectTextId, i.ProjectId);
            Assert.Equal(title, i.Title);
            Assert.NotNull(i.TimeStats);
        }

        [Fact]
        public async Task CreatedIssueCanBeRetrieved()
        {
            //arrange
            string title = Guid.NewGuid().ToString();

            var issue = await _sut.CreateAsync(TestProjectTextId, new CreateIssueRequest(title)
            {
                Assignees = new List<int> { 1 },
                Confidential = true,
                Description = "Description",
                Labels = new List<string> { "Label1" }
            });

            //act
            var issueById = await _sut.GetAsync(TestProjectId, issue.Iid);
            var issueByProjectId = (await _sut.GetAllAsync(options: o => o.IssueIds = new[] { issue.Iid })).FirstOrDefault(i => i.Title == title);
            var ownedIssue = (await _sut.GetAllAsync(options: o => o.Scope = Scope.CreatedByMe)).FirstOrDefault(i => i.Title == title);

            //assert
            Assert.Equal(TestProjectTextId, issue.ProjectId);
            Assert.True(issue.Confidential);
            Assert.Equal(title, issue.Title);
            Assert.Equal("Description", issue.Description);
            Assert.Equal(new[] { "Label11" }, issue.Labels);

            Assert.EquivalentWithExclusions(issue, issueById, nameof(issue.UpdatedAt));
            Assert.EquivalentWithExclusions(issue, issueByProjectId, nameof(issue.UpdatedAt));
            Assert.EquivalentWithExclusions(issue, ownedIssue, nameof(issue.UpdatedAt));
        }

        [Fact]
        public async Task CreatedIssueNoteCanBeRetrieved()
        {
            //arrange
            string body = "comment1";
            var issue = await _sut.CreateAsync(TestProjectTextId, new CreateIssueRequest(Guid.NewGuid().ToString())
            {
                Description = "Description"
            });

            //act
            var note = await _sut.CreateNoteAsync(TestProjectTextId, issue.Iid, new CreateIssueNoteRequest
            {
                Body = body,
                CreatedAt = DateTime.Now
            });
            var issueNotes = (await _sut.GetNotesAsync(TestProjectId, issue.Iid)).FirstOrDefault(i => i.Body == body);
            var issueNote = await _sut.GetNoteAsync(TestProjectId, issue.Iid, note.Id);

            //assert
            Assert.Equal(body, note.Body);

            Assert.EquivalentWithExclusions(note, issueNotes, nameof(issue.UpdatedAt));
            Assert.EquivalentWithExclusions(note, issueNote, nameof(issue.UpdatedAt));
        }

        [Fact]
        public async Task CreatedIssueNoteCanBeUpdated()
        {
            //arrange
            var createdIssue = await _sut.CreateAsync(TestProjectTextId, new CreateIssueRequest(Guid.NewGuid().ToString())
            {
                Description = "Description1"
            });
            var createdIssueNote = await _sut.CreateNoteAsync(TestProjectTextId, createdIssue.Iid, new CreateIssueNoteRequest("comment2"));

            //act
            var updatedIssueNote = await _sut.UpdateNoteAsync(TestProjectTextId, createdIssue.Iid, createdIssueNote.Id, new UpdateIssueNoteRequest("comment22"));

            //assert
            Assert.Equal("comment22",updatedIssueNote.Body);
        }

        [Fact]
        public async Task CreateIssueWithTasks()
        {
            //arrange
            string title = Guid.NewGuid().ToString();
            await _sut.CreateAsync(TestProjectTextId, new CreateIssueRequest(title)
            {
                Description = @"Description1
- [ ] Task 1
- [ ] Task 2
- [x] Task 3
"
            });

            //act
            var listedIssues = await _sut.GetAllAsync(projectId: TestProjectTextId, options: o => o.Filter = title);

            //assert
            var i = listedIssues.Single();
            Assert.Equal(TestProjectTextId, i.ProjectId);
            Assert.Equal(title, i.Title);
            Assert.NotNull(i.TaskCompletionStatus);
            Assert.Equal(3, i.TaskCompletionStatus.Count);
            Assert.Equal(1, i.TaskCompletionStatus.Completed);
            Assert.NotNull(i.TimeStats);
        }
    }
}
