using GoodBadHabitsTracker.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GoodBadHabitsTracker.WebApi.Conventions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class GenericControllerConventions : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if(!controller.ControllerType.IsGenericType || controller.ControllerType.GetGenericTypeDefinition() != typeof(GenericController<,,>))
                return;

            var entityType = controller.ControllerType.GenericTypeArguments[0];
            controller.ControllerName = entityType.Name + "s";
            controller.RouteValues["Controller"] = entityType.Name + "s";
        }
    }
}
