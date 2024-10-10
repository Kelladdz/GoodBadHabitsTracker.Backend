using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using CommandLine;
using Newtonsoft.Json.Linq;



namespace GoodBadHabitsTracker.Application.Commands.Generic.Update
{
    internal sealed class UpdateCommandValidator<TEntity> : AbstractValidator<UpdateCommand<TEntity>> 
        where TEntity : class
    {
        public UpdateCommandValidator()
        {
            var entityPropertiesNames = Activator.CreateInstance<TEntity>()!.GetType().GetProperties()
                .Select(x => x.Name).ToList();

            var pathsPossibles = new List<string>();

            foreach (var entityPropertyName in entityPropertiesNames)
            {
                pathsPossibles.Add(char.ToLower(entityPropertyName[0]) + entityPropertyName.Substring(1));
            }

            RuleFor(x => x.Id)
                .NotNull().WithMessage("Id is required")
                .Must(x => x.GetType() == typeof(Guid)).WithMessage("Id must be a valid GUID");
            RuleForEach(x => (x.Request as JsonPatchDocument<Habit>)!.Operations)
                .ChildRules(operation =>
                {
                    operation.RuleFor(x => x)
                        .NotNull().WithMessage("Operation is required");
                    operation.RuleFor(x => x.op)
                        .NotNull().WithMessage("Operation type is required")
                        .NotEmpty().WithMessage("Operation type is required")
                        .Must(x => x == "add" || x == "remove"
                        || x == "replace" || x == "move" || x == "copy"
                        || x == "test").WithMessage("Invalid operation type");

                    operation.RuleFor(x => x.path)
                        .NotNull().WithMessage("Path is required")
                        .NotEmpty().WithMessage("Path is required")
                        .Custom((path, context) =>
                        {
                            if ((!pathsPossibles.Contains(path.Split('/')[1]) || !path.StartsWith("/"))
                                || (path.Split('/').Length > 2 && !Int32.TryParse(path.Split('/')[2], out int _) && path.Split('/')[2] != "-"))
                            {
                                context.AddFailure("Invalid path");
                            }
                        });

                    operation.RuleFor(x => x.value)
                        .Custom((value, context) =>
                        {
                            if ((context.InstanceToValidate.OperationType == OperationType.Add
                            || context.InstanceToValidate.OperationType == OperationType.Replace
                            || context.InstanceToValidate.OperationType == OperationType.Test))
                            {
                                if (value == null)
                                {
                                    context.AddFailure("Value is required");
                                }
                                else
                                {
                                    var pathProperty = context.InstanceToValidate.path.Split('/')[1];
                                    pathProperty = char.ToUpper(pathProperty[0]) + pathProperty.Substring(1);
                                    var property = typeof(Habit).GetProperty(pathProperty);
                                    

                                    if (property is null)
                                    {
                                        context.AddFailure("Invalid path");
                                    }

                                    var propertyType = property!.PropertyType;
                                    if (propertyType.IsPrimitive && value.GetType() != propertyType)
                                    {
                                        context.AddFailure($"Invalid {pathProperty}");
                                    }

                                    switch (pathProperty)
                                    {
                                        case "Name":
                                            if (typeof(TEntity) == typeof(Habit) && string.IsNullOrWhiteSpace(value.ToString())
                                            || value.ToString()!.Length < 3 || value.ToString()!.Length > 50)
                                            {
                                                context.AddFailure("Invalid name");
                                            } else if (typeof(TEntity) == typeof(Group) && string.IsNullOrWhiteSpace(value.ToString())
                                            || value.ToString()!.Length < 3 || value.ToString()!.Length > 15)
                                            {
                                                context.AddFailure("Invalid name");
                                            }
                                            break;
                                        case "IconPath":
                                            if (string.IsNullOrWhiteSpace(value.ToString())
                                            || value.ToString()!.Length > 100)
                                            {
                                                context.AddFailure("Invalid icon path");
                                            }
                                            break;
                                        case "HabitType":
                                            if (!Enum.IsDefined(typeof(HabitTypes), value))
                                            {
                                                context.AddFailure("Invalid habit type");
                                            }
                                            break;
                                        case "StartDate":
                                            if (!DateTime.TryParse(value.ToString(), out DateTime date))
                                            {
                                                context.AddFailure("Invalid start date");
                                            }
                                            break;
                                        case "IsTimeBased":
                                            context.AddFailure("IsTimeBased is not allowed to be updated");
                                            break;
                                        case "Quantity":
                                            if (!Int32.TryParse(value.ToString(), out _))
                                            {
                                                context.AddFailure("Invalid quantity");
                                            }
                                            break;
                                        case "Frequency":
                                            if (!Enum.IsDefined(typeof(Frequencies), value))
                                            {
                                                context.AddFailure("Invalid frequency");
                                            }
                                            break;
                                        case "RepeatMode":
                                            if (!Enum.IsDefined(typeof(RepeatModes), value))
                                            {
                                                context.AddFailure("Invalid repeat mode");
                                            }
                                            break;
                                        case "RepeatDaysOfMonth":
                                            if (!Int32.TryParse(value.ToString(), out int day)
                                                || day < 1 || day > 31)
                                            {
                                                context.AddFailure("Invalid day of month");
                                            }
                                            break;
                                        case "RepeatDaysOfWeek":
                                            if (!Enum.IsDefined(typeof(DayOfWeek), value))
                                            {
                                                context.AddFailure("Invalid day of week");
                                            }
                                            break;
                                        case "RepeatInterval":
                                            if (!Int32.TryParse(value.ToString(), out int interval)
                                                || interval < 1 || interval > 8)
                                            {
                                                context.AddFailure("Invalid interval");
                                            }
                                            break;
                                        case "ReminderTimes":
                                            if (!TimeOnly.TryParse(value.ToString(), out TimeOnly time))
                                            {
                                                context.AddFailure("Invalid time");
                                            }
                                            break;
                                        case "Comments":
                                            if (Activator.CreateInstance<Comment>().GetType().GetProperty("Body").GetValue(value).ToString().Length > 255)
                                                context.AddFailure("Comment is too long");
                                            if (string.IsNullOrWhiteSpace(Activator.CreateInstance<Comment>().GetType().GetProperty("Body").GetValue(value).ToString()))
                                                context.AddFailure("Comment cannot be empty");
                                            break;
                                        case "DayResults":
                                            var dayResultsJObject = JObject.Parse(value.ToString());
                                            var status = (Statuses)Enum.Parse(typeof(Statuses), dayResultsJObject["status"].ToString());
                                            var progress = (int)dayResultsJObject["progress"];

                                            if (status == null || progress == null)
                                            {
                                                context.AddFailure("Invalid DayResult properties");
                                                break;
                                            }

                                            if (!Enum.IsDefined(typeof(Statuses), status!))
                                            {
                                                context.AddFailure("Invalid DayResult status");
                                            }

                                            if (progress < 0)
                                            {
                                                context.AddFailure("Progress should be greater than or equal to 0");
                                            }
                                            break;
                                        default:
                                            context.AddFailure("Invalid value");
                                            break;
                                    }
                                }
                            }
                        });
                });
        }
    }   
}
