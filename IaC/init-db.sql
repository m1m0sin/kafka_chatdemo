DROP TABLE IF EXISTS public.chat_messages;

CREATE TABLE IF NOT EXISTS public.chat_messages (
    id UUID PRIMARY KEY,
    type INT,
    from_user VARCHAR(100),
    to_user VARCHAR(100),
    message TEXT,
    is_private BOOLEAN,
    message_date VARCHAR(150)
);

TRUNCATE TABLE public.chat_messages;

SELECT * from public.chat_messages;