import { AuthContext, type AuthContextValue } from "@/components/auth/AuthProvider";
import { useContext } from "react";

export default function useAuth(): AuthContextValue {
  const authContext = useContext(AuthContext);
  if (authContext === undefined || authContext === null)
    throw new Error("useAuth must be used within a ThemeProvider");
  return authContext;
}
