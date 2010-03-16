namespace dropkick.Engine
{
    using System;
    using System.IO;
    using DeploymentFinders;
    using log4net;
    using Settings;

    public static class Runner
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(Runner));
        static readonly SettingsParser _parser = new SettingsParser();

        public static void Deploy(string commandLine)
        {
            try
            {
                var newArgs = DeploymentCommandLineParser.Parse(commandLine);

                //what is the better way to state this?
                DeploymentFinder finder = newArgs.Deployment == "SEARCH" ?
                    new SearchesForAnAssemblyEndingInDeployment() :
                        newArgs.Deployment.EndsWith(".dll") || newArgs.Deployment.EndsWith(".exe") ?
                            new AssemblyWasSpecifiedAssumingOnlyOneDeploymentClass() :
                            (DeploymentFinder)new TypeWasSpecifiedAssumingItHasADefaultConstructor();


                var deployment = finder.Find(newArgs.Deployment);


                var pathToFile = Path.Combine(newArgs.SettingsDirectory,
                                              "{0}.settings".FormatWith(newArgs.Environment));

                _log.InfoFormat("Command: {0}", newArgs.Command);
                _log.InfoFormat("Deployment: {0}", newArgs.Deployment);
                _log.InfoFormat("Environment: {0}", newArgs.Environment);
                _log.InfoFormat("Role: {0}", newArgs.Role);
                _log.InfoFormat("ServerMappings: {0}", newArgs.ServerMappings);
                _log.InfoFormat("Settings Path: {0}", pathToFile);

                Console.WriteLine("Press enter to kick it out there");
                Console.ReadKey(true);


                var settings = _parser.Parse<object>(new FileInfo(pathToFile));
                deployment.Initialize(settings);
                DeploymentPlanDispatcher.KickItOutThereAlready(deployment, newArgs);

            }
            catch (Exception ex)
            {
                _log.Debug(commandLine);
                _log.Error(ex);
            }
        }
    }
}