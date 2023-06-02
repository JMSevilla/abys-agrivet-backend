using abys_agrivet_backend.BusinessLayer.Logics;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Model;

namespace abys_agrivet_backend.BusinessLayer.Constructors;

public class BaseConstructorVerification: EntityFrameworkVerification<Verification, APIDBContext>
{
    public BaseConstructorVerification(APIDBContext context) : base(context) {}
}