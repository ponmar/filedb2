using GoogleCast.Channels;
using GoogleCast.Models.Media;
using GoogleCast;
using System.Linq;
using System.Threading.Tasks;
using FileDBInterface.Model;

namespace FileDB.FileBrowsingPlugins
{
    public class Cast : IBrowsingPlugin
    {
        private readonly string castUrl;

        public Cast(string httpServerInterface, int httpServerPort)
        {
            castUrl = $"http://{httpServerInterface}:{httpServerPort}/filedb/file/something";
        }

        public void FileLoaded(FilesModel file)
        {
            _ = Instance_FileLoadedEventAsync(file);
        }

        private async Task Instance_FileLoadedEventAsync(FilesModel file)
{
            var absFilePath = model.FilesystemAccess.ToAbsolutePath(file.Path);
            FileCaster.LoadFile(absFilePath);

            // TODO: make sender and receiver members of this class (only connect once when program start)

            var receiver = (await new DeviceLocator().FindReceiversAsync()).First();

            var sender = new Sender();
            await sender.ConnectAsync(receiver);
            var mediaChannel = sender.GetChannel<IMediaChannel>();
            await sender.LaunchAsync(mediaChannel);

            var mediaStatus = await mediaChannel.LoadAsync(new MediaInformation() { ContentId = castUrl });
            await mediaChannel.PlayAsync();
        }
    }
}
