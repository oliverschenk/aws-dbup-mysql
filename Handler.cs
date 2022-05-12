using System.Reflection;
using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;
using DbUp;
using AwsDbUpMySql.Models;
using MySql.Data.MySqlClient;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AwsDbUpMySql;

public class Handler
{
    private static readonly string DbSecretName =
        Environment.GetEnvironmentVariable("DB_SECRET_NAME");
    private static readonly string DbName =
        Environment.GetEnvironmentVariable("DB_NAME");

    private SecretsManagerCache _cache;
    private ILambdaLogger _logger;

    public Handler()
    {
        // get timeout configuration from environment variables
        var timeoutVariable = Environment.GetEnvironmentVariable("TIMEOUT");
        var timeout = 2000;

        // try to parse timeout configuration if value exists
        if (!string.IsNullOrEmpty(timeoutVariable))
        {
            int.TryParse(timeoutVariable, out timeout);
        }

        AmazonSecretsManagerConfig config = new AmazonSecretsManagerConfig
        {
            Timeout = TimeSpan.FromMilliseconds(timeout)
        };
        IAmazonSecretsManager client = new AmazonSecretsManagerClient(config);

        Console.WriteLine($"Secrets Manager client configuration");
        Console.WriteLine(JsonSerializer.Serialize(config));

        _cache = new SecretsManagerCache(client);
    }

    public async Task FunctionHandler(ILambdaContext context)
    {
        _logger = context.Logger;

        _logger.LogInformation("ENVIRONMENT VARIABLES: " + JsonSerializer.Serialize(System.Environment.GetEnvironmentVariables()));
        _logger.LogInformation("CONTEXT: " + JsonSerializer.Serialize(context));

        try
        {
            var secret = await GetDatabaseSecretFromCache();

            _logger.LogDebug($"Connecting to host '{secret.Host}' with username '{secret.Username}'");

            var connectionString = $"server='{secret.Host}';uid={secret.Username};pwd='{secret.Password}';database={DbName};oldguids=true";

            await PerformDatabaseMigration(connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Migration failed.");
            _logger.LogError(ex.Message);
        }
    }

    private async Task PerformDatabaseMigration(string connectionString)
    {
        EnsureDatabase.For.MySqlDatabase(connectionString);

        var upgrader =
            DeployChanges.To
                .MySqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransactionPerScript()
                .LogToConsole()
                .Build();

        if (upgrader.IsUpgradeRequired())
        {
            _logger.LogDebug("Performing database migration.");
            var result = upgrader.PerformUpgrade();

            if (result.Successful)
            {
                _logger.LogInformation("Migration completed successfully.");

                await OutputEmployeeData(connectionString);
            }
            else
            {
                throw new Exception(result.Error.Message);
            }
        }
        else
        { 
            _logger.LogInformation("Migration is not required.");
        }
    }

    private async Task<Secret> GetDatabaseSecretFromCache()
    {
        _logger.LogDebug($"Reading secret '{DbSecretName}' from cache");

        var response = await _cache.GetSecretString(DbSecretName);

        Secret secret = null;
        if (response != null)
        {
            secret = JsonSerializer.Deserialize<Secret>(response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        else
        {
            throw new Exception("Failed to fetch secret");
        }

        return secret;
    }

    private async Task OutputEmployeeData(string connectionString)
    {
        _logger.LogInformation("Verifying database migration");

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        // create a DB command and set the SQL statement with parameters
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM employees";

        // execute the command and read the results
        using var reader = await command.ExecuteReaderAsync();

        while (reader.Read())
        {
            var emp_no = reader["emp_no"];
            var birth_date = reader["birth_date"];
            var first_name = reader["first_name"];
            var last_name = reader["last_name"];
            var gender = reader["gender"];
            var hire_date = reader["hire_date"];

            _logger.LogInformation($"{emp_no}\t{birth_date}\t{first_name}\t{last_name}\t{gender}\t{hire_date}");
        }
    }
}
