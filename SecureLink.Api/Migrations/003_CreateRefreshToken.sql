CREATE TABLE IF NOT EXISTS refresh_token
(
    row_id      int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id          uuid NOT NULL UNIQUE,
    user_id     uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    value       text NOT NULL,
    expires_at  timestamp with time zone NOT NULL,
    created_at  timestamp with time zone NOT NULL,
    revoked_at  timestamp with time zone
);