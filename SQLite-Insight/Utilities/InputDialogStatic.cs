using SQLite_Insight.View;

public static class InputDialogStatic
{
    public static string? ShowDialog(string title, string query = "Enter the text")
    {
        var inputDialog = new InputDialog(title, query);
        return inputDialog.ShowDialog() == true ? inputDialog.InputText : null;
    }
}