namespace Aspirate.Shared.Enums;

public class IngressController : SmartEnum<IngressController, string>
{
    private IngressController(string name, string value) : base(name, value)
    {
    }

    public static IngressController Nginx = new(nameof(Nginx), "nginx");
}
