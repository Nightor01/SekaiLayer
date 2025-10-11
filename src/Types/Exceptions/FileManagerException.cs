namespace SekaiLayer.Types.Exceptions;

/// <summary>
/// Exception notifying the caller that an operation with FileManager did not run successfully
/// </summary>
public class FileManagerException(string message) : Exception(message);