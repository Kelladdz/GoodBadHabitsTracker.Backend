using FluentValidation.TestHelper;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests
{
    public abstract class ValidatorTestBase<TModel>
    {
        protected abstract TModel CreateValidObject();
        protected TestValidationResult<TModel> Validate(Action<TModel> mutate)
        {
            var model = CreateValidObject();
            mutate(model);
            var validator = CreateValidator();
            return validator.TestValidate(model);
        }
        protected abstract IValidator<TModel> CreateValidator();
    }
}
