using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Model;

[Table("tp_auth")]
public class TPAuth : Itpauth
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid id { get; set; }
    public string key { get; set; }
    public string oauth { get; set; }
}