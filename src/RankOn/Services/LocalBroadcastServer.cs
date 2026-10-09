using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RankOn.Services;

public sealed class LocalBroadcastServer
{
    private readonly OverlayStateService _stateService;
    private readonly int _preferredPort;
    private WebApplication? _app;
    private int _port;

    public LocalBroadcastServer(OverlayStateService stateService, int port)
    {
        _stateService = stateService;
        _preferredPort = port;
        _port = port;
    }

    public string Address => $"http://127.0.0.1:{_port}/overlay";
    public bool IsRunning => _app is not null;
    public string? LastError { get; private set; }

    public async Task StartAsync()
    {
        if (_app is not null)
        {
            return;
        }

        Exception? lastError = null;

        for (var port = _preferredPort; port <= _preferredPort + 10; port++)
        {
            WebApplication? app = null;

            try
            {
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    Args = Array.Empty<string>()
                });

                builder.Logging.ClearProviders();
                builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
                builder.Services.AddSingleton(_stateService);

                app = builder.Build();
                app.MapGet("/api/state", (OverlayStateService state) => Results.Json(state.Get()));
                app.MapGet("/api/health", () => Results.Json(new { ok = true, port }));
                app.MapGet("/assets/rank-tiers", async () =>
                {
                    var bytes = await TierIconService.GetSpriteBytesAsync();
                    return bytes is null
                        ? Results.NotFound()
                        : Results.File(bytes, "image/png");
                });
                app.MapGet("/overlay", () => Results.Content(OverlayHtml, "text/html; charset=utf-8"));

                await app.StartAsync();

                _app = app;
                _port = port;
                LastError = null;
                return;
            }
            catch (Exception ex)
            {
                lastError = ex;

                if (app is not null)
                {
                    await app.DisposeAsync();
                }
            }
        }

        LastError = "사용 가능한 방송 출력 포트를 찾지 못했습니다.";
        throw new InvalidOperationException(LastError, lastError);
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
        LastError = null;
    }

    private const string OverlayHtml = """
<!doctype html>
<html lang="ko">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<style>
html,body{margin:0;width:100%;height:100%;overflow:hidden;background:transparent;font-family:Segoe UI,Arial,sans-serif}
#root{display:none;box-sizing:border-box;flex-direction:column;gap:10px;padding:14px 18px;color:#fff;width:max-content;min-width:430px;transform-origin:top left}
#root.vertical{min-width:190px}
.top{display:flex;align-items:center;gap:16px}
#root.vertical .top{flex-direction:column;text-align:center}
#root.vertical .main{align-items:center}
#root.vertical .rankline{justify-content:center}
#root.vertical .session{margin-left:0;padding-left:0}
#root.vertical .recent{align-items:center}
.badge{width:62px;height:62px;display:block;background-image:url('/assets/rank-tiers');background-repeat:no-repeat;background-size:738.1px 143.19px;background-position-y:-40.48px}
.main{display:flex;flex-direction:column;gap:4px}
.name{font-size:15px;font-weight:700}
.rankline{display:flex;align-items:baseline;gap:8px}
.tier{font-size:18px;font-weight:700}
.rp,.place,.season{font-size:13px;color:#b9c2d0}
.target{font-size:12px;color:#7aa2ff}
.session{margin-left:auto;padding-left:18px;font-size:18px;font-weight:700}
.positive{color:#70d6a6}.negative{color:#ff8e8e}
.recent{display:flex;flex-direction:column;align-items:flex-start}
.recent-title{font-size:11px;color:#aeb7c3}
.recent-grid{display:grid;grid-template-columns:repeat(5,25px);grid-template-rows:repeat(2,25px);gap:4px;margin-top:5px}
.match{width:25px;height:25px;border-radius:4px;display:flex;align-items:center;justify-content:center;box-sizing:border-box;font-size:11px;font-weight:600;color:#fff;background:rgba(58,66,77,.35)}
.match.first{background:#c99326}
.match.second{background:#8f9aa8}
.match.third{background:#a86a43}
.match.other{background:#3a424d}
</style>
</head>
<body>
<div id="root">
  <div class="top">
    <div class="badge" id="badge"></div>
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
  <div class="recent" id="recent">
    <div class="recent-title">최근 전적</div>
    <div class="recent-grid" id="recent-grid"></div>
  </div>
</div>
<script>
const root=document.getElementById('root');
const get=id=>document.getElementById(id);
const show=(id,on)=>get(id).style.display=on?'':'none';
const tierIndexes={iron:0,bronze:1,silver:2,gold:3,platinum:4,diamond:5,meteorite:6,mythril:7,demigod:8,eternity:9};

function renderRecent(matches,on){
  const recent=get('recent');
  recent.style.display=on?'flex':'none';
  const grid=get('recent-grid');
  grid.replaceChildren();

  for(let i=0;i<10;i++){
    const item=Array.isArray(matches)&&i<matches.length?matches[i]:null;
    const rank=item&&Number.isInteger(item.rank)?item.rank:null;
    const tile=document.createElement('div');
    tile.className='match';

    if(rank===1) tile.classList.add('first');
    else if(rank===2) tile.classList.add('second');
    else if(rank===3) tile.classList.add('third');
    else if(rank>=4&&rank<=8) tile.classList.add('other');

    tile.textContent=rank?String(rank):'';
    grid.appendChild(tile);
  }
}

async function refresh(){
  try{
    const response=await fetch('/api/state',{cache:'no-store'});
    if(!response.ok) return;
    const state=await response.json();
    if(!state.hasData){root.style.display='none';return}
    root.style.display='flex';
    root.className=String(state.preset||'').toLowerCase()==='vertical'?'vertical':'';
    root.style.background=state.backgroundEnabled?'rgba(16,18,24,'+state.backgroundOpacity+')':'transparent';
    root.style.border=state.backgroundEnabled?'1px solid rgba(255,255,255,.10)':'0';
    root.style.borderRadius=state.cornerRadius+'px';
    root.style.transform='none';
    root.style.zoom=String(1.6*state.fontScale);
    get('name').textContent=state.nickname;
    get('tier').textContent=state.tier;
    get('rp').textContent=state.rp.toLocaleString()+' RP';
    get('place').textContent=state.rank>0?'#'+state.rank.toLocaleString():'';
    get('season').textContent=state.seasonRemaining||'';
    get('target').textContent=state.targetRpText||'';
    const badge=get('badge');
    const tierIndex=tierIndexes[state.tierKey];
    if(state.showTierIcon&&Number.isInteger(tierIndex)){
      badge.style.backgroundPositionX=(-(tierIndex*73.81+5.9))+'px';
      show('badge',true);
    }else{
      show('badge',false);
    }
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
    renderRecent(state.recentMatches||[],state.showRecentMatches);
  }catch{}
}
refresh();
setInterval(refresh,750);
</script>
</body>
</html>
""";
}
