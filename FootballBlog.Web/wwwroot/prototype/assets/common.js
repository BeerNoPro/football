/* ===================================================
   FOOTBALLBLOG — Common JS
   Shared interactions for all prototype pages
=================================================== */

// Left sidebar: toggle country group expand/collapse
function toggleCountry(id) {
  document.getElementById(id).classList.toggle('collapsed');
}

// Left sidebar: select league → highlight + scroll center to that league
function selectLeague(el, leagueId) {
  // If we're on league page, navigate to different league
  if (window.location.pathname.includes('league-page.html')) {
    window.location.href = 'league-page.html?league=' + leagueId;
    return;
  }

  // If we're on home page, just highlight and show ALL leagues (no filtering)
  // Remove active class from all league items
  document.querySelectorAll('.league-item').forEach(i => i.classList.remove('active'));

  // Add active class to selected item
  el.classList.add('active');

  // Show ALL league groups (no hiding)
  document.querySelectorAll('.lg').forEach(lg => {
    lg.style.display = 'block';
  });

  // Scroll to selected league group
  const target = document.getElementById('m-' + leagueId);
  if (target) {
    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    target.style.outline = '1px solid rgba(200,240,77,0.3)';
    setTimeout(() => target.style.outline = '', 1200);
  }

  console.log('Highlighted league:', leagueId);
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
