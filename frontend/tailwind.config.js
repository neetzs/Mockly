import tailwindcssAnimate from "tailwindcss-animate";

/** Color que sale de una variable CSS de index.css (formato HSL sin hsl()). */
const tok = (name) => `hsl(var(--${name}) / <alpha-value>)`;

/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ["class"],
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    container: {
      center: true,
      padding: "2rem",
      screens: {
        "2xl": "1400px",
      },
    },
    extend: {
      fontFamily: {
        // Geist (instalada vía @fontsource-variable/geist, importada en main.tsx)
        sans: ['"Geist Variable"', "ui-sans-serif", "system-ui", "sans-serif"],
      },
      keyframes: {
        // Barras de puntaje del hero de la landing: crecen de 0 al ancho final una sola vez.
        "score-fill": {
          from: { transform: "scaleX(0)" },
          to: { transform: "scaleX(1)" },
        },
      },
      animation: {
        "score-fill": "score-fill 900ms cubic-bezier(0.22, 1, 0.36, 1) both",
      },
      // Colores semánticos: leen las variables de index.css, que cambian con la clase "dark".
      // <alpha-value> permite modificadores de opacidad (bg-surface/40, border-border/80).
      colors: {
        border: tok("border"),
        input: tok("input"),
        ring: tok("ring"),
        background: tok("background"),
        foreground: tok("foreground"),
        surface: tok("surface"),
        soft: tok("soft"),
        subtle: tok("subtle"),
        faint: tok("faint"),
        brand: tok("brand"),
        success: tok("success"),
        warning: tok("warning"),
        danger: tok("danger"),
        primary: { DEFAULT: tok("primary"), foreground: tok("primary-foreground") },
        secondary: { DEFAULT: tok("secondary"), foreground: tok("secondary-foreground") },
        destructive: { DEFAULT: tok("destructive"), foreground: tok("destructive-foreground") },
        muted: { DEFAULT: tok("muted"), foreground: tok("muted-foreground") },
        accent: { DEFAULT: tok("accent"), foreground: tok("accent-foreground") },
        popover: { DEFAULT: tok("popover"), foreground: tok("popover-foreground") },
        card: { DEFAULT: tok("card"), foreground: tok("card-foreground") },
        chart: { 1: tok("chart-1"), 2: tok("chart-2"), 3: tok("chart-3"), 4: tok("chart-4"), 5: tok("chart-5") },
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
    },
  },
  plugins: [tailwindcssAnimate],
};