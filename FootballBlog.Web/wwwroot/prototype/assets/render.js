/* ===================================================
   FOOTBALLBLOG — Render Functions
   Loads data from JSON files (mirrors real API DTOs).
   When real API is ready: change DATA_BASE to '/api/'
   and update fetchData() — render functions stay the same.
=================================================== */

/* ---- DATA LOADER ---- */

const DATA_BASE = 'data/';

async function fetchData(name) {
  const res = await fetch(`${DATA_BASE}${name}.json`);
  if (!res.ok) throw new Error(`Failed to load ${name}.json (${res.status})`);
  return res.json();
}

/* ---- PAGE INITIALIZERS ---- */

/**
 * Home page — loads leagues, matches, posts in parallel
 */
async function initHomePage() {
  try {
    const [leagues, matches, posts] = await Promise.all([
      fetchData('leagues'),
      fetchData('matches'),
      fetchData('posts')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    const matchesList = document.querySelector('.matches-list');
    const rightScroll = document.querySelector('.right-scroll');
    if (leagueTree)  renderLeagueTree(leagueTree, leagues);
    if (matchesList) renderMatches(matchesList, matches);
    if (rightScroll) renderPosts(rightScroll, posts);
    updateLivePill(matches.liveCount);
    applyLeagueParam();

    // Cache posts for tab filtering
    window.__homePosts = posts;

    // Tab filter: Nhận định=LIVE, Dự đoán=SCH, Phân tích=FT
    document.addEventListener('rightTabChange', (e) => {
      const tab = e.detail;
      const rs = document.querySelector('.right-scroll');
      if (!rs || !window.__homePosts) return;
      const all = window.__homePosts;
      let filtered;
      if (tab === 'Dự đoán') {
        filtered = { featured: null, items: all.items.filter(p => p.match.status === 'SCH') };
      } else if (tab === 'Phân tích') {
        filtered = { featured: null, items: all.items.filter(p => p.match.status === 'FT') };
      } else {
        // Nhận định — live + featured
        filtered = { featured: all.featured, items: all.items.filter(p => p.match.status === 'LIVE') };
      }
      renderPosts(rs, filtered);
    });
  } catch (e) {
    console.error('[initHomePage]', e);
  }
}

/**
 * League page — loads sidebar leagues + league detail (standings, scorers, fixtures)
 */
async function initLeaguePage() {
  try {
    const [leagues, detail] = await Promise.all([
      fetchData('leagues'),
      fetchData('league-detail')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
    if (typeof renderLeagueDetail === 'function') renderLeagueDetail(detail);
  } catch (e) {
    console.error('[initLeaguePage]', e);
  }
}

/**
 * Match detail page — loads sidebar leagues + match detail
 */
async function initMatchDetailPage() {
  try {
    const [leagues, match] = await Promise.all([
      fetchData('leagues'),
      fetchData('match-detail')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
    if (typeof renderMatchDetail === 'function') renderMatchDetail(match);
  } catch (e) {
    console.error('[initMatchDetailPage]', e);
  }
}

/**
 * Team profile page — loads sidebar leagues + team data
 */
async function initTeamPage() {
  try {
    const [leagues, team] = await Promise.all([
      fetchData('leagues'),
      fetchData('team')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
    if (typeof renderTeam === 'function') renderTeam(team);
  } catch (e) {
    console.error('[initTeamPage]', e);
  }
}

/**
 * Player profile page — loads sidebar leagues + player data
 */
async function initPlayerPage() {
  try {
    const [leagues, player] = await Promise.all([
      fetchData('leagues'),
      fetchData('player')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
    if (typeof renderPlayer === 'function') renderPlayer(player);
  } catch (e) {
    console.error('[initPlayerPage]', e);
  }
}

/**
 * Predictions listing page
 */
async function initPredictionsPage() {
  try {
    const [leagues, predictions] = await Promise.all([
      fetchData('leagues'),
      fetchData('predictions')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
    if (typeof renderPredictions === 'function') renderPredictions(predictions);
  } catch (e) {
    console.error('[initPredictionsPage]', e);
  }
}

/**
 * Category / Tag listing page
 */
async function initCategoryPage() {
  try {
    const [leagues, categories] = await Promise.all([
      fetchData('leagues'),
      fetchData('categories')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
    if (typeof renderCategory === 'function') renderCategory(categories);
  } catch (e) {
    console.error('[initCategoryPage]', e);
  }
}

/**
 * Post detail page — loads sidebar leagues only (main content is hardcoded)
 */
async function initPostDetailPage() {
  try {
    const leagues = await fetchData('leagues');
    const leagueTree = document.querySelector('.league-tree');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
  } catch (e) {
    console.error('[initPostDetailPage]', e);
  }
}

/**
 * News listing page
 */
async function initNewsPage() {
  try {
    const [leagues, posts] = await Promise.all([
      fetchData('leagues'),
      fetchData('posts')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    const rightScroll = document.querySelector('.right-scroll');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
    if (rightScroll) renderPosts(rightScroll, posts);
  } catch (e) {
    console.error('[initNewsPage]', e);
  }
}

/**
 * Search results page
 */
async function initSearchPage() {
  try {
    // Populate search input from ?q= URL param
    const q = new URLSearchParams(location.search).get('q') || '';
    const searchInput = document.querySelector('.search-hero-bar input');
    if (searchInput && q) searchInput.value = q;

    const [leagues, results] = await Promise.all([
      fetchData('leagues'),
      fetchData('search')
    ]);
    const leagueTree = document.querySelector('.league-tree');
    if (leagueTree) renderLeagueTree(leagueTree, leagues);
    if (typeof renderSearch === 'function') renderSearch(results);
  } catch (e) {
    console.error('[initSearchPage]', e);
  }
}

/* ---- SIDEBAR: League Tree ---- */

/**
 * Renders the left sidebar league tree.
 * @param {HTMLElement} container  — #league-tree element
 * @param {Array}       leagues    — data from leagues.json (List<LeagueSidebarDto>)
 */
function renderLeagueTree(container, leagues) {
  container.innerHTML = leagues.map((g, i) => {
    const cgId = 'cg-' + g.countryId;
    const items = g.leagues.map(l => {
      const live = l.liveCount
        ? `<span class="league-live-count">${l.liveCount}</span>` : '';
      return `<a class="league-item" href="league-page.html"
                 data-league="${l.id}"
                 onclick="selectLeague(this, '${l.id}'); event.preventDefault();">
                ${l.name}${live}
              </a>`;
    }).join('');

    return `
      <div class="country-group" id="${cgId}">
        <div class="country-header" onclick="toggleCountry('${cgId}')">
          <span class="country-flag">${g.flag}</span>
          <span class="country-name">${g.country}</span>
          <span class="country-chevron">▾</span>
        </div>
        <div class="sub-leagues">${items}</div>
      </div>`;
  }).join('');
}

/* ---- MATCHES: Home center column ---- */

/**
 * Renders the full matches list.
 * @param {HTMLElement} container — .matches-list element
 * @param {Object}      data      — data from matches.json (MatchDayDto)
 */
function renderMatches(container, data) {
  container.innerHTML = data.byLeague.map(lg => renderLeagueGroup(lg)).join('');
}

function renderLeagueGroup(lg) {
  const rows = lg.matches.map(m => renderMatchRow(m)).join('');
  return `
    <div class="lg" id="m-${lg.leagueId}">
      <div class="lg-header" onclick="toggleLg(this)">
        <div class="lg-icon">${lg.countryFlag}</div>
        <a class="lg-name lg-name-link" href="league-page.html?league=${lg.leagueId}" onclick="event.stopPropagation()">${lg.country} — ${lg.leagueName}</a>
        <span class="lg-round">${lg.round}</span>
        <span class="lg-chevron">▾</span>
      </div>
      <div class="lg-matches">${rows}</div>
    </div>`;
}

function renderMatchRow(m) {
  const isLive = m.status === 'LIVE';
  const isFt   = m.status === 'FT';

  const timeHtml = isLive
    ? `<div class="mr-time is-live">${m.elapsed}'</div>`
    : `<div class="mr-time">${m.kickoff}</div>`;

  const liveClass = isLive ? ' live-team' : '';

  const scoreHtml = isLive
    ? `<div class="score-c is-live-score">${m.score.home} – ${m.score.away}</div>`
    : isFt && m.score
    ? `<div class="score-c has-score">${m.score.home} – ${m.score.away}</div>`
    : `<div class="score-c no-score">–</div>`;

  const badge = isLive
    ? `<span class="badge-live"><span class="live-dot" style="width:5px;height:5px;"></span>LIVE</span>`
    : isFt
    ? `<span class="badge-ft">KT</span>`
    : `<span class="badge-sch">${m.kickoff}</span>`;

  return `
    <div class="match-row" onclick="location.href='${m.detailUrl}'">
      ${timeHtml}
      <div class="mr-home${liveClass}">
        <span class="tn" onclick="event.stopPropagation(); location.href='${m.homeTeam.url || 'team-profile.html'}'">${m.homeTeam.name}</span>
        <div class="tl">${m.homeTeam.logo}</div>
      </div>
      ${scoreHtml}
      <div class="mr-away${liveClass}">
        <div class="tl">${m.awayTeam.logo}</div>
        <span class="tn" onclick="event.stopPropagation(); location.href='${m.awayTeam.url || 'team-profile.html'}'">${m.awayTeam.name}</span>
      </div>
      <div class="mr-status">${badge}</div>
    </div>`;
}

/* ---- POSTS: Right sidebar ---- */

/**
 * Renders AI prediction posts.
 * @param {HTMLElement} container — .right-scroll element
 * @param {Object}      data      — data from posts.json (PredictionPostListDto)
 */
function renderPosts(container, data) {
  const featuredHtml = data.featured ? renderFeaturedPost(data.featured) : '';
  const itemsHtml = data.items.map(p => renderPostItem(p)).join('');
  container.innerHTML = featuredHtml + itemsHtml;
}

function renderFeaturedPost(p) {
  const matchStatus = p.match.status === 'LIVE'
    ? `<span style="color:var(--live);">● LIVE ${p.match.elapsed}'</span>`
    : p.match.status === 'FT'
    ? `<span style="color:var(--muted);">● KT</span>`
    : `<span style="color:var(--accent);">● ${p.updatedAt}</span>`;

  return `
    <a class="post-featured" href="${p.url}">
      <div class="post-featured-img" style="background:${p.thumbGradient};">
        <span style="font-size:48px;opacity:0.4;">${p.thumbEmoji}</span>
        <div class="match-label">
          <span>${p.match.home} vs ${p.match.away}</span>
          ${matchStatus}
        </div>
      </div>
      <div class="post-featured-body">
        <div class="post-tag">${p.tag}</div>
        <div class="post-title">${p.title}</div>
        <div class="post-excerpt">${p.excerpt}</div>
        <div class="post-meta">
          <span class="post-time">Cập nhật lúc ${p.updatedAt}</span>
          <span class="post-read">Đọc thêm →</span>
        </div>
      </div>
    </a>`;
}

function renderPostItem(p) {
  const scoreVal   = p.actualScore || p.aiScore;
  const scoreLabel = p.actualScore ? 'Kết<br>quả' : 'AI<br>dự đoán';
  const scoreStyle = p.actualScore ? 'opacity:0.5;' : '';

  const matchMeta = p.match.status === 'LIVE'
    ? `LIVE ${p.match.elapsed}' · ${p.updatedAt}`
    : p.match.status === 'FT'
    ? `Kết thúc · ${p.updatedAt}`
    : `Lịch đấu · ${p.updatedAt}`;

  return `
    <a class="post-item" href="${p.url}">
      <div class="post-thumb" style="background:${p.thumbGradient};">${p.thumbEmoji}</div>
      <div class="post-item-body">
        <div class="post-item-tag">${p.tag}</div>
        <div class="post-item-title">${p.title}</div>
        <div class="post-item-meta">${matchMeta}</div>
      </div>
      <div class="pred-score" style="${scoreStyle}">
        <div class="ps-val">${scoreVal}</div>
        <div class="ps-label">${scoreLabel}</div>
      </div>
    </a>`;
}

/* ---- LIVE PILL: update count ---- */
function updateLivePill(count) {
  const pill = document.querySelector('.live-pill strong');
  if (pill) pill.textContent = count;
  // Also update tab badge if present
  const tabBadge = document.querySelector('.tab-badge');
  if (tabBadge) tabBadge.textContent = count;
}
