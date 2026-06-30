namespace VentureOS.Domain.Common;

public readonly record struct Confidence
{
    private Confidence(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Confidence FromPercentage(int value)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Confidence must be between 0 and 100.");
        }

        return new Confidence(value);
    }
}