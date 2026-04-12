/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html}",
    "./Components/**/*.{razor,cs}",
  ],
  theme: {
    extend: {
      colors: {
        // Dark theme design tokens — khớp với prototype/home.html
        'bg':          '#141414',
        'bg-sidebar':  '#111111',
        'bg-card':     '#1c1c1c',
        'bg-dark':     '#0d0d0d',
        'accent':      '#c8f04d',
        'text-main':   '#efefef',
        'muted':       '#666666',
        'muted2':      '#999999',
        'border-main': '#242424',
        'border2':     '#2e2e2e',
        'live':        '#4ade80',
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
        display: ['Oswald', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
