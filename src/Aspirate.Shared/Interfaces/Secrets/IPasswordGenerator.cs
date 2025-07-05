namespace Aspirate.Shared.Interfaces.Secrets;

public interface IPasswordGenerator
{
    string Generate(Generate options);

    bool Validate(string value, Generate options);
}
