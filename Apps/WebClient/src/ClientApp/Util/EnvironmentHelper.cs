using System;
using System.Collections;

namespace WebClient
{
    public interface IEnvironment
    {
        string GetValue(string key);
    }

    public class EnvironmentManager : IEnvironment
    {
        private Hashtable environmentVariables;

        public string GetValue(string key)
        {
            if (environmentVariables == null)
            {
                loadEnvironmentVariables();
            }

            return (string)environmentVariables[key];
        }

        private void loadEnvironmentVariables()
        {
            environmentVariables = (Hashtable)Environment.GetEnvironmentVariables();
        }
    }
}