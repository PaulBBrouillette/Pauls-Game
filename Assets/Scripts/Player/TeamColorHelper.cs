using UnityEngine;

public static class TeamColorHelper {
    private static readonly string one = "#D00000";
    private static readonly string two = "#337CA0";
    private static readonly string three = "#4DA167";
    private static readonly string four = "#FFBA08";
    static Color color;

    internal static Color GetTeamColor(Team team) {
        if (team == Team.One) if (UnityEngine.ColorUtility.TryParseHtmlString(one, out color)) return color;
        if (team == Team.Two) if (UnityEngine.ColorUtility.TryParseHtmlString(two, out color)) return color;
        if (team == Team.Three) if (UnityEngine.ColorUtility.TryParseHtmlString(three, out color)) return color;
        if (team == Team.Four) if (UnityEngine.ColorUtility.TryParseHtmlString(four, out color)) return color;
        return Color.black;
    }
}
