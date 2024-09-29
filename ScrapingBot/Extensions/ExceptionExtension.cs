using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ScrapingBot.Extensions;

public static class ExceptionExtension {
    public static List<Exception> CaptureLogExceptions(this Exception exception, Action action, ILogger logger) {
        var exceptions = new List<Exception>();

        try {
            action();
        }
        catch(Exception ex) {
            exceptions.Add(ex);
        }

        exceptions.Add(exception);

        foreach(var ex in exceptions) {
            logger.LogError(ex.ToString());
        }

        return exceptions;
    }
}

