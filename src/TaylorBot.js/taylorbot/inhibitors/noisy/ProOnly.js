'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');

class ProOnlyInhibitor extends NoisyInhibitor {
    async getBlockedMessage({ message, client }, command) {
        if (command.command.proOnly) {
            const { database } = client.master;
            const { author, channel, guild } = message;

            if (channel.type === 'text') {
                const { guild_exists } = await database.pros.proGuildExists(guild);
                if (guild_exists === false) {
                    const proUser = await database.pros.getUser(author);

                    if (proUser === null || proUser.expires_at !== null && proUser.expires_at < new Date()) {
                        return {
                            ui: `You can't use \`${command.name}\` because it is restricted to supporters and supporter servers, use \`support\` for more info.`,
                            log: 'They are not a pro user and the server is not pro.'
                        };
                    }
                }
            }
            else {
                const proUser = await database.pros.getUser(author);

                if (proUser === null || proUser.expires_at !== null && proUser.expires_at < new Date()) {
                    return {
                        ui: `You can't use \`${command.name}\` because it is restricted to supporters, use \`support\` for more info.`,
                        log: 'They are not a pro user.'
                    };
                }
            }
        }

        return null;
    }
}

module.exports = ProOnlyInhibitor;