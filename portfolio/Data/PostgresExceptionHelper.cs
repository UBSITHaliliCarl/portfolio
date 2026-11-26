using Npgsql;

namespace portfolio.Data
{
    public static class PostgresExceptionHelper
    {
        public static string ToFriendlyMessage(PostgresException ex)
        {
            return ex.SqlState switch
            {
                // 23505 = unique_violation
                "23505" => ex.ConstraintName switch
                {
                    "uq_course_code" => "A course with that course code already exists.",
                    "uq_project_course_title" => "This course already has a project with that title.",
                    "uq_project_image_url" => "Duplicate image URL detected: each image URL in a project must be unique.",
                    _ => "A duplicate value violated a unique constraint in the database."
                },

                // 23503 = foreign_key_violation
                "23503" => ex.ConstraintName switch
                {
                    "fk_project_course" when ex.MessageText.Contains("still referenced", StringComparison.OrdinalIgnoreCase)
                        => "Cannot delete this course because one or more projects are still linked to it.",
                    "fk_project_course"
                        => "The selected course does not exist.",
                    "fk_image_project"
                        => "The selected project does not exist.",
                    _ => "Operation refused because a referenced record does not exist or still has dependent records."
                },

                // 23514 = check_violation
                "23514" => ex.ConstraintName switch
                {
                    "chk_course_units" => "Units must be between 1 and 6.",
                    "chk_image_sort_order" => "Image sort order must be 1 or greater.",
                    "chk_course_code_not_blank" => "Course code cannot be blank.",
                    "chk_course_title_not_blank" => "Course title cannot be blank.",
                    "chk_project_title_not_blank" => "Project title cannot be blank.",
                    "chk_project_summary_not_blank" => "Project summary cannot be blank.",
                    "chk_image_url_not_blank" => "Image URL cannot be blank.",
                    _ => $"Database check rule failed ({ex.ConstraintName})."
                },

                _ => $"Database error ({ex.SqlState}): {ex.MessageText}"
            };
        }
    }
}