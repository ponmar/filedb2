using FileDBApp.ViewModel;

namespace FileDBApp.Comparers
{
    public class PersonsByDaysLeftUntilBirthdaySorter : IComparer<Person>
    {
        public int Compare(Person x, Person y)
        {
            if (x.DaysLeft == y.DaysLeft)
            {
                return x.Name.CompareTo(y.Name);
            }

            return x.DaysLeft.CompareTo(y.DaysLeft);
        }
    }
}
