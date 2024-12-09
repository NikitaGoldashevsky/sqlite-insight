using SQLite_Insight.View;
using System.Collections.Generic;

public static class SelectDialogStatic
{
    public static string? ShowDialog(string title, string query, List<string> options)
    {
        var selectDialog = new SelectDialog(title, query, options);
        return selectDialog.ShowDialog() == true ? selectDialog.SelectedOption : null;
    }
}