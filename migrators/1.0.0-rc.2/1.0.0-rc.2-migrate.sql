ALTER TABLE attributes.attributes ALTER COLUMN created_at TYPE timestamp with time zone USING TO_TIMESTAMP(created_at::double precision / 1000::double precision);
ALTER TABLE attributes.attributes ALTER COLUMN created_at SET DEFAULT NOW();

ALTER TABLE users.cleverbot_sessions ALTER COLUMN session_created_at TYPE timestamp with time zone USING TO_TIMESTAMP(session_created_at::double precision / 1000::double precision);
ALTER TABLE users.cleverbot_sessions ALTER COLUMN session_created_at SET DEFAULT NOW();

REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA guilds FROM remote_access;
REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA users FROM remote_access;
REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA commands FROM remote_access;
REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA checkers FROM remote_access;
REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public FROM remote_access;
REVOKE CONNECT ON DATABASE taylorbot FROM remote_access;
DROP OWNED BY remote_access;
DROP ROLE remote_access;