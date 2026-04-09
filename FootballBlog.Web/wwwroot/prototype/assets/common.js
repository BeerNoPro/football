/* ===================================================
   FOOTBALLBLOG — Common JS
   Shared interactions for all prototype pages
=================================================== */

// Left sidebar: toggle country group expand/collapse
function toggleCountry(id) {
  document.getElementById(id).classList.toggle('collapsed');
}

// Left sidebar: select league
// - home.html → filter center column to show only that league
// - all other pages → navigate to league-page.html?league=X
function selectLeague(el, leagueId) {
  const path = window.location.pathname;
  const isHome = path.includes('home.html') || path === '/' || path.endsWith('/index.html');

  if (!isHome) {
    // Any non-home page: navigate to the league page
    window.location.href = 'league-page.html?league=' + leagueId;
    return;
  }

  // --- Home page: filter behavior ---

  // Toggle: click active league again → deselect and show all
  if (el.classList.contains('active')) {
    el.classList.remove('active');
    document.querySelectorAll('.lg').forEach(lg => lg.style.display = 'block');
    const matchList = document.querySelector('.matches-list');
    if (matchList) matchList.scrollTop = 0;
    return;
  }

  // Deselect all, activate selected
  document.querySelectorAll('.league-item').forEach(i => i.classList.remove('active'));
  el.classList.add('active');

  // Filter center column: show only selected league group
  document.querySelectorAll('.lg').forEach(lg => {
    lg.style.display = (lg.id === 'm-' + leagueId) ? 'block' : 'none';
  });

  // Scroll match list to top
  const matchList = document.querySelector('.matches-list');
  if (matchList) matchList.scrollTop = 0;
}

// Left sidebar: live search filter
function filterLeagues(q) {
  const lower = q.toLowerCase();
  document.querySelectorAll('.country-group').forEach(cg => {
    let anyVisible = false;
    cg.querySelectorAll('.league-item').forEach(item => {
      const match = item.textContent.toLowerCase().includes(lower);
      item.style.display = match ? '' : 'none';
      if (match) anyVisible = true;
    });
    cg.style.display = (anyVisible || lower === '') ? '' : 'none';
    if (lower !== '') cg.classList.remove('collapsed');
  });
}

// Center: collapse/expand league group in match list
function toggleLg(hdr) {
  hdr.closest('.lg').classList.toggle('collapsed');
}

// Generic tab switch — switches within the closest .tabs parent
function setTab(el) {
  el.closest('.tabs').querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
  el.classList.add('active');
}

// Generic tab switch — switches within the closest .tab-bar parent
function setTabBar(el) {
  el.closest('.tab-bar').querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
  el.classList.add('active');
}

// Center: date bar switch
function setDate(el) {
  document.querySelectorAll('.date-btn2').forEach(b => b.classList.remove('active'));
  el.classList.add('active');
}

// Right panel: tab switch
function setRightTab(el) {
  el.closest('.right-tabs').querySelectorAll('.right-tab').forEach(b => b.classList.remove('active'));
  el.classList.add('active');
}

// Match detail: tab panel switch
// Usage: setDetailTab(el, 'tab-id')
// Requires: data-tab on button, matching id on panel
function setDetailTab(el, groupClass) {
  const container = el.closest('.' + groupClass + '-tabs') || el.closest('.detail-tabs');
  if (container) {
    container.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
  }
  el.classList.add('active');
  const tabId = el.dataset.tab;
  if (tabId) {
    document.querySelectorAll('.detail-panel').forEach(p => p.style.display = 'none');
    const panel = document.getElementById(tabId);
    if (panel) panel.style.display = '';
  }
}
