/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,cs}",   // Blazor components — nguồn chính
    "./Pages/**/*.{razor,cs}",        // Razor pages nếu có
    "!./wwwroot/prototype/**",        // Exclude prototype HTML (dùng common.css, không phải Tailwind)
    "!./wwwroot/**",                  // Exclude tất cả wwwroot output
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
