ALTER TABLE attributes.attributes ALTER COLUMN created_at TYPE timestamp with time zone USING TO_TIMESTAMP(created_at::double precision / 1000::double precision);
ALTER TABLE attributes.attributes ALTER COLUMN created_at SET DEFAULT NOW();

ALTER TABLE users.cleverbot_sessions ALTER COLUMN session_created_at TYPE timestamp with time zone USING TO_TIMESTAMP(session_created_at::double precision / 1000::double precision);
ALTER TABLE users.cleverbot_sessions ALTER COLUMN session_created_at SET DEFAULT NOW();