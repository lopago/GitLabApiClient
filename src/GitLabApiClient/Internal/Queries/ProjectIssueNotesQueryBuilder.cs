using System;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Notes.Requests;

namespace GitLabApiClient.Internal.Queries
{
    internal class ProjectIssueNotesQueryBuilder : QueryBuilder<IssueNotesQueryOptions>
    {
        protected override void BuildCore(Query query, IssueNotesQueryOptions options)
        {
            if (options.SortOrder != SortOrder.Descending)
            {
                query.Add("sort", GetSortOrderQueryValue(options.SortOrder));
            }

            if (options.Order != NoteOrder.CreatedAt)
            {
                query.Add("order_by", GetNoteOrderQueryValue(options.Order));
            }

            if (options.ActivityFilter != NoteActivityFilter.AllNotes)
            {
                query.Add("activity_filter", GetNoteActivityQueryValue(options.ActivityFilter));
            }
        }

        private static string GetNoteActivityQueryValue(NoteActivityFilter activityFilter)
        {
            switch (activityFilter)
            {
                case NoteActivityFilter.AllNotes:
                    return "all_notes";
                case NoteActivityFilter.OnlyComments:
                    return "only_comments";
                case NoteActivityFilter.OnlyActivity:
                    return "only_activity";
                default:
                    throw new NotSupportedException($"{nameof(activityFilter)} = {activityFilter}");
            }
        }

        private static string GetNoteOrderQueryValue(NoteOrder order)
        {
            switch (order)
            {
                case NoteOrder.CreatedAt:
                    return "created_at";
                case NoteOrder.UpdatedAt:
                    return "updated_at";
                default:
                    throw new NotSupportedException($"Order {order} is not supported");
            }
        }
    }
}
