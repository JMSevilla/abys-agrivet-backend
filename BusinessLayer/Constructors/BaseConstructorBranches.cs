using abys_agrivet_backend.BusinessLayer.Logics;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Model;
using Microsoft.AspNetCore.Components;

namespace abys_agrivet_backend.BusinessLayer.Constructors;

public class BaseConstructorBranches : EntityFrameworkBranches<Branch, APIDBContext>
{
    public BaseConstructorBranches(APIDBContext context): base(context) {}
}