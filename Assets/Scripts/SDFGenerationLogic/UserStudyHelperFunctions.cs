public static class UserStudyHelperFunctions
{
    public static string GenerateParticipantID(int index, string prepend = "")
    {
        return prepend + index.ToString("000");
    }
}
