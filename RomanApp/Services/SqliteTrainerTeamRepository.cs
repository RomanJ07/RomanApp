using RomanApp.Models;
using SQLite;
using System.Text.Json;

namespace RomanApp.Services;

public class SqliteTrainerTeamRepository : ITrainerTeamRepository
{
    private const string DatabaseName = "trainer_team.db3";
    private SQLiteAsyncConnection? _connection;

    public async Task SaveTeamAsync(IReadOnlyList<TrainerTeamMember> members)
    {
        var connection = await GetConnectionAsync();

        await connection.DeleteAllAsync<TrainerTeamEntity>();

        if (members.Count == 0)
        {
            return;
        }

        await connection.InsertAllAsync(members.Select(ToEntity));
    }

    public async Task<IReadOnlyList<TrainerTeamMember>> LoadTeamAsync()
    {
        var connection = await GetConnectionAsync();

        var entities = await connection.Table<TrainerTeamEntity>()
            .OrderBy(entity => entity.SlotNumber)
            .ToListAsync();

        return entities.Select(ToMember).ToList();
    }

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, DatabaseName);
        _connection = new SQLiteAsyncConnection(dbPath);
        await _connection.CreateTableAsync<TrainerTeamEntity>();
        await EnsureSchemaAsync(_connection);

        return _connection;
    }

    private static List<string> DeserializeTypes(string? rawTypes)
    {
        if (string.IsNullOrWhiteSpace(rawTypes))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(rawTypes) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static async Task EnsureSchemaAsync(SQLiteAsyncConnection connection)
    {
        var columns = await connection.QueryAsync<PragmaColumnInfo>("PRAGMA table_info(TrainerTeam)");
        var hasTypesColumn = columns.Any(column => string.Equals(column.Name, "Types", StringComparison.OrdinalIgnoreCase));
        if (!hasTypesColumn)
        {
            await connection.ExecuteAsync("ALTER TABLE TrainerTeam ADD COLUMN Types TEXT");
        }
    }

    private static TrainerTeamEntity ToEntity(TrainerTeamMember member)
    {
        return new TrainerTeamEntity
        {
            SlotNumber = member.SlotNumber,
            Name = member.Name,
            Description = member.Description,
            ImageUrl = member.ImageUrl,
            Hp = member.Hp,
            Attack = member.Attack,
            Types = JsonSerializer.Serialize(member.Types)
        };
    }

    private static TrainerTeamMember ToMember(TrainerTeamEntity entity)
    {
        return new TrainerTeamMember
        {
            SlotNumber = entity.SlotNumber,
            Name = entity.Name,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            Hp = entity.Hp,
            Attack = entity.Attack,
            Types = DeserializeTypes(entity.Types)
        };
    }

    [Table("TrainerTeam")]
    private class TrainerTeamEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int SlotNumber { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public int Hp { get; set; }

        public int Attack { get; set; }

        public string Types { get; set; } = "[]";
    }

    private class PragmaColumnInfo
    {
        public string Name { get; set; } = string.Empty;
    }
}

