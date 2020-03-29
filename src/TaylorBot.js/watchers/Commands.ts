import { MessageWatcher } from '../structures/MessageWatcher';
import Log = require('../tools/Logger.js');
import Format = require('../modules/DiscordFormatter.js');
import ArrayUtil = require('../modules/ArrayUtil.js');
import CommandError = require('../commands/CommandError.js');
import ArgumentParsingError = require('../types/ArgumentParsingError.js');
import MessageContext = require('../structures/MessageContext.js');
import CommandMessageContext = require('../commands/CommandMessageContext.js');
import { Message } from 'discord.js';
import { TaylorBotClient } from '../client/TaylorBotClient';
import { CachedCommand } from '../client/registry/CachedCommand';

class CommandsWatcher extends MessageWatcher {
    async messageHandler(client: TaylorBotClient, message: Message): Promise<void> {
        const { author, channel } = message;
        if (client.user === null || author === null || author.bot)
            return;

        const messageContext = new MessageContext(message, client);

        if (messageContext.isGuild) {
            const prefix = await client.master.registry.guilds.getPrefix(message.guild);
            messageContext.prefix = prefix;
        }

        let text = message.content;
        const mentionPrefix = `<@${client.user.id}> `;
        const mentionGuildPrefix = `<@!${client.user.id}> `;

        if (text.startsWith(messageContext.prefix)) {
            text = text.substring(messageContext.prefix.length);
        }
        else if (text.startsWith(mentionPrefix)) {
            text = text.substring(mentionPrefix.length);
        }
        else if (text.startsWith(mentionGuildPrefix)) {
            text = text.substring(mentionGuildPrefix.length);
        }
        else {
            return;
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
        }
        catch (e) {
            await client.sendEmbedError(channel, `${author} Oops, an unknown command error occurred. Sorry about that. 😕`);
            registry.commands.addUnhandledErrorCount(cachedCommand);
            throw e;
        }
        finally {
            if (messageContext.wasOnGoingCommandAdded)
                await registry.onGoingCommands.removeOngoingCommandAsync(author);
        }
    }

    static async runCommand(messageContext: MessageContext, cachedCommand: CachedCommand, argString: string): Promise<void> {
        const { client, message }: { client: TaylorBotClient; message: Message } = messageContext;
        const { author, channel } = message;
        const { registry } = client.master;

        for (const inhibitor of registry.inhibitors.getSilentInhibitors()) {
            const logMessage = await inhibitor.shouldBeBlocked(messageContext, cachedCommand, argString);
            if (logMessage !== null) {
                Log.warn(
                    `${Format.user(author)} can't use '${cachedCommand.name}' with args '${argString}' in ${Format.channel(channel)} (silent): ${logMessage}`
                );
                return;
            }
        }

        for (const inhibitor of registry.inhibitors.getNoisyInhibitors()) {
            const blockedMessage = await inhibitor.getBlockedMessage(messageContext, cachedCommand, argString);
            if (blockedMessage !== null) {
                Log.warn(
                    `${Format.user(author)} can't use '${cachedCommand.name}' with args '${argString}' in ${Format.channel(channel)}: ${blockedMessage.log}`
                );
                await client.sendEmbedError(channel, [
                    `${author} Oops! \`${cachedCommand.name}\` was blocked. ⛔`,
                    blockedMessage.ui
                ].join('\n'));
                return;
            }
        }

        Log.verbose(`${Format.user(author)} using '${cachedCommand.name}' with args '${argString}' in ${Format.channel(channel)}.`);

        const commandContext = new CommandMessageContext(messageContext, cachedCommand);
        const { command } = cachedCommand;

        const regexString =
            commandContext.args.reduceRight((
                acc: string,
                { mustBeQuoted, includesSpaces, includesNewLines, canBeEmpty }: { mustBeQuoted: boolean; includesSpaces: boolean; includesNewLines: boolean; canBeEmpty: boolean }
            ) => {
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

                const matching = (invalidChars: string[]): string => `([^${invalidChars.join('')}]${quantifier})`;
                const matchingQuoted = (invalidChars: string[]): string[] => [`"${matching(['"', ...invalidChars])}"`, `'${matching(["'", ...invalidChars])}'`];

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
            await client.sendEmbedError(channel, [
                `${author} Oops! Looks like something was off with your command usage. 🤔`,
                `Command Format: \`${commandContext.usage()}\``,
                `Example: \`${commandContext.example()}\``,
                `For examples and details, use \`${commandContext.helpUsage()}\`.`
            ].join('\n'));
            return;
        }

        const matchedGroups = matches.slice(1).filter(m => m !== undefined);

        const parsedArgs: Record<string, any> = {};

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
                    await client.sendEmbedError(channel, [
                        `${author} Format: \`${commandContext.usage()}\``,
                        `\`<${info.label}>\`: ${e.message}`
                    ].join('\n'));
                    return;
                }
                else {
                    throw e;
                }
            }
        }

        try {
            await command.run(commandContext, parsedArgs);
        }
        catch (e) {
            if (e instanceof CommandError) {
                await client.sendEmbedError(channel, e.message);
                return;
            }
            else {
                throw e;
            }
        }
        finally {
            if (cachedCommand.command.maxDailyUseCount !== null) {
                await registry.cooldowns.addDailyUseCount(author, cachedCommand);
            }
        }
    }
}

export = CommandsWatcher;
