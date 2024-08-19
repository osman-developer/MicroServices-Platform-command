using System.Diagnostics;
using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessing {
  public class EventProcessor : IEventProcessor {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;

    //here we can't inject the repo, bcz this interface,service should be singelton, wheres the repo is scoped
    //so the lifetime of eventprocessor is bigger than that of repo so it wont work, instead we use the scopefactory 
    //to access the repo
    public EventProcessor (IServiceScopeFactory scopeFactory, IMapper mapper) {
      _scopeFactory = scopeFactory;
      _mapper = mapper;
    }
    public void ProcessEvent (string message) {
      var eventType = DetermineEvent (message);

      switch (eventType) {
        case EventType.PlatformPublished:
          AddPlatform (message);
          break;
        default:
          break;
      }
    }

    private EventType DetermineEvent (string notificationMessage) {
      Console.WriteLine ("--> Determining Event");
      var eventType = JsonSerializer.Deserialize<GenericEventDto> (notificationMessage);

      switch (eventType.Event) {
        case "Platform_Published":
          Console.WriteLine ("--> Platform Publish Event Detected");
          return EventType.PlatformPublished;
        default:
          Console.WriteLine ("--> Could not determine event type");
          return EventType.Undetermined;
      }
    }

    private void AddPlatform (string platformPublishedMessage) {
      using (var scope = _scopeFactory.CreateScope ()) {
        var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo> ();
        var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto> (platformPublishedMessage);
        try {
          var plat = _mapper.Map<Platform> (platformPublishedDto);
          if (!repo.ExternalPlatformExist (plat.ExternalId)) {
            repo.CreatePlatform (plat);
            repo.SaveChanges ();
          } else {
            Console.WriteLine ("--> Platform already exists ..");
          }
        } catch (Exception ex) {
          Console.WriteLine ($"--> Could not add Platform to DB {ex.Message}");
        }
      }
    }
  }

  enum EventType {
    PlatformPublished,
    Undetermined
  }
}