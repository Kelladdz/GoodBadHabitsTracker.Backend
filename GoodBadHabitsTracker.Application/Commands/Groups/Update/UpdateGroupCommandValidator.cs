using FluentValidation;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Update
{
    internal sealed class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
    {
        public UpdateGroupCommandValidator()
        {
            var groupPropertiesNames = typeof(Group).GetProperties()
                .Select(x => x.Name).ToList();
            var groupPathsPossibles = new List<string>();

            foreach (var groupPropertyName in groupPropertiesNames)
            {
                groupPathsPossibles.Add($"/{char.ToLower(groupPropertyName[0]) + groupPropertyName.Substring(1)}");
            }

            RuleForEach(x => x.Request.Operations)
                    .ChildRules(operation =>
                    {
                        operation.When(x => x != null, () =>
                        {
                            operation.RuleFor(x => x.op)
                                .Cascade(CascadeMode.Stop)
                                .Must(x => x == "add" || x == "remove"
                                || x == "replace" || x == "move" || x == "copy"
                                || x == "test").WithMessage("Invalid operation type");

                            operation.RuleFor(x => x.path)
                                .Cascade(CascadeMode.Stop)
                                .Custom((path, context) =>
                                {
                                    if (!path.Contains('/')
                                        || path.Split('/').Length == 2 && (!groupPathsPossibles.Contains(path) || !path.StartsWith("/"))
                                        || path.Split('/').Length > 2 && (!Int32.TryParse(path.Split('/')[2], out int _) && path.Split('/')[2] != "-"))
                                    {
                                        context.AddFailure("Invalid path");
                                    }
                                });
                            operation.RuleFor(x => x.value)
                                .Cascade(CascadeMode.Stop)
                                .Custom((value, context) =>
                                {
                                    var pathProperty = context.InstanceToValidate.path.Split('/')[1];
                                    pathProperty = char.ToUpper(pathProperty[0]) + pathProperty.Substring(1);
                                    var property = typeof(Group).GetProperty(pathProperty);
                                    var propertyType = property!.PropertyType;

                                    if ((context.InstanceToValidate.OperationType == OperationType.Add
                                        || context.InstanceToValidate.OperationType == OperationType.Replace
                                        || context.InstanceToValidate.OperationType == OperationType.Test))
                                    {
                                        switch (pathProperty)
                                        {
                                            case "Name":
                                                if (string.IsNullOrWhiteSpace(value.ToString())
                                                || value.ToString()!.Length < 3 || value.ToString()!.Length > 15)
                                                    context.AddFailure("Invalid Name");
                                                break;
                                            case "UserId":
                                                context.AddFailure("UserId is not allowed to be updated");
                                                break;
                                            case "Habits":
                                                context.AddFailure("Habits is not allowed to be updated");
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                });
                        });
                    });
        }
       
    }
}
