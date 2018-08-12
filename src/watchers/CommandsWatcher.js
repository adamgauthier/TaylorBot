'use strict';

const { Paths } = require('globalobjects');

const MessageWatcher = require('../structures/MessageWatcher.js');
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);
const ArrayUtil = require(Paths.ArrayUtil);
const CommandError = require('../structures/CommandError.js');
const ArgumentParsingError = require(Paths.ArgumentParsingError);
const MessageContext = require(Paths.MessageContext);
const CommandMessageContext = require(Paths.CommandMessageContext);

class CommandsWatcher extends MessageWatcher {
    async messageHandler(client, message) {
        const { author } = message;
        if (author.bot)
            return;

        const messageContext = new MessageContext(message, client);

        let text = message.content;
        if (messageContext.isGuild) {
            const { prefix } = messageContext.guildSettings;

            if (text.startsWith(prefix)) {
                text = text.substring(prefix.length);
            }
            else {
                const { id } = client.user;
                const matches =
                    text.match(new RegExp(`^<@${id}> (.+)$`)) ||
                    text.match(new RegExp(`^(.+) <@${id}>$`));

                if (matches) {
                    text = `cleverbot ${matches.slice(1)[0]}`;
                }
                else {
                    return;
                }
            }
        }

        const { registry } = client.master;

        const commandName = text.split(' ')[0].toLowerCase();
        const cachedCommand = registry.commands.resolve(commandName);

        if (!cachedCommand)
            return;

        for (const inhibitor of registry.inhibitors.values()) {
            if (inhibitor.shouldBeBlocked(messageContext, cachedCommand)) {
                return;
            }
        }

        const { channel } = message;
        const commandContext = new CommandMessageContext(messageContext, cachedCommand);

        const argString = text.substring(commandName.length);
        Log.verbose(
            `${Format.user(author)} using '${cachedCommand.name}' with args '${argString}' in ${Format.channel(channel)}.`
        );

        const { command } = cachedCommand;

        const regexString =
            commandContext.args.reduceRight((acc, { mustBeQuoted, includesSpaces, includesNewLines, canBeEmpty }) => {
                const quantifier = canBeEmpty ? '*' : '+';
                const separator = canBeEmpty ? '[\\ ]{0,1}' : ' ';

                const invalidCharacters = [];
                if (!includesSpaces) {
                    invalidCharacters.push(' ');
                }
                if (!includesNewLines) {
                    invalidCharacters.push('\\r');
                    invalidCharacters.push('\\n');
                }

                const matching = invalidChars => `([^${invalidChars.join('')}]${quantifier})`;
                const matchingQuoted = invalidChars => [`"${matching(['"', ...invalidChars])}"`, `'${matching(["'", ...invalidChars])}'`];

                const groups = [];

                if (mustBeQuoted) {
                    groups.push(...matchingQuoted(invalidCharacters));
                }
                else {
                    groups.push(matching(invalidCharacters));

                    if (includesSpaces) {
                        groups.push(...matchingQuoted(invalidCharacters));
                    }
                }

                const group = `(?:${groups.join('|')})`;

                return `${separator}${group}${acc}`;
            }, '');

        const regex = new RegExp(`^${regexString}$`);

        const matches = argString.match(regex);

        if (!matches) {
            return client.sendEmbedError(channel, [
                'Oops! Looks like something was off with your command usage. 🤔',
                `Command Format: \`${commandContext.usage()}\``
            ].join('\n'));
        }

        const matchedGroups = matches.slice(1).filter(m => m !== undefined);

        const parsedArgs = {};

        for (const [match, { info, type }] of ArrayUtil.iterateArrays(matchedGroups, commandContext.args)) {
            if (match === '') {
                parsedArgs[info.key] = type.default(commandContext, info);
                continue;
            }

            try {
                const parsedArg = await type.parse(match, commandContext, info);

                parsedArgs[info.key] = parsedArg;
            }
            catch (e) {
                if (e instanceof ArgumentParsingError) {
                    return client.sendEmbedError(channel, [
                        `Command Format: \`${commandContext.usage()}\``,
                        `\`<${info.label}>\`: ${e.message}`
                    ].join('\n'));
                }
                else {
                    await client.sendEmbedError(channel, 'Oops, an unknown parsing error occured. Sorry about that. 😕');
                    throw e;
                }
            }
        }

        const commandTime = Date.now();
        registry.users.updateLastCommand(author, commandTime);

        try {
            await command.run(commandContext, parsedArgs);
        }
        catch (e) {
            if (e instanceof CommandError) {
                return client.sendEmbedError(channel, e.message);
            }
            else {
                await client.sendEmbedError(channel, 'Oops, an unknown command error occured. Sorry about that. 😕');
                throw e;
            }
        }
        finally {
            const answeredTime = Date.now();
            registry.users.updateLastAnswered(author, answeredTime);
        }
    }
}

module.exports = CommandsWatcher;