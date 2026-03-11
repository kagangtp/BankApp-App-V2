namespace IlkProjem.Core.Utilities.Helpers;

public static class ValidationHelpers
{
    public static bool BeValidTcKimlik(string tc)
{
    if (string.IsNullOrEmpty(tc) || tc.Length != 11 || tc[0] == '0') return false;

    int[] d = tc.Select(c => c - '0').ToArray();

    int oddSum = d[0] + d[2] + d[4] + d[6] + d[8];
    
    int evenSum = d[1] + d[3] + d[5] + d[7];

    int firstCheck = ((oddSum * 7) - evenSum) % 10;
    if (firstCheck != d[9]) return false;

    int secondCheck = d.Take(10).Sum() % 10;
    if (secondCheck != d[10]) return false;

    return true;
}
}