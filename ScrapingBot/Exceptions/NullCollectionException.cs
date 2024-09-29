using System;

namespace ScrapingBot.Exceptions;

public class NullCollectionException(string collectionName, string methodName, string url)
    : Exception($"The {collectionName} collection cannot be null in the method {methodName}, URL: {url}") {
}