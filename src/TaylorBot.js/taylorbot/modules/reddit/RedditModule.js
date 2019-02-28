'use strict';

const fetch = require('node-fetch');

class RedditModule {
    static getSubredditAbout(subreddit) {
        return fetch(`https://www.reddit.com/r/${subreddit}/about/.json`).then(res => res.json());
    }
}

module.exports = RedditModule;