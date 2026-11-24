SET search_path TO portfolio, public;

INSERT INTO portfolio.course (course_code, course_title, term_taken, units, description)
VALUES
    ('DBMWEB1', 'Database Management and Web Systems', '1st Sem SY 2026-2027', 3, 'Relational database modeling, SQL constraints, transactions, and ASP.NET Core MVC web applications.'),
    ('INASEC1', 'Information Assurance and Networking Security', '2nd Sem SY 2025-2026', 3, 'Routing protocols, VLAN segmentation, ACLs, NAT, and Windows/Linux server administration.')
ON CONFLICT (course_code) DO NOTHING;

INSERT INTO portfolio.project (course_id, project_title, summary, repo_url, video_url)
SELECT c.course_id,
       'Course & Project Portfolio Web App',
       'Full-stack ASP.NET Core MVC portfolio powered by Npgsql, parameterized SQL, and atomic transactions.',
       'https://github.com/yourusername/portfolio',
       'https://www.youtube.com/watch?v=dQw4w9WgXcQ'
FROM portfolio.course c
WHERE c.course_code = 'DBMWEB1'
ON CONFLICT (course_id, project_title) DO NOTHING;

INSERT INTO portfolio.project (course_id, project_title, summary, repo_url, video_url)
SELECT c.course_id,
       'Campus Data Center Topology',
       'Multi-VLAN enterprise network simulation with OSPF routing, NAT, and hardened firewall policies.',
       'https://github.com/yourusername/datacenter-lab',
       'https://youtu.be/dQw4w9WgXcQ'
FROM portfolio.course c
WHERE c.course_code = 'NETSEC3'
ON CONFLICT (course_id, project_title) DO NOTHING;

INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
SELECT p.project_id,
       'https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=1200',
       'Repository and MVC architecture overview',
       1
FROM portfolio.project p
WHERE p.project_title = 'Course & Project Portfolio Web App'
ON CONFLICT (project_id, image_url) DO NOTHING;

INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
SELECT p.project_id,
       'https://images.unsplash.com/photo-1518770660439-4636190af475?w=1200',
       'Database schema and constraint verification',
       2
FROM portfolio.project p
WHERE p.project_title = 'Course & Project Portfolio Web App'
ON CONFLICT (project_id, image_url) DO NOTHING;