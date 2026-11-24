CREATE SCHEMA IF NOT EXISTS portfolio;
SET search_path TO portfolio, public;

DROP TABLE IF EXISTS portfolio.project_image CASCADE;
DROP TABLE IF EXISTS portfolio.project CASCADE;
DROP TABLE IF EXISTS portfolio.course CASCADE;

CREATE TABLE portfolio.course (
    course_id SERIAL PRIMARY KEY,
    course_code VARCHAR(20) NOT NULL,
    course_title VARCHAR(150) NOT NULL,
    term_taken VARCHAR(40) NOT NULL,
    units INT NOT NULL,
    description TEXT,

    CONSTRAINT uq_course_code UNIQUE (course_code),
    CONSTRAINT chk_course_units CHECK (units BETWEEN 1 AND 6),
    CONSTRAINT chk_course_code_not_blank CHECK (length(trim(course_code)) > 0),
    CONSTRAINT chk_course_title_not_blank CHECK (length(trim(course_title)) > 0)
);

CREATE TABLE portfolio.project (
    project_id SERIAL PRIMARY KEY,
    course_id INT NOT NULL,
    project_title VARCHAR(150) NOT NULL,
    summary TEXT NOT NULL,
    repo_url VARCHAR(300),
    video_url VARCHAR(300),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_project_course FOREIGN KEY (course_id)
        REFERENCES portfolio.course (course_id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT uq_project_course_title UNIQUE (course_id, project_title),
    CONSTRAINT chk_project_title_not_blank CHECK (length(trim(project_title)) > 0),
    CONSTRAINT chk_project_summary_not_blank CHECK (length(trim(summary)) > 0)
);

CREATE TABLE portfolio.project_image (
    image_id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    image_url VARCHAR(500) NOT NULL,
    caption VARCHAR(200),
    sort_order INT NOT NULL DEFAULT 1,

    CONSTRAINT fk_image_project FOREIGN KEY (project_id)
        REFERENCES portfolio.project (project_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT uq_project_image_url UNIQUE (project_id, image_url),
    CONSTRAINT chk_image_sort_order CHECK (sort_order >= 1),
    CONSTRAINT chk_image_url_not_blank CHECK (length(trim(image_url)) > 0)
);