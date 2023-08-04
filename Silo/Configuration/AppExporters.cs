namespace Silo.Configuration
{
    public class AppExporters
    {
        public string AppInsightsConnectionString { get; set; } = string.Empty;

        public string GenevaConnectionString { get; set; } = string.Empty;

        // Default value for linux is Endpoint=unix:/var/etw/mdm_ifx.socket
        public string GenevaMetricsConnectionString { get; set; } = string.Empty;
    }
}
