/**
 * MOCK_MATCHES — today's match list
 * Mirrors: GET /api/matches?date=2025-04-09 → MatchDayDto
 *
 * MatchDayDto:
 *   date, liveCount, byLeague: MatchLeagueGroupDto[]
 *
 * MatchLeagueGroupDto:
 *   leagueId, leagueName, country, countryFlag, round, matches: MatchRowDto[]
 *
 * MatchRowDto:
 *   id, kickoff (HH:mm), status (SCH|LIVE|FT), elapsed (null if SCH),
 *   homeTeam: { id, name, logo }, awayTeam: { id, name, logo },
 *   score: { home, away } | null,
 *   detailUrl
 */
window.MOCK_MATCHES = {
  date: '2025-04-09',
  liveCount: 4,
  byLeague: [
    {
      leagueId: 'vleague1', leagueName: 'V.League 1',
      country: 'Vietnam', countryFlag: '🇻🇳', round: 'Vòng 15',
      matches: [
        {
          id: 1001, kickoff: '17:00', status: 'FT', elapsed: 90,
          homeTeam: { id: 1, name: 'Hà Nội FC',  logo: '🏅' },
          awayTeam: { id: 2, name: 'HAGL',        logo: '🟡' },
          score: { home: 1, away: 2 }, detailUrl: 'match-detail.html'
        },
        {
          id: 1002, kickoff: '19:45', status: 'LIVE', elapsed: 65,
          homeTeam: { id: 3, name: 'TPHCM FC',   logo: '🔴' },
          awayTeam: { id: 4, name: 'Bình Dương',  logo: '🔵' },
          score: { home: 0, away: 0 }, detailUrl: 'match-detail.html'
        },
        {
          id: 1003, kickoff: '20:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 5, name: 'Hải Phòng FC', logo: '⚓' },
          awayTeam: { id: 6, name: 'Viettel FC',   logo: '🔴' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 1004, kickoff: '20:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 7, name: 'Nam Định', logo: '🟥' },
          awayTeam: { id: 8, name: 'Đà Nẵng',  logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'epl', leagueName: 'Premier League',
      country: 'England', countryFlag: '🏴󠁧󠁢󠁥󠁮󠁧󠁿', round: 'GW 32',
      matches: [
        {
          id: 2001, kickoff: '19:45', status: 'LIVE', elapsed: 78,
          homeTeam: { id: 10, name: 'Manchester City', logo: '🔵' },
          awayTeam: { id: 11, name: 'Arsenal',         logo: '🔴' },
          score: { home: 1, away: 0 }, detailUrl: 'match-detail.html'
        },
        {
          id: 2002, kickoff: '20:00', status: 'FT', elapsed: 90,
          homeTeam: { id: 12, name: 'Chelsea',   logo: '🔵' },
          awayTeam: { id: 13, name: 'Liverpool', logo: '🔴' },
          score: { home: 2, away: 1 }, detailUrl: 'match-detail.html'
        },
        {
          id: 2003, kickoff: '22:15', status: 'SCH', elapsed: null,
          homeTeam: { id: 14, name: 'Tottenham',  logo: '⚪' },
          awayTeam: { id: 15, name: 'Man United', logo: '🔴' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 2004, kickoff: '22:15', status: 'SCH', elapsed: null,
          homeTeam: { id: 16, name: 'Newcastle Utd', logo: '⚫' },
          awayTeam: { id: 17, name: 'Everton',       logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'ucl', leagueName: 'Champions League',
      country: 'UEFA', countryFlag: '🏆', round: 'Tứ kết',
      matches: [
        {
          id: 3001, kickoff: '02:00', status: 'FT', elapsed: 90,
          homeTeam: { id: 20, name: 'Real Madrid',  logo: '⚪' },
          awayTeam: { id: 21, name: 'Bayern Munich', logo: '🔴' },
          score: { home: 3, away: 1 }, detailUrl: 'match-detail.html'
        },
        {
          id: 3002, kickoff: '02:00', status: 'FT', elapsed: 90,
          homeTeam: { id: 22, name: 'Barcelona', logo: '🔵' },
          awayTeam: { id: 23, name: 'PSG',       logo: '🔴' },
          score: { home: 2, away: 2 }, detailUrl: 'match-detail.html'
        },
        {
          id: 3003, kickoff: '02:00', status: 'LIVE', elapsed: 45,
          homeTeam: { id: 24, name: 'Inter Milan',     logo: '🔵' },
          awayTeam: { id: 25, name: 'Atlético Madrid', logo: '🔴' },
          score: { home: 1, away: 0 }, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'bund', leagueName: 'Bundesliga',
      country: 'Germany', countryFlag: '🇩🇪', round: 'Vòng 28',
      matches: [
        {
          id: 4001, kickoff: '21:30', status: 'FT', elapsed: 90,
          homeTeam: { id: 30, name: 'Bayern Munich',   logo: '🔴' },
          awayTeam: { id: 31, name: 'B. Dortmund',     logo: '🟡' },
          score: { home: 4, away: 0 }, detailUrl: 'match-detail.html'
        },
        {
          id: 4002, kickoff: '21:30', status: 'FT', elapsed: 90,
          homeTeam: { id: 32, name: 'Bayer Leverkusen', logo: '🔴' },
          awayTeam: { id: 33, name: 'RB Leipzig',       logo: '🔴' },
          score: { home: 1, away: 1 }, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'laliga', leagueName: 'La Liga',
      country: 'Spain', countryFlag: '🇪🇸', round: 'Vòng 31',
      matches: [
        {
          id: 5001, kickoff: '22:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 40, name: 'Atlético Madrid', logo: '🔴' },
          awayTeam: { id: 41, name: 'Real Sociedad',   logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 5002, kickoff: '00:30', status: 'SCH', elapsed: null,
          homeTeam: { id: 42, name: 'Sevilla',    logo: '🔴' },
          awayTeam: { id: 43, name: 'Villarreal', logo: '🟡' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'vleague2', leagueName: 'V.League 2',
      country: 'Vietnam', countryFlag: '🇻🇳', round: 'Vòng 12',
      matches: [
        {
          id: 6001, kickoff: '18:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 50, name: 'Phù Đổng',  logo: '🟢' },
          awayTeam: { id: 51, name: 'Đà Nẵng II', logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 6002, kickoff: '19:30', status: 'SCH', elapsed: null,
          homeTeam: { id: 52, name: 'Quảng Nam', logo: '🟡' },
          awayTeam: { id: 53, name: 'Long An',   logo: '🔴' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'vcup', leagueName: 'Cúp Quốc gia',
      country: 'Vietnam', countryFlag: '🇻🇳', round: 'Tứ kết',
      matches: [
        {
          id: 7001, kickoff: '19:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 1, name: 'Hà Nội FC',  logo: '🏅' },
          awayTeam: { id: 3, name: 'TPHCM FC',   logo: '🔴' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'championship', leagueName: 'Championship',
      country: 'England', countryFlag: '🏴󠁧󠁢󠁥󠁮󠁧󠁿', round: 'Matchday 40',
      matches: [
        {
          id: 8001, kickoff: '20:45', status: 'SCH', elapsed: null,
          homeTeam: { id: 60, name: 'Leeds United',  logo: '⚪' },
          awayTeam: { id: 61, name: 'Norwich City',  logo: '🟡' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 8002, kickoff: '21:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 62, name: 'Middlesbrough', logo: '🔴' },
          awayTeam: { id: 63, name: 'Cardiff City',  logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'facup', leagueName: 'FA Cup',
      country: 'England', countryFlag: '🏴󠁧󠁢󠁥󠁮󠁧󠁿', round: 'Semi-finals',
      matches: [
        {
          id: 9001, kickoff: '17:30', status: 'SCH', elapsed: null,
          homeTeam: { id: 10, name: 'Manchester City', logo: '🔵' },
          awayTeam: { id: 12, name: 'Chelsea',         logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'uel', leagueName: 'Europa League',
      country: 'UEFA', countryFlag: '🏆', round: 'Round of 16',
      matches: [
        {
          id: 10001, kickoff: '21:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 70, name: 'Roma',      logo: '🟡' },
          awayTeam: { id: 71, name: 'Brighton',  logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 10002, kickoff: '21:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 72, name: 'Marseille', logo: '🔵' },
          awayTeam: { id: 73, name: 'AEK Athens', logo: '⚪' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'uecl', leagueName: 'Conference League',
      country: 'UEFA', countryFlag: '🏆', round: 'Quarter-finals',
      matches: [
        {
          id: 11001, kickoff: '20:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 80, name: 'Fiorentina',  logo: '🟣' },
          awayTeam: { id: 81, name: 'Club Brugge', logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'bund2', leagueName: '2. Bundesliga',
      country: 'Germany', countryFlag: '🇩🇪', round: 'Matchday 28',
      matches: [
        {
          id: 12001, kickoff: '19:30', status: 'SCH', elapsed: null,
          homeTeam: { id: 90, name: 'Hamburger SV', logo: '🔵' },
          awayTeam: { id: 91, name: 'Hertha BSC',   logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 12002, kickoff: '20:30', status: 'SCH', elapsed: null,
          homeTeam: { id: 92, name: 'Fortuna Düsseldorf', logo: '🔴' },
          awayTeam: { id: 93, name: 'Paderborn',          logo: '⚪' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'laliga2', leagueName: 'La Liga 2',
      country: 'Spain', countryFlag: '🇪🇸', round: 'Matchday 33',
      matches: [
        {
          id: 13001, kickoff: '21:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 100, name: 'Albacete',        logo: '⚪' },
          awayTeam: { id: 101, name: 'Racing Santander', logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'cdr', leagueName: 'Copa del Rey',
      country: 'Spain', countryFlag: '🇪🇸', round: 'Quarter-finals',
      matches: [
        {
          id: 14001, kickoff: '21:30', status: 'SCH', elapsed: null,
          homeTeam: { id: 20, name: 'Real Madrid', logo: '⚪' },
          awayTeam: { id: 22, name: 'Barcelona',   logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'seriea', leagueName: 'Serie A',
      country: 'Italy', countryFlag: '🇮🇹', round: 'Matchday 31',
      matches: [
        {
          id: 15001, kickoff: '20:45', status: 'SCH', elapsed: null,
          homeTeam: { id: 110, name: 'Juventus', logo: '⚪' },
          awayTeam: { id: 111, name: 'Napoli',   logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 15002, kickoff: '20:45', status: 'SCH', elapsed: null,
          homeTeam: { id: 112, name: 'AC Milan',   logo: '🔴' },
          awayTeam: { id: 24,  name: 'Inter Milan', logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'serieb', leagueName: 'Serie B',
      country: 'Italy', countryFlag: '🇮🇹', round: 'Matchday 32',
      matches: [
        {
          id: 16001, kickoff: '18:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 120, name: 'Parma',   logo: '🟡' },
          awayTeam: { id: 121, name: 'Venezia', logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'coppait', leagueName: 'Coppa Italia',
      country: 'Italy', countryFlag: '🇮🇹', round: 'Semi-finals',
      matches: [
        {
          id: 17001, kickoff: '21:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 80,  name: 'Fiorentina', logo: '🟣' },
          awayTeam: { id: 130, name: 'Atalanta',   logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'ligue1', leagueName: 'Ligue 1',
      country: 'France', countryFlag: '🇫🇷', round: 'Matchday 29',
      matches: [
        {
          id: 18001, kickoff: '21:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 23,  name: 'PSG',       logo: '🔵' },
          awayTeam: { id: 72,  name: 'Marseille', logo: '🔴' },
          score: null, detailUrl: 'match-detail.html'
        },
        {
          id: 18002, kickoff: '21:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 140, name: 'Lyon',   logo: '🔵' },
          awayTeam: { id: 141, name: 'Monaco', logo: '⚪' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    },
    {
      leagueId: 'ligue2', leagueName: 'Ligue 2',
      country: 'France', countryFlag: '🇫🇷', round: 'Matchday 30',
      matches: [
        {
          id: 19001, kickoff: '20:00', status: 'SCH', elapsed: null,
          homeTeam: { id: 150, name: 'Toulouse II', logo: '🟣' },
          awayTeam: { id: 151, name: 'Amiens',      logo: '🔵' },
          score: null, detailUrl: 'match-detail.html'
        }
      ]
    }
  ]
};
