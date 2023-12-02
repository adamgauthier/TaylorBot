namespace TaylorBot.Net.InstagramNotifier.Domain;

public class InstagramPost
{
    public string ShortCode { get; }
    public DateTimeOffset TakenAt { get; }
    public ulong LikesCount { get; }
    public ulong CommentsCount { get; }
    public string ThumbnailSrc { get; }
    public string? Caption { get; }
    public string AuthorFullName { get; }
    public string AuthorUsername { get; }
    public string AuthorProfilePicUrl { get; }

    public InstagramPost(string shortCode, DateTimeOffset takenAt, ulong likesCount, ulong commentsCount, string thumbnailSrc, string? caption, string authorFullName, string authorUsername, string authorProfilePicUrl)
    {
        ShortCode = shortCode;
        TakenAt = takenAt;
        LikesCount = likesCount;
        CommentsCount = commentsCount;
        ThumbnailSrc = thumbnailSrc;
        Caption = caption;
        AuthorFullName = authorFullName;
        AuthorUsername = authorUsername;
        AuthorProfilePicUrl = authorProfilePicUrl;
    }
}

public interface IInstagramClient
{
    ValueTask<InstagramPost> GetLatestPostAsync(string username);
}
