INSERT INTO tours (title, price_text, duration_text, image_url, is_top)
VALUES
('Rome, Italy',  '$5,42k', '10 Days Trip', '/jadoo/assets/img/dest/dest1.jpg', TRUE),
('London, UK',   '$4.2k',  '12 Days Trip', '/jadoo/assets/img/dest/dest2.jpg', FALSE),
('Full Europe',  '$15k',   '28 Days Trip', '/jadoo/assets/img/dest/dest3.jpg', TRUE)
ON CONFLICT DO NOTHING;