namespace TaylorBot.Net.InstagramNotifier.Domain;

public class InstagramPost(string shortCode, DateTimeOffset takenAt, ulong likesCount, ulong commentsCount, string thumbnailSrc, string? caption, string authorFullName, string authorUsername, string authorProfilePicUrl)
{
    public string ShortCode { get; } = shortCode;
    public DateTimeOffset TakenAt { get; } = takenAt;
    public ulong LikesCount { get; } = likesCount;
    public ulong CommentsCount { get; } = commentsCount;
    public string ThumbnailSrc { get; } = thumbnailSrc;
    public string? Caption { get; } = caption;
    public string AuthorFullName { get; } = authorFullName;
    public string AuthorUsername { get; } = authorUsername;
    public string AuthorProfilePicUrl { get; } = authorProfilePicUrl;
}

public interface IInstagramClient
{
    ValueTask<InstagramPost> GetLatestPostAsync(string username);
}
