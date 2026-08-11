import React, { createContext, useContext, useReducer } from "react";
import * as loginApi from "@/api/auth/login";

// states of currently logged in user. 
interface UserLogin {
  isAuthenticated: boolean,
  // userId: number | null,
  username: string | null,
  accessToken: string | null,
  // isLogging: boolean
}

// actions to dispatch UserLogin states
type UserLoginAction =
  // | { type: "logging", }
  | {
    type: "logged_in",
    payload: {
      // userId: number,
      username: string,
      accessToken: string
    }
  }
  | { type: "logged_out" }
// | { type: "logging_failed" }

// reducer to update UserLogin states
function userLoginReducer(userLogin: UserLogin, action: UserLoginAction): UserLogin {
  switch (action.type) {
    // case "logging":
    //   return {
    //     ...userLogin,
    //     isLogging: true
    //   }
    case "logged_in":
      return {
        ...userLogin,
        isAuthenticated: true,
        // userId: action.payload.userId,
        username: action.payload.username,
        accessToken: action.payload.accessToken,
      }
    case "logged_out":
      return {
        ...userLogin,
        isAuthenticated: false,
        // userId: null,
        username: null,
        accessToken: null
      }
    // case "logging_failed":
    //   return {
    //     ...userLogin,
    //     isLogging: false
    //   }
  }
}

interface AuthContextValue {
  userLogin: UserLogin,
  login: (username: string, password: string) => Promise<boolean>,
  logout: () => Promise<void>,
}

export const AuthContext = createContext<AuthContextValue | null>(null);

// a wrapper component to provide AuthContext to its children, which is the entire app
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [userLogin, dispatchUserLogin] = useReducer(userLoginReducer, {
    isAuthenticated: false,
    // userId: null,
    username: null,
    accessToken: null
  });
  const login = async (username: string, password: string) => {
    const loginResponse = await loginApi.login({
      username: username,
      password: password
    });
    if (loginResponse.success) {
      dispatchUserLogin({
        type: "logged_in",
        payload: {
          // userId: 123,
          username: username,
          accessToken: loginResponse.data.accessToken
        }
      });
      return true;
    }
    else {
      // ignore the login error from API since login failure error is rather trivial.
      // the login page will just display a general error message.
      return false;
    }
  }
  const logout = async () => {

  };

  return <AuthContext value={{ userLogin, login, logout }}>
    {children}
  </AuthContext>
}

// a helper class for children to access auth context
export function useAuth(): AuthContextValue {
  const authContext = useContext(AuthContext);
  if (authContext === null)
    throw new Error("Auth context is not defined, which shouldn't happen.");
  return authContext;
}

// login flow:

// login page: submit form => call api
// - success: set current user info with access token
// - failure: prompt error in login page
// the login page needs:
// - user login dispatch
// - loading status


// logout flow:
// any page: click log out => call api
// - success: set current user info
// - failure: prompt error in the page.


// access resource flow /urls:
// on load: check if authenticated, if no, redirect to login page
// if authenticated: send request with access token,
// on 401 failure, redirect to login page (don't consider refresh first)

