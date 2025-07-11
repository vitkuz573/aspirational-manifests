AspirateCli.WelcomeMessage();

var configuration = new CommandLineConfiguration(new AspirateCli());
return await configuration.InvokeAsync(args);
