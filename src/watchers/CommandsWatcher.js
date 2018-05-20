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
    constructor() {
        super(true);
    }

    async messageHandler(client, message) {
        const { author } = message;
        if (author.bot)
            return;

        const { oldRegistry } = client;
        const { channel } = message;
        let text = message.content;
        if (channel.type === 'text') {
            const { guild } = message;

            const { prefix } = oldRegistry.guilds.get(guild.id);

            if (text.startsWith(prefix)) {
                text = text.substring(prefix.length);
            }
            else {
                return;
            }
        }

        const args = text.split(' ');
        const commandName = args.shift().toLowerCase();
        const cachedCommand = oldRegistry.commands.resolve(commandName);

        if (!cachedCommand)
            return;

        const argString = args.join(' ');

        for (const inhibitor of oldRegistry.inhibitors.values()) {
            if (inhibitor.shouldBeBlocked(message, cachedCommand)) {
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

        const regexString = command.info.args
            .map(argInfo => {
                return argInfo.quoted ? `(?:"(.*)"|'(.*)')` : '(.*)';
            })
            .join(`[\\${command.info.separator}]{0,1}`);

        const regex = new RegExp(`^${regexString}$`);

        const matches = argString.trim().match(regex);

        if (!matches) {
            // SEND ERROR MESSAGE
            return client.sendEmbed(channel, EmbedUtil.error('Wrong command usage'));
        }

        const matchedGroups = matches.slice(1);

        const parsedArgs = {};

        for (const [match, argInfo] of ArrayUtil.iterateArrays(matchedGroups, command.info.args)) {
            const type = oldRegistry.types.getType(argInfo.type);
            if (type.isEmpty(match, message, argInfo)) {
                return client.sendEmbed(channel, EmbedUtil.error(`\`<${argInfo.label}>\` must not be empty.`));
            }

            try {
                const parsedArg = await type.parse(match, message, argInfo);

                parsedArgs[argInfo.key] = parsedArg;
            }
            catch (e) {
                // UPDATE ANSWERED?

                if (e instanceof ArgumentParsingError) {
                    return client.sendEmbed(channel, EmbedUtil.error(`\`<${argInfo.label}>\`: ${e.message}`));
                }
                else {
                    await client.sendEmbed(channel, EmbedUtil.error('Oops, an unknown parsing error occured. Sorry about that. 😕'));
                    throw e;
                }
            }
        }

        const commandTime = new Date().getTime();
        oldRegistry.users.updateLastCommand(author, commandTime);

        try {
            await command.run({ client, message }, parsedArgs);
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
            oldRegistry.users.updateLastAnswered(author, answeredTime);
        }
    }
}

module.exports = CommandsWatcher;