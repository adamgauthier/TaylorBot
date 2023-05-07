﻿using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers;

public record ParsedAttachment(Interaction.Attachment Value);

public class AttachmentParser : IOptionParser<ParsedAttachment>
{
    private readonly OptionalAttachmentParser _optionalAttachmentParser;

    public AttachmentParser(OptionalAttachmentParser optionalAttachmentParser)
    {
        _optionalAttachmentParser = optionalAttachmentParser;
    }

    public async ValueTask<Result<ParsedAttachment, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedAttachment = await _optionalAttachmentParser.ParseAsync(context, optionValue, resolved);
        if (parsedAttachment)
        {
            if (parsedAttachment.Value.Value == null)
            {
                return Error(new ParsingFailed("Attachment option is required."));
            }
            else
            {
                return new ParsedAttachment(parsedAttachment.Value.Value);
            }
        }
        else
        {
            return Error(parsedAttachment.Error);
        }
    }
}

public record ParsedOptionalAttachment(Interaction.Attachment? Value);

public class OptionalAttachmentParser : IOptionParser<ParsedOptionalAttachment>
{
    public ValueTask<Result<ParsedOptionalAttachment, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (!optionValue.HasValue)
        {
            return new(new ParsedOptionalAttachment(null));
        }

        if (resolved?.attachments == null)
        {
            return new(Error(new ParsingFailed("No attachment uploaded.")));
        }

        var attachmentId = optionValue.Value.GetString()!;

        if (!resolved.attachments.TryGetValue(attachmentId, out var attachment))
        {
            return new(Error(new ParsingFailed("Attachment not found.")));
        }

        return new(new ParsedOptionalAttachment(attachment));
    }
}