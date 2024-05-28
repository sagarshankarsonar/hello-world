using PCiServer.DEM.Web.Api.Common;
using PCiServer.DEM.Web.Api.Common.Identity;
using PCiServer.DEM.Web.Api.Modules.Admin.Models;
using PCiServer.DEM.Web.Api.Modules.Admin.Commands;
using PCiServer.Framework.Web.Interfaces;
using System;
using System.Collections.Generic;
using PCiServer.DEM.Web.Api.Modules.Admin.Utils;

namespace PCiServer.DEM.Web.Api.Modules.Admin.Services
{
    public class LoginService : ILoginService
    {
        private const string MODULE = "Admin";

        private readonly LoginCommand _loginCommand;

        public LoginService(LoginCommand loginCommand)
        {
            this._loginCommand = loginCommand;
        }

        public LoginResponseModel AuthenticateUser(LoginData loginData)
        {
            if (!ValidateInput(loginData, out List<string> errorMessages))
            {
                return new LoginResponseModel
                {
                    status = "fail",
                    messages = errorMessages
                };
            }


            Commands.LoginCommand.Input input = new Commands.LoginCommand.Input();
            input.Login = loginData.userName;
            input.Password = loginData.password;
            IActionResult result = _loginCommand.Execute(input);
            if (result.IsSuccessful)
            {
                string token = TokenManager.GenerateToken(UserUtils.GetLoginUser(result), MODULE);
                return new LoginResponseModel
                {
                    status = "success",
                    data = new ResponseData
                    {
                        accessToken = token
                    }
                };
            }
            else
            {
                return new LoginResponseModel
                {
                    status = "fail",
                    messages = new List<string> { result.ErrorMessage }
                };
            }

        }

        private bool ValidateInput(LoginData loginData, out List<string> errorMessages)
        {
            bool isValid = true;
            errorMessages = new List<string>();

            if (string.IsNullOrEmpty(loginData.userName))
            {
                errorMessages.Add("User Name is required");
                isValid = false;
            }

            if (string.IsNullOrEmpty(loginData.password))
            {
                errorMessages.Add("Password is required");
                isValid = false;
            }

            return isValid;
        }

    }
}
