using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chetch.Services;

abstract public class Service<T> : BackgroundService where T : BackgroundService
{
    #region Constants
    public const String APPSETTINGS_FILE = "appsettings.json";
    public const String APPSETTINGS_LOCAL_FILE = "appsettings.local.json";
    #endregion

    #region Static stuff
    static public IConfiguration Config {get; internal set; } = null;

    static public void Run(String[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSystemd();
        builder.Services.AddHostedService<T>();

        var host = builder.Build();
        host.Run();
    }
    #endregion

    #region Properties
    public ILogger<T> Logger { get; protected set; }

    public String ServiceName { get; set; } = String.Empty;
    #endregion

    #region Constructors
    public Service(ILogger<T> logger){
        Logger = logger;

        if(Config == null)
        {
            String[] appSettingsFiles = [APPSETTINGS_LOCAL_FILE, APPSETTINGS_FILE];
            
            foreach(var appSettingsFile in appSettingsFiles)
            {
                try{
                    var configBuilder = new ConfigurationBuilder().AddJsonFile(appSettingsFile, false);
                    Config = configBuilder.Build();
                    Logger.LogInformation(String.Format("Loaded config from: {0}", appSettingsFile));
                    break;
                }
                catch(Exception e)
                {
                    Logger.LogError(e, String.Format("Cannot load {0}", appSettingsFile));
                }
            }
        }
    }
    #endregion

    #region Lifecycle methods
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation(10, "Starting service at: {time}", DateTimeOffset.Now);
        return base.StartAsync(cancellationToken);
    }

    abstract protected Task Execute(CancellationToken stoppingToken);
    
    protected void OnError(Exception ex, int eventID = 0)
    {
        Logger.LogError(eventID, ex, "Exception: {0}", ex.Message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Logger.LogInformation(100, "Service started executing at: {time}", DateTimeOffset.Now);

            await Execute(stoppingToken);
            
            Logger.LogInformation(100, "Service finished executing at: {time}", DateTimeOffset.Now);
        
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning(100, "Service cancelled at: {time}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation(1000, "Stopping service at: {time}", DateTimeOffset.Now);
        return base.StopAsync(cancellationToken);
    }
    #endregion
}