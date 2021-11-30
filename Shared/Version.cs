namespace FileDB
{
    public class ReleaseInformation
    {
        public static Version Version = new(2, 2);
    }

    public record Version(int Major, int Minor);
}
