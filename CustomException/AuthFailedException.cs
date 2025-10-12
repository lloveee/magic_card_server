namespace StdbModule.CustomException;

public class AuthFailedException(string message) : Exception(message);