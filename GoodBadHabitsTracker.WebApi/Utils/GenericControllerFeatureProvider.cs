using GoodBadHabitsTracker.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace GoodBadHabitsTracker.WebApi.Utils
{
/*    public class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    { 
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var modelType in EntityTypes.ModelTypes)
            {
                var entityType = modelType.Key;
                var entityRequestType = modelType.Value[0];
                var entityResponseType = modelType.Value[1];
                
                Type[] typeArgs = { entityType, entityRequestType, entityResponseType };
                var controllerType = typeof(GenericController<,,>).MakeGenericType(typeArgs).GetTypeInfo();
                feature.Controllers.Add(controllerType);
            }
        }
    }*/
}

