namespace Stock_Analysis.Engine.NSE;

public class FindTradingDays
{
    public List<DateTime> GetDays(DateTime startDate, int numberOfDays)
    {
        var days = new List<DateTime>();

        DateTime currentDate = startDate;

        while (days.Count < numberOfDays)
        {
            if (IsTradingDay(currentDate))
            {
                days.Add(currentDate);
            }

            currentDate = currentDate.AddDays(-1);
        }

        return days;
    }

    private bool IsTradingDay(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday)
        {
            return false;
        }

        if (date.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        return true;
    }
}