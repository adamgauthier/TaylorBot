ALTER TABLE checkers.instagram_checker
    ADD COLUMN last_taken_at timestamp with time zone NOT NULL DEFAULT TIMESTAMP WITH TIME ZONE '2000-01-01 00:00:00';
