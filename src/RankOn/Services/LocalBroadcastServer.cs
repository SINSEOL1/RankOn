using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RankOn.Services;

public sealed class LocalBroadcastServer
{
    private readonly OverlayStateService _stateService;
    private readonly int _port;
    private WebApplication? _app;

    public LocalBroadcastServer(OverlayStateService stateService, int port)
    {
        _stateService = stateService;
        _port = port;
    }

    public string Address => $"http://127.0.0.1:{_port}/overlay";
    public bool IsRunning => _app is not null;

    public async Task StartAsync()
    {
        if (_app is not null)
        {
            return;
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = Array.Empty<string>()
        });

        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://127.0.0.1:{_port}");
        builder.Services.AddSingleton(_stateService);

        var app = builder.Build();

        app.MapGet("/api/state", (OverlayStateService state) => Results.Json(state.Get()));
        app.MapGet("/overlay", () => Results.Content(OverlayHtml, "text/html; charset=utf-8"));

        await app.StartAsync();
        _app = app;
    }

    public async Task StopAsync()
    {
        if (_app is null)
        {
            return;
        }

        await _app.StopAsync();
        await _app.DisposeAsync();
        _app = null;
    }

    private const string OverlayHtml = """
<!doctype html>
<html lang="ko">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<style>
html,body{margin:0;width:100%;height:100%;overflow:hidden;background:transparent;font-family:Segoe UI,Arial,sans-serif}
#root{display:none;box-sizing:border-box;align-items:center;gap:16px;padding:14px 18px;color:#fff;background:rgba(16,18,24,.88);border:1px solid rgba(255,255,255,.10);border-radius:14px;width:max-content;min-width:430px}
.badge{width:62px;height:62px;border-radius:12px;background:rgba(255,255,255,.08);display:flex;align-items:center;justify-content:center;font-weight:700;font-size:13px;color:#b9c2d0}
.main{display:flex;flex-direction:column;gap:4px}
.name{font-size:15px;font-weight:700}
.rankline{display:flex;align-items:baseline;gap:8px}
.tier{font-size:18px;font-weight:700}
.rp,.place,.season{font-size:13px;color:#b9c2d0}
.session{margin-left:auto;padding-left:18px;font-size:18px;font-weight:700}
.positive{color:#70d6a6}
.negative{color:#ff8e8e}
</style>
</head>
<body>
<div id="root">
  <div class="badge">RANK</div>
  <div class="main">
    <div class="name" id="name"></div>
    <div class="rankline">
      <span class="tier" id="tier"></span>
      <span class="rp" id="rp"></span>
      <span class="place" id="place"></span>
    </div>
    <div class="season" id="season"></div>
  </div>
  <div class="session" id="session"></div>
</div>
<script>
const root=document.getElementById('root');
const set=(id,value)=>document.getElementById(id).textContent=value;
async function refresh(){
  try{
    const response=await fetch('/api/state',{cache:'no-store'});
    const state=await response.json();
    if(!state.hasData){root.style.display='none';return}
    root.style.display='flex';
    set('name',state.nickname);
    set('tier',state.tier);
    set('rp',state.rp.toLocaleString()+' RP');
    set('place','#'+state.rank.toLocaleString());
    set('season',state.seasonRemaining);
    const session=document.getElementById('session');
    const delta=state.sessionDelta;
    session.textContent=(delta>0?'+':'')+delta.toLocaleString()+' RP';
    session.className='session '+(delta>=0?'positive':'negative');
  }catch{}
}
refresh();
setInterval(refresh,1000);
</script>
</body>
</html>
""";
}
