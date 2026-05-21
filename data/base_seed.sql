INSERT IGNORE INTO DIFFICULTY (diff_name, score, icon_url) VALUES
('VERY EASY', 5, NULL),
('EASY', 10, NULL),
('HARD', 15, NULL),
('VERY HARD', 20, NULL),
('TOUGH', 25, NULL),
('VERY TOUGH', 30, NULL);

INSERT IGNORE INTO TIER (tier_name, icon_url)
SELECT CONCAT(stage_name, ' ', base_name), NULL
FROM (
    SELECT 'normal' AS stage_name UNION ALL
    SELECT 'core' UNION ALL
    SELECT 'magic' UNION ALL
    SELECT 'rare' UNION ALL
    SELECT 'elite' UNION ALL
    SELECT 'unique' UNION ALL
    SELECT 'legend'
) AS stages
CROSS JOIN (
    SELECT 'bronze' AS base_name UNION ALL
    SELECT 'silver' UNION ALL
    SELECT 'gold' UNION ALL
    SELECT 'platinum' UNION ALL
    SELECT 'diamond' UNION ALL
    SELECT 'master' UNION ALL
    SELECT 'challenger'
) AS bases;
