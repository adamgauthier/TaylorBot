'use strict';

const Commando = require('discord.js-commando');
const { GlobalPaths } = require('globalobjects');

const ArgumentInfos = require(GlobalPaths.ArgumentInfos);
const UserGroups = require(GlobalPaths.UserGroups);
const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);

class UserInfoCommand extends Commando.Command {
	constructor(client) {
		super(client, {
			name: 'userinfo',
			aliases: ['uinfo'],
			group: 'util',
			memberName: 'userinfo',
			description: 'Gets information about a user.',
			examples: ['uinfo @Enchanted13#1989', 'uinfo Enchanted13'],
			guildOnly: true,
			argsPromptLimit: 0,

			args: [
				{
					key: 'member',
					label: 'user',
					...ArgumentInfos.MemberOrAuthor
				}
			]
		});
	}

	run(message, { member }) {
		return this.client.sendEmbed(message.channel, DiscordEmbedFormatter.member(member));
	}
};

module.exports = UserInfoCommand;