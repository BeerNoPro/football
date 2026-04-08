// ===== COMMON JAVASCRIPT =====

// Filter leagues based on search input
function filterLeagues(query) {
  const groups = document.querySelectorAll('.country-group');
  const lowerQuery = query.toLowerCase();

  groups.forEach(group => {
    const leagueItems = group.querySelectorAll('.league-item');
    let hasVisibleItems = false;

    leagueItems.forEach(item => {
      const text = item.textContent.toLowerCase();
      const isVisible = text.includes(lowerQuery);
      item.style.display = isVisible ? 'block' : 'none';
      if (isVisible) hasVisibleItems = true;
    });

    // Show/hide country group
    group.style.display = hasVisibleItems || lowerQuery === '' ? 'block' : 'none';
  });
}

// Toggle country expand/collapse
function toggleCountry(countryId) {
  const group = document.getElementById(countryId);
  const subLeagues = group.querySelector('.sub-leagues');
  const chevron = group.querySelector('.country-chevron');

  if (subLeagues.classList.contains('open')) {
    subLeagues.classList.remove('open');
    chevron.style.transform = 'rotate(0deg)';
  } else {
    subLeagues.classList.add('open');
    chevron.style.transform = 'rotate(180deg)';
  }
}

// Select league
function selectLeague(element, leagueId, event) {
  // Prevent default navigation if event is provided
  if (event) {
    event.preventDefault();
  }

  // Always navigate to league page with league parameter
  // This works for both home page and league page
  window.location.href = 'league-page.html?league=' + leagueId;
}

// Toggle league group expand/collapse
function toggleLg(header) {
  const lg = header.closest('.lg');
  const matches = lg.querySelector('.lg-matches');
  const chevron = header.querySelector('.lg-chevron');

  if (matches.classList.contains('open')) {
    matches.classList.remove('open');
    chevron.style.transform = 'rotate(0deg)';
  } else {
    matches.classList.add('open');
    chevron.style.transform = 'rotate(180deg)';
  }
}

// Set active tab
function setTab(button) {
  // Remove active class from all tabs
  document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.classList.remove('active');
  });

  // Add active class to clicked tab
  button.classList.add('active');

  // Here you would typically update content based on tab
  console.log('Selected tab:', button.textContent.trim());
}

// Set active date
function setDate(button) {
  // Remove active class from all date buttons
  document.querySelectorAll('.date-btn2').forEach(btn => {
    btn.classList.remove('active');
  });

  // Add active class to clicked date
  button.classList.add('active');

  // Here you would typically filter matches by date
  console.log('Selected date:', button.querySelector('.dd').textContent);
}

// Set active right tab
function setRightTab(button) {
  // Remove active class from all right tabs
  document.querySelectorAll('.right-tab').forEach(btn => {
    btn.classList.remove('active');
  });

  // Add active class to clicked tab
  button.classList.add('active');

  // Here you would typically update right panel content
  console.log('Selected right tab:', button.textContent);
}

// Switch league tab (for league page)
function switchLeagueTab(el) {
  document.querySelectorAll('.league-tabs .tab-btn').forEach(b => b.classList.remove('active'));
  el.classList.add('active');
  document.querySelectorAll('.league-panel').forEach(p => p.classList.remove('active'));
  document.getElementById(el.dataset.tab).classList.add('active');
}

// Switch detail tab (for match detail page)
function switchDetailTab(el) {
  document.querySelectorAll('.detail-tab-bar .tab-btn').forEach(b => b.classList.remove('active'));
  el.classList.add('active');
  document.querySelectorAll('.detail-panel').forEach(p => p.classList.remove('active'));
  document.getElementById(el.dataset.tab).classList.add('active');
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
  // Expand first country group by default
  const firstGroup = document.querySelector('.country-group');
  if (firstGroup) {
    const subLeagues = firstGroup.querySelector('.sub-leagues');
    const chevron = firstGroup.querySelector('.country-chevron');
    if (subLeagues && chevron) {
      subLeagues.classList.add('open');
      chevron.style.transform = 'rotate(180deg)';
    }
  }

  // Expand first league group by default
  const firstLg = document.querySelector('.lg');
  if (firstLg) {
    const matches = firstLg.querySelector('.lg-matches');
    const chevron = firstLg.querySelector('.lg-chevron');
    if (matches && chevron) {
      matches.classList.add('open');
      chevron.style.transform = 'rotate(180deg)';
    }
  }

  // Check if we're on league page and have URL parameter
  const urlParams = new URLSearchParams(window.location.search);
  const leagueParam = urlParams.get('league');

  if (leagueParam) {
    // Find and activate the league item
    const leagueItem = document.querySelector(`[data-league="${leagueParam}"]`);
    if (leagueItem) {
      selectLeague(leagueItem, leagueParam);
    }
  } else {
    // Show all leagues by default (no filtering)
    // No default selection - all leagues visible
  }
});