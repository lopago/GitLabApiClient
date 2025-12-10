namespace GitLabApiClient.Models
{
    public enum Scope
    {
        All,
        CreatedByMe,
        AssignedToMe,
        ReviewsForMe //usable when requesting merge requests
    }
}
