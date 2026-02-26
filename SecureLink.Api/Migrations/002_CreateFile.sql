CREATE TYPE file_status AS ENUM ('Pending', 'Available', 'CleanupRequired', 'Deleted');

CREATE TABLE IF NOT EXISTS files
(
    row_id            int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id                uuid NOT NULL UNIQUE,
    filename          text NOT NULL,
    user_filename     text NOT NULL,
    content_type      text NOT NULL,
    location          text,
    owner             uuid NOT NULL REFERENCES users(id),
    status            file_status NOT NULL DEFAULT 'Pending',
    metadata          jsonb NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamp with time zone NOT NULL,
    last_modified_at  timestamp with time zone NOT NULL
);