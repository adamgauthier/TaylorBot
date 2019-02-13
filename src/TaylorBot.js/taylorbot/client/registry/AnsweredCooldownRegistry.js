'use strict';

const SECONDS_EXPIRE = 5;

class AnsweredCooldownRegistry {
    constructor(redis) {
        this.redis = redis;
    }

    key(user) {
        return `unanswered:${user.id}`;
    }

    addUnanswered(user) {
        return this.redis.setExpire(
            this.key(user),
            SECONDS_EXPIRE,
            1
        );
    }

    setAnswered(user) {
        return this.redis.delete(
            this.key(user)
        );
    }

    async isAnswered(user) {
        const result = await this.redis.exists(
            this.key(user)
        );
        return !!result;
    }
}

module.exports = AnsweredCooldownRegistry;