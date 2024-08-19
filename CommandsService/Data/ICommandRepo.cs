using CommandsService.Models;
namespace CommandsService.Data {
    public interface ICommandRepo {
        bool SaveChanges ();

        //platforms
        IEnumerable<Platform> GetAllPlatforms ();
        void CreatePlatform (Platform platform);
        bool PlatformExists (int platformId);
        bool ExternalPlatformExist (int externalPlatformId);

        //commands
        IEnumerable<Command> GetCommandsForPlatform (int platformId);
        Command GetCommand (int platformId, int commandId);
        void CreateCommand (int platformId, Command command);
    }
}