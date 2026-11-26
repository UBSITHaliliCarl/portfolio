using Npgsql;
using portfolio.Models;

namespace portfolio.Data
{
    public class CourseRepository : ICourseRepository
    {
        private readonly string _connectionString;

        public CourseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.");
        }

        public async Task<List<Course>> GetAllAsync()
        {
            var list = new List<Course>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT course_id, course_code, course_title, term_taken, units, description
                FROM portfolio.course
                ORDER BY course_code ASC;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapCourse(reader));
            }
            return list;
        }

        public async Task<Course?> GetByIdAsync(int courseId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT course_id, course_code, course_title, term_taken, units, description
                FROM portfolio.course
                WHERE course_id = @id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", courseId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapCourse(reader) : null;
        }

        public async Task<int> CreateAsync(Course course)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO portfolio.course (course_code, course_title, term_taken, units, description)
                VALUES (@code, @title, @term, @units, @desc)
                RETURNING course_id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("code", course.CourseCode.Trim());
            cmd.Parameters.AddWithValue("title", course.CourseTitle.Trim());
            cmd.Parameters.AddWithValue("term", course.TermTaken.Trim());
            cmd.Parameters.AddWithValue("units", course.Units);
            cmd.Parameters.AddWithValue("desc", (object?)course.Description?.Trim() ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Course course)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                UPDATE portfolio.course
                SET course_code = @code,
                    course_title = @title,
                    term_taken = @term,
                    units = @units,
                    description = @desc
                WHERE course_id = @id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", course.CourseId);
            cmd.Parameters.AddWithValue("code", course.CourseCode.Trim());
            cmd.Parameters.AddWithValue("title", course.CourseTitle.Trim());
            cmd.Parameters.AddWithValue("term", course.TermTaken.Trim());
            cmd.Parameters.AddWithValue("units", course.Units);
            cmd.Parameters.AddWithValue("desc", (object?)course.Description?.Trim() ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int courseId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = "DELETE FROM portfolio.course WHERE course_id = @id;";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", courseId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static Course MapCourse(NpgsqlDataReader reader) => new()
        {
            CourseId = reader.GetInt32(0),
            CourseCode = reader.GetString(1),
            CourseTitle = reader.GetString(2),
            TermTaken = reader.GetString(3),
            Units = reader.GetInt32(4),
            Description = reader.IsDBNull(5) ? null : reader.GetString(5)
        };
    }
}