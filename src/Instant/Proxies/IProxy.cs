namespace Radical.Instant.Proxies;

using Rubrics;

public interface IProxy : IInstant
{
    IRubrics Rubrics { get; set; }

    object Target { get; set; }
}
