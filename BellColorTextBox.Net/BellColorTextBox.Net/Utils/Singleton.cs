namespace Bell.Utils;

internal static class Singleton
{
    private static readonly ThreadLocal<TextBox> ThreadLocalTextBox = new();
    internal static TextBox TextBox
    {
        get => ThreadLocalTextBox.Value ?? throw new Exception("No TextBox set");
        set => ThreadLocalTextBox.Value = value;
    }
}