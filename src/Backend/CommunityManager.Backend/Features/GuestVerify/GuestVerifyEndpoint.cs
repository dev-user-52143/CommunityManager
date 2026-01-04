using CommunityManager.Backend.Features.GuestVerify.Enums;
using FastEndpoints;
using RabbitMQ.Client;
namespace CommunityManager.Backend.Features.GuestVerify;

public class GuestVerifyEndpoint : Endpoint<GuestVerifyRequest>
{
    private readonly ILogger<GuestVerifyEndpoint> _logger;

    public GuestVerifyEndpoint(ILogger<GuestVerifyEndpoint> logger)
    {
        _logger = logger;
    }
    
    public override void Configure()
    {
        Get("verify/guest");
        AllowAnonymous();
        Options(x => x.RequireCors(p => p.AllowAnyOrigin()));
    }

    public override async Task HandleAsync(GuestVerifyRequest req, CancellationToken ct)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        await Send.EventStreamAsync("verification-event", StartVerification(req,cts.Token), cts.Token);
    }

    private async IAsyncEnumerable<object> StartVerification(GuestVerifyRequest req, CancellationToken ct)
    {
        if(!ct.IsCancellationRequested)
            yield return new { Status =  VerificationState.Started};
        
        _logger.LogInformation("Guest verification started for user {AuthorizationCode}", req.AuthorizationCode);
        
        if(!ct.IsCancellationRequested)
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        
        if(!ct.IsCancellationRequested)
            yield return new { Status =  VerificationState.Completed};
    }
}

public class GuestVerifyRequest
{
    [FromQuery]
    public AuthData AuthorizationCode { get; set; }
}


public record AuthData(string code);