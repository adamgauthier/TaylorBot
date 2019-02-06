'use strict';

const MessageWatcher = require('../structures/MessageWatcher.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');
const ArrayUtil = require('../modules/ArrayUtil.js');
const CommandError = require('../commands/CommandError.js');
const ArgumentParsingError = require('../types/ArgumentParsingError.js');
const MessageContext = require('../structures/MessageContext.js');
const CommandMessageContext = require('../commands/CommandMessageContext.js');

class CommandsWatcher extends MessageWatcher {
    async messageHandler(client, message) {
        const { author, channel } = message;
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
        const spaceMatches = text.match(/\s/);
        const commandName = spaceMatches ? text.substring(0, spaceMatches.index) : text;
        const cachedCommand = registry.commands.resolve(commandName.toLowerCase());

        if (!cachedCommand)
            return;

        const argString = text.substring(commandName.length);

        try {
            await CommandsWatcher.runCommand(messageContext, cachedCommand, argString);
            registry.commands.addSuccessfulUseCount(cachedCommand);
        } catch (e) {
            await client.sendEmbedError(channel, `${author} Oops, an unknown command error occured. Sorry about that. 😕`);
            registry.commands.addUnhandledErrorCount(cachedCommand);
            throw e;
        }
    }

    static async runCommand(messageContext, cachedCommand, argString) {
        const { client, message } = messageContext;
        const { registry } = client.master;

        for (const inhibitor of registry.inhibitors.values()) {
            if (await inhibitor.shouldBeBlocked(messageContext, cachedCommand, argString)) {
                return;
            }
        }

        const { author, channel } = message;

        Log.verbose(`${Format.user(author)} using '${cachedCommand.name}' with args '${argString}' in ${Format.channel(channel)}.`);

        const commandContext = new CommandMessageContext(messageContext, cachedCommand);
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
                `Command Format: \`${commandContext.usage()}\``,
                `Example: \`${commandContext.example()}\``,
                `For examples and details, use \`${commandContext.helpUsage()}\`.`
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
                    throw e;
                }
            }
        }

        registry.users.updateLastCommand(author, Date.now());

        try {
            await command.run(commandContext, parsedArgs);
        }
        catch (e) {
            if (e instanceof CommandError) {
                return client.sendEmbedError(channel, e.message);
            }
            else {
                throw e;
            }
        }
        finally {
            registry.users.updateLastAnswered(author, Date.now());
        }
    }
}

module.exports = CommandsWatcher;