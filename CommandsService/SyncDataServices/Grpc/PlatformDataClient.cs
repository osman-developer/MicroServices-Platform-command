using AutoMapper;
using CommandsService.Models;
using Grpc.Net.Client;
using PlatformService;

namespace CommandsService.SyncDataServices.Grpc {
  public class PlatformDataClient : IIPlatformDataClient {
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public PlatformDataClient (IConfiguration configuration, IMapper mapper) {
      _configuration = configuration;
      _mapper = mapper;
    }
    public IEnumerable<Platform> ReturnAllPlatforms () {
      Console.WriteLine ($"--> Calling the GRPC Servie {_configuration["GrpcPlatform"]}");
      var channel = GrpcChannel.ForAddress (_configuration["GrpcPlatform"]);
      var client = new GrpcPlatform.GrpcPlatformClient (channel);
      var request = new GetAllRequests ();

      try {
        var reply = client.GetAllPlatforms (request);
        return _mapper.Map<IEnumerable<Platform>> (reply.Platform);
      } catch (Exception e) {
        Console.WriteLine ($"Couldn't call GRPC Server {e.Message}");
        return null;
      }
    }
  }
}