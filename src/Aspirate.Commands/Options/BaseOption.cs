namespace Aspirate.Commands.Options;

public abstract class BaseOption<T>(
    string[] aliases,
    string envName,
    T defaultValue) :
        Option<T>(
            aliases,
            getDefaultValue: GetOptionDefault(envName, defaultValue)),
        IBaseOption
{
    public abstract bool IsSecret { get; }

    public T GetOptionDefault() => GetOptionDefault(envName, defaultValue)();

    object? IBaseOption.GetOptionDefault() => GetOptionDefault();

    private static Func<TReturnValue> GetOptionDefault<TReturnValue>(string envVarName, TReturnValue defaultValue) =>
        () =>
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            if (envVarName == "ASPIRATE_SECRET_PASSWORD")
            {
                Environment.SetEnvironmentVariable(envVarName, null);
            }
            if (envValue == null)
            {
                return defaultValue;
            }

            try
            {
                return (TReturnValue)Convert.ChangeType(envValue, typeof(TReturnValue));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        };
}
