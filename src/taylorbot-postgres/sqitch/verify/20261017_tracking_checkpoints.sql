-- Verify taylorbot-postgres:20261017_tracking_checkpoints on pg

BEGIN;

SELECT queue_name, sequence FROM tracking.queue_checkpoints WHERE false;

DO $$
BEGIN
    ASSERT (SELECT count(*) FROM tracking.queue_checkpoints) = 3;
    ASSERT NOT EXISTS (SELECT FROM tracking.queue_checkpoints WHERE sequence < 0);
END $$;

ROLLBACK;
