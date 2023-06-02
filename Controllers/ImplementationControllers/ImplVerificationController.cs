using abys_agrivet_backend.Authentication;
using abys_agrivet_backend.BusinessLayer.Constructors;
using abys_agrivet_backend.Controllers.BaseControllers;
using abys_agrivet_backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.ImplementationControllers;
[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class ImplVerificationController : BaseVerificationController<Verification, BaseConstructorVerification>
{
    public ImplVerificationController(BaseConstructorVerification repository) : base(repository) {}
}