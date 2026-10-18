-- Deploy taylorbot-postgres:20261017_tracking_checkpoints to pg

BEGIN;

CREATE SCHEMA tracking;

CREATE TABLE tracking.queue_checkpoints (
    queue_name text PRIMARY KEY CHECK (queue_name IN (
        'guild-user-last-spoke-updates',
        'member-message-count-increments',
        'channel-message-count-increments'
    )),
    sequence bigint NOT NULL DEFAULT 0 CHECK (sequence >= 0)
);

INSERT INTO tracking.queue_checkpoints (queue_name) VALUES
    ('guild-user-last-spoke-updates'),
    ('member-message-count-increments'),
    ('channel-message-count-increments');

COMMIT;
