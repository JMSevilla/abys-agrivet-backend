namespace abys_agrivet_backend.Interfaces;
public interface IUsers
{
    int id { get; set; }
    string firstname { get; set; }
    string? middlename { get; set; }
    string lastname { get; set; }
    string username { get; set; }
    string email { get; set; }
    string password { get; set; }
    int branch { get; set; }
    string phoneNumber { get; set; }
    char status { get; set; }
    char verified { get; set; }
    string? imgurl { get; set; }
    int access_level { get; set; }
    DateTime created_at { get; set; }
    DateTime updated_at { get; set; }
}