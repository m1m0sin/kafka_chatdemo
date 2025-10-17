CREATE TABLE IF NOT EXISTS orders (
  order_id INT PRIMARY KEY,
  customer TEXT NOT NULL,
  amount NUMERIC
);

TRUNCATE TABLE public.orders;

SELECT * from public.orders;

