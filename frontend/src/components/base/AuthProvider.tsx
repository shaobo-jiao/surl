import { registerAuthHandlers, setAccessToken } from "@/api/apiClient";
import { loginApi } from "@/api/auth/login";
import { logoutApi } from "@/api/auth/logout";
import { meApi } from "@/api/auth/me";
import { refreshApi } from "@/api/auth/refresh";
import { Spinner } from "@/components/ui/spinner";

import { createContext, useEffect, useReducer, useState } from "react";

interface UserLogin {
  isAuthenticated: boolean,
  // displayName: string | null, // to be reviewed
  username: string | null,
}

// actions to dispatch UserLogin states
type UserLoginAction =
  | { type: "logged_in", payload: { username: string } }
  | { type: "logged_out" }

function userLoginReducer(userLogin: UserLogin, action: UserLoginAction) {
  switch (action.type) {
    case "logged_in":
      return {
        ...userLogin,
        isAuthenticated: true,
        username: action.payload.username,
      }
    case "logged_out":
      return {
        ...userLogin,
        isAuthenticated: false,
        username: null,
      }
  }
}

export interface AuthContextValue {
  userLogin: UserLogin
  login: typeof loginApi,
  logout: () => Promise<void>,
}
export const AuthContext = createContext<AuthContextValue | null>(null);

// a wrapper component to provide AuthContext to the app
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [userLogin, dispatchUserLogin] = useReducer(userLoginReducer, {
    isAuthenticated: false,
    username: null,
  });
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    registerAuthHandlers(
      () => {
        // onSuccess: do nothing; user identity is unchanged, and access token 
        // is stored by the interceptor in apiClient; 
      },
      () => {
        // onFailure: the refresh token is expired or invalid; the session is over
        dispatchUserLogin({ type: "logged_out" });
      }
    );

    // auto login on mount if a valid refresh token is present
    const restoreSession = async () => {
      try {
        // refreshApi throws if there's no valid refresh token; stay logged out
        const { accessToken } = await refreshApi();
        setAccessToken(accessToken);

        const meResponse = await meApi();
        if (!meResponse.success) {
          // refresh succeeded but we couldn't load the user; treat as logged out
          setAccessToken(null);
          return;
        }

        dispatchUserLogin({
          type: "logged_in",
          payload: { username: meResponse.data.username }
        });
      } catch {
        // missing/expired/invalid refresh token, or network error; stay logged out
        setAccessToken(null);
      } finally {
        setIsLoading(false);
      }
    };

    restoreSession();

    // clear the handlers when the provider unmounts
    return () => registerAuthHandlers(() => {}, () => {});
  }, []);

  const login: typeof loginApi = async (request) => {
    const response = await loginApi(request);
    if (!response.success) {
      return response;
    }

    // on login success, set the access token first so apiClient can
    // authenticate the /me request, then retrieve the user's info
    setAccessToken(response.data.accessToken);
    const meResponse = await meApi();
    if (!meResponse.success) {
      // couldn't load user info; treat login as failed and clear the token
      setAccessToken(null);
      return { success: false, error: meResponse.error };
    }

    const me = meResponse.data;
    dispatchUserLogin({
      type: "logged_in",
      payload: { username: me.username }
    });

    return response;
  };

  const logout = async () => {
    await logoutApi();
    // upon logout, regardless of success or failure, clear access token and userLogin
    setAccessToken(null);
    dispatchUserLogin({ type: "logged_out" });
  };

  const authContextValue: AuthContextValue = {
    userLogin: userLogin,
    login: login,
    logout: logout,
  }

  // while restoring the session, render a full-page placeholder instead of
  // the app, so protected routes don't briefly redirect to /login
  if (isLoading) {
    return (
      <div className="flex h-screen w-full items-center justify-center">
        <Spinner className="size-8" />
      </div>
    );
  }

  return <AuthContext value={authContextValue}>
    {children}
  </AuthContext>
}
