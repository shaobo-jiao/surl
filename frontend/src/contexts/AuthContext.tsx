import React, { createContext, useContext, useReducer } from "react";

// states of currently logged in user. 
interface UserLogin {
  isAuthenticated: boolean,
  username: string | null,
  accessToken: string | null,
}

// actions to dispatch UserLogin states
type UserLoginAction =
  | {
    type: "logged_in",
    payload: {
      username: string,
      accessToken: string
    }
  }
  | { type: "logged_out" }

// reducer to update UserLogin states
function userLoginReducer(userLogin: UserLogin, action: UserLoginAction): UserLogin {
  switch (action.type) {
    case "logged_in":
      return {
        ...userLogin,
        isAuthenticated: true,
        username: action.payload.username,
        accessToken: action.payload.accessToken,
      }
    case "logged_out":
      return {
        ...userLogin,
        isAuthenticated: false,
        username: null,
        accessToken: null
      }
  }
}

interface AuthContextValue {
  userLogin: UserLogin,
  dispatchLogin: (username: string, accessToken: string) => void,
  dispatchLogout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

// a wrapper component to provide AuthContext to its children, which is the entire app
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [userLogin, dispatchUserLogin] = useReducer(userLoginReducer, {
    isAuthenticated: false,
    username: null,
    accessToken: null
  });

  const dispatchLogin = (username: string, accessToken: string) => {
    dispatchUserLogin({
      type: "logged_in",
      payload: {
        username: username,
        accessToken: accessToken
      }
    });
  };

  const dispatchLogout = () => {
    dispatchUserLogin({
      type: "logged_out"
    });
  };

  return <AuthContext value={{ userLogin, dispatchLogin, dispatchLogout }}>
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

