/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html}",
    "./Components/**/*.{razor,cs}",
  ],
  theme: {
    extend: {
      colors: {
        // Sẽ cập nhật sau khi có Figma design tokens
        primary: '#1a56db',
        'pitch-green': '#2d6a4f',
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
        display: ['Oswald', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
