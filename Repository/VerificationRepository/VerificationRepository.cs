using abys_agrivet_backend.Helper.VerificationCodeGenerator;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Repository.VerificationRepository;

public interface VerificationRepository<T> where T : class, IVerification
{
    public Task<dynamic> SMSVerificationDataManagement(T entity, VerificationParamsRequest verificationParamsRequest);
}