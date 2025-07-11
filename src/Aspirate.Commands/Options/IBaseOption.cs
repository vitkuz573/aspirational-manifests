namespace Aspirate.Commands.Options;

public interface IBaseOption
{
    bool IsSecret { get; }

    object? GetOptionDefault();
}
