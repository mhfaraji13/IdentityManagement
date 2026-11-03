using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.Services
{
    public interface ICookieValidatorService
    {
        Task ValidateAsync(CookieValidatePrincipalContext context);
    }
}
