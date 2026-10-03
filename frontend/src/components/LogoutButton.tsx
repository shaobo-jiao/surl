import { Button } from "./ui/button";
import { useState } from "react";
import { Spinner } from "./ui/spinner";
import { useAuth } from "@/hooks/useAuth";


export default function LogoutButton() {
  const [isLoggingOut, setIsLoggingOut] = useState<boolean>(false);
  const { logout } = useAuth();

  const handleClick = async () => {
    setIsLoggingOut(true);
    await logout();
    setIsLoggingOut(false);
  };

  return isLoggingOut
    ? <Button variant="link" disabled> <Spinner /> Logout</Button>
    : <Button variant="link" onClick={handleClick}>Logout</Button>;
}