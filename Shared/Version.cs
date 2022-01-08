namespace FileDB
{
    public class ReleaseInformation
    {
        public static Version Version = new(2, 7);
    }

    public record Version(int Major, int Minor);
}
