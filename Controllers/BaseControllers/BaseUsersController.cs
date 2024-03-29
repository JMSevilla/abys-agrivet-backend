﻿using abys_agrivet_backend.Helper.ForgotPassword;
using abys_agrivet_backend.Helper.JWT;
using abys_agrivet_backend.Helper.LoginParams;
using abys_agrivet_backend.Helper.UsersProps;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.UsersRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.BaseControllers;

[Route("api/[controller]")]
[ApiController]
public class BaseUsersController<TEntity, TRepository> : ControllerBase
where TEntity : class, IUsers
where TRepository : UsersRepository<TEntity>
{
    private readonly TRepository _repository;

    public BaseUsersController(TRepository repository)
    {
        this._repository = repository;
    }

    [Route("check-users"), HttpGet]
    public async Task<IActionResult> checkUsersTable()
    {
        bool result = await _repository.SetupApplicationFindAnyUsersFromDB();
        if (result)
        {
            return Ok("exist");
        }

        return Ok("not_exist");
    }

    [Route("add-new-user-setup"), HttpPost]
    public async Task<IActionResult> AddNewUserSetup([FromBody] TEntity entity)
    {
        await _repository.SetupAccountFirstUserOfTheApplication(entity);
        return Ok(200);
    }

    [Route("login"), HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginParameters loginParameters)
    {
        var result = await _repository.AccountSigningIn(loginParameters);
        return Ok(result);
    }

    [Route("refresh-token"), HttpPost]
    public async Task<IActionResult> RefreshToken([FromBody] AccessWithRefresh accessWithRefresh)
    {
        var result = await _repository.RefreshToken(accessWithRefresh);
        return Ok(result);
    }
    
    [Route("uam-add-new-user"), HttpPost]
    public async Task<IActionResult> UAMCreateNewUser([FromBody] TEntity entity)
    {
        var result = await _repository.UAM(entity);
        return Ok(result);
    }
    
    [Authorize]
    [Route("uam-get-all"), HttpGet]
    public ActionResult UAMGetAll()
    {
        return Ok(_repository.UAMGetAll());
    }

    [Route("customer-account-registration"), HttpPost]
    public async Task<IActionResult> CustomerAccountCreation([FromBody] TEntity entity)
    {
        var result = await _repository.CustomerAccountRegistration(entity);
        return Ok(result);
    }

    [Route("check-email-users/{email}"), HttpGet]
    public async Task<IActionResult> CheckEmails([FromRoute] string email)
    {
        var result = await _repository.ReUsableCheckingEmail(email);
        return Ok(result);
    }

    [Route("change-password"), HttpPut]
    public async Task<IActionResult> ChangePassword([FromBody] ForgotPasswordParams forgotPasswordParams)
    {
        var result = await _repository.ChangePassword(forgotPasswordParams);
        return Ok(result);
    }

    [Route("uam-delete-user/{id}"), HttpDelete]
    public async Task<IActionResult> UAMDeleteUser([FromRoute] int id)
    {
        var result = await _repository.DeleteUser(id);
        return Ok(result);
    }

    [Route("update-profile-user"), HttpPut]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UsersParameters usersParameters)
    {
        var result = await _repository.UpdateProfile(usersParameters);
        return Ok(result);
    }

    [Route("filter-uam-by-accesslevel/{access_level}"), HttpGet]
    public async Task<IActionResult> FilteredAccessLevel([FromRoute] int access_level)
    {
        var result = await _repository.FilterByAccessLevel(access_level);
        return Ok(result);
    }

    [Route("filter-services/{branch_id}"), HttpGet]
    public async Task<IActionResult> FilterServices([FromRoute] int branch_id)
    {
        var result = await _repository.FilterServicesByBranch(branch_id);
        return Ok(result);
    }
}