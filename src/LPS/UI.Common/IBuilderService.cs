using LPS.Domain.Common.Interfaces;
using LPS.DTOs;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LPS.UI.Common
{
    internal interface IBuilderService<TDto> where TDto : IDto<TDto>
    {
        TDto Build(ref TDto dto);
    }
}
