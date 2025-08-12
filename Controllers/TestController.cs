using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace WaterQualityAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DiagnosticsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("dns-lookup")]
        public async Task<IActionResult> DnsLookup()
        {
            try
            {
                var host = "db.onhcovwfqfkyrifyfyix.supabase.co";
                var results = new StringBuilder();

                // DNS lookup
                results.AppendLine("DNS Lookup Results:");
                try
                {
                    var hostEntry = await Dns.GetHostEntryAsync(host);
                    results.AppendLine($"  Hostname: {hostEntry.HostName}");
                    results.AppendLine("  IP Addresses:");
                    foreach (var address in hostEntry.AddressList)
                    {
                        results.AppendLine($"    {address}");
                    }
                }
                catch (Exception ex)
                {
                    results.AppendLine($"  DNS Resolution failed: {ex.Message}");
                }

                // TCP connection test
                results.AppendLine("\nTCP Connection Test (Port 5432):");
                try
                {
                    using var client = new TcpClient();
                    var connectTask = client.ConnectAsync(host, 5432);
                    var completed = await Task.WhenAny(connectTask, Task.Delay(5000));

                    if (completed == connectTask)
                    {
                        await connectTask; // Unwrap any exceptions
                        results.AppendLine("  Successfully connected to port 5432");
                    }
                    else
                    {
                        results.AppendLine("  Connection timed out after 5 seconds");
                    }
                }
                catch (Exception ex)
                {
                    results.AppendLine($"  TCP Connection failed: {ex.Message}");
                }

                return Ok(results.ToString());
            }
            catch (Exception ex)
            {
                return BadRequest($"Diagnostic failed: {ex.Message}");
            }
        }

        [HttpGet("test-supabase")]
        public async Task<IActionResult> TestSupabase()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("SupabaseConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return BadRequest("Supabase connection string is missing in configuration");
                }

                // Don't log the full connection string (security risk)
                var sanitizedConnectionString = SanitizeConnectionString(connectionString);
                var results = new StringBuilder();
                results.AppendLine($"Testing connection with: {sanitizedConnectionString}");

                using var connection = new NpgsqlConnection(connectionString);

                // Test opening connection
                results.AppendLine("\nConnection Test:");
                try
                {
                    await connection.OpenAsync();
                    results.AppendLine($"  Connection opened successfully!");
                    results.AppendLine($"  Server Version: {connection.PostgreSqlVersion}");
                    results.AppendLine($"  Database: {connection.Database}");
                    results.AppendLine($"  Connection Timeout: {connection.ConnectionTimeout}");

                    // Test simple query
                    results.AppendLine("\nQuery Test:");
                    using var cmd = new NpgsqlCommand("SELECT current_database(), current_user", connection);
                    using var reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        results.AppendLine($"  Current Database: {reader.GetString(0)}");
                        results.AppendLine($"  Current User: {reader.GetString(1)}");
                    }
                }
                catch (Exception ex)
                {
                    results.AppendLine($"  Connection failed: {ex.Message}");

                    // Get more detailed exception info if available
                    if (ex is NpgsqlException npgEx)
                    {
                        results.AppendLine($"  Postgres Error Code: {npgEx.SqlState}");
                    }
                }

                return Ok(results.ToString());
            }
            catch (Exception ex)
            {
                return BadRequest($"Test failed: {ex.Message}");
            }
        }

        private string SanitizeConnectionString(string connectionString)
        {
            // Remove password from connection string for logging
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            builder.Password = "***REDACTED***";
            return builder.ToString();
        }
    }
}