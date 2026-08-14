import type React from "react";
import { Button } from "./ui/button";
import { useState } from "react";
import { Spinner } from "./ui/spinner";
import * as logoutApi from "@/api/auth/logout";
import { useAuth } from "@/contexts/AuthContext";


export default function LogoutButton() {
  const [isLoggingOut, setIsLoggingOut] = useState<boolean>(false);
  const { dispatchLogout } = useAuth();

  const handleClick = async (e: React.MouseEvent<HTMLButtonElement>) => {
    setIsLoggingOut(true);

    await logoutApi.logout();    
    // no matter of api logout result, clear UserLogin state;
    dispatchLogout();

    setIsLoggingOut(false);
  };

  return isLoggingOut
    ? <Button variant="link" disabled> <Spinner /> Logout</Button>
    : <Button variant="link" onClick={handleClick}>Logout</Button>;
}