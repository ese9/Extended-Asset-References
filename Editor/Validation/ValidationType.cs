namespace Nine.AssetReferences.Editor.Validation
{
    public enum ValidationType
    {
        Valid = 0, Warning = 1, Error = 2
    }

    public record ValidationResult(ValidationType Type,
                                   string Message,
                                   IFixRequest Request = null);
}