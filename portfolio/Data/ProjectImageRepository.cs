using Npgsql;
using portfolio.Models;

namespace portfolio.Data
{
    public class ProjectImageRepository : IProjectImageRepository
    {
        private readonly string _connectionString;

        public ProjectImageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.");
        }

        public async Task<List<ProjectImage>> GetAllAsync()
        {
            var list = new List<ProjectImage>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT i.image_id, i.project_id, p.project_title, i.image_url, i.caption, i.sort_order
                FROM portfolio.project_image i
                INNER JOIN portfolio.project p ON p.project_id = i.project_id
                ORDER BY p.project_title ASC, i.sort_order ASC, i.image_id ASC;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapImage(reader));
            }
            return list;
        }

        public async Task<List<ProjectImage>> GetByProjectIdAsync(int projectId)
        {
            var list = new List<ProjectImage>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT i.image_id, i.project_id, p.project_title, i.image_url, i.caption, i.sort_order
                FROM portfolio.project_image i
                INNER JOIN portfolio.project p ON p.project_id = i.project_id
                WHERE i.project_id = @projectId
                ORDER BY i.sort_order ASC, i.image_id ASC;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("projectId", projectId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapImage(reader));
            }
            return list;
        }

        public async Task<ProjectImage?> GetByIdAsync(int imageId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT i.image_id, i.project_id, p.project_title, i.image_url, i.caption, i.sort_order
                FROM portfolio.project_image i
                INNER JOIN portfolio.project p ON p.project_id = i.project_id
                WHERE i.image_id = @id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", imageId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapImage(reader) : null;
        }

        public async Task<int> CreateAsync(ProjectImage image)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
                VALUES (@projectId, @imageUrl, @caption, @sortOrder)
                RETURNING image_id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("projectId", image.ProjectId);
            cmd.Parameters.AddWithValue("imageUrl", image.ImageUrl.Trim());
            cmd.Parameters.AddWithValue("caption", (object?)image.Caption?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("sortOrder", image.SortOrder);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> UpdateAsync(ProjectImage image)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                UPDATE portfolio.project_image
                SET project_id = @projectId,
                    image_url = @imageUrl,
                    caption = @caption,
                    sort_order = @sortOrder
                WHERE image_id = @id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", image.ImageId);
            cmd.Parameters.AddWithValue("projectId", image.ProjectId);
            cmd.Parameters.AddWithValue("imageUrl", image.ImageUrl.Trim());
            cmd.Parameters.AddWithValue("caption", (object?)image.Caption?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("sortOrder", image.SortOrder);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int imageId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = "DELETE FROM portfolio.project_image WHERE image_id = @id;";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", imageId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static ProjectImage MapImage(NpgsqlDataReader reader) => new()
        {
            ImageId = reader.GetInt32(0),
            ProjectId = reader.GetInt32(1),
            ProjectTitle = reader.GetString(2),
            ImageUrl = reader.GetString(3),
            Caption = reader.IsDBNull(4) ? null : reader.GetString(4),
            SortOrder = reader.GetInt32(5)
        };
    }
}