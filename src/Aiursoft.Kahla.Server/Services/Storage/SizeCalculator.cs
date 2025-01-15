namespace Aiursoft.Kahla.Server.Services.Storage;

/// <summary>
/// Provides functionality to calculate size based on powers of two.
/// </summary>
/// <remarks>
/// This class is used to compute size values by finding the smallest
/// power of two that is greater than or equal to the input.
/// </remarks>
public static class SizeCalculator
{
    private static IEnumerable<int> GetTwoPowers()
    {
        yield return 0;

        // 16384
        for (var i = 1; i <= 0x4000; i *= 2)
        {
            yield return i;
        }
    }

    /// <summary>
    /// Calculates the smallest power of two that is greater than or equal to the specified input value.
    /// </summary>
    /// <param name="input">The input integer for which the smallest greater or equal power of two is to be calculated. Must be less than or equal to 16384.</param>
    /// <returns>The smallest power of two that is greater than or equal to the input value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the calculation fails due to unexpected conditions.</exception>
    public static int Ceiling(int input)
    {
        if (input >= 0x4000)
        {
            return 0x4000;
        }

        foreach (var optional in GetTwoPowers())
        {
            if (optional >= input)
            {
                return optional;
            }
        }

        // Logic shall not reach here.
        throw new InvalidOperationException($"Image size calculation failed with input: {input}.");
    }
}