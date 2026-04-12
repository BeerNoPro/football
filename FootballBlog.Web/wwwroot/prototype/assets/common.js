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
    // Bất kỳ trang nào → về home và highlight league đó
    window.location.href = 'home.html?league=' + leagueId;
    return;
  }

  // Toggle: click active league again → deselect
  if (el.classList.contains('active')) {
    el.classList.remove('active');
    return;
  }

  // Deselect all, activate selected
  document.querySelectorAll('.league-item').forEach(i => i.classList.remove('active'));
  el.classList.add('active');

  // Update hero tag text with selected league name
  const leagueName = el.textContent.trim();
  const heroTag = document.querySelector('.hero-tag');
  if (heroTag) {
    const liveDot = heroTag.querySelector('.live-dot');
    heroTag.textContent = '';
    if (liveDot) heroTag.appendChild(liveDot);
    heroTag.appendChild(document.createTextNode(' ' + leagueName));
  }

  // Scroll center column to show the league group at top
  const target = document.getElementById('m-' + leagueId);
  if (target) {
    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}

// Đọc ?league= từ URL và auto-highlight + scroll sau khi render xong
function applyLeagueParam() {
  const params = new URLSearchParams(window.location.search);
  const leagueId = params.get('league');
  if (!leagueId) return;

  const leagueEl = document.querySelector(`.league-item[data-league="${leagueId}"]`);
  if (leagueEl) {
    document.querySelectorAll('.league-item').forEach(i => i.classList.remove('active'));
    leagueEl.classList.add('active');
    // Expand country group nếu đang collapsed
    const cg = leagueEl.closest('.country-group');
    if (cg) cg.classList.remove('collapsed');

    // Update hero tag text with selected league name
    const leagueName = leagueEl.textContent.trim();
    const heroTag = document.querySelector('.hero-tag');
    if (heroTag) {
      const liveDot = heroTag.querySelector('.live-dot');
      heroTag.textContent = '';
      if (liveDot) heroTag.appendChild(liveDot);
      heroTag.appendChild(document.createTextNode(' ' + leagueName));
    }
  }

  const target = document.getElementById('m-' + leagueId);
  if (target) {
    setTimeout(() => target.scrollIntoView({ behavior: 'smooth', block: 'start' }), 80);
  }
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

// Center: collapse/expand league group in match list (works for .lg and .pred-section)
function toggleLg(hdr) {
  const parent = hdr.closest('.lg') || hdr.closest('.pred-section');
  if (parent) parent.classList.toggle('collapsed');
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

// Right panel: tab switch (Nhận định / Dự đoán / Phân tích)
// Content filtering handled by render.js initHomePage via 'rightTabChange' event
function setRightTab(el) {
  el.closest('.right-tabs').querySelectorAll('.right-tab').forEach(b => b.classList.remove('active'));
  el.classList.add('active');
  document.dispatchEvent(new CustomEvent('rightTabChange', { detail: el.textContent.trim() }));
}

// Admin sidebar: dropdown toggle
function toggleDD(id) {
  document.getElementById(id).classList.toggle('open');
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
