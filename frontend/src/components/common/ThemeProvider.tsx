import type React from "react"
import { createContext, useEffect, useState } from "react"

type Theme = "dark" | "light" | "system"

interface ThemeContextValue {
  theme: Theme,
  setTheme: (theme: Theme) => void,
}

export const ThemeContext = createContext<ThemeContextValue>({
  theme: "system",
  setTheme: () => { }
});

type ThemeProviderProps = {
  children: React.ReactNode,
  defaultTheme?: Theme,
  storageKey?: string
}

export default function ThemeProvider({
  children,
  defaultTheme = "system",
  storageKey = "vite-ui-theme",
  ...props
}: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(
    () => (localStorage.getItem(storageKey) as Theme) || defaultTheme);

  useEffect(() => {
    const root = window.document.documentElement;
    root.classList.remove("light", "dark");

    if (theme === "system") {
      const systemTheme = window.matchMedia("(prefers-color-scheme: dark)").matches
        ? "dark" : "light";
      root.classList.add(systemTheme);
      return;
    }
    root.classList.add(theme)
  }, [theme]);

  const value = {
    theme: theme,
    setTheme: (theme: Theme) => {
      localStorage.setItem(storageKey, theme);
      setTheme(theme);
    }
  }

  return (
    <ThemeContext.Provider value={value} {...props}>
      {children}
    </ThemeContext.Provider>
  )
}
