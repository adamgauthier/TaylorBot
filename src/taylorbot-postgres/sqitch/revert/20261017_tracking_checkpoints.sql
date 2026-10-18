-- Revert taylorbot-postgres:20261017_tracking_checkpoints from pg

BEGIN;

DO $$
BEGIN
    IF EXISTS (SELECT FROM tracking.queue_checkpoints WHERE sequence <> 0) THEN
        RAISE EXCEPTION 'Cannot remove used tracking checkpoints: delayed Redis batches could be replayed';
    END IF;
END $$;

DROP TABLE tracking.queue_checkpoints;
DROP SCHEMA tracking;

COMMIT;
