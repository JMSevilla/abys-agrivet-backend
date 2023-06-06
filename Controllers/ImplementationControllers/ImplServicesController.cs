using abys_agrivet_backend.BusinessLayer.Constructors;
using abys_agrivet_backend.Controllers.BaseControllers;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.ImplementationControllers;

public class ImplServicesController : BaseServicesController<Model.Services, BaseConstructorServices>
{
    public ImplServicesController(BaseConstructorServices repository) : base(repository){}
}