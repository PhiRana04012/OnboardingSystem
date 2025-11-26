/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#e1f2ef',
          100: '#b4ded5',
          200: '#84c9bb',
          300: '#54b39f',
          400: '#2c9c83',
          500: '#117f6b',
          600: '#004746',
          700: '#003f3d',
          800: '#00302f',
          900: '#001f1f',
        },
        brandText: '#414042',
        logo: '#004746',
      },
    },
  },
  plugins: [],
}



