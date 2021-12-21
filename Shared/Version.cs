namespace FileDB
{
    public class ReleaseInformation
    {
        public static Version Version = new(2, 4);
    }

    public record Version(int Major, int Minor);
}
