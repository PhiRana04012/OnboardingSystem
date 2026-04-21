/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
      colors: {
        primary: {
          50: '#ebf4f4',
          100: '#cce3e3',
          200: '#99c7c7',
          300: '#66abab',
          400: '#338f8f',
          500: '#007373',
          600: '#004746',
          700: '#003837',
          800: '#002928',
          900: '#001a19',
        },
      },
    },
  },
  plugins: [],
}
