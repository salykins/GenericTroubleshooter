using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace GenericTroubleshooting.Config
{
    class SolutionConfigs
    {
        IConfigurationRoot config;
        private static SolutionConfigs _instance;
        public static SolutionConfigs Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                else
                {
                    return _instance = new SolutionConfigs();
                }
            }
        }

        private SolutionConfigs()
        {

        }

        public ExecutionContext ExecContext { get; set; }

        public string GetConfig(string configName)
        {
            if (config == null) BuildConfig(context: ExecContext);
            return config[configName];
        }

        private void BuildConfig(ExecutionContext context)
        {
            config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}

