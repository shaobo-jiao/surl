import { AuthContext, type AuthContextValue } from "@/components/auth/AuthProvider";
import { useContext } from "react";

export function useAuth(): AuthContextValue {
  const authContext = useContext(AuthContext);
  if (authContext === null)
    throw new Error("Auth context is not defined, which shouldn't happen.");
  return authContext;
}
