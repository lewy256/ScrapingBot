using System;

namespace ScrapingBot.Exceptions;

public class EmptyCollectionException(string collectionName, string methodName, string url)
    : Exception($"The {collectionName} collection is empty in method {methodName}, URL: {url}") {
}