namespace Aspirate.Commands.Options;

public abstract class BaseOption<T> : Option<T>, IBaseOption
{
    private readonly string _envName;
    private readonly T _defaultValue;

    public abstract bool IsSecret { get; }

    protected BaseOption(string name, string[] aliases, string envName, T defaultValue) : base(name, aliases)
    {
        _envName = envName;
        _defaultValue = defaultValue;

        DefaultValueFactory = _ => GetOptionDefault(_envName, _defaultValue)();
    }

    public T GetOptionDefault() => GetOptionDefault(_envName, _defaultValue)();

    object? IBaseOption.GetOptionDefault() => GetOptionDefault();

    private static Func<TReturnValue> GetOptionDefault<TReturnValue>(string envVarName, TReturnValue defaultValue) => () =>
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
