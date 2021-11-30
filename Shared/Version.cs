namespace FileDB
{
    public class Shared
    {
        public static Version Version = new(2, 2);
    }

    public record Version(int Major, int Minor);
}
