namespace abys_agrivet_backend.Interfaces;

public interface Itpauth
{
    Guid id { get; set; }
    string key { get; set; }
    string oauth { get; set; }
}