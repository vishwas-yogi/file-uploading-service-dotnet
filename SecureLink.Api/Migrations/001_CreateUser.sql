CREATE TABLE IF NOT EXISTS users
(
    row_id            int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id                uuid NOT NULL UNIQUE,
    username          text NOT NULL,
    email             text,
    name              text NOT NULL,
    password_hash     text NOT NULL,
    created_at        timestamp with time zone NOT NULL,
    last_modified_at  timestamp with time zone NOT NULL
);

-- TODO: add indexes later when we have clear context of columns in our table