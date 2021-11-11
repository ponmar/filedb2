namespace FileDB
{
    public class Shared
    {
        public static Version Version = new(2, 1);
    }

    public record Version(int Major, int Minor);
}
