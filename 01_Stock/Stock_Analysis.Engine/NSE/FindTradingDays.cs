namespace Stock_Analysis.Engine.NSE;

public class FindTradingDays
{
    public List<DateTime> GetDays(
        DateTime startDate,
        int numberOfDays)
    {
        var days =
            new List<DateTime>();

        DateTime currentDate =
            startDate.Date;

       
        currentDate =
            currentDate.AddDays(-1);

        while (days.Count < numberOfDays)
        {
            if (IsWeekday(currentDate))
            {
                days.Add(currentDate);
            }

            currentDate =
                currentDate.AddDays(-1);
        }

        return days;
    }

    private bool IsWeekday(
        DateTime date)
    {
        return date.DayOfWeek !=
                   DayOfWeek.Saturday &&
               date.DayOfWeek !=
                   DayOfWeek.Sunday;
    }
}