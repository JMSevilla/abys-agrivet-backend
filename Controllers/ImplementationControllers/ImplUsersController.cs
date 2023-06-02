using abys_agrivet_backend.Authentication;
using abys_agrivet_backend.BusinessLayer.Constructors;
using abys_agrivet_backend.Controllers.BaseControllers;
using abys_agrivet_backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.ImplementationControllers;
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class ImplUsersController : BaseUsersController<Users, BaseConstructorUsers>
{
   public ImplUsersController(BaseConstructorUsers repository) : base(repository) {}
}