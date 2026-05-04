/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        navy: '#1e3a5f',
        accent: '#3b82f6',
      },
    },
  },
  plugins: [],
}
