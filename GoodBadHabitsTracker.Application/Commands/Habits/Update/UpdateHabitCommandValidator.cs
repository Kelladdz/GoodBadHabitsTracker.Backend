using FluentValidation;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Update
{
    internal sealed class UpdateHabitCommandValidator : AbstractValidator<UpdateHabitCommand>
    {
        public UpdateHabitCommandValidator()
        {
            var habitPropertiesNames = typeof(Habit).GetProperties()
                .Select(x => x.Name).ToList();
            var habitPathsPossibles = new List<string>();

            foreach (var habitPropertyName in habitPropertiesNames)
            {
                habitPathsPossibles.Add($"/{char.ToLower(habitPropertyName[0]) + habitPropertyName.Substring(1)}");
            }
            RuleForEach(x => x.Request.Operations)
                    .ChildRules(operation =>
                    {
                        ClassLevelCascadeMode = CascadeMode.Stop;
                        RuleLevelCascadeMode = CascadeMode.Stop;
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
                                    || (path.Split('/').Length == 2 && (!habitPathsPossibles.Contains(path) || !path.StartsWith("/")))
                                    || (path.Split('/').Length > 2 && (!Int32.TryParse(path.Split('/')[2], out int _) && path.Split('/')[2] != "-")))
                                    {
                                        context.AddFailure("Invalid path");
                                    }
                                });

                            operation.RuleFor(x => x.value)
                                .Cascade(CascadeMode.Stop)
                                .Custom((value, context) =>
                                {
                                    if (!context.InstanceToValidate.path.Contains('/'))
                                        context.AddFailure("Invalid path");

                                    else
                                    {
                                        var pathProperty = context.InstanceToValidate.path.Split('/')[1];
                                        pathProperty = char.ToUpper(pathProperty[0]) + pathProperty.Substring(1);
                                        var property = typeof(Habit).GetProperty(pathProperty);
                                        var propertyType = property!.PropertyType;

                                        if ((context.InstanceToValidate.OperationType == OperationType.Add
                                        || context.InstanceToValidate.OperationType == OperationType.Replace
                                        || context.InstanceToValidate.OperationType == OperationType.Test))
                                        {
                                            switch (pathProperty)
                                            {
                                                case "Name":
                                                    if (string.IsNullOrWhiteSpace(value.ToString())
                                                    || value.ToString()!.Length < 3 || value.ToString()!.Length > 50)
                                                    {
                                                        context.AddFailure("Invalid Name");
                                                    }
                                                    break;
                                                case "IconPath":
                                                    if (string.IsNullOrWhiteSpace(value.ToString())
                                                    || value.ToString()!.Length > 100)
                                                    {
                                                        context.AddFailure("Invalid IconPath");
                                                    }
                                                    break;
                                                case "HabitType":
                                                    context.AddFailure("HabitType is not allowed to be updated");
                                                    break;
                                                case "StartDate":
                                                    if (!DateTime.TryParse(value.ToString(), out DateTime date))
                                                    {
                                                        context.AddFailure("Invalid StartDate");
                                                    }
                                                    break;
                                                case "IsTimeBased":
                                                    context.AddFailure("IsTimeBased is not allowed to be updated");
                                                    break;
                                                case "Quantity":
                                                    if (!Int32.TryParse(value.ToString(), out _))
                                                    {
                                                        context.AddFailure("Invalid Quantity");
                                                    }
                                                    break;
                                                case "Frequency":
                                                    if (!Enum.IsDefined(typeof(Frequencies), value))
                                                    {
                                                        context.AddFailure("Invalid Frequency");
                                                    }
                                                    break;
                                                case "RepeatMode":
                                                    if (!Enum.IsDefined(typeof(RepeatModes), value))
                                                    {
                                                        context.AddFailure("Invalid RepeatMode");
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
                                                        context.AddFailure("Invalid interval value");
                                                    }
                                                    break;
                                                case "ReminderTimes":
                                                    if (!TimeOnly.TryParse(value.ToString(), out TimeOnly time))
                                                    {
                                                        context.AddFailure("Invalid time");
                                                    }
                                                    break;
                                                case "Comments":
                                                    var commentJObject = JObject.Parse(value.ToString()!);
                                                    var body = commentJObject["Body"]!.ToString();
                                                    if (body.Length > 255)
                                                        context.AddFailure("Comment is too long");
                                                    if (string.IsNullOrWhiteSpace(body))
                                                        context.AddFailure("Comment cannot be empty");
                                                    break;
                                                case "DayResults":
                                                    var dayResultsJObject = JObject.Parse(value.ToString()!);
                                                    var status = (Statuses)Enum.Parse(typeof(Statuses), dayResultsJObject["Status"]!.ToString());
                                                    var progress = (int?)dayResultsJObject["Progress"];

                                                    if (progress < 0)
                                                    {
                                                        context.AddFailure("Progress should be greater than or equal to 0");
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else if (context.InstanceToValidate.OperationType == OperationType.Remove)
                                        {
                                            if (pathProperty != "RepeatDaysOfMonth"
                                            && pathProperty != "RepeatDaysOfWeek"
                                            && pathProperty != "ReminderTimes"
                                            && pathProperty != "Comments"
                                            && pathProperty != "DayResults")
                                            {
                                                context.AddFailure($"{pathProperty} is not allowed to remove");
                                            }
                                        }
                                    }

                                });

                        });
                    });

        }

    }
}
