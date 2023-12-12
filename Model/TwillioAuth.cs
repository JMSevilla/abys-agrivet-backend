using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Model;

[Table("twillio_auth")]
public class TwillioAuth : ITwillioAuth
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid id { get; set; }
    public string accountSID { get; set; }
    public string authtoken { get; set; }
    public string identifier { get; set; }
}