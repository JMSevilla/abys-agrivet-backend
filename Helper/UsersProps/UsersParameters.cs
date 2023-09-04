﻿namespace abys_agrivet_backend.Helper.UsersProps;

public class UsersParameters
{
    public int id { get; set; }
    public string firstname { get; set; }
    public string? middlename { get; set; }
    public string lastname { get; set; }
    public string username { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string? newPassword { get; set; }
    public int branch { get; set; }
    public string phoneNumber { get; set; }
    public char status { get; set; }
    public char verified { get; set; }
    public string? imgurl { get; set; }
    public int access_level { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}