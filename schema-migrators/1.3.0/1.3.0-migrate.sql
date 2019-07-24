ALTER TABLE checkers.youtube_checker
    ADD COLUMN last_published_at timestamp with time zone;

UPDATE checkers.youtube_checker SET last_published_at = CURRENT_TIMESTAMP WHERE last_video_id IS NOT NULL;
