using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Translator
{
    public class TranslationConfig
    {

        public Configuration Config { get; set; }
        public string GetAppSetting(string key)
        {
            KeyValueConfigurationElement element = Config.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }


        public TranslationConfig()
        {
            Config = null;
            string exeConfigPath = this.GetType().Assembly.Location;
            try
            {
                Config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
