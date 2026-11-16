using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Runtime.InteropServices;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
    .CreateLogger();
logger.Information("Iniciando a execucao do Job...");
logger.Information($"Versao do .NET em uso: {RuntimeInformation
    .FrameworkDescription} - Ambiente: {Environment.MachineName} - Kernel: {Environment
    .OSVersion.VersionString}");

try
{
    var endpointRequest = configuration["EndpointRequest"];
    logger.Information($"URL para envio da requisicao: {endpointRequest}");

    var waitingTimeBeforeRequestInSeconds = int.Parse(configuration["WaitingTimeBeforeRequestInSeconds"]!);
    var waitingTimeAfterRequestInSeconds = int.Parse(configuration["WaitingTimeAfterRequestInSeconds"]!);

    if (waitingTimeBeforeRequestInSeconds > 0)
    {
        logger.Information($"Aguardando {waitingTimeBeforeRequestInSeconds} segundos antes de enviar a requisicao...");
        await Task.Delay(waitingTimeBeforeRequestInSeconds * 1000);
    }
    
    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync(endpointRequest);
    logger.Information($"Response Status Code = {(int)response.StatusCode} {response.StatusCode}");
    response.EnsureSuccessStatusCode();
    logger.Information("Notificacao enviada com sucesso!");
    logger.Information($"Dados recebidos = {await response.Content.ReadAsStringAsync()}");

    if (waitingTimeAfterRequestInSeconds > 0)
    {
        logger.Information($"Aguardando {waitingTimeAfterRequestInSeconds} segundos apos enviar a requisicao...");
        await Task.Delay(waitingTimeAfterRequestInSeconds * 1000);
    }

    logger.Information("Job executado com sucesso!");
}
catch (Exception ex)
{
    logger.Error($"Erro durante a execucao do Job: {ex.Message}");
    Environment.ExitCode = 1;
}