﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Services.RefreshTokenHandler
{
    public interface IRefreshTokenHandler
    {
        public string GenerateRefreshToken();
    }
}
