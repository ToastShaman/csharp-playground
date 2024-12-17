using System.ComponentModel.DataAnnotations;

namespace OpenMeteo;

public class OpenMeteoConfig
{
    [Required]
    public required Uri BaseUrl { get; set; }

    [Required]
    public required TimeSpan Timeout { get; set; }

    [Required]
    public required TimeSpan RetryDelay { get; set; }

    [Required]
    public required TimeSpan MaxDelay { get; set; }

    [Required]
    [Range(1, 10)]
    public required int MaxRetryAttempts { get; set; }

    public override string ToString() =>
        $"BaseUrl: {BaseUrl}, Timeout: {Timeout}, RetryDelay: {RetryDelay}, MaxDelay: {MaxDelay}, MaxRetryAttempts: {MaxRetryAttempts}";
}
