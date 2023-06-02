﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using abys_agrivet_backend.Helper.JWT;
using abys_agrivet_backend.Helper.LoginParams;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Repository.UsersRepository;

public interface UsersRepository<T> where T : class, IUsers
{
    public Task<Boolean> SetupApplicationFindAnyUsersFromDB();
    public Task<T> SetupAccountFirstUserOfTheApplication(T entity);

    public Task<dynamic> UAM(T entity);
    public List<T> UAMGetAll();
    public Task<dynamic> AccountSigningIn(LoginParameters loginParameters);

    public JwtSecurityToken CreateToken(List<Claim> claims);
    public string GenerateRefreshToken();
    public Task<dynamic> RefreshToken(AccessWithRefresh accessWithRefresh);
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);

    public Task<dynamic> CustomerAccountRegistration(T entity);
    public Task<dynamic> JWTAccountCreation(JWTCredentials jwtCredentials);
}