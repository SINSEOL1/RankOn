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
        if (_app is not null) return;

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
        if (_app is null) return;

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
#root{display:none;box-sizing:border-box;align-items:center;gap:16px;padding:14px 18px;color:#fff;width:max-content;min-width:430px;transform-origin:top left}
#root.vertical{flex-direction:column;text-align:center;min-width:190px}
#root.vertical .main{align-items:center}
#root.vertical .rankline{justify-content:center}
#root.vertical .session{margin-left:0;padding-left:0}
.badge{width:62px;height:62px;border-radius:12px;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:13px;color:#fff}
.main{display:flex;flex-direction:column;gap:4px}
.name{font-size:15px;font-weight:700}
.rankline{display:flex;align-items:baseline;gap:8px}
.tier{font-size:18px;font-weight:700}
.rp,.place,.season{font-size:13px;color:#b9c2d0}
.target{font-size:12px;color:#7aa2ff}
.session{margin-left:auto;padding-left:18px;font-size:18px;font-weight:700}
.positive{color:#70d6a6}.negative{color:#ff8e8e}
</style>
</head>
<body>
<div id="root">
  <div class="badge" id="badge">RANK</div>
  <div class="main">
    <div class="name" id="name"></div>
    <div class="rankline">
      <span class="tier" id="tier"></span>
      <span class="rp" id="rp"></span>
      <span class="place" id="place"></span>
    </div>
    <div class="season" id="season"></div>
    <div class="target" id="target"></div>
  </div>
  <div class="session" id="session"></div>
</div>
<script>
const root=document.getElementById('root');
const get=id=>document.getElementById(id);
const show=(id,on)=>get(id).style.display=on?'':'none';
const badgeText={eternity:'ET',demigod:'DG',mythril:'MI',meteorite:'ME',diamond:'DI',platinum:'PL',gold:'GO',silver:'SI',bronze:'BR',iron:'IR'};
const badgeColor={eternity:'#915dff',demigod:'#df5ddb',mythril:'#61cee2',meteorite:'#7270ff',diamond:'#5791ff',platinum:'#4abeb9',gold:'#d3a449',silver:'#95a3b4',bronze:'#ac6f4b',iron:'#686f7c'};
async function refresh(){
  try{
    const response=await fetch('/api/state',{cache:'no-store'});
    const state=await response.json();
    if(!state.hasData){root.style.display='none';return}
    root.style.display='flex';
    root.className=String(state.preset||'').toLowerCase()==='vertical'?'vertical':'';
    root.style.background=state.backgroundEnabled?'rgba(16,18,24,'+state.backgroundOpacity+')':'transparent';
    root.style.border=state.backgroundEnabled?'1px solid rgba(255,255,255,.10)':'0';
    root.style.borderRadius=state.cornerRadius+'px';
    root.style.transform='scale('+state.fontScale+')';
    get('name').textContent=state.nickname;
    get('tier').textContent=state.tier;
    get('rp').textContent=state.rp.toLocaleString()+' RP';
    get('place').textContent=state.rank>0?'#'+state.rank.toLocaleString():'';
    get('season').textContent=state.seasonRemaining||'';
    get('target').textContent=state.targetRpText||'';
    const badge=get('badge');
    badge.textContent=badgeText[state.tierKey]||'RANK';
    badge.style.background=badgeColor[state.tierKey]||'#686f7c';
    const session=get('session');
    const delta=state.sessionDelta;
    session.textContent=(delta>0?'+':'')+delta.toLocaleString()+' RP';
    session.className='session '+(delta>=0?'positive':'negative');
    show('name',state.showNickname);
    show('tier',state.showTier);
    show('rp',state.showRp);
    show('place',state.showRank);
    show('session',state.showSession);
    show('season',state.showSeason&&!!state.seasonRemaining);
    show('target',state.showTarget&&!!state.targetRpText);
  }catch{}
}
refresh();
setInterval(refresh,1000);
</script>
</body>
</html>
""";
}
