﻿using Ops.Exchange.Forwarder;

namespace Ops.Host.Core.Services;

public interface IAlarmService
{
    Task HandleAsync(ForwardData data);
}
