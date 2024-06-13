using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public class RegisterRequest
    {
        public string? Email { get; init; }
        public string? Name { get; init; }
        public string? Password { get; init; }
        public string? ConfirmPassword { get; init; }
    }
}
