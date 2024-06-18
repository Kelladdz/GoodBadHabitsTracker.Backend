using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Auth.Response
{
     public record LoginResponse(string AccessToken, string RefreshToken, string UserFingerprint);
}
