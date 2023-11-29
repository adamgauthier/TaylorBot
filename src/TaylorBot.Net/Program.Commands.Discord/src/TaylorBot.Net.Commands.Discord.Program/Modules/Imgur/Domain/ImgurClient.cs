namespace TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Domain;

public interface IImgurResult { }
public record UploadSuccess(string Url) : IImgurResult
{
    public record Pod(string PlainText, string ImageUrl);
};
public record GenericImgurError() : IImgurResult;
public record FileTypeInvalid() : IImgurResult;

public interface ImgurClient
{
    ValueTask<IImgurResult> UploadAsync(string url);
}
