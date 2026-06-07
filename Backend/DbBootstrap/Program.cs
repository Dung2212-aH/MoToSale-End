using AuthService.Data;
using CatalogService.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Data;
using PaymentService.Data;

const string ConnectionString =
    "Server=(localdb)\\MSSQLLocalDB;Database=ShowroomDB;Trusted_Connection=True;TrustServerCertificate=True";

Console.WriteLine("== EF Schema Bootstrap ==");
Console.WriteLine($"Connection: {ConnectionString}");

// ----- Phase 1: Auth (canonical NGUOIDUNG, VAITRO, ...) -----
var authOptions = new DbContextOptionsBuilder<AuthDbContext>()
    .UseSqlServer(ConnectionString)
    .Options;
using (var ctx = new AuthDbContext(authOptions))
{
    Console.WriteLine("[1/4] AuthDbContext.EnsureCreated()...");
    ctx.Database.EnsureCreated();
    Console.WriteLine("      done");
}

// ----- Phase 2: Catalog (canonical SANPHAM, BIENSANPHAM + many) -----
ApplyCreateScript("CatalogDbContext", new DbContextOptionsBuilder<CatalogDbContext>()
    .UseSqlServer(ConnectionString)
    .Options, opts => new CatalogDbContext(opts));

// ----- Phase 2b: Catalog created partial DONHANG/CHITIET_DONHANG (ReviewOrder/Item).
//      Drop them along with any FKs referencing them so OrderService can recreate
//      these tables with the full schema in Phase 3.
DropPartialOrderTables();

// ----- Phase 3: Order (canonical DONHANG, CHITIET_DONHANG, GIOHANG, ...) -----
ApplyCreateScript("OrderDbContext", new DbContextOptionsBuilder<OrderDbContext>()
    .UseSqlServer(ConnectionString)
    .Options, opts => new OrderDbContext(opts));

// ----- Phase 4: Payment (canonical THANHTOAN) -----
ApplyCreateScript("PaymentDbContext", new DbContextOptionsBuilder<PaymentDbContext>()
    .UseSqlServer(ConnectionString)
    .Options, opts => new PaymentDbContext(opts));

Console.WriteLine();
PrintTableSummary();
Console.WriteLine("All DbContexts processed.");

static void DropPartialOrderTables()
{
    Console.WriteLine("[..] Dropping partial DONHANG / CHITIET_DONHANG (created by Catalog) ...");
    using var conn = new SqlConnection(ConnectionString);
    conn.Open();

    // 1) Drop every foreign key that points to DONHANG or CHITIET_DONHANG
    const string dropFkSql = @"
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(fk.parent_object_id) + '].[' + OBJECT_NAME(fk.parent_object_id) + '] DROP CONSTRAINT [' + fk.name + '];' + CHAR(10)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id = fk.referenced_object_id
WHERE t.name IN ('DONHANG','CHITIET_DONHANG');
EXEC sp_executesql @sql;";
    using (var cmd = new SqlCommand(dropFkSql, conn)) cmd.ExecuteNonQuery();

    // 2) Drop the partial tables (Catalog only modeled a subset of columns).
    foreach (var table in new[] { "CHITIET_DONHANG", "DONHANG" })
    {
        var drop = $"IF OBJECT_ID(N'[dbo].[{table}]', 'U') IS NOT NULL DROP TABLE [dbo].[{table}];";
        using var cmd = new SqlCommand(drop, conn);
        cmd.ExecuteNonQuery();
    }
    Console.WriteLine("      partial tables dropped");
}

static void PrintTableSummary()
{
    using var conn = new SqlConnection(ConnectionString);
    conn.Open();
    using var cmd = new SqlCommand(
        "SELECT name FROM sys.tables ORDER BY name;", conn);
    using var reader = cmd.ExecuteReader();
    var tables = new List<string>();
    while (reader.Read()) tables.Add(reader.GetString(0));
    Console.WriteLine($"Tables in ShowroomDB ({tables.Count}):");
    foreach (var t in tables) Console.WriteLine("  - " + t);
}

static void ApplyCreateScript<TContext>(
    string name,
    DbContextOptions<TContext> options,
    Func<DbContextOptions<TContext>, TContext> factory) where TContext : DbContext
{
    Console.WriteLine($"[..] {name} create script...");
    using var ctx = factory(options);
    var creator = ctx.GetService<IRelationalDatabaseCreator>();
    var script = creator.GenerateCreateScript();
    var conn = ctx.Database.GetDbConnection();
    if (conn.State != System.Data.ConnectionState.Open)
    {
        conn.Open();
    }

    var batches = SplitBatches(script);
    int applied = 0, skipped = 0;
    var skippedDetails = new List<string>();
    foreach (var batch in batches)
    {
        if (string.IsNullOrWhiteSpace(batch)) continue;
        using var cmd = conn.CreateCommand();
        cmd.CommandText = batch;
        try
        {
            cmd.ExecuteNonQuery();
            applied++;
        }
        catch (SqlException ex) when (IsBenign(ex))
        {
            skipped++;
            skippedDetails.Add($"  skip [{ex.Number}] {ex.Message.Split('\n')[0]}");
        }
    }

    Console.WriteLine($"     applied={applied} skipped={skipped}");
    if (skipped > 0)
    {
        foreach (var line in skippedDetails) Console.WriteLine(line);
    }
}

static bool IsBenign(SqlException ex)
{
    foreach (SqlError err in ex.Errors)
    {
        // 2714 = There is already an object named ...
        // 1913 = Index name already exists
        // 2779 / 2711 = duplicate constraint
        // 1779 = Table already has primary key
        // 1750 = Could not create constraint or index (when target column already covered)
        // 1769 = FK references invalid column (when column missing on partial table)
        // 1911 = Column does not exist in target table (when partial table doesn't have FK column)
        // 4902 = Cannot find table ... (rare during reordering)
        if (err.Number is 2714 or 1913 or 2779 or 2711 or 1779 or 1750 or 1769 or 1911 or 4902)
            return true;
    }
    return false;
}

static IEnumerable<string> SplitBatches(string script)
{
    var lines = script.Replace("\r\n", "\n").Split('\n');
    var sb = new System.Text.StringBuilder();
    foreach (var line in lines)
    {
        if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
        {
            yield return sb.ToString();
            sb.Clear();
        }
        else
        {
            sb.AppendLine(line);
        }
    }
    if (sb.Length > 0) yield return sb.ToString();
}
