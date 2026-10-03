import { setAccessToken } from "@/api/apiClient";
import { loginApi, type LoginRequest } from "@/api/auth/login";
import { logoutApi } from "@/api/auth/logout";

import { createContext, useReducer } from "react";

interface UserLogin {
  isAuthenticated: boolean,
  // displayName: string | null, // to be reviewed
  username: string | null,
}

// actions to dispatch UserLogin states
type UserLoginAction =
  | { type: "logged_in", payload: { username: string } }
  | { type: "logged_out" }
  | { type: "refresh_success", payload: { username: string } }

function userLoginReducer(userLogin: UserLogin, action: UserLoginAction) {
  switch (action.type) {
    case "logged_in":
    case "refresh_success":
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

  const login = async (request: LoginRequest) => {
    const response = await loginApi(request);
    if (response.success) {
      // upon login, set access token and userLogin 
      setAccessToken(response.data.accessToken);
      dispatchUserLogin({
        type: "logged_in",
        payload: { username: request.username }
      });
    }
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
  return <AuthContext value={authContextValue}>
    {children}
  </AuthContext>
}
