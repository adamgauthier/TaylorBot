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
			minimumGroup: UserGroups.Everyone,

			args: [
				{
					key: 'member',
					label: 'user',
					...ArgumentInfos.MemberOrAuthor
				}
			]
		});
	}

	async run(message, { member }) {
		const { user } = member;
		return this.client.sendEmbed(message.channel, DiscordEmbedFormatter.member(member));
		// return msg.reply(stripIndents`
		// 	Info on **${user.username}#${user.discriminator}** (ID: ${user.id})

		// 	**❯ Member Details**
		// 	${member.nickname !== null ? ` • Nickname: ${member.nickname}` : ' • No nickname'}
		// 	 • Roles: ${member.roles.map(roles => `\`${roles.name}\``).join(', ')}
		// 	 • Joined at: ${member.joinedAt}

		// 	**❯ User Details**
		// 	 • Created at: ${user.createdAt}${user.bot ? '\n • Is a bot account' : ''}
		// 	 • Status: ${user.presence.status}
		// 	 • Game: ${user.presence.game ? user.presence.game.name : 'None'}
		// `);
	}
};

module.exports = UserInfoCommand;