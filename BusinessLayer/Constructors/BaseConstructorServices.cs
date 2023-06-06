using abys_agrivet_backend.BusinessLayer.Logics;
using abys_agrivet_backend.DB;

namespace abys_agrivet_backend.BusinessLayer.Constructors;

public class BaseConstructorServices: EntityFrameworkServices<Model.Services, APIDBContext>
{
    public BaseConstructorServices(APIDBContext context) : base(context){}
}