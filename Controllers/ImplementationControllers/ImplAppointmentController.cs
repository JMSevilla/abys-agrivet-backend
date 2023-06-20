using abys_agrivet_backend.BusinessLayer.Constructors;
using abys_agrivet_backend.Controllers.BaseControllers;
using abys_agrivet_backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.ImplementationControllers;

public class ImplAppointmentController : BaseAppointmentController<Appointment, BaseConstructorAppointment>
{
    public ImplAppointmentController(BaseConstructorAppointment repository) : base(repository){}
}