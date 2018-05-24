'use strict';

const { Paths } = require('globalobjects');

const MessageWatcher = require(Paths.MessageWatcher);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);
const ArrayUtil = require(Paths.ArrayUtil);
const EmbedUtil = require(Paths.EmbedUtil);
const CommandError = require(Paths.CommandError);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class CommandsWatcher extends MessageWatcher {
    async messageHandler(client, message) {
        const { author } = message;
        if (author.bot)
            return;

        const { registry } = client.master;
        const { channel } = message;
        let text = message.content;
        if (channel.type === 'text') {
            const { guild } = message;

            const { prefix } = registry.guilds.get(guild.id);

            if (text.startsWith(prefix)) {
                text = text.substring(prefix.length);
            }
            else {
                return;
            }
        }

        const commandName = text.split(' ')[0].toLowerCase();
        const cachedCommand = registry.commands.resolve(commandName);

        if (!cachedCommand)
            return;

        const commandContext = { message, client };

        const argString = text.substring(commandName.length);

        for (const inhibitor of registry.inhibitors.values()) {
            if (inhibitor.shouldBeBlocked(commandContext, cachedCommand)) {
                return;
            }
        }

        Log.verbose(
            `${Format.user(author)} using '${cachedCommand.name}' with args '${argString}' in ${
                channel.type === 'dm' ?
                    Format.dmChannel(channel) :
                    Format.guildChannel(channel, '#name (#id) on #gName (#gId)')
            }.`
        );

        const { command } = cachedCommand;

        const args = command.args.map(arg => {
            const type = registry.types.getType(arg.type);
            const canBeEmpty = type.canBeEmpty(commandContext, arg);

            return {
                arg,
                type,
                canBeEmpty
            };
        });

        const regexString = 
            args.reduceRight((acc, { type, canBeEmpty }) => {
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
            return client.sendEmbed(channel,
                EmbedUtil.error(`Oops! Looks like something was off with your command usage. 🤔 \nCommand Format: \`${
                    [command.name, ...args.map(({ arg, canBeEmpty }) => `<${arg.label}${canBeEmpty ? '?': ''}>`)].join(' ')
                }\``));
        }

        const matchedGroups = matches.slice(1);

        const parsedArgs = {};

        for (const [match, { arg, type, canBeEmpty }] of ArrayUtil.iterateArrays(matchedGroups, args)) {
            if (!canBeEmpty && type.isEmpty(match, message, arg)) {
                return client.sendEmbed(channel, EmbedUtil.error(`\`<${arg.label}>\` must not be empty.`));
            }

            try {
                const parsedArg = await type.parse(match, message, arg);

                parsedArgs[arg.key] = parsedArg;
            }
            catch (e) {
                // UPDATE ANSWERED?

                if (e instanceof ArgumentParsingError) {
                    return client.sendEmbed(channel, EmbedUtil.error(`\`<${arg.label}>\`: ${e.message}`));
                }
                else {
                    await client.sendEmbed(channel, EmbedUtil.error('Oops, an unknown parsing error occured. Sorry about that. 😕'));
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
                return client.sendEmbed(channel, EmbedUtil.error(e.message));
            }
            else {
                await client.sendEmbed(channel, EmbedUtil.error('Oops, an unknown command error occured. Sorry about that. 😕'));
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