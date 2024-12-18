using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Comments.Create
{
    internal sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.Request.Body)
                .NotEmpty().WithMessage("Comment body is required")
                .MaximumLength(255).WithMessage("Your comment is too long");
        }

    }
}
