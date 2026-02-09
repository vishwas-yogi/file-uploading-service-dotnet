CREATE TABLE IF NOT EXISTS users
(
    row_id        int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id            uuid NOT NULL UNIQUE,
    user_name     text NOT NULL,
    email         text NOT NULL,
    first_name    text NOT NULL,
    last_name     text NOT NULL
);