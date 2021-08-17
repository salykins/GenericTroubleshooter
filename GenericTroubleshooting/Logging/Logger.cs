using Honeycomb;
using GenericTroubleshooting.Config;

namespace GenericTroubleshooting.Logging
{
    class Logger
    {
        private static Logger _instance;
        public static Logger Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                else
                {
                    return _instance = new Logger();
                }
            }
        }

        public Logger()
        {
            _logger = new LibHoney(writeKey: SolutionConfigs.Instance.GetConfig(configName: "HONEYCOMB_API_KEY")
                , dataSet: SolutionConstants.HoneyCombConfig.DataSets.debug.ToString());
        }

        private LibHoney _logger;
        public LibHoney HoneyComb
        {
            get
            {
                return _logger;
            }

            private set
            {
                _logger = value;
            }
        }
    }
}
