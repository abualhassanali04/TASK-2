namespace ProductCatalogApi.Models
{
    /// <summary>
    /// Entities that record who created/last changed them and when.
    /// Filled automatically by <see cref="Data.AuditInterceptor"/> — never set these by hand.
    /// All times are UTC.
    /// </summary>
    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string? UpdatedBy { get; set; }
    }
}
