using LPS.Domain.Common.Interfaces;
using LPS.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace LPS.UI.Common
{
    internal interface IChallengeUserService<TDto> where TDto : IDto<TDto>
    {
        bool SkipOptionalFields { get; }
        TDto Dto { get;}
        public void Challenge();
        public void ForceOptionalFields();

    }
}
