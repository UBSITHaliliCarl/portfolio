using Npgsql;
using portfolio.Models;

namespace portfolio.Data
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly string _connectionString;

        public ProjectRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.");
        }

        public async Task<List<Project>> GetAllAsync()
        {
            var list = new List<Project>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT p.project_id, p.course_id, c.course_code, c.course_title,
                       p.project_title, p.summary, p.repo_url, p.video_url, p.created_at
                FROM portfolio.project p
                INNER JOIN portfolio.course c ON c.course_id = p.course_id
                ORDER BY c.course_code ASC, p.project_title ASC;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapProject(reader));
            }
            return list;
        }

        public async Task<List<Project>> GetByCourseIdAsync(int courseId)
        {
            var list = new List<Project>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT p.project_id, p.course_id, c.course_code, c.course_title,
                       p.project_title, p.summary, p.repo_url, p.video_url, p.created_at
                FROM portfolio.project p
                INNER JOIN portfolio.course c ON c.course_id = p.course_id
                WHERE p.course_id = @courseId
                ORDER BY p.created_at DESC, p.project_title ASC;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("courseId", courseId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapProject(reader));
            }
            return list;
        }

        public async Task<Project?> GetByIdAsync(int projectId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT p.project_id, p.course_id, c.course_code, c.course_title,
                       p.project_title, p.summary, p.repo_url, p.video_url, p.created_at
                FROM portfolio.project p
                INNER JOIN portfolio.course c ON c.course_id = p.course_id
                WHERE p.project_id = @id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", projectId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapProject(reader) : null;
        }

        // Atomic transaction: inserts project + every image URL, or rolls back everything
        public async Task<int> CreateWithImagesAsync(Project project, IReadOnlyList<string> imageUrls)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                const string insertProjectSql = @"
                    INSERT INTO portfolio.project (course_id, project_title, summary, repo_url, video_url)
                    VALUES (@courseId, @title, @summary, @repoUrl, @videoUrl)
                    RETURNING project_id;";

                int newProjectId;
                await using (var cmd = new NpgsqlCommand(insertProjectSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("courseId", project.CourseId);
                    cmd.Parameters.AddWithValue("title", project.ProjectTitle.Trim());
                    cmd.Parameters.AddWithValue("summary", project.Summary.Trim());
                    cmd.Parameters.AddWithValue("repoUrl", (object?)project.RepoUrl?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("videoUrl", (object?)project.VideoUrl?.Trim() ?? DBNull.Value);

                    newProjectId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                const string insertImageSql = @"
                    INSERT INTO portfolio.project_image (project_id, image_url, sort_order)
                    VALUES (@projectId, @imageUrl, @sortOrder);";

                for (int i = 0; i < imageUrls.Count; i++)
                {
                    await using var imgCmd = new NpgsqlCommand(insertImageSql, conn, tx);
                    imgCmd.Parameters.AddWithValue("projectId", newProjectId);
                    imgCmd.Parameters.AddWithValue("imageUrl", imageUrls[i]);
                    imgCmd.Parameters.AddWithValue("sortOrder", i + 1);
                    await imgCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return newProjectId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // Atomic transaction: updates project + replaces image list, or rolls back everything
        public async Task<bool> UpdateWithImagesAsync(Project project, IReadOnlyList<string> imageUrls)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                const string updateProjectSql = @"
                    UPDATE portfolio.project
                    SET course_id = @courseId,
                        project_title = @title,
                        summary = @summary,
                        repo_url = @repoUrl,
                        video_url = @videoUrl
                    WHERE project_id = @id;";

                int rows;
                await using (var cmd = new NpgsqlCommand(updateProjectSql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("id", project.ProjectId);
                    cmd.Parameters.AddWithValue("courseId", project.CourseId);
                    cmd.Parameters.AddWithValue("title", project.ProjectTitle.Trim());
                    cmd.Parameters.AddWithValue("summary", project.Summary.Trim());
                    cmd.Parameters.AddWithValue("repoUrl", (object?)project.RepoUrl?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("videoUrl", (object?)project.VideoUrl?.Trim() ?? DBNull.Value);
                    rows = await cmd.ExecuteNonQueryAsync();
                }

                if (rows == 0)
                {
                    await tx.RollbackAsync();
                    return false;
                }

                const string deleteImagesSql = "DELETE FROM portfolio.project_image WHERE project_id = @projectId;";
                await using (var delCmd = new NpgsqlCommand(deleteImagesSql, conn, tx))
                {
                    delCmd.Parameters.AddWithValue("projectId", project.ProjectId);
                    await delCmd.ExecuteNonQueryAsync();
                }

                const string insertImageSql = @"
                    INSERT INTO portfolio.project_image (project_id, image_url, sort_order)
                    VALUES (@projectId, @imageUrl, @sortOrder);";

                for (int i = 0; i < imageUrls.Count; i++)
                {
                    await using var imgCmd = new NpgsqlCommand(insertImageSql, conn, tx);
                    imgCmd.Parameters.AddWithValue("projectId", project.ProjectId);
                    imgCmd.Parameters.AddWithValue("imageUrl", imageUrls[i]);
                    imgCmd.Parameters.AddWithValue("sortOrder", i + 1);
                    await imgCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int projectId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = "DELETE FROM portfolio.project WHERE project_id = @id;";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", projectId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static Project MapProject(NpgsqlDataReader reader) => new()
        {
            ProjectId = reader.GetInt32(0),
            CourseId = reader.GetInt32(1),
            CourseCode = reader.GetString(2),
            CourseTitle = reader.GetString(3),
            ProjectTitle = reader.GetString(4),
            Summary = reader.GetString(5),
            RepoUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
            VideoUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
            CreatedAt = reader.GetDateTime(8)
        };
    }
}