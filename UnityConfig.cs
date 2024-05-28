using PCiServer.DEM.Web.Api.Modules.Admin.Commands;
using PCiServer.DEM.Web.Api.Modules.Admin.Services;
using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace PCiServer.DEM.Web.Api
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            container.RegisterType<LoginCommand>();
            container.RegisterType<ILoginService, LoginService>();
            container.RegisterType<CreateGroupCommand>();
            container.RegisterType<IGroupService, GroupService>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}
