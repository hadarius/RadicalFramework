namespace Radical.Instant.Proxies;

using Rubrics;

public interface IInnerProxy : IInstant
{
    IRubrics Rubrics { get; }

    IProxy Proxy { get; set; }
}