﻿using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Domain.Common.Interfaces
{
    public enum HandlerType 
    { 
        Capture,
        StopeAfter,
        StopIf,
        Pause,
        Read,  
    }
    public interface ISessionHandler: IDomainEntity, IBusinessEntity, IValidEntity
    {
       public HandlerType HandlerType { get;}
    }
}
