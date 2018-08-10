'use strict';

const fetch = require('node-fetch');

const CleverBotConfig = require('../../config/cleverbot.json');

class CleverBotModule {
    static async create(nick) {
        const response = await fetch('https://cleverbot.io/1.0/create',
            {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    user: CleverBotConfig.user,
                    key: CleverBotConfig.key,
                    nick
                })
            })
            .then(res => res.json());

        if (response.status !== 'success')
            throw new Error(`Cleverbot.io's create response status was ${response.status}.`);

        return response.nick;
    }

    static async ask(nick, text) {
        const response = await fetch('https://cleverbot.io/1.0/ask',
            {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    user: CleverBotConfig.user,
                    key: CleverBotConfig.key,
                    nick,
                    text
                })
            })
            .then(res => res.json());

        if (response.status !== 'success')
            throw new Error(`Cleverbot.io's ask response status was ${response.status}.`);

        return response.response;
    }
}

module.exports = CleverBotModule;