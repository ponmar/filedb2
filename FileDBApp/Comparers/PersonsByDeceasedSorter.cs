using FileDBApp.ViewModel;

namespace FileDBApp.Comparers
{
    public class PersonsByDeceasedSorter : IComparer<DeceasedPerson>
    {
        public int Compare(DeceasedPerson x, DeceasedPerson y)
        {
            // TODO: sort by deceased date?
            return x.Header.CompareTo(y.Header);
        }
    }
}
