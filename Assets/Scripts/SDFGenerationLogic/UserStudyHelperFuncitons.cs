public static class UserStudyHelperFuncitons
{
    public static string GenerateParticipantID(int index, string prepend = "")
    {
        return prepend + index.ToString("000");
    }
}
