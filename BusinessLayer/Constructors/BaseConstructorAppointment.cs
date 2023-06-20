﻿using abys_agrivet_backend.BusinessLayer.Logics;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.MailSettings;
using abys_agrivet_backend.Model;
using Microsoft.Extensions.Options;

namespace abys_agrivet_backend.BusinessLayer.Constructors;

public class BaseConstructorAppointment : EntityFrameworkAppointment<Appointment, APIDBContext>
{
    public BaseConstructorAppointment(APIDBContext context, IOptions<MailSettings> mailSettings) : base(context, mailSettings){}
}