CREATE TABLE IF NOT EXISTS tours (
    id              SERIAL PRIMARY KEY,
    title           TEXT NOT NULL,
    price_text      TEXT NOT NULL,
    duration_text   TEXT NOT NULL,
    image_url       TEXT NOT NULL,
    is_top          BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE TABLE IF NOT EXISTS categories (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL
);

ALTER TABLE tours
  ADD COLUMN IF NOT EXISTS category_id INT NULL REFERENCES categories(id);

-- (optional later) tags + tour_tags
CREATE TABLE IF NOT EXISTS tags (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS tour_tags (
  tour_id INT NOT NULL REFERENCES tours(id) ON DELETE CASCADE,
  tag_id  INT NOT NULL REFERENCES tags(id)  ON DELETE CASCADE,
  PRIMARY KEY (tour_id, tag_id)
);