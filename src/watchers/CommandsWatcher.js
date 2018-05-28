'use strict';

const { Paths } = require('globalobjects');

const MessageWatcher = require(Paths.MessageWatcher);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);
const ArrayUtil = require(Paths.ArrayUtil);
const CommandError = require(Paths.CommandError);
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
                return;
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
            `${Format.user(author)} using '${cachedCommand.name}' with args '${argString}' in ${
                channel.type === 'dm' ?
                    Format.dmChannel(channel) :
                    Format.guildChannel(channel, '#name (#id) on #gName (#gId)')
            }.`
        );

        const { command } = cachedCommand;

        const regexString =
            commandContext.args.reduceRight((acc, { type, canBeEmpty }) => {
                const quantifier = canBeEmpty ? '*' : '+';
                const separator = canBeEmpty ? '[\\ ]{0,1}' : ' ';

                let invalidCharacters = '';
                if (!type.includesSpaces) {
                    invalidCharacters += ` '"`;
                }
                if (!type.includesNewLines) {
                    invalidCharacters += '\\r\\n';
                }

                const matching = `([^${invalidCharacters}]${quantifier})`;
                const group = type.includesSpaces ? `(?:"${matching}"|'${matching}'|${matching})` : matching;

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

        const matchedGroups = matches.slice(1);

        const parsedArgs = {};

        for (const [match, { info, type, canBeEmpty }] of ArrayUtil.iterateArrays(matchedGroups, commandContext.args)) {
            if (!canBeEmpty && type.isEmpty(match, commandContext, info)) {
                return client.sendEmbedError(channel, `\`<${info.label}>\` must not be empty.`);
            }

            try {
                const parsedArg = await type.parse(match, commandContext, info);

                parsedArgs[info.key] = parsedArg;
            }
            catch (e) {
                // UPDATE ANSWERED?

                if (e instanceof ArgumentParsingError) {
                    return client.sendEmbedError(channel, `\`<${info.label}>\`: ${e.message}`);
                }
                else {
                    await client.sendEmbedError(channel, 'Oops, an unknown parsing error occured. Sorry about that. 😕');
                    throw e;
                }
            }
        }

        const commandTime = new Date().getTime();
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
            const answeredTime = new Date().getTime();
            registry.users.updateLastAnswered(author, answeredTime);
        }
    }
}

module.exports = CommandsWatcher;